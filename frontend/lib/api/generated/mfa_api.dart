// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Mfa in the contract.
class MfaApi {
  MfaApi(this.client);
  final HrApiClient client;

  /// GET /api/app/account/mfa  (Get my MFA status)
  Future<MfaStatusDto> Mfa_GetStatus({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Mfa_GetStatus',
      method: 'GET',
      path: '/api/app/account/mfa',
      cancelToken: cancelToken,
    );
    return MfaStatusDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/account/mfa/enroll  (Start TOTP enrollment)
  Future<MfaEnrollmentDto> Mfa_StartEnrollment({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Mfa_StartEnrollment',
      method: 'POST',
      path: '/api/app/account/mfa/enroll',
      cancelToken: cancelToken,
    );
    return MfaEnrollmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/account/mfa/confirm  (Confirm TOTP enrollment)
  Future<MfaRecoveryCodesDto> Mfa_ConfirmEnrollment({
    required MfaCodeInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Mfa_ConfirmEnrollment',
      method: 'POST',
      path: '/api/app/account/mfa/confirm',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MfaRecoveryCodesDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/account/mfa/recovery-codes  (Regenerate my recovery codes)
  Future<MfaRecoveryCodesDto> Mfa_RegenerateRecoveryCodes({
    required MfaCodeInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Mfa_RegenerateRecoveryCodes',
      method: 'POST',
      path: '/api/app/account/mfa/recovery-codes',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MfaRecoveryCodesDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/user-account/{id}/mfa/reset  (Reset a user's MFA after a lost device)
  Future<void> UserAccount_ResetMfa({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'UserAccount_ResetMfa',
      method: 'POST',
      path: '/api/app/user-account/${Uri.encodeComponent(id)}/mfa/reset',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
  }

}
