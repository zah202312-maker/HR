import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../api/runtime/api_client.dart';
import '../../app/config.dart';
import '../auth/auth_service.dart';
import '../auth/token_store.dart';
import '../l10n/locale_controller.dart';
import 'interceptors.dart';
import 'session_signals.dart';

Dio _baseDio(Ref ref) {
  final dio = Dio(BaseOptions(
    baseUrl: AppConfig.apiBaseUrl,
    connectTimeout: const Duration(seconds: 20),
    receiveTimeout: const Duration(seconds: 60),
  ));
  dio.interceptors.add(LanguageInterceptor(() => ref.read(localeProvider).languageCode));
  return dio;
}

final sessionSignalsProvider = Provider<SessionSignals>((ref) => SessionSignals());
final tokenStoreProvider = Provider<TokenStore>((ref) => TokenStore());

/// Client for the anonymous OpenIddict operations (discovery, /connect/token).
final authApiProvider = Provider<HrApi>((ref) => HrApi(HrApiClient(_baseDio(ref))));

final authServiceProvider = Provider<AuthService>(
  (ref) => AuthService(ref.watch(authApiProvider), ref.watch(tokenStoreProvider)),
);

/// The client every feature uses.
final hrApiProvider = Provider<HrApi>((ref) {
  final dio = _baseDio(ref);
  dio.interceptors.add(AuthInterceptor(
    auth: ref.watch(authServiceProvider),
    signals: ref.watch(sessionSignalsProvider),
  ));
  return HrApi(HrApiClient(dio));
});
