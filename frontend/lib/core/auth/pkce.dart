import 'dart:convert';
import 'dart:math';

import 'package:crypto/crypto.dart';

/// RFC 7636 code verifier and S256 challenge.
class Pkce {
  Pkce._(this.verifier, this.challenge);

  final String verifier;
  final String challenge;

  factory Pkce.generate() {
    final random = Random.secure();
    final bytes = List<int>.generate(64, (_) => random.nextInt(256));
    final verifier = base64UrlEncode(bytes).replaceAll('=', '');
    final challenge = base64UrlEncode(sha256.convert(ascii.encode(verifier)).bytes).replaceAll('=', '');
    return Pkce._(verifier, challenge);
  }

  static String randomState() {
    final random = Random.secure();
    return base64UrlEncode(List<int>.generate(24, (_) => random.nextInt(256))).replaceAll('=', '');
  }
}
