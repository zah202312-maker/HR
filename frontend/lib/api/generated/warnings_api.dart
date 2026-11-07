// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Warnings in the contract.
class WarningsApi {
  WarningsApi(this.client);
  final HrApiClient client;

  /// POST /api/app/warning  (Submit a warning recommendation)
  Future<WarningRecommendationDto> Warning_Create({
    String? idempotencyKey,
    required WarningCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_Create',
      method: 'POST',
      path: '/api/app/warning',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return WarningRecommendationDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning  (List warnings)
  Future<WarningPagedResultDto> Warning_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? employeeId,
    String? departmentId,
    WarningStatus? status,
    WarningLevel? level,
    DateTime? incidentDateFrom,
    DateTime? incidentDateTo,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_List',
      method: 'GET',
      path: '/api/app/warning',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (status != null) 'status': status.toJson(),
        if (level != null) 'level': level.toJson(),
        if (incidentDateFrom != null) 'incidentDateFrom': hrDateToJson(incidentDateFrom),
        if (incidentDateTo != null) 'incidentDateTo': hrDateToJson(incidentDateTo),
      },
      cancelToken: cancelToken,
    );
    return WarningPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/objection  (List warnings with objections)
  Future<WarningPagedResultDto> Warning_GetObjections({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isPending,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_GetObjections',
      method: 'GET',
      path: '/api/app/warning/objection',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isPending != null) 'isPending': isPending,
      },
      cancelToken: cancelToken,
    );
    return WarningPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/recommended-by-me  (List my warning recommendations)
  Future<WarningRecommendationPagedResultDto> Warning_GetRecommendedByMe({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    WarningRecommendationStage? stage,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_GetRecommendedByMe',
      method: 'GET',
      path: '/api/app/warning/recommended-by-me',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (stage != null) 'stage': stage.toJson(),
      },
      cancelToken: cancelToken,
    );
    return WarningRecommendationPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/my  (List my warnings)
  Future<WarningEmployeePagedResultDto> Warning_GetMyList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_GetMyList',
      method: 'GET',
      path: '/api/app/warning/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return WarningEmployeePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/my/{id}  (Get my warning)
  Future<WarningEmployeeDto> Warning_GetMy({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_GetMy',
      method: 'GET',
      path: '/api/app/warning/my/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return WarningEmployeeDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/employee/{employeeId}  (List warnings in an employee file)
  Future<WarningFileEntryPagedResultDto> Warning_GetEmployeeFile({
    required String employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_GetEmployeeFile',
      method: 'GET',
      path: '/api/app/warning/employee/${Uri.encodeComponent(employeeId)}',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return WarningFileEntryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/{id}  (Get a warning)
  Future<WarningDto> Warning_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_Get',
      method: 'GET',
      path: '/api/app/warning/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return WarningDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/warning/{id}/attachment/{attachmentId}/download-url  (Get a warning attachment download URL)
  Future<FileDownloadUrlDto> Warning_GetAttachmentDownloadUrl({
    required String id,
    required String attachmentId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_GetAttachmentDownloadUrl',
      method: 'GET',
      path: '/api/app/warning/${Uri.encodeComponent(id)}/attachment/${Uri.encodeComponent(attachmentId)}/download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/warning/{id}/notify  (Record the incident and notify the employee)
  Future<WarningDto> Warning_Notify({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_Notify',
      method: 'POST',
      path: '/api/app/warning/${Uri.encodeComponent(id)}/notify',
      cancelToken: cancelToken,
    );
    return WarningDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/warning/{id}/respond  (Submit the employee reply)
  Future<WarningEmployeeDto> Warning_Respond({
    required String id,
    required WarningRespondInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_Respond',
      method: 'POST',
      path: '/api/app/warning/${Uri.encodeComponent(id)}/respond',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WarningEmployeeDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/warning/{id}/decide  (Record the HR decision)
  Future<WarningDto> Warning_Decide({
    required String id,
    required WarningDecideInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_Decide',
      method: 'POST',
      path: '/api/app/warning/${Uri.encodeComponent(id)}/decide',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WarningDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/warning/{id}/object  (Object to the warning decision)
  Future<WarningEmployeeDto> Warning_Object({
    required String id,
    required WarningObjectInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_Object',
      method: 'POST',
      path: '/api/app/warning/${Uri.encodeComponent(id)}/object',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WarningEmployeeDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/warning/{id}/resolve-objection  (Resolve the objection)
  Future<WarningDto> Warning_ResolveObjection({
    required String id,
    required WarningResolveObjectionInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Warning_ResolveObjection',
      method: 'POST',
      path: '/api/app/warning/${Uri.encodeComponent(id)}/resolve-objection',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WarningDto.fromJson(data as Map<String, dynamic>);
  }

}
