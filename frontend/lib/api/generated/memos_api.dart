// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Memos in the contract.
class MemosApi {
  MemosApi(this.client);
  final HrApiClient client;

  /// POST /api/app/memo  (Submit a memo)
  Future<MemoDto> Memo_Create({
    String? idempotencyKey,
    required MemoCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_Create',
      method: 'POST',
      path: '/api/app/memo',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MemoDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/memo/my  (List my memos)
  Future<MemoListItemPagedResultDto> Memo_GetMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    MemoStatus? status,
    MemoKind? kind,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_GetMy',
      method: 'GET',
      path: '/api/app/memo/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (kind != null) 'kind': kind.toJson(),
      },
      cancelToken: cancelToken,
    );
    return MemoListItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/memo/inbox  (List memos assigned to me)
  Future<MemoListItemPagedResultDto> Memo_GetInbox({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    MemoStatus? status,
    MemoKind? kind,
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_GetInbox',
      method: 'GET',
      path: '/api/app/memo/inbox',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (kind != null) 'kind': kind.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return MemoListItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/memo/{id}  (Get memo details)
  Future<MemoDto> Memo_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_Get',
      method: 'GET',
      path: '/api/app/memo/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return MemoDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/memo/{id}/forward  (Forward a memo to HR)
  Future<MemoDto> Memo_Forward({
    required String id,
    NoteInputDto? body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_Forward',
      method: 'POST',
      path: '/api/app/memo/${Uri.encodeComponent(id)}/forward',
      body: body?.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MemoDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/memo/{id}/convert  (Convert a memo to a leave request or an incident)
  Future<MemoDto> Memo_Convert({
    required String id,
    required MemoConvertInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_Convert',
      method: 'POST',
      path: '/api/app/memo/${Uri.encodeComponent(id)}/convert',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MemoDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/memo/{id}/close  (Close a memo without conversion)
  Future<MemoDto> Memo_Close({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Memo_Close',
      method: 'POST',
      path: '/api/app/memo/${Uri.encodeComponent(id)}/close',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MemoDto.fromJson(data as Map<String, dynamic>);
  }

}
