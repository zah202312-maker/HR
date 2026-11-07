// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Auth in the contract.
class AuthApi {
  AuthApi(this.client);
  final HrApiClient client;

  /// GET /.well-known/openid-configuration  (Get the OpenID Connect discovery document)
  Future<OpenIdConfigurationDto> OpenIdConfiguration_Get({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'OpenIdConfiguration_Get',
      method: 'GET',
      path: '/.well-known/openid-configuration',
      skipAuth: true,
      cancelToken: cancelToken,
    );
    return OpenIdConfigurationDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /connect/token  (Issue or refresh tokens)
  Future<ConnectTokenResponseDto> ConnectToken_Issue({
    required ConnectTokenRequestDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ConnectToken_Issue',
      method: 'POST',
      path: '/connect/token',
      body: body.toJson(),
      contentType: 'application/x-www-form-urlencoded',
      skipAuth: true,
      cancelToken: cancelToken,
    );
    return ConnectTokenResponseDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/abp/application-configuration  (Get user, permissions, localization and settings)
  Future<ApplicationConfigurationDto> ApplicationConfiguration_Get({
    bool? includeLocalizationResources,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ApplicationConfiguration_Get',
      method: 'GET',
      path: '/api/abp/application-configuration',
      query: <String, dynamic>{
        if (includeLocalizationResources != null) 'includeLocalizationResources': includeLocalizationResources,
      },
      cancelToken: cancelToken,
    );
    return ApplicationConfigurationDto.fromJson(data as Map<String, dynamic>);
  }

}
