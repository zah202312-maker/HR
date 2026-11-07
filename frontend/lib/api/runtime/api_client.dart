// Hand-written transport for the generated client. It does not know any route:
// every route, method and body comes from the generated *_api.dart files.
import 'dart:convert';
import 'dart:typed_data';

import 'package:dio/dio.dart';

/// Thrown by every generated method. UI code switches on [code], never on [message].
class ApiException implements Exception {
  ApiException({
    required this.statusCode,
    this.code,
    this.message,
    this.details,
    this.validationErrors = const <ApiValidationError>[],
    this.data,
    this.operationId,
    this.isNetworkError = false,
  });

  /// HTTP status, 0 when the server was not reached.
  final int statusCode;

  /// `error.code` of RemoteServiceErrorResponse (`Hr:<Module>:<Name>`), or the OAuth `error` value.
  final String? code;
  final String? message;
  final String? details;
  final List<ApiValidationError> validationErrors;
  final Map<String, dynamic>? data;
  final String? operationId;
  final bool isNetworkError;

  bool get isUnauthorized => statusCode == 401;
  bool get isForbidden => statusCode == 403;
  bool get isNotFound => statusCode == 404;
  bool get isConflict => statusCode == 409;
  bool get isBusinessRule => statusCode == 422;
  bool get isValidation => statusCode == 400;

  static const passwordChangeRequired = 'Hr:Identity:PasswordChangeRequired';
  static const mfaEnrollmentRequired = 'Hr:Identity:MfaEnrollmentRequired';
  bool get isAccessGate => code == passwordChangeRequired || code == mfaEnrollmentRequired;

  factory ApiException.fromDio(DioException e, String operationId) {
    final response = e.response;
    if (response == null) {
      return ApiException(statusCode: 0, operationId: operationId, isNetworkError: true, message: e.message);
    }
    var body = response.data;
    if (body is List<int>) {
      // Binary endpoints: the error envelope arrives as bytes.
      try {
        body = jsonDecode(utf8.decode(body));
      } catch (_) {}
    } else if (body is String && body.isNotEmpty) {
      try {
        body = jsonDecode(body);
      } catch (_) {}
    }
    Map<String, dynamic>? error;
    String? oauthError;
    if (body is Map) {
      final map = body.cast<String, dynamic>();
      if (map['error'] is Map) {
        error = (map['error'] as Map).cast<String, dynamic>();
      } else if (map['error'] is String) {
        oauthError = map['error'] as String; // /connect/token uses OAuth 2.0 errors
        error = <String, dynamic>{'message': map['error_description']};
      }
    }
    final validation = <ApiValidationError>[];
    final rawValidation = error?['validationErrors'];
    if (rawValidation is List) {
      for (final v in rawValidation) {
        if (v is Map) {
          validation.add(ApiValidationError(
            message: v['message'] as String?,
            members: (v['members'] as List?)?.map((m) => m.toString()).toList() ?? const <String>[],
          ));
        }
      }
    }
    return ApiException(
      statusCode: response.statusCode ?? 0,
      code: oauthError ?? error?['code'] as String?,
      message: error?['message'] as String?,
      details: error?['details'] as String?,
      validationErrors: validation,
      data: (error?['data'] as Map?)?.cast<String, dynamic>(),
      operationId: operationId,
    );
  }

  @override
  String toString() => 'ApiException($statusCode, $code, $operationId)';
}

class ApiValidationError {
  const ApiValidationError({this.message, this.members = const <String>[]});
  final String? message;

  /// camelCase JSON paths of the invalid fields.
  final List<String> members;
}

/// A file returned by a binary endpoint (export, PDF, attachment).
class HrBinaryResponse {
  const HrBinaryResponse({required this.bytes, this.fileName, this.contentType});
  final Uint8List bytes;
  final String? fileName;
  final String? contentType;
}

/// Dio wrapper used by the generated APIs. Interceptors (token, language,
/// access gate) are attached by the app when it creates the Dio instance.
class HrApiClient {
  HrApiClient(this.dio);
  final Dio dio;

  static const skipAuthKey = 'hrSkipAuth';
  static const operationKey = 'hrOperationId';

  Options _options(String operationId, String method, Map<String, dynamic>? headers, String? contentType,
      bool skipAuth, ResponseType? responseType) {
    return Options(
      method: method,
      headers: headers,
      contentType: contentType,
      responseType: responseType,
      extra: <String, dynamic>{operationKey: operationId, skipAuthKey: skipAuth},
    );
  }

  Future<Object?> send({
    required String operationId,
    required String method,
    required String path,
    Map<String, dynamic>? query,
    Map<String, dynamic>? headers,
    Object? body,
    String? contentType,
    bool skipAuth = false,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await dio.request<Object?>(
        path,
        data: body,
        queryParameters: query,
        cancelToken: cancelToken,
        options: _options(operationId, method, headers, contentType, skipAuth, ResponseType.json),
      );
      return response.data;
    } on DioException catch (e) {
      if (e.type == DioExceptionType.cancel) rethrow;
      throw ApiException.fromDio(e, operationId);
    }
  }

  Future<HrBinaryResponse> sendBinary({
    required String operationId,
    required String method,
    required String path,
    Map<String, dynamic>? query,
    Map<String, dynamic>? headers,
    Object? body,
    String? contentType,
    bool skipAuth = false,
    CancelToken? cancelToken,
  }) async {
    try {
      final response = await dio.request<List<int>>(
        path,
        data: body,
        queryParameters: query,
        cancelToken: cancelToken,
        options: _options(operationId, method, headers, contentType, skipAuth, ResponseType.bytes),
      );
      final disposition = response.headers.value('content-disposition');
      return HrBinaryResponse(
        bytes: Uint8List.fromList(response.data ?? const <int>[]),
        fileName: _fileName(disposition),
        contentType: response.headers.value('content-type'),
      );
    } on DioException catch (e) {
      if (e.type == DioExceptionType.cancel) rethrow;
      throw ApiException.fromDio(e, operationId);
    }
  }

  static String? _fileName(String? disposition) {
    if (disposition == null) return null;
    final star = RegExp(r"filename\*=UTF-8''([^;]+)", caseSensitive: false).firstMatch(disposition);
    if (star != null) return Uri.decodeComponent(star.group(1)!);
    final plain = RegExp(r'filename="?([^";]+)"?', caseSensitive: false).firstMatch(disposition);
    return plain?.group(1);
  }
}
