// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged ComplaintHandlers in the contract.
class ComplaintHandlersApi {
  ComplaintHandlersApi(this.client);
  final HrApiClient client;

  /// POST /api/app/complaint-handler  (Request a complaint handler assignment)
  Future<ComplaintHandlerAssignmentDto> ComplaintHandler_Request({
    String? idempotencyKey,
    required ComplaintHandlerAssignmentCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ComplaintHandler_Request',
      method: 'POST',
      path: '/api/app/complaint-handler',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintHandlerAssignmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/complaint-handler  (List handler assignments)
  Future<ComplaintHandlerAssignmentPagedResultDto> ComplaintHandler_GetList({
    ComplaintHandlerAssignmentStatus? status,
    ComplaintLevel? level,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ComplaintHandler_GetList',
      method: 'GET',
      path: '/api/app/complaint-handler',
      query: <String, dynamic>{
        if (status != null) 'status': status.toJson(),
        if (level != null) 'level': level.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ComplaintHandlerAssignmentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint-handler/{id}/approve  (Approve a handler assignment)
  Future<ComplaintHandlerAssignmentDto> ComplaintHandler_Approve({
    required String id,
    required ComplaintHandlerAssignmentApproveDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ComplaintHandler_Approve',
      method: 'POST',
      path: '/api/app/complaint-handler/${Uri.encodeComponent(id)}/approve',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintHandlerAssignmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint-handler/{id}/reject  (Reject a handler assignment)
  Future<ComplaintHandlerAssignmentDto> ComplaintHandler_Reject({
    required String id,
    required ComplaintHandlerAssignmentReasonDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ComplaintHandler_Reject',
      method: 'POST',
      path: '/api/app/complaint-handler/${Uri.encodeComponent(id)}/reject',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintHandlerAssignmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/complaint-handler/{id}/revoke  (Revoke a handler assignment)
  Future<ComplaintHandlerAssignmentDto> ComplaintHandler_Revoke({
    required String id,
    required ComplaintHandlerAssignmentReasonDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ComplaintHandler_Revoke',
      method: 'POST',
      path: '/api/app/complaint-handler/${Uri.encodeComponent(id)}/revoke',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ComplaintHandlerAssignmentDto.fromJson(data as Map<String, dynamic>);
  }

}
