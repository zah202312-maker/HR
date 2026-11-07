// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged RolesPermissions in the contract.
class RolesPermissionsApi {
  RolesPermissionsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/user-account/{id}/permission  (Get effective ordinary permissions of a user)
  Future<PermissionListResultDto> UserAccount_GetPermissions({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_GetPermissions',
      method: 'GET',
      path: '/api/app/user-account/${Uri.encodeComponent(id)}/permission',
      cancelToken: cancelToken,
    );
    return PermissionListResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/user-account/{id}/permission  (Update direct ordinary permissions of a user)
  Future<PermissionListResultDto> UserAccount_UpdatePermissions({
    required String id,
    required PermissionGrantUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_UpdatePermissions',
      method: 'PUT',
      path: '/api/app/user-account/${Uri.encodeComponent(id)}/permission',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PermissionListResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/role  (List roles)
  Future<RolePagedResultDto> Role_List({
    String? search,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Role_List',
      method: 'GET',
      path: '/api/app/role',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return RolePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/role  (Create a custom role)
  Future<RoleDto> Role_Create({
    required RoleCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Role_Create',
      method: 'POST',
      path: '/api/app/role',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RoleDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/role/{id}  (Get a role)
  Future<RoleDto> Role_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Role_Get',
      method: 'GET',
      path: '/api/app/role/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return RoleDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/role/{id}  (Rename a role)
  Future<RoleDto> Role_Update({
    required String id,
    required RoleUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Role_Update',
      method: 'PUT',
      path: '/api/app/role/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return RoleDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/role/{id}  (Delete a custom role)
  Future<void> Role_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'Role_Delete',
      method: 'DELETE',
      path: '/api/app/role/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

  /// GET /api/app/role/{id}/permission  (Get ordinary permissions of a role)
  Future<PermissionListResultDto> Role_GetPermissions({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Role_GetPermissions',
      method: 'GET',
      path: '/api/app/role/${Uri.encodeComponent(id)}/permission',
      cancelToken: cancelToken,
    );
    return PermissionListResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/role/{id}/permission  (Update ordinary permissions of a role)
  Future<PermissionListResultDto> Role_UpdatePermissions({
    required String id,
    required PermissionGrantUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Role_UpdatePermissions',
      method: 'PUT',
      path: '/api/app/role/${Uri.encodeComponent(id)}/permission',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PermissionListResultDto.fromJson(data as Map<String, dynamic>);
  }

}
