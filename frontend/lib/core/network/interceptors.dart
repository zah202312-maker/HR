import 'package:dio/dio.dart';

import '../../api/runtime/api_client.dart';
import '../auth/auth_service.dart';
import 'session_signals.dart';

/// One place sets Accept-Language (x-server-requirements.language).
class LanguageInterceptor extends Interceptor {
  LanguageInterceptor(this.language);
  final String Function() language;

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    options.headers['Accept-Language'] = language();
    handler.next(options);
  }
}

/// Adds the bearer token, refreshes it once on expiry or 401, and turns the
/// access-gate 403 codes into session signals (x-global-rules.access-gates).
class AuthInterceptor extends QueuedInterceptor {
  AuthInterceptor({required this.auth, required this.signals});

  final AuthService auth;
  final SessionSignals signals;

  /// The retry goes through a bare Dio so it does not wait on this queued interceptor.
  final Dio _retryDio = Dio();

  static const _retried = 'hrRetried';

  @override
  Future<void> onRequest(RequestOptions options, RequestInterceptorHandler handler) async {
    if (options.extra[HrApiClient.skipAuthKey] == true) return handler.next(options);
    final token = await auth.validAccessToken();
    if (token != null) options.headers['Authorization'] = 'Bearer $token';
    handler.next(options);
  }

  @override
  Future<void> onError(DioException err, ErrorInterceptorHandler handler) async {
    final status = err.response?.statusCode;
    final options = err.requestOptions;
    if (status == 401 && options.extra[HrApiClient.skipAuthKey] != true && options.extra[_retried] != true) {
      final refreshed = await auth.refresh();
      if (refreshed != null) {
        options.extra[_retried] = true;
        options.headers['Authorization'] = 'Bearer $refreshed';
        try {
          final response = await _retryDio.fetch<Object?>(options);
          return handler.resolve(response);
        } on DioException catch (e) {
          return handler.next(e);
        }
      }
      signals.emit(SessionSignal.signedOut);
    } else if (status == 403) {
      final data = err.response?.data;
      final code = data is Map && data['error'] is Map ? (data['error'] as Map)['code'] : null;
      if (code == ApiException.passwordChangeRequired) signals.emit(SessionSignal.passwordChangeRequired);
      if (code == ApiException.mfaEnrollmentRequired) signals.emit(SessionSignal.mfaEnrollmentRequired);
    }
    handler.next(err);
  }
}
