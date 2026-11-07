import 'package:flutter/foundation.dart';

/// Build-time configuration. Override with --dart-define, for example:
///   flutter run -d chrome --web-port 5000 \
///     --dart-define=HR_API_BASE_URL=https://localhost:44382 \
///     --dart-define=HR_WEB_REDIRECT_URI=http://localhost:5000/auth/callback.html
class AppConfig {
  const AppConfig._();

  /// The single server (API + OpenIddict) from the contract `servers` list.
  static const String apiBaseUrl =
      String.fromEnvironment('HR_API_BASE_URL', defaultValue: 'https://localhost:44382');

  /// x-oauth-client in openapi.yaml.
  static const String clientId = 'HR_Mangment2_Flutter';
  static const List<String> scopes = <String>['openid', 'profile', 'offline_access', 'HR_Mangment2'];

  static const String mobileRedirectUri =
      String.fromEnvironment('HR_MOBILE_REDIRECT_URI', defaultValue: 'com.hrsystem.app://oauth/callback');

  /// The contract says: replace the web URL per environment. The page must be served by the
  /// Flutter web app itself (web/auth/callback.html) and be seeded in OpenIddict RedirectUris.
  static const String webRedirectUri = String.fromEnvironment('HR_WEB_REDIRECT_URI',
      defaultValue: 'http://localhost:5000/auth/callback.html');

  static String get redirectUri => kIsWeb ? webRedirectUri : mobileRedirectUri;

  /// Custom URL scheme that flutter_web_auth_2 waits for on mobile.
  static String get callbackScheme => kIsWeb ? Uri.parse(webRedirectUri).scheme : Uri.parse(mobileRedirectUri).scheme;
}
