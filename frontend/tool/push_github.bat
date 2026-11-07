@echo off
rem Commits the current work and pushes the whole repository to GitHub.
rem Log read by Claude: frontend\tool\push_github.log
setlocal EnableDelayedExpansion
set "PATH=C:\Users\sajad\AppData\Local\Programs\Git\cmd;%PATH%"
set "REMOTE=https://github.com/zah202312-maker/HR.git"
set "LOG=%~dp0push_github.log"
cd /d "%~dp0..\.."
echo ===== %DATE% %TIME% ===== > "%LOG%"

where git >> "%LOG%" 2>&1
if errorlevel 1 (
  echo Git was not found. >> "%LOG%"
  echo Git was not found. Install it from https://git-scm.com and run this again.
  goto end
)

rem --- Your name and email on the commits (asked once) ---
for /f "delims=" %%a in ('git config user.name') do set "GNAME=%%a"
for /f "delims=" %%a in ('git config user.email') do set "GMAIL=%%a"
if not defined GNAME (
  set /p "GNAME=Your name for GitHub commits: "
  git config --global user.name "!GNAME!"
)
if not defined GMAIL (
  set /p "GMAIL=Your GitHub email: "
  git config --global user.email "!GMAIL!"
)
echo USER=!GNAME! ^<!GMAIL!^> >> "%LOG%"

rem --- Commit everything that is not ignored (secrets and build output are ignored) ---
set "SUBJECT=Reviewer manager rule and reviewer follow-up report"
echo --- status --- >> "%LOG%"
git branch --show-current >> "%LOG%" 2>&1
rem Copies of the secrets file (.bak) were picked up by an earlier run: take them out of git, keep them on disk.
git rm --cached --ignore-unmatch -q "src/HR_Mangment2.DbMigrator/appsettings.secrets.json.bak" "src/HR_Mangment2.HttpApi.Host/appsettings.secrets.json.bak" >> "%LOG%" 2>&1
git add -A >> "%LOG%" 2>&1
git status --short >> "%LOG%" 2>&1
for /f "delims=" %%a in ('git log -1 --format^=%%s') do set "LAST=%%a"
git diff --cached --quiet
if errorlevel 1 (
  rem The previous run's commit was never pushed (no remote yet), so it is rewritten without the backups.
  if "!LAST!"=="%SUBJECT%" (
    git commit --amend --no-edit >> "%LOG%" 2>&1
    echo AMEND_EXIT=!ERRORLEVEL! >> "%LOG%"
  ) else (
    git commit -m "%SUBJECT%" -m "The reviewer reports to the executive director or the HR manager and starts at HR. Adds ReviewReport_* (contract, backend, tests, Flutter screens)." >> "%LOG%" 2>&1
    echo COMMIT_EXIT=!ERRORLEVEL! >> "%LOG%"
  )
) else (
  echo NOTHING_TO_COMMIT >> "%LOG%"
)

rem --- Safety check: no secret in the files or anywhere in the history that would be pushed ---
git ls-files | findstr /i /l "appsettings.secrets.json .pfx" > "%TEMP%\hr_secret_check.txt"
git log --all --name-only --format= -- "*appsettings.secrets.json*" "*.pfx" >> "%TEMP%\hr_secret_check.txt" 2>nul
for %%s in ("%TEMP%\hr_secret_check.txt") do if %%~zs GTR 0 (
  echo SECRET_FILES_TRACKED: >> "%LOG%"
  type "%TEMP%\hr_secret_check.txt" >> "%LOG%"
  echo A secret file is tracked by git. Nothing was pushed. Tell Claude.
  goto end
)

rem --- Remote and push (a GitHub sign-in window opens the first time) ---
git remote remove origin >nul 2>&1
git remote add origin "%REMOTE%" >> "%LOG%" 2>&1
echo --- push --- >> "%LOG%"
echo Pushing to %REMOTE% ... if a GitHub sign-in window opens, sign in and allow access.
git push -u origin --all >> "%LOG%" 2>&1
echo PUSH_EXIT=!ERRORLEVEL! >> "%LOG%"
git show-ref --verify --quiet refs/heads/main
if errorlevel 1 (
  git push origin HEAD:main >> "%LOG%" 2>&1
  echo PUSH_MAIN_EXIT=!ERRORLEVEL! >> "%LOG%"
)
git push origin --tags >> "%LOG%" 2>&1
git remote -v >> "%LOG%" 2>&1
git log --oneline -3 >> "%LOG%" 2>&1

:end
echo ===== DONE ===== >> "%LOG%"
echo Done. You can close this window.
pause
endlocal
