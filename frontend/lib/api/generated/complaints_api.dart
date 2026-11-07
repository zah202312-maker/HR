// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Complaints in the contract.
class ComplaintsApi {
  ComplaintsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/complaint/respondents  (List selectable complaint respondents)
  Future<ComplaintRespondentPagedResultDto> Complaint_GetRespondents({
    String? filter,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_GetRespondents',
      method: 'GET',
      path: '/api/app/complaint/respondents',
      query: <String, dynamic>{
        if (filter != null) 'filter': filter,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ComplaintRespondentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint/preview-routing  (Preview minimum level and route)
  Future<ComplaintRoutingPreviewDto> Complaint_PreviewRouting({
    required ComplaintRoutingPreviewInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_PreviewRouting',
      method: 'POST',
      path: '/api/app/complaint/preview-routing',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintRoutingPreviewDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint  (Submit a complaint)
  Future<ComplaintSubmitResultDto> Complaint_Submit({
    String? idempotencyKey,
    required ComplaintCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_Submit',
      method: 'POST',
      path: '/api/app/complaint',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintSubmitResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint/my  (List my complaints)
  Future<ComplaintComplainantSummaryPagedResultDto> Complaint_GetMyList({
    ComplaintStatus? status,
    ComplaintLevel? level,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_GetMyList',
      method: 'GET',
      path: '/api/app/complaint/my',
      query: <String, dynamic>{
        if (status != null) 'status': status.toJson(),
        if (level != null) 'level': level.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ComplaintComplainantSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint/my/{id}  (Get my complaint as complainant)
  Future<ComplaintComplainantDetailDto> Complaint_GetMyDetail({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_GetMyDetail',
      method: 'GET',
      path: '/api/app/complaint/my/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ComplaintComplainantDetailDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint/assigned  (List complaints assigned to me)
  Future<ComplaintHandlerSummaryPagedResultDto> Complaint_GetAssignedList({
    ComplaintStatus? status,
    ComplaintLevel? level,
    bool? isLate,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_GetAssignedList',
      method: 'GET',
      path: '/api/app/complaint/assigned',
      query: <String, dynamic>{
        if (status != null) 'status': status.toJson(),
        if (level != null) 'level': level.toJson(),
        if (isLate != null) 'isLate': isLate,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ComplaintHandlerSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint/{id}  (Get complaint details as handler)
  Future<ComplaintHandlerDetailDto> Complaint_GetDetail({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_GetDetail',
      method: 'GET',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ComplaintHandlerDetailDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint/{id}/raise-level  (Raise complaint level)
  Future<ComplaintTransitionResultDto> Complaint_RaiseLevel({
    required String id,
    required ComplaintRaiseLevelInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_RaiseLevel',
      method: 'POST',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/raise-level',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintTransitionResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint/{id}/action  (Add a handler action)
  Future<ComplaintActionDto> Complaint_AddAction({
    required String id,
    String? idempotencyKey,
    required ComplaintActionCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_AddAction',
      method: 'POST',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/action',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintActionDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint/{id}/forward  (Forward a normal complaint to HR)
  Future<ComplaintTransitionResultDto> Complaint_Forward({
    required String id,
    required ComplaintForwardInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_Forward',
      method: 'POST',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/forward',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintTransitionResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint/{id}/close  (Close a complaint)
  Future<ComplaintTransitionResultDto> Complaint_Close({
    required String id,
    required ComplaintCloseInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_Close',
      method: 'POST',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/close',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintTransitionResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint/{id}/access-log  (Get complaint access log)
  Future<ComplaintAccessLogEntryPagedResultDto> Complaint_GetAccessLog({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_GetAccessLog',
      method: 'GET',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/access-log',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ComplaintAccessLogEntryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint/{id}/attachment  (Upload a complaint attachment)
  Future<AttachmentDto> Complaint_UploadAttachment({
    required String id,
    String? idempotencyKey,
    required ComplaintAttachmentUploadDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Complaint_UploadAttachment',
      method: 'POST',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/attachment',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AttachmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint/{id}/attachment/{attachmentId}  (Download a complaint attachment)
  Future<HrBinaryResponse> Complaint_DownloadAttachment({
    required String id,
    required String attachmentId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'Complaint_DownloadAttachment',
      method: 'GET',
      path: '/api/app/complaint/${Uri.encodeComponent(id)}/attachment/${Uri.encodeComponent(attachmentId)}',
      cancelToken: cancelToken,
    );
    return data;
  }

}
