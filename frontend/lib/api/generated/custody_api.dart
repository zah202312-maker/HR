// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Custody in the contract.
class CustodyApi {
  CustodyApi(this.client);
  final HrApiClient client;

  /// POST /api/app/custody  (Add custody directly)
  Future<CustodyDto> Custody_Create({
    String? idempotencyKey,
    required CustodyCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_Create',
      method: 'POST',
      path: '/api/app/custody',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return CustodyDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/custody  (List all custody records)
  Future<CustodyPagedResultDto> Custody_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CustodyKind? kind,
    CustodyStatus? status,
    String? employeeId,
    String? departmentId,
    String? itemId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_GetList',
      method: 'GET',
      path: '/api/app/custody',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (kind != null) 'kind': kind.toJson(),
        if (status != null) 'status': status.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (itemId != null) 'itemId': itemId,
      },
      cancelToken: cancelToken,
    );
    return CustodyPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/custody/my  (List my custody)
  Future<CustodyPagedResultDto> Custody_GetMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CustodyStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_GetMy',
      method: 'GET',
      path: '/api/app/custody/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return CustodyPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/custody/team  (List team custody)
  Future<CustodyPagedResultDto> Custody_GetTeam({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CustodyKind? kind,
    CustodyStatus? status,
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_GetTeam',
      method: 'GET',
      path: '/api/app/custody/team',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (kind != null) 'kind': kind.toJson(),
        if (status != null) 'status': status.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return CustodyPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/custody/{id}  (Get a custody record)
  Future<CustodyDto> Custody_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_Get',
      method: 'GET',
      path: '/api/app/custody/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return CustodyDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/custody/{id}/acknowledge  (Acknowledge custody receipt)
  Future<CustodyDto> Custody_Acknowledge({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_Acknowledge',
      method: 'POST',
      path: '/api/app/custody/${Uri.encodeComponent(id)}/acknowledge',
      cancelToken: cancelToken,
    );
    return CustodyDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/custody/{id}/return  (Record custody return)
  Future<CustodyDto> Custody_Return({
    required String id,
    required CustodyReturnInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Custody_Return',
      method: 'POST',
      path: '/api/app/custody/${Uri.encodeComponent(id)}/return',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return CustodyDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/custody-count  (Record a department custody count)
  Future<CustodyCountDto> CustodyCount_Create({
    String? idempotencyKey,
    required CustodyCountCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'CustodyCount_Create',
      method: 'POST',
      path: '/api/app/custody-count',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return CustodyCountDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/custody-count  (List department custody counts)
  Future<CustodyCountPagedResultDto> CustodyCount_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? departmentId,
    CustodyCountResult? result,
    DateTime? fromDate,
    DateTime? toDate,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'CustodyCount_GetList',
      method: 'GET',
      path: '/api/app/custody-count',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (departmentId != null) 'departmentId': departmentId,
        if (result != null) 'result': result.toJson(),
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
      },
      cancelToken: cancelToken,
    );
    return CustodyCountPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
