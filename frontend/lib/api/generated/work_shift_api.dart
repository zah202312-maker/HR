// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged WorkShift in the contract.
class WorkShiftApi {
  WorkShiftApi(this.client);
  final HrApiClient client;

  /// GET /api/app/work-shift  (List work shift templates)
  Future<WorkShiftPagedResultDto> WorkShift_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    WorkShiftType? type,
    bool? isActive,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'WorkShift_List',
      method: 'GET',
      path: '/api/app/work-shift',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (type != null) 'type': type.toJson(),
        if (isActive != null) 'isActive': isActive,
      },
      cancelToken: cancelToken,
    );
    return WorkShiftPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/work-shift  (Create a work shift template)
  Future<WorkShiftDto> WorkShift_Create({
    String? idempotencyKey,
    required WorkShiftCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'WorkShift_Create',
      method: 'POST',
      path: '/api/app/work-shift',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WorkShiftDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/work-shift/{id}  (Get a work shift template)
  Future<WorkShiftDto> WorkShift_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'WorkShift_Get',
      method: 'GET',
      path: '/api/app/work-shift/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return WorkShiftDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/work-shift/{id}  (Update a work shift template)
  Future<WorkShiftDto> WorkShift_Update({
    required String id,
    required WorkShiftUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'WorkShift_Update',
      method: 'PUT',
      path: '/api/app/work-shift/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WorkShiftDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/employee/{id}/shift  (Assign a shift to an employee from a start date)
  Future<EmployeeShiftDto> EmployeeShift_Assign({
    required String id,
    required EmployeeShiftAssignDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EmployeeShift_Assign',
      method: 'PUT',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/shift',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EmployeeShiftDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/employee/{id}/shift  (Get an employee shift history)
  Future<EmployeeShiftPagedResultDto> EmployeeShift_GetHistory({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EmployeeShift_GetHistory',
      method: 'GET',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/shift',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EmployeeShiftPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
