// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AttendanceSettings in the contract.
class AttendanceSettingsApi {
  AttendanceSettingsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/attendance-settings  (Get attendance settings)
  Future<AttendanceSettingsDto> AttendanceSettings_Get({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceSettings_Get',
      method: 'GET',
      path: '/api/app/attendance-settings',
      cancelToken: cancelToken,
    );
    return AttendanceSettingsDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/attendance-settings  (Update attendance settings)
  Future<AttendanceSettingsDto> AttendanceSettings_Update({
    required AttendanceSettingsUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceSettings_Update',
      method: 'PUT',
      path: '/api/app/attendance-settings',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AttendanceSettingsDto.fromJson(data as Map<String, dynamic>);
  }

}
