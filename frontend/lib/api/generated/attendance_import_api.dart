// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AttendanceImport in the contract.
class AttendanceImportApi {
  AttendanceImportApi(this.client);
  final HrApiClient client;

  /// POST /api/app/attendance-import  (Upload an attendance file for import)
  Future<AttendanceImportDto> AttendanceImport_Create({
    required AttendanceImportUploadDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceImport_Create',
      method: 'POST',
      path: '/api/app/attendance-import',
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AttendanceImportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance-import  (List attendance imports)
  Future<AttendanceImportPagedResultDto> AttendanceImport_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    AttendanceImportStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceImport_List',
      method: 'GET',
      path: '/api/app/attendance-import',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return AttendanceImportPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance-import/{id}  (Get an attendance import result)
  Future<AttendanceImportDto> AttendanceImport_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceImport_Get',
      method: 'GET',
      path: '/api/app/attendance-import/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return AttendanceImportDto.fromJson(data as Map<String, dynamic>);
  }

}
