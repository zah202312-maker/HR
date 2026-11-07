// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged LeaveCalendar in the contract.
class LeaveCalendarApi {
  LeaveCalendarApi(this.client);
  final HrApiClient client;

  /// GET /api/app/leave-calendar/team  (Get the team leave calendar)
  Future<LeaveCalendarEntryPagedResultDto> LeaveCalendar_GetTeam({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required DateTime fromDate,
    required DateTime toDate,
    String? employeeId,
    bool? includePending,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'LeaveCalendar_GetTeam',
      method: 'GET',
      path: '/api/app/leave-calendar/team',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'fromDate': hrDateToJson(fromDate),
        'toDate': hrDateToJson(toDate),
        if (employeeId != null) 'employeeId': employeeId,
        if (includePending != null) 'includePending': includePending,
      },
      cancelToken: cancelToken,
    );
    return LeaveCalendarEntryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
