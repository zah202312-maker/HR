// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged OtherLeaveType in the contract.
class OtherLeaveTypeApi {
  OtherLeaveTypeApi(this.client);
  final HrApiClient client;

  /// GET /api/app/other-leave-type  (List other leave types for administration)
  Future<OtherLeaveTypePagedResultDto> OtherLeaveType_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'OtherLeaveType_List',
      method: 'GET',
      path: '/api/app/other-leave-type',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
      },
      cancelToken: cancelToken,
    );
    return OtherLeaveTypePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/other-leave-type  (Create an other leave type)
  Future<OtherLeaveTypeDto> OtherLeaveType_Create({
    String? idempotencyKey,
    required OtherLeaveTypeCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'OtherLeaveType_Create',
      method: 'POST',
      path: '/api/app/other-leave-type',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return OtherLeaveTypeDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/other-leave-type/{id}  (Update an other leave type)
  Future<OtherLeaveTypeDto> OtherLeaveType_Update({
    required String id,
    required OtherLeaveTypeUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'OtherLeaveType_Update',
      method: 'PUT',
      path: '/api/app/other-leave-type/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return OtherLeaveTypeDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/other-leave-type/available  (List other leave types available to the current user)
  Future<OtherLeaveTypeOptionListDto> OtherLeaveType_ListAvailable({
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'OtherLeaveType_ListAvailable',
      method: 'GET',
      path: '/api/app/other-leave-type/available',
      query: <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return OtherLeaveTypeOptionListDto.fromJson(data as Map<String, dynamic>);
  }

}
