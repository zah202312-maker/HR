// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Holiday in the contract.
class HolidayApi {
  HolidayApi(this.client);
  final HrApiClient client;

  /// GET /api/app/holiday  (List official holidays)
  Future<HolidayPagedResultDto> Holiday_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    DateTime? fromDate,
    DateTime? toDate,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Holiday_List',
      method: 'GET',
      path: '/api/app/holiday',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
      },
      cancelToken: cancelToken,
    );
    return HolidayPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/holiday  (Add an official holiday)
  Future<HolidayDto> Holiday_Create({
    String? idempotencyKey,
    required HolidayWriteDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Holiday_Create',
      method: 'POST',
      path: '/api/app/holiday',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return HolidayDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/holiday/{id}  (Update an official holiday)
  Future<HolidayDto> Holiday_Update({
    required String id,
    required HolidayUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Holiday_Update',
      method: 'PUT',
      path: '/api/app/holiday/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return HolidayDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/holiday/{id}  (Delete an official holiday)
  Future<void> Holiday_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'Holiday_Delete',
      method: 'DELETE',
      path: '/api/app/holiday/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

  /// GET /api/app/weekly-off-setting  (Get company weekly off days)
  Future<WeeklyOffSettingDto> WeeklyOffSetting_Get({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'WeeklyOffSetting_Get',
      method: 'GET',
      path: '/api/app/weekly-off-setting',
      cancelToken: cancelToken,
    );
    return WeeklyOffSettingDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/weekly-off-setting  (Update company weekly off days)
  Future<WeeklyOffSettingDto> WeeklyOffSetting_Update({
    required WeeklyOffSettingUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'WeeklyOffSetting_Update',
      method: 'PUT',
      path: '/api/app/weekly-off-setting',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return WeeklyOffSettingDto.fromJson(data as Map<String, dynamic>);
  }

}
