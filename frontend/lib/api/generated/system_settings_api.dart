// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged SystemSettings in the contract.
class SystemSettingsApi {
  SystemSettingsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/system-settings  (List settings sections and owners)
  Future<SystemSettingsSectionListDto> SystemSettings_ListSections({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_ListSections',
      method: 'GET',
      path: '/api/app/system-settings',
      cancelToken: cancelToken,
    );
    return SystemSettingsSectionListDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/system-settings/reminders  (Get reminder and escalation settings)
  Future<SystemSettingsRemindersDto> SystemSettings_GetReminders({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_GetReminders',
      method: 'GET',
      path: '/api/app/system-settings/reminders',
      cancelToken: cancelToken,
    );
    return SystemSettingsRemindersDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/system-settings/reminders  (Update reminder and escalation settings)
  Future<SystemSettingsRemindersDto> SystemSettings_UpdateReminders({
    required SystemSettingsRemindersUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_UpdateReminders',
      method: 'PUT',
      path: '/api/app/system-settings/reminders',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return SystemSettingsRemindersDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/system-settings/contracts  (Get contract alert settings)
  Future<SystemSettingsContractsDto> SystemSettings_GetContracts({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_GetContracts',
      method: 'GET',
      path: '/api/app/system-settings/contracts',
      cancelToken: cancelToken,
    );
    return SystemSettingsContractsDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/system-settings/contracts  (Update contract alert settings)
  Future<SystemSettingsContractsDto> SystemSettings_UpdateContracts({
    required SystemSettingsContractsUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_UpdateContracts',
      method: 'PUT',
      path: '/api/app/system-settings/contracts',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return SystemSettingsContractsDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/system-settings/complaints  (Get complaint deadline settings)
  Future<SystemSettingsComplaintsDto> SystemSettings_GetComplaints({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_GetComplaints',
      method: 'GET',
      path: '/api/app/system-settings/complaints',
      cancelToken: cancelToken,
    );
    return SystemSettingsComplaintsDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/system-settings/complaints  (Update complaint deadline settings)
  Future<SystemSettingsComplaintsDto> SystemSettings_UpdateComplaints({
    required SystemSettingsComplaintsUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_UpdateComplaints',
      method: 'PUT',
      path: '/api/app/system-settings/complaints',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return SystemSettingsComplaintsDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/system-settings/hourly-leave  (Get hourly leave settings)
  Future<SystemSettingsHourlyLeaveDto> SystemSettings_GetHourlyLeave({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_GetHourlyLeave',
      method: 'GET',
      path: '/api/app/system-settings/hourly-leave',
      cancelToken: cancelToken,
    );
    return SystemSettingsHourlyLeaveDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/system-settings/hourly-leave  (Update hourly leave settings)
  Future<SystemSettingsHourlyLeaveDto> SystemSettings_UpdateHourlyLeave({
    required SystemSettingsHourlyLeaveUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_UpdateHourlyLeave',
      method: 'PUT',
      path: '/api/app/system-settings/hourly-leave',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return SystemSettingsHourlyLeaveDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/system-settings/finance  (Get payroll approver setting)
  Future<SystemSettingsFinanceDto> SystemSettings_GetFinance({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_GetFinance',
      method: 'GET',
      path: '/api/app/system-settings/finance',
      cancelToken: cancelToken,
    );
    return SystemSettingsFinanceDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/system-settings/finance  (Update payroll approver setting)
  Future<SystemSettingsFinanceDto> SystemSettings_UpdateFinance({
    required SystemSettingsFinanceUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SystemSettings_UpdateFinance',
      method: 'PUT',
      path: '/api/app/system-settings/finance',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return SystemSettingsFinanceDto.fromJson(data as Map<String, dynamic>);
  }

}
