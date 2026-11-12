# Run the app on a USB-connected Android phone against the local backend.
# The backend (HttpApi.Host) must be running on https://localhost:44382.
param([string]$Device = '')

$sdk = Join-Path $env:LOCALAPPDATA 'Android\Sdk\platform-tools'
$env:Path = "D:\development\flutter\bin;$sdk;$env:Path"

# Phone's localhost:44382 -> this computer's localhost:44383, the phone-only HTTPS endpoint whose
# certificate is signed by the local dev CA installed on the phone. The OAuth issuer stays
# https://localhost:44382. See frontend/README.md "Physical Android phone".
adb reverse tcp:44382 tcp:44383 | Out-Null

Set-Location (Join-Path $PSScriptRoot '..')
$deviceArgs = @()
if ($Device) { $deviceArgs = @('-d', $Device) }
flutter run @deviceArgs --dart-define=HR_API_BASE_URL=https://localhost:44382
