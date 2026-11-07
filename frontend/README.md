# HR — Flutter front end (mobile + web)

Built from `api-contract/openapi.yaml` (the only source for routes and fields) and the
approved UI/UX package in `frontend/HR_project` + `frontend/HR-Design-System.md`.

## Layout
```
tool/generate_api.py        contract → Dart client generator (do not hand-write API calls)
lib/api/generated/          GENERATED: models, one *_api.dart per tag, operations.dart
lib/api/runtime/            transport (Dio wrapper, ApiException, JSON helpers)
lib/core/                   auth (PKCE), network (interceptors), session (access gate),
                            theme (Design System V3 tokens), l10n (ar/en), widgets
lib/features/<feature>/     screens: home, requests, leaves, attendance, notifications, profile, auth
web/auth/callback.html      OAuth redirect page for Flutter Web
```
Stack: Riverpod, go_router, Dio, flutter_web_auth_2 (Authorization Code + PKCE).

## First run (Windows, from the repository root)
```powershell
cd frontend
flutter create --org com.hrsystem --project-name hr_app --platforms=android,ios,web .
flutter pub get
flutter analyze
flutter test
```
`flutter create .` only adds the missing platform folders; it does not overwrite `lib/`.

## Regenerate the API client after any contract change
```powershell
pip install pyyaml
python frontend/tool/generate_api.py
```
Every method name is the full `operationId` (`x-server-requirements.clients`), for example
`api.requests.Request_Approve(id: ..., body: NoteInputDto(...))`.

## Run
Web (needs the two backend settings in PROGRESS.md, section "الفرونت", F1/F2):
```powershell
flutter run -d chrome --web-port 5000 `
  --dart-define=HR_API_BASE_URL=https://localhost:44382 `
  --dart-define=HR_WEB_REDIRECT_URI=http://localhost:5000/auth/callback.html
```
Android emulator: `--dart-define=HR_API_BASE_URL=https://10.0.2.2:44382` (the dev certificate must be trusted
by the emulator).

### Android: OAuth callback
After `flutter create`, add inside `<application>` in `android/app/src/main/AndroidManifest.xml`:
```xml
<activity android:name="com.linusu.flutter_web_auth_2.CallbackActivity" android:exported="true">
  <intent-filter android:label="flutter_web_auth_2">
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="com.hrsystem.app" />
  </intent-filter>
</activity>
```

## Rules kept in code
- UI visibility from `grantedPolicies` + each operation's `x-permission` (`Session.can(HrOps.X)`); the server still decides.
- Errors are matched on `error.code` (`lib/core/l10n/error_messages.dart`, texts from the UI/UX package), never on message text.
- Access gate: 403 `Hr:Identity:PasswordChangeRequired` / `MfaEnrollmentRequired` and `accessGate` from `/me` route to the mandatory screen.
- `Accept-Language` set in one interceptor; tokens in secure storage on mobile and memory only on web.
- `Idempotency-Key` (uuid per form) on create operations that declare it.
