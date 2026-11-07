// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged LeaveRequest in the contract.
class LeaveRequestApi {
  LeaveRequestApi(this.client);
  final HrApiClient client;

  /// POST /api/app/leave-request/preview  (Preview a leave request before submit)
  Future<LeaveRequestPreviewDto> LeaveRequest_Preview({
    required LeaveRequestPreviewInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_Preview',
      method: 'POST',
      path: '/api/app/leave-request/preview',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return LeaveRequestPreviewDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/leave-request  (Create a leave request)
  Future<LeaveRequestDto> LeaveRequest_Create({
    String? idempotencyKey,
    required LeaveRequestCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_Create',
      method: 'POST',
      path: '/api/app/leave-request',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return LeaveRequestDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-request/my  (List my leave requests)
  Future<LeaveRequestPagedResultDto> LeaveRequest_ListMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    RequestStatus? status,
    LeaveKind? kind,
    DateTime? fromDate,
    DateTime? toDate,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_ListMy',
      method: 'GET',
      path: '/api/app/leave-request/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (kind != null) 'kind': kind.toJson(),
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
      },
      cancelToken: cancelToken,
    );
    return LeaveRequestPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-request/employee/{id}  (List an employee leave requests)
  Future<LeaveRequestPagedResultDto> LeaveRequest_ListByEmployee({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    RequestStatus? status,
    LeaveKind? kind,
    DateTime? fromDate,
    DateTime? toDate,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_ListByEmployee',
      method: 'GET',
      path: '/api/app/leave-request/employee/${Uri.encodeComponent(id)}',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (kind != null) 'kind': kind.toJson(),
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
      },
      cancelToken: cancelToken,
    );
    return LeaveRequestPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/leave-request/{id}/reverse  (Reverse an approved leave)
  Future<LeaveRequestDto> LeaveRequest_Reverse({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_Reverse',
      method: 'POST',
      path: '/api/app/leave-request/${Uri.encodeComponent(id)}/reverse',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return LeaveRequestDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-request/{id}  (Get a leave request)
  Future<LeaveRequestDto> LeaveRequest_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_Get',
      method: 'GET',
      path: '/api/app/leave-request/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return LeaveRequestDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/leave-request/attachment  (Upload a leave attachment)
  Future<AttachmentDto> LeaveRequest_UploadAttachment({
    required LeaveAttachmentUploadDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_UploadAttachment',
      method: 'POST',
      path: '/api/app/leave-request/attachment',
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AttachmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-request/{id}/attachment/{attachmentId}/download-url  (Get a temporary download URL for a leave attachment)
  Future<FileDownloadUrlDto> LeaveRequest_GetAttachmentDownloadUrl({
    required String id,
    required String attachmentId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveRequest_GetAttachmentDownloadUrl',
      method: 'GET',
      path: '/api/app/leave-request/${Uri.encodeComponent(id)}/attachment/${Uri.encodeComponent(attachmentId)}/download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

}
