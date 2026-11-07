@echo off
rem Starts only the Flutter web app (backend must already be running).
rem First stops any stuck Flutter/Dart process that locks the web SDK download.
rem Logs read by Claude: tool\web_setup.log and tool\web_run.log (app output, errors included)
setlocal
set "PATH=D:\development\flutter\bin;C:\Users\sajad\AppData\Local\Programs\Git\cmd;%PATH%"
set "TOOL=%~dp0"
echo Stopping stuck Flutter/Dart processes...
taskkill /F /IM dart.exe /T > "%TOOL%web_setup.log" 2>&1
taskkill /F /IM dartaotruntime.exe /T >> "%TOOL%web_setup.log" 2>&1
timeout /t 2 /nobreak >nul
del /f /q "D:\development\flutter\bin\cache\downloads\storage.googleapis.com\flutter_infra_release\flutter\*\flutter-web-sdk.zip" >> "%TOOL%web_setup.log" 2>&1
echo Downloading the Flutter web SDK (first time only)...
call flutter precache --web >> "%TOOL%web_setup.log" 2>&1
echo PRECACHE_EXIT=%ERRORLEVEL% >> "%TOOL%web_setup.log"
cd /d "%TOOL%.."
echo Starting the app on http://localhost:5000 (Edge)...
call flutter run -d edge --web-port 5000 --no-web-resources-cdn --dart-define=HR_API_BASE_URL=https://localhost:44382 --dart-define=HR_WEB_REDIRECT_URI=http://localhost:5000/auth/callback.html 2>&1 | powershell -NoProfile -Command "$input | Tee-Object -FilePath '%TOOL%web_run.log'"
endlocal
