// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AttendanceCorrection in the contract.
class AttendanceCorrectionApi {
  AttendanceCorrectionApi(this.client);
  final HrApiClient client;

  /// POST /api/app/attendance-correction  (Submit an attendance correction request)
  Future<AttendanceCorrectionDto> AttendanceCorrection_Create({
    String? idempotencyKey,
    required AttendanceCorrectionCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceCorrection_Create',
      method: 'POST',
      path: '/api/app/attendance-correction',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AttendanceCorrectionDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance-correction/{id}  (Get an attendance correction request)
  Future<AttendanceCorrectionDto> AttendanceCorrection_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceCorrection_Get',
      method: 'GET',
      path: '/api/app/attendance-correction/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return AttendanceCorrectionDto.fromJson(data as Map<String, dynamic>);
  }

}
