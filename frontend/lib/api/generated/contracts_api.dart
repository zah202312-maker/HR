// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Contracts in the contract.
class ContractsApi {
  ContractsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/contract  (List contracts)
  Future<ContractPagedResultDto> Contract_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? employeeId,
    String? departmentId,
    ContractStatus? status,
    DateTime? endDateFrom,
    DateTime? endDateTo,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_List',
      method: 'GET',
      path: '/api/app/contract',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (status != null) 'status': status.toJson(),
        if (endDateFrom != null) 'endDateFrom': hrDateToJson(endDateFrom),
        if (endDateTo != null) 'endDateTo': hrDateToJson(endDateTo),
      },
      cancelToken: cancelToken,
    );
    return ContractPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/contract  (Create a contract)
  Future<ContractDto> Contract_Create({
    String? idempotencyKey,
    required ContractCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_Create',
      method: 'POST',
      path: '/api/app/contract',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ContractDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/contract/expiring  (List contracts nearing expiry)
  Future<ContractExpiringPagedResultDto> Contract_GetExpiring({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    int? withinDays,
    String? departmentId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_GetExpiring',
      method: 'GET',
      path: '/api/app/contract/expiring',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (withinDays != null) 'withinDays': withinDays,
        if (departmentId != null) 'departmentId': departmentId,
      },
      cancelToken: cancelToken,
    );
    return ContractExpiringPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/contract/{id}  (Get a contract)
  Future<ContractDto> Contract_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_Get',
      method: 'GET',
      path: '/api/app/contract/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ContractDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/contract/{id}/recommend  (Submit contract position and recommendation)
  Future<ContractDto> Contract_Recommend({
    required String id,
    required ContractRecommendInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_Recommend',
      method: 'POST',
      path: '/api/app/contract/${Uri.encodeComponent(id)}/recommend',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ContractDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/contract/{id}/decide  (Decide contract renewal)
  Future<ContractDto> Contract_Decide({
    required String id,
    required ContractDecideInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_Decide',
      method: 'POST',
      path: '/api/app/contract/${Uri.encodeComponent(id)}/decide',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ContractDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/contract/my  (List my contracts)
  Future<ContractEmployeePagedResultDto> Contract_GetMyList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_GetMyList',
      method: 'GET',
      path: '/api/app/contract/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ContractEmployeePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/contract/my/{id}  (Get my contract)
  Future<ContractEmployeeDto> Contract_GetMy({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Contract_GetMy',
      method: 'GET',
      path: '/api/app/contract/my/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ContractEmployeeDto.fromJson(data as Map<String, dynamic>);
  }

}
