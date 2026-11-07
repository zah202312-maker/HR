// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged UserAccounts in the contract.
class UserAccountsApi {
  UserAccountsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/user-account  (List user accounts)
  Future<UserAccountPagedResultDto> UserAccount_List({
    String? search,
    String? roleName,
    bool? isActive,
    bool? mustChangePassword,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_List',
      method: 'GET',
      path: '/api/app/user-account',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (roleName != null) 'roleName': roleName,
        if (isActive != null) 'isActive': isActive,
        if (mustChangePassword != null) 'mustChangePassword': mustChangePassword,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return UserAccountPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/user-account  (Create an account for an employee without one)
  Future<UserAccountCreatedDto> UserAccount_Create({
    required UserAccountCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_Create',
      method: 'POST',
      path: '/api/app/user-account',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return UserAccountCreatedDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/user-account/{id}  (Get a user account)
  Future<UserAccountDto> UserAccount_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_Get',
      method: 'GET',
      path: '/api/app/user-account/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return UserAccountDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/user-account/{id}/reset-password  (Reset a password and notify the user)
  Future<PasswordResetResultDto> UserAccount_ResetPassword({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_ResetPassword',
      method: 'POST',
      path: '/api/app/user-account/${Uri.encodeComponent(id)}/reset-password',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PasswordResetResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/user-account/{id}/roles  (Replace the roles of a user)
  Future<UserAccountDto> UserAccount_SetRoles({
    required String id,
    required UserRolesUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_SetRoles',
      method: 'PUT',
      path: '/api/app/user-account/${Uri.encodeComponent(id)}/roles',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return UserAccountDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/user-account/lookup  (Look up user accounts by name)
  Future<UserRefPagedResultDto> UserAccount_Lookup({
    String? search,
    int? skipCount,
    int? maxResultCount,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'UserAccount_Lookup',
      method: 'GET',
      path: '/api/app/user-account/lookup',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
      },
      cancelToken: cancelToken,
    );
    return UserRefPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
