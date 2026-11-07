// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged DepartmentReports in the contract.
class DepartmentReportsApi {
  DepartmentReportsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/department-report  (List department reports)
  Future<DepartmentReportSummaryPagedResultDto> DepartmentReport_GetList({
    String? cycleId,
    DepartmentReportStatus? status,
    String? departmentId,
    bool? isLate,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DepartmentReport_GetList',
      method: 'GET',
      path: '/api/app/department-report',
      query: <String, dynamic>{
        if (cycleId != null) 'cycleId': cycleId,
        if (status != null) 'status': status.toJson(),
        if (departmentId != null) 'departmentId': departmentId,
        if (isLate != null) 'isLate': isLate,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return DepartmentReportSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/department-report/{id}  (Get a department monthly report)
  Future<DepartmentReportDto> DepartmentReport_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DepartmentReport_Get',
      method: 'GET',
      path: '/api/app/department-report/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return DepartmentReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// PATCH /api/app/department-report/{id}  (Add manager notes to the report)
  Future<DepartmentReportDto> DepartmentReport_UpdateNotes({
    required String id,
    required DepartmentReportNotesPatchDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DepartmentReport_UpdateNotes',
      method: 'PATCH',
      path: '/api/app/department-report/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return DepartmentReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/department-report/{id}/submit  (Submit the department report)
  Future<DepartmentReportDto> DepartmentReport_Submit({
    required String id,
    required DepartmentReportSubmitDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'DepartmentReport_Submit',
      method: 'POST',
      path: '/api/app/department-report/${Uri.encodeComponent(id)}/submit',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return DepartmentReportDto.fromJson(data as Map<String, dynamic>);
  }

}
