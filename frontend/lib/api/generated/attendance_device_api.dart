// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AttendanceDevice in the contract.
class AttendanceDeviceApi {
  AttendanceDeviceApi(this.client);
  final HrApiClient client;

  /// POST /api/attendance-devices/punches  (Ingest raw punches from a device or local agent)
  Future<AttendancePunchBatchResultDto> AttendanceDevice_IngestPunches({
    required String deviceKey,
    required AttendancePunchBatchDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceDevice_IngestPunches',
      method: 'POST',
      path: '/api/attendance-devices/punches',
      headers: <String, dynamic>{
        'X-Device-Key': deviceKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AttendancePunchBatchResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance-device  (List attendance devices)
  Future<AttendanceDevicePagedResultDto> AttendanceDevice_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceDevice_List',
      method: 'GET',
      path: '/api/app/attendance-device',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
      },
      cancelToken: cancelToken,
    );
    return AttendanceDevicePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/attendance-device  (Register an attendance device)
  Future<AttendanceDeviceKeyDto> AttendanceDevice_Create({
    String? idempotencyKey,
    required AttendanceDeviceCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceDevice_Create',
      method: 'POST',
      path: '/api/app/attendance-device',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AttendanceDeviceKeyDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/attendance-device/{id}  (Update an attendance device)
  Future<AttendanceDeviceDto> AttendanceDevice_Update({
    required String id,
    required AttendanceDeviceUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceDevice_Update',
      method: 'PUT',
      path: '/api/app/attendance-device/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AttendanceDeviceDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/attendance-device/{id}/rotate-key  (Rotate a device key)
  Future<AttendanceDeviceKeyDto> AttendanceDevice_RotateKey({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AttendanceDevice_RotateKey',
      method: 'POST',
      path: '/api/app/attendance-device/${Uri.encodeComponent(id)}/rotate-key',
      cancelToken: cancelToken,
    );
    return AttendanceDeviceKeyDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance-device/{id}/user-map  (List device user mappings)
  Future<DeviceUserMapPagedResultDto> DeviceUserMap_List({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DeviceUserMap_List',
      method: 'GET',
      path: '/api/app/attendance-device/${Uri.encodeComponent(id)}/user-map',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return DeviceUserMapPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/attendance-device/{id}/user-map  (Map a device user to an employee)
  Future<DeviceUserMapDto> DeviceUserMap_Create({
    required String id,
    required DeviceUserMapCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DeviceUserMap_Create',
      method: 'POST',
      path: '/api/app/attendance-device/${Uri.encodeComponent(id)}/user-map',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return DeviceUserMapDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/attendance-device/{id}/user-map/{mapId}  (Remove a device user mapping)
  Future<void> DeviceUserMap_Delete({
    required String id,
    required String mapId,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'DeviceUserMap_Delete',
      method: 'DELETE',
      path: '/api/app/attendance-device/${Uri.encodeComponent(id)}/user-map/${Uri.encodeComponent(mapId)}',
      cancelToken: cancelToken,
    );
  }

  /// GET /api/app/attendance-device/{id}/unmapped-user  (List device users with no employee mapping)
  Future<UnmappedDeviceUserPagedResultDto> DeviceUserMap_ListUnmapped({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DeviceUserMap_ListUnmapped',
      method: 'GET',
      path: '/api/app/attendance-device/${Uri.encodeComponent(id)}/unmapped-user',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return UnmappedDeviceUserPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
