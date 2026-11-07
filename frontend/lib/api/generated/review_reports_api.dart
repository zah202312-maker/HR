// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged ReviewReports in the contract.
class ReviewReportsApi {
  ReviewReportsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/review-report  (List reviewer follow-up reports)
  Future<ReviewReportSummaryPagedResultDto> ReviewReport_GetList({
    ReviewReportPeriodType? periodType,
    ReviewReportStatus? status,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewReport_GetList',
      method: 'GET',
      path: '/api/app/review-report',
      query: <String, dynamic>{
        if (periodType != null) 'periodType': periodType.toJson(),
        if (status != null) 'status': status.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ReviewReportSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/review-report  (Start a reviewer follow-up report)
  Future<ReviewReportDto> ReviewReport_Create({
    required ReviewReportCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewReport_Create',
      method: 'POST',
      path: '/api/app/review-report',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/review-report/{id}  (Get a reviewer follow-up report)
  Future<ReviewReportDto> ReviewReport_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewReport_Get',
      method: 'GET',
      path: '/api/app/review-report/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ReviewReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/review-report/{id}  (Save a reviewer report draft)
  Future<ReviewReportDto> ReviewReport_Update({
    required String id,
    required ReviewReportUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewReport_Update',
      method: 'PUT',
      path: '/api/app/review-report/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/review-report/{id}  (Delete a reviewer report draft)
  Future<void> ReviewReport_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'ReviewReport_Delete',
      method: 'DELETE',
      path: '/api/app/review-report/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

  /// POST /api/app/review-report/{id}/submit  (Submit the reviewer report to the executive director)
  Future<ReviewReportDto> ReviewReport_Submit({
    required String id,
    required ReviewReportSubmitDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewReport_Submit',
      method: 'POST',
      path: '/api/app/review-report/${Uri.encodeComponent(id)}/submit',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewReportDto.fromJson(data as Map<String, dynamic>);
  }

}
