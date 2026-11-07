// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged CurrentUser in the contract.
class CurrentUserApi {
  CurrentUserApi(this.client);
  final HrApiClient client;

  /// GET /api/app/me  (Get my account and access gate)
  Future<MyAccountDto> Me_Get({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Me_Get',
      method: 'GET',
      path: '/api/app/me',
      cancelToken: cancelToken,
    );
    return MyAccountDto.fromJson(data as Map<String, dynamic>);
  }

  /// PATCH /api/app/me  (Update my preferred language)
  Future<MyAccountDto> Me_Update({
    required MyAccountUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Me_Update',
      method: 'PATCH',
      path: '/api/app/me',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return MyAccountDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/me/employee-profile  (Get my own employee file)
  Future<EmployeeSelfDto> Me_GetEmployeeProfile({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Me_GetEmployeeProfile',
      method: 'GET',
      path: '/api/app/me/employee-profile',
      cancelToken: cancelToken,
    );
    return EmployeeSelfDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/me/inbox-summary  (Get counts for my pending actions)
  Future<InboxSummaryDto> Me_GetInboxSummary({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Me_GetInboxSummary',
      method: 'GET',
      path: '/api/app/me/inbox-summary',
      cancelToken: cancelToken,
    );
    return InboxSummaryDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/me/administrative-task-log  (Get my administrative task log as a manager)
  Future<AdministrativeTaskLogPagedResultDto> Me_GetAdministrativeTaskLog({
    DateTime? fromDate,
    DateTime? toDate,
    AdministrativeTaskType? taskType,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Me_GetAdministrativeTaskLog',
      method: 'GET',
      path: '/api/app/me/administrative-task-log',
      query: <String, dynamic>{
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (taskType != null) 'taskType': taskType.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return AdministrativeTaskLogPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/account/change-password  (Change my password)
  Future<ChangePasswordResultDto> Account_ChangePassword({
    required ChangePasswordInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Account_ChangePassword',
      method: 'POST',
      path: '/api/app/account/change-password',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ChangePasswordResultDto.fromJson(data as Map<String, dynamic>);
  }

}
