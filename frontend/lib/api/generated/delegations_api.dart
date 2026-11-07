// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Delegations in the contract.
class DelegationsApi {
  DelegationsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/delegation  (List all delegations for HR)
  Future<DelegationPagedResultDto> Delegation_List({
    String? fromUserId,
    String? toUserId,
    DelegationStatus? status,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Delegation_List',
      method: 'GET',
      path: '/api/app/delegation',
      query: <String, dynamic>{
        if (fromUserId != null) 'fromUserId': fromUserId,
        if (toUserId != null) 'toUserId': toUserId,
        if (status != null) 'status': status.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return DelegationPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/delegation  (Delegate my approvals to another user)
  Future<DelegationDto> Delegation_Create({
    required DelegationCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Delegation_Create',
      method: 'POST',
      path: '/api/app/delegation',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return DelegationDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/delegation/my  (List delegations I gave or received)
  Future<DelegationPagedResultDto> Delegation_GetMy({
    DelegationDirection? direction,
    DelegationStatus? status,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Delegation_GetMy',
      method: 'GET',
      path: '/api/app/delegation/my',
      query: <String, dynamic>{
        if (direction != null) 'direction': direction.toJson(),
        if (status != null) 'status': status.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return DelegationPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/delegation/candidate  (List users who can receive a delegation)
  Future<DelegationCandidatePagedResultDto> Delegation_GetCandidates({
    String? search,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Delegation_GetCandidates',
      method: 'GET',
      path: '/api/app/delegation/candidate',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return DelegationCandidatePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/delegation/{id}  (Cancel my delegation)
  Future<void> Delegation_Cancel({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'Delegation_Cancel',
      method: 'DELETE',
      path: '/api/app/delegation/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

}
