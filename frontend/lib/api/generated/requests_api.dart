// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Requests in the contract.
class RequestsApi {
  RequestsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/request-type  (List active request types)
  Future<RequestTypePagedResultDto> RequestType_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'RequestType_GetList',
      method: 'GET',
      path: '/api/app/request-type',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return RequestTypePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/request-type  (Create a request type)
  Future<RequestTypeAdminDto> RequestType_Create({
    required RequestTypeCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'RequestType_Create',
      method: 'POST',
      path: '/api/app/request-type',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RequestTypeAdminDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/request-type/admin  (List all request types for admin)
  Future<RequestTypeAdminPagedResultDto> RequestType_GetAdminList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'RequestType_GetAdminList',
      method: 'GET',
      path: '/api/app/request-type/admin',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
      },
      cancelToken: cancelToken,
    );
    return RequestTypeAdminPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/request-type/{id}  (Get a request type for admin)
  Future<RequestTypeAdminDto> RequestType_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'RequestType_Get',
      method: 'GET',
      path: '/api/app/request-type/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return RequestTypeAdminDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/request-type/{id}  (Update a request type)
  Future<RequestTypeAdminDto> RequestType_Update({
    required String id,
    required RequestTypeUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'RequestType_Update',
      method: 'PUT',
      path: '/api/app/request-type/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RequestTypeAdminDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/request  (Submit a request, personally or on behalf of an employee)
  Future<RequestDetailsDto> Request_Submit({
    String? idempotencyKey,
    required RequestSubmitInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_Submit',
      method: 'POST',
      path: '/api/app/request',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RequestDetailsDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/request  (List requests for HR or a manager team)
  Future<RequestSummaryPagedResultDto> Request_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    RequestStatus? status,
    String? requestTypeId,
    String? employeeId,
    DateTime? fromDate,
    DateTime? toDate,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_GetList',
      method: 'GET',
      path: '/api/app/request',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (requestTypeId != null) 'requestTypeId': requestTypeId,
        if (employeeId != null) 'employeeId': employeeId,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
      },
      cancelToken: cancelToken,
    );
    return RequestSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/request/my  (List my requests)
  Future<MyRequestPagedResultDto> Request_GetMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    RequestEmployeeStatus? employeeStatus,
    String? requestTypeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_GetMy',
      method: 'GET',
      path: '/api/app/request/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeStatus != null) 'employeeStatus': employeeStatus.toJson(),
        if (requestTypeId != null) 'requestTypeId': requestTypeId,
      },
      cancelToken: cancelToken,
    );
    return MyRequestPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/request/inbox  (List requests waiting for my action)
  Future<RequestInboxItemPagedResultDto> Request_GetInbox({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? requestTypeId,
    RequestStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_GetInbox',
      method: 'GET',
      path: '/api/app/request/inbox',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (requestTypeId != null) 'requestTypeId': requestTypeId,
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return RequestInboxItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/request/{id}  (Get request details with timeline)
  Future<RequestDetailsDto> Request_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_Get',
      method: 'GET',
      path: '/api/app/request/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return RequestDetailsDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/request/{id}/approve  (Approve the current step)
  Future<RequestDetailsDto> Request_Approve({
    required String id,
    NoteInputDto? body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_Approve',
      method: 'POST',
      path: '/api/app/request/${Uri.encodeComponent(id)}/approve',
      body: body?.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RequestDetailsDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/request/{id}/reject  (Reject the request at the current step)
  Future<RequestDetailsDto> Request_Reject({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_Reject',
      method: 'POST',
      path: '/api/app/request/${Uri.encodeComponent(id)}/reject',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RequestDetailsDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/request/{id}/cancel  (Cancel my request)
  Future<RequestDetailsDto> Request_Cancel({
    required String id,
    NoteInputDto? body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Request_Cancel',
      method: 'POST',
      path: '/api/app/request/${Uri.encodeComponent(id)}/cancel',
      body: body?.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RequestDetailsDto.fromJson(data as Map<String, dynamic>);
  }

}
