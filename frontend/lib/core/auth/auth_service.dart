import 'package:flutter_web_auth_2/flutter_web_auth_2.dart';

import '../../api/generated/hr_api.dart';
import '../../app/config.dart';
import 'pkce.dart';
import 'token_store.dart';

class AuthFailure implements Exception {
  AuthFailure(this.reason);
  final String reason;
  @override
  String toString() => 'AuthFailure($reason)';
}

/// Authorization Code + PKCE against the server's hosted login page (x-oauth-client).
/// No password ever passes through the app.
class AuthService {
  AuthService(this._api, this._store);

  final HrApi _api;
  final TokenStore _store;
  OpenIdConfigurationDto? _discovery;
  Future<String?>? _refreshing;

  Future<OpenIdConfigurationDto> discovery() async =>
      _discovery ??= await _api.auth.OpenIdConfiguration_Get();

  Future<bool> hasSession() async => (await _store.load()) != null;

  Future<void> signIn({required String language, String? theme}) async {
    final config = await discovery();
    final pkce = Pkce.generate();
    final state = Pkce.randomState();
    final url = Uri.parse(config.authorizationEndpoint).replace(queryParameters: <String, String>{
      'response_type': 'code',
      'client_id': AppConfig.clientId,
      'redirect_uri': AppConfig.redirectUri,
      'scope': AppConfig.scopes.join(' '),
      'code_challenge': pkce.challenge,
      'code_challenge_method': 'S256',
      'state': state,
      'ui_locales': language,
      'culture': language,
      'ui-culture': language,
      // read by the server sign-in page so it opens in the app's light/dark mode
      if (theme != null) 'hr_theme': theme,
    });
    final result = await FlutterWebAuth2.authenticate(
      url: url.toString(),
      callbackUrlScheme: AppConfig.callbackScheme,
    );
    final callback = Uri.parse(result);
    final params = callback.queryParameters;
    if (params['error'] != null) throw AuthFailure(params['error']!);
    if (params['state'] != state) throw AuthFailure('state_mismatch');
    final code = params['code'];
    if (code == null) throw AuthFailure('missing_code');
    final response = await _api.auth.ConnectToken_Issue(
      body: ConnectTokenRequestDto(
        grantType: 'authorization_code',
        clientId: AppConfig.clientId,
        code: code,
        redirectUri: AppConfig.redirectUri,
        codeVerifier: pkce.verifier,
      ),
    );
    await _store.save(_toTokenSet(response, previousRefresh: null));
  }

  /// A token that is valid now, refreshing it first if it expired.
  Future<String?> validAccessToken() async {
    final tokens = await _store.load();
    if (tokens == null) return null;
    if (!tokens.isExpired) return tokens.accessToken;
    return refresh();
  }

  /// Exchanges the refresh token. Concurrent callers share one request.
  Future<String?> refresh() {
    return _refreshing ??= _doRefresh().whenComplete(() => _refreshing = null);
  }

  Future<String?> _doRefresh() async {
    final tokens = await _store.load();
    final refreshToken = tokens?.refreshToken;
    if (refreshToken == null) return null;
    try {
      final response = await _api.auth.ConnectToken_Issue(
        body: ConnectTokenRequestDto(
          grantType: 'refresh_token',
          clientId: AppConfig.clientId,
          refreshToken: refreshToken,
        ),
      );
      final next = _toTokenSet(response, previousRefresh: refreshToken);
      await _store.save(next);
      return next.accessToken;
    } catch (_) {
      await _store.clear();
      return null;
    }
  }

  /// Clears local tokens and, when the server advertises it, ends the hosted-login session
  /// so the next sign-in asks for credentials again.
  Future<void> signOut() async {
    final idToken = _store.current?.idToken;
    await _store.clear();
    try {
      final endSession = (await discovery()).endSessionEndpoint;
      if (endSession == null) return;
      final url = Uri.parse(endSession).replace(queryParameters: <String, String>{
        if (idToken != null) 'id_token_hint': idToken,
        'post_logout_redirect_uri': AppConfig.redirectUri,
        'client_id': AppConfig.clientId,
      });
      await FlutterWebAuth2.authenticate(url: url.toString(), callbackUrlScheme: AppConfig.callbackScheme);
    } catch (_) {
      // Best effort: local tokens are already gone.
    }
  }

  TokenSet _toTokenSet(ConnectTokenResponseDto r, {required String? previousRefresh}) => TokenSet(
        accessToken: r.accessToken,
        refreshToken: r.refreshToken ?? previousRefresh,
        idToken: r.idToken,
        expiresAt: DateTime.now().add(Duration(seconds: r.expiresIn)),
      );
}
