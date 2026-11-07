@echo off
rem Reviewer manager rule + reviewer follow-up report (2026-11-07):
rem stops the running backend, adds the EF migration once, builds, migrates the dev database,
rem runs the backend tests and the Flutter checks.
rem Log read by Claude: frontend\tool\review_check.log
setlocal EnableDelayedExpansion
set "PATH=D:\development\flutter\bin;C:\Users\sajad\AppData\Local\Programs\Git\cmd;%USERPROFILE%\.dotnet\tools;%PATH%"
set "TOOL=%~dp0"
set "LOG=%~dp0review_check.log"
cd /d "%~dp0..\.."
echo ===== %DATE% %TIME% ===== > "%LOG%"

echo [1/7] Stopping the running backend (it locks the build output)...
taskkill /F /IM HR_Mangment2.HttpApi.Host.exe >> "%LOG%" 2>&1

echo [2/7] Checking dotnet-ef...
echo --- dotnet-ef --- >> "%LOG%"
dotnet ef --version >> "%LOG%" 2>&1
if errorlevel 1 (
  dotnet tool install --global dotnet-ef --version 10.0.* >> "%LOG%" 2>&1
  dotnet ef --version >> "%LOG%" 2>&1
)

echo [3/7] Building...
echo --- build --- >> "%LOG%"
dotnet build HR_Mangment2.slnx -nologo -v q -clp:NoSummary >> "%LOG%" 2>&1
echo BUILD_EXIT=%ERRORLEVEL% >> "%LOG%"

echo [4/7] Adding the migration (only the first time)...
echo --- migration --- >> "%LOG%"
dir /b "src\HR_Mangment2.EntityFrameworkCore\Migrations\*_Hr_ReviewReports.cs" >nul 2>&1
if errorlevel 1 (
  pushd "src\HR_Mangment2.EntityFrameworkCore"
  dotnet ef migrations add Hr_ReviewReports --no-build >> "%LOG%" 2>&1
  echo MIGRATION_EXIT=!ERRORLEVEL! >> "%LOG%"
  popd
  dotnet build HR_Mangment2.slnx -nologo -v q -clp:NoSummary >> "%LOG%" 2>&1
  echo REBUILD_EXIT=!ERRORLEVEL! >> "%LOG%"
) else (
  echo MIGRATION_ALREADY_EXISTS >> "%LOG%"
)
dir /b "src\HR_Mangment2.EntityFrameworkCore\Migrations\*_Hr_ReviewReports*.cs" >> "%LOG%" 2>&1

echo [5/7] Migrating and seeding the development database...
echo --- migrator --- >> "%LOG%"
pushd "src\HR_Mangment2.DbMigrator"
dotnet run --no-build >> "%LOG%" 2>&1
echo MIGRATOR_EXIT=%ERRORLEVEL% >> "%LOG%"
popd

echo [6/7] Running the backend tests (several minutes)...
echo --- tests --- >> "%LOG%"
dotnet test HR_Mangment2.slnx --no-build -nologo -v q --logger "console;verbosity=minimal" >> "%LOG%" 2>&1
echo TEST_EXIT=%ERRORLEVEL% >> "%LOG%"

echo [7/7] Flutter analyze and test...
cd /d "%TOOL%.."
echo --- flutter analyze --- >> "%LOG%"
call flutter pub get >> "%LOG%" 2>&1
call flutter analyze --no-fatal-infos >> "%LOG%" 2>&1
echo ANALYZE_EXIT=%ERRORLEVEL% >> "%LOG%"
echo --- flutter test --- >> "%LOG%"
call flutter test >> "%LOG%" 2>&1
echo FLUTTER_TEST_EXIT=%ERRORLEVEL% >> "%LOG%"

echo ===== DONE ===== >> "%LOG%"
echo Done. Start the app again with tool\run_dev.bat. You can close this window.
endlocal
