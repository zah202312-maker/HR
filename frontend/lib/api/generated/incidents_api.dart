// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Incidents in the contract.
class IncidentsApi {
  IncidentsApi(this.client);
  final HrApiClient client;

  /// POST /api/app/incident  (Submit a fault report)
  Future<IncidentDto> Incident_Create({
    String? idempotencyKey,
    required IncidentCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Incident_Create',
      method: 'POST',
      path: '/api/app/incident',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return IncidentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/incident/my  (List my fault reports)
  Future<IncidentListItemPagedResultDto> Incident_GetMy({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    IncidentStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Incident_GetMy',
      method: 'GET',
      path: '/api/app/incident/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return IncidentListItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/incident/inbox  (List fault reports I handle)
  Future<IncidentListItemPagedResultDto> Incident_GetInbox({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    IncidentStatus? status,
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Incident_GetInbox',
      method: 'GET',
      path: '/api/app/incident/inbox',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return IncidentListItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/incident/{id}  (Get fault report details)
  Future<IncidentDto> Incident_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Incident_Get',
      method: 'GET',
      path: '/api/app/incident/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return IncidentDto.fromJson(data as Map<String, dynamic>);
  }

  /// PATCH /api/app/incident/{id}  (Update fault report status)
  Future<IncidentDto> Incident_UpdateStatus({
    required String id,
    required IncidentStatusUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Incident_UpdateStatus',
      method: 'PATCH',
      path: '/api/app/incident/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return IncidentDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/incident/{id}/forward  (Forward a fault report to HR)
  Future<IncidentDto> Incident_Forward({
    required String id,
    NoteInputDto? body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Incident_Forward',
      method: 'POST',
      path: '/api/app/incident/${Uri.encodeComponent(id)}/forward',
      body: body?.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return IncidentDto.fromJson(data as Map<String, dynamic>);
  }

}
