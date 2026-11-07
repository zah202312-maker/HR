@echo off
rem Restores NuGet packages and rebuilds the backend on this computer.
rem Needed after moving the project: the old build points to D:\projects\HR\HR_Mangment2.
rem Log read by Claude: tool\build.log
setlocal
set "TOOL=%~dp0"
cd /d "%~dp0..\.."
echo Restoring and building the backend (this can take several minutes)...
echo ===== %DATE% %TIME% ===== > "%TOOL%build.log"
dotnet restore HR_Mangment2.slnx >> "%TOOL%build.log" 2>&1
echo RESTORE_EXIT=%ERRORLEVEL% >> "%TOOL%build.log"
dotnet build src\HR_Mangment2.HttpApi.Host\HR_Mangment2.HttpApi.Host.csproj --no-restore -nologo -v q -clp:NoSummary >> "%TOOL%build.log" 2>&1
echo BUILD_EXIT=%ERRORLEVEL% >> "%TOOL%build.log"
echo ===== DONE ===== >> "%TOOL%build.log"
echo Done. You can close this window.
endlocal
