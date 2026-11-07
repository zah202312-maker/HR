// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Employees in the contract.
class EmployeesApi {
  EmployeesApi(this.client);
  final HrApiClient client;

  /// GET /api/app/employee  (List employees for HR)
  Future<EmployeeListItemPagedResultDto> Employee_List({
    String? search,
    String? departmentId,
    String? managerEmployeeId,
    EmployeeStatus? status,
    String? roleName,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_List',
      method: 'GET',
      path: '/api/app/employee',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (departmentId != null) 'departmentId': departmentId,
        if (managerEmployeeId != null) 'managerEmployeeId': managerEmployeeId,
        if (status != null) 'status': status.toJson(),
        if (roleName != null) 'roleName': roleName,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EmployeeListItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/employee  (Create an employee with account, shift, manager and roles)
  Future<EmployeeCreatedDto> Employee_Create({
    required EmployeeCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_Create',
      method: 'POST',
      path: '/api/app/employee',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EmployeeCreatedDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/employee/lookup  (Look up employees by name and department only)
  Future<EmployeeLookupPagedResultDto> Employee_Lookup({
    String? search,
    String? departmentId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_Lookup',
      method: 'GET',
      path: '/api/app/employee/lookup',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (departmentId != null) 'departmentId': departmentId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EmployeeLookupPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/employee/{id}  (Get the full employee file for HR)
  Future<EmployeeHrDto> Employee_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_Get',
      method: 'GET',
      path: '/api/app/employee/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return EmployeeHrDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/employee/{id}  (Update an employee file)
  Future<EmployeeHrDto> Employee_Update({
    required String id,
    required EmployeeUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_Update',
      method: 'PUT',
      path: '/api/app/employee/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EmployeeHrDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/employee/{id}/correct-hire-date  (Correct a wrong hire date)
  Future<EmployeeHrDto> Employee_CorrectHireDate({
    required String id,
    required EmployeeHireDateCorrectionDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_CorrectHireDate',
      method: 'POST',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/correct-hire-date',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EmployeeHrDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/employee/{id}/manager  (Change an employee's manager)
  Future<ManagerChangeResultDto> Employee_ChangeManager({
    required String id,
    required ManagerChangeInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_ChangeManager',
      method: 'PUT',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/manager',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ManagerChangeResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/employee/{id}/manager-history  (Get manager link history)
  Future<EmployeeManagerHistoryPagedResultDto> Employee_GetManagerHistory({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_GetManagerHistory',
      method: 'GET',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/manager-history',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EmployeeManagerHistoryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/employee/{id}/administrative-task-log  (Get a manager's administrative task log for HR)
  Future<AdministrativeTaskLogPagedResultDto> Employee_GetAdministrativeTaskLog({
    required String id,
    DateTime? fromDate,
    DateTime? toDate,
    AdministrativeTaskType? taskType,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Employee_GetAdministrativeTaskLog',
      method: 'GET',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/administrative-task-log',
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

  /// POST /api/app/manager/{id}/assign-employees  (Assign employees to a manager in bulk)
  Future<ManagerAssignEmployeesResultDto> Manager_AssignEmployees({
    required String id,
    required ManagerAssignEmployeesInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Manager_AssignEmployees',
      method: 'POST',
      path: '/api/app/manager/${Uri.encodeComponent(id)}/assign-employees',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ManagerAssignEmployeesResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/manager/my-team  (List my direct reports)
  Future<EmployeeTeamMemberPagedResultDto> Manager_GetMyTeam({
    String? search,
    EmployeeStatus? status,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Manager_GetMyTeam',
      method: 'GET',
      path: '/api/app/manager/my-team',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (status != null) 'status': status.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EmployeeTeamMemberPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/manager/my-team/{id}  (Get a direct report as seen by their manager)
  Future<EmployeeTeamMemberDto> Manager_GetTeamMember({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Manager_GetTeamMember',
      method: 'GET',
      path: '/api/app/manager/my-team/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return EmployeeTeamMemberDto.fromJson(data as Map<String, dynamic>);
  }

}
