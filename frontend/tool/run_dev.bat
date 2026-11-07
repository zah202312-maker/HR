@echo off
rem Starts the backend (already built) and the Flutter web app for local development.
rem Logs read by Claude: src\HR_Mangment2.HttpApi.Host\Logs\logs.txt, tool\health.txt, tool\web.log
setlocal
set "PATH=D:\development\flutter\bin;C:\Users\sajad\AppData\Local\Programs\Git\cmd;%PATH%"
set "TOOL=%~dp0"
set "HOST=%~dp0..\..\src\HR_Mangment2.HttpApi.Host"

if /i "%1"=="migrate" (
  echo Seeding the database...
  pushd "%~dp0..\..\src\HR_Mangment2.DbMigrator"
  dotnet run > "%TOOL%migrate.log" 2>&1
  popd
)

echo [1/2] Starting the backend on https://localhost:44382 (window "HR backend")...
start "HR backend" /D "%HOST%" cmd /k "set ASPNETCORE_ENVIRONMENT=Development&& set ASPNETCORE_URLS=https://localhost:44382&& bin\Debug\net10.0\HR_Mangment2.HttpApi.Host.exe"

echo Waiting for the backend...
set /a N=0
:wait
set /a N+=1
curl -sk -o nul -w "%%{http_code}" https://localhost:44382/.well-known/openid-configuration > "%TOOL%health.txt" 2>nul
findstr /c:"200" "%TOOL%health.txt" >nul && goto up
if %N% GEQ 40 goto up
timeout /t 3 /nobreak >nul
goto wait
:up
echo BACKEND_ROUNDS=%N% >> "%TOOL%health.txt"

echo [2/2] Starting the Flutter web app on http://localhost:5000 (Edge)...
cd /d "%TOOL%.."
start "HR web" cmd /k "flutter run -d edge --web-port 5000 --no-web-resources-cdn --dart-define=HR_API_BASE_URL=https://localhost:44382 --dart-define=HR_WEB_REDIRECT_URI=http://localhost:5000/auth/callback.html"
echo Done. Keep both windows open while testing.
endlocal
