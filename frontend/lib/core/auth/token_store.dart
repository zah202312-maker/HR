import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class TokenSet {
  const TokenSet({required this.accessToken, this.refreshToken, this.idToken, required this.expiresAt});

  final String accessToken;
  final String? refreshToken;
  final String? idToken;
  final DateTime expiresAt;

  bool get isExpired => DateTime.now().isAfter(expiresAt.subtract(const Duration(seconds: 30)));
}

/// Mobile: secure storage. Web: memory only, as ConnectToken_Issue requires
/// ("على Flutter Web يُحفظ الرمز في الذاكرة"), so a page reload signs the user in again.
class TokenStore {
  TokenStore() : _secure = kIsWeb ? null : const FlutterSecureStorage();

  final FlutterSecureStorage? _secure;
  TokenSet? _memory;

  static const _kAccess = 'hr.access';
  static const _kRefresh = 'hr.refresh';
  static const _kId = 'hr.id';
  static const _kExpires = 'hr.expires';

  TokenSet? get current => _memory;

  Future<TokenSet?> load() async {
    if (_memory != null || _secure == null) return _memory;
    final access = await _secure.read(key: _kAccess);
    final expires = await _secure.read(key: _kExpires);
    if (access == null || expires == null) return null;
    _memory = TokenSet(
      accessToken: access,
      refreshToken: await _secure.read(key: _kRefresh),
      idToken: await _secure.read(key: _kId),
      expiresAt: DateTime.parse(expires),
    );
    return _memory;
  }

  Future<void> save(TokenSet tokens) async {
    _memory = tokens;
    final s = _secure;
    if (s == null) return;
    await s.write(key: _kAccess, value: tokens.accessToken);
    await s.write(key: _kExpires, value: tokens.expiresAt.toIso8601String());
    if (tokens.refreshToken != null) await s.write(key: _kRefresh, value: tokens.refreshToken);
    if (tokens.idToken != null) await s.write(key: _kId, value: tokens.idToken);
  }

  Future<void> clear() async {
    _memory = null;
    final s = _secure;
    if (s == null) return;
    await s.delete(key: _kAccess);
    await s.delete(key: _kRefresh);
    await s.delete(key: _kId);
    await s.delete(key: _kExpires);
  }
}
