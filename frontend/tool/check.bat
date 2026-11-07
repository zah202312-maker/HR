@echo off
rem Runs the front-end checks and writes everything to tool\check.log (read by Claude).
setlocal
cd /d "%~dp0.."
where flutter >nul 2>&1 || if exist "D:\development\flutter\bin\flutter.bat" set "PATH=D:\development\flutter\bin;%PATH%"
set LOG=%~dp0check.log
echo ===== %DATE% %TIME% ===== > "%LOG%"
echo --- tools --- >> "%LOG%"
where flutter >> "%LOG%" 2>&1
where dart >> "%LOG%" 2>&1
where python >> "%LOG%" 2>&1
where dotnet >> "%LOG%" 2>&1
where git >> "%LOG%" 2>&1
call flutter --version >> "%LOG%" 2>&1
if errorlevel 1 (
  echo FLUTTER_NOT_FOUND >> "%LOG%"
  dir /b "C:\src" "C:\flutter" "D:\flutter" "%USERPROFILE%\flutter" "%LOCALAPPDATA%\flutter" >> "%LOG%" 2>&1
  goto end
)
if not exist android (
  echo --- flutter create --- >> "%LOG%"
  call flutter create --org com.hrsystem --project-name hr_app --platforms=android,ios,web . >> "%LOG%" 2>&1
)
echo --- pub get --- >> "%LOG%"
call flutter pub get >> "%LOG%" 2>&1
echo --- analyze --- >> "%LOG%"
call flutter analyze --no-fatal-infos >> "%LOG%" 2>&1
echo ANALYZE_EXIT=%ERRORLEVEL% >> "%LOG%"
echo --- test --- >> "%LOG%"
call flutter test >> "%LOG%" 2>&1
echo TEST_EXIT=%ERRORLEVEL% >> "%LOG%"
:end
echo ===== DONE ===== >> "%LOG%"
endlocal
