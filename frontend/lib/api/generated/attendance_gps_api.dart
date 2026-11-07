// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AttendanceGps in the contract.
class AttendanceGpsApi {
  AttendanceGpsApi(this.client);
  final HrApiClient client;

  /// POST /api/app/attendance/gps-punch  (Record a GPS attendance punch)
  Future<AttendanceGpsPunchResultDto> Attendance_CreateGpsPunch({
    String? idempotencyKey,
    required AttendanceGpsPunchCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Attendance_CreateGpsPunch',
      method: 'POST',
      path: '/api/app/attendance/gps-punch',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AttendanceGpsPunchResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance/gps-punch/flagged  (List suspicious GPS punches)
  Future<AttendanceFlaggedGpsPunchPagedResultDto> Attendance_ListFlaggedGpsPunches({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Attendance_ListFlaggedGpsPunches',
      method: 'GET',
      path: '/api/app/attendance/gps-punch/flagged',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
      },
      cancelToken: cancelToken,
    );
    return AttendanceFlaggedGpsPunchPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/geo-fence  (List geofences)
  Future<GeoFencePagedResultDto> GeoFence_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'GeoFence_List',
      method: 'GET',
      path: '/api/app/geo-fence',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return GeoFencePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/geo-fence  (Create a geofence)
  Future<GeoFenceDto> GeoFence_Create({
    String? idempotencyKey,
    required GeoFenceCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'GeoFence_Create',
      method: 'POST',
      path: '/api/app/geo-fence',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return GeoFenceDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/geo-fence/{id}  (Update a geofence)
  Future<GeoFenceDto> GeoFence_Update({
    required String id,
    required GeoFenceUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'GeoFence_Update',
      method: 'PUT',
      path: '/api/app/geo-fence/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return GeoFenceDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/geo-fence/{id}  (Delete a geofence)
  Future<void> GeoFence_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'GeoFence_Delete',
      method: 'DELETE',
      path: '/api/app/geo-fence/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

}
