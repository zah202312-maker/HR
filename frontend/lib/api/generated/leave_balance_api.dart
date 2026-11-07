// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged LeaveBalance in the contract.
class LeaveBalanceApi {
  LeaveBalanceApi(this.client);
  final HrApiClient client;

  /// GET /api/app/leave-balance/my  (Get my leave balance)
  Future<LeaveBalanceDto> LeaveBalance_GetMy({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveBalance_GetMy',
      method: 'GET',
      path: '/api/app/leave-balance/my',
      cancelToken: cancelToken,
    );
    return LeaveBalanceDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-balance/employee/{id}  (Get an employee leave balance)
  Future<LeaveBalanceDto> LeaveBalance_GetByEmployee({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveBalance_GetByEmployee',
      method: 'GET',
      path: '/api/app/leave-balance/employee/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return LeaveBalanceDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/leave-balance/employee/{id}/entitlement  (Set an employee annual entitlement)
  Future<LeaveBalanceDto> LeaveBalance_SetEntitlement({
    required String id,
    required LeaveEntitlementUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveBalance_SetEntitlement',
      method: 'PUT',
      path: '/api/app/leave-balance/employee/${Uri.encodeComponent(id)}/entitlement',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return LeaveBalanceDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/leave-balance/employee/{id}/adjust  (Adjust an employee balance with a reason)
  Future<LeaveBalanceDto> LeaveBalance_Adjust({
    required String id,
    String? idempotencyKey,
    required LeaveBalanceAdjustDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveBalance_Adjust',
      method: 'POST',
      path: '/api/app/leave-balance/employee/${Uri.encodeComponent(id)}/adjust',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return LeaveBalanceDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-ledger/employee/{id}  (Read an employee leave ledger)
  Future<LeaveLedgerEntryPagedResultDto> LeaveLedger_GetByEmployee({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    String? leaveYearId,
    LeaveLedgerType? type,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveLedger_GetByEmployee',
      method: 'GET',
      path: '/api/app/leave-ledger/employee/${Uri.encodeComponent(id)}',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (leaveYearId != null) 'leaveYearId': leaveYearId,
        if (type != null) 'type': type.toJson(),
      },
      cancelToken: cancelToken,
    );
    return LeaveLedgerEntryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/leave-ledger/my  (Read my leave ledger)
  Future<LeaveLedgerEntryPagedResultDto> LeaveLedger_GetMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    String? leaveYearId,
    LeaveLedgerType? type,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveLedger_GetMy',
      method: 'GET',
      path: '/api/app/leave-ledger/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (leaveYearId != null) 'leaveYearId': leaveYearId,
        if (type != null) 'type': type.toJson(),
      },
      cancelToken: cancelToken,
    );
    return LeaveLedgerEntryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
