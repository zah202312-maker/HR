// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AuditLog in the contract.
class AuditLogApi {
  AuditLogApi(this.client);
  final HrApiClient client;

  /// GET /api/app/audit-log  (Read the audit log)
  Future<AuditLogEntryPagedResultDto> AuditLog_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? from,
    DateTime? to,
    String? userId,
    AuditActionType? action,
    String? entityType,
    String? entityId,
    bool? succeeded,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AuditLog_List',
      method: 'GET',
      path: '/api/app/audit-log',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (from != null) 'from': hrDateTimeToJson(from),
        if (to != null) 'to': hrDateTimeToJson(to),
        if (userId != null) 'userId': userId,
        if (action != null) 'action': action.toJson(),
        if (entityType != null) 'entityType': entityType,
        if (entityId != null) 'entityId': entityId,
        if (succeeded != null) 'succeeded': succeeded,
      },
      cancelToken: cancelToken,
    );
    return AuditLogEntryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
