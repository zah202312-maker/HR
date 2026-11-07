// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Attendance in the contract.
class AttendanceApi {
  AttendanceApi(this.client);
  final HrApiClient client;

  /// GET /api/app/attendance/my  (Get my attendance)
  Future<AttendanceDayPagedResultDto> Attendance_GetMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    AttendanceDayStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Attendance_GetMy',
      method: 'GET',
      path: '/api/app/attendance/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return AttendanceDayPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance/team  (Get team attendance)
  Future<AttendanceDayPagedResultDto> Attendance_GetTeam({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    String? employeeId,
    AttendanceDayStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Attendance_GetTeam',
      method: 'GET',
      path: '/api/app/attendance/team',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (employeeId != null) 'employeeId': employeeId,
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return AttendanceDayPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance/employee/{id}  (Get an employee attendance)
  Future<AttendanceDayPagedResultDto> Attendance_GetByEmployee({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    AttendanceDayStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Attendance_GetByEmployee',
      method: 'GET',
      path: '/api/app/attendance/employee/${Uri.encodeComponent(id)}',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return AttendanceDayPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/attendance/day/{id}  (Get an attendance day with raw punches and edit history)
  Future<AttendanceDayDetailDto> Attendance_GetDayDetail({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Attendance_GetDayDetail',
      method: 'GET',
      path: '/api/app/attendance/day/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return AttendanceDayDetailDto.fromJson(data as Map<String, dynamic>);
  }

}
