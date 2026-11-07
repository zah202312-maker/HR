// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Departments in the contract.
class DepartmentsApi {
  DepartmentsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/department  (List departments for administration)
  Future<DepartmentPagedResultDto> Department_List({
    String? search,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Department_List',
      method: 'GET',
      path: '/api/app/department',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return DepartmentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/department  (Create a department)
  Future<DepartmentDto> Department_Create({
    required DepartmentCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Department_Create',
      method: 'POST',
      path: '/api/app/department',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return DepartmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/department/lookup  (Look up departments by localized name)
  Future<DepartmentLookupPagedResultDto> Department_Lookup({
    String? search,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Department_Lookup',
      method: 'GET',
      path: '/api/app/department/lookup',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return DepartmentLookupPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/department/{id}  (Get a department)
  Future<DepartmentDto> Department_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Department_Get',
      method: 'GET',
      path: '/api/app/department/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return DepartmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/department/{id}  (Update a department)
  Future<DepartmentDto> Department_Update({
    required String id,
    required DepartmentUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Department_Update',
      method: 'PUT',
      path: '/api/app/department/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return DepartmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/department/{id}  (Delete an unused department)
  Future<void> Department_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'Department_Delete',
      method: 'DELETE',
      path: '/api/app/department/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

}
