// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged HrEvaluations in the contract.
class HrEvaluationsApi {
  HrEvaluationsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/hr-evaluation-criterion  (List the HR evaluation criteria)
  Future<HrEvaluationCriterionPagedResultDto> HrEvaluationCriterion_GetList({
    bool? isActive,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationCriterion_GetList',
      method: 'GET',
      path: '/api/app/hr-evaluation-criterion',
      query: <String, dynamic>{
        if (isActive != null) 'isActive': isActive,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrEvaluationCriterionPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/hr-evaluation-criterion  (Add an HR evaluation criterion)
  Future<HrEvaluationCriterionDto> HrEvaluationCriterion_Create({
    required HrEvaluationCriterionCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationCriterion_Create',
      method: 'POST',
      path: '/api/app/hr-evaluation-criterion',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return HrEvaluationCriterionDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/hr-evaluation-criterion/{id}  (Update an HR evaluation criterion)
  Future<HrEvaluationCriterionDto> HrEvaluationCriterion_Update({
    required String id,
    required HrEvaluationCriterionUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationCriterion_Update',
      method: 'PUT',
      path: '/api/app/hr-evaluation-criterion/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return HrEvaluationCriterionDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/hr-evaluation  (List HR evaluations)
  Future<HrEvaluationPagedResultDto> HrEvaluation_GetList({
    String? employeeId,
    bool? isSent,
    DateTime? from,
    DateTime? to,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluation_GetList',
      method: 'GET',
      path: '/api/app/hr-evaluation',
      query: <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId,
        if (isSent != null) 'isSent': isSent,
        if (from != null) 'from': hrDateToJson(from),
        if (to != null) 'to': hrDateToJson(to),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrEvaluationPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/hr-evaluation  (Evaluate an employee)
  Future<HrEvaluationDto> HrEvaluation_Create({
    String? idempotencyKey,
    required HrEvaluationCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluation_Create',
      method: 'POST',
      path: '/api/app/hr-evaluation',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return HrEvaluationDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/hr-evaluation/{id}  (Get an HR evaluation)
  Future<HrEvaluationDto> HrEvaluation_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluation_Get',
      method: 'GET',
      path: '/api/app/hr-evaluation/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return HrEvaluationDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/hr-evaluation/{id}/send  (Send one HR evaluation to the executive director)
  Future<HrEvaluationReportDto> HrEvaluation_Send({
    required String id,
    String? idempotencyKey,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluation_Send',
      method: 'POST',
      path: '/api/app/hr-evaluation/${Uri.encodeComponent(id)}/send',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      cancelToken: cancelToken,
    );
    return HrEvaluationReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/hr-evaluation-report  (List the HR evaluation reports)
  Future<HrEvaluationReportPagedResultDto> HrEvaluationReport_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationReport_GetList',
      method: 'GET',
      path: '/api/app/hr-evaluation-report',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrEvaluationReportPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/hr-evaluation-report  (Send the unsent HR evaluations to the executive director)
  Future<HrEvaluationReportDto> HrEvaluationReport_Send({
    String? idempotencyKey,
    required HrEvaluationReportCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationReport_Send',
      method: 'POST',
      path: '/api/app/hr-evaluation-report',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return HrEvaluationReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/hr-evaluation-report/{id}  (Get an HR evaluation report)
  Future<HrEvaluationReportDto> HrEvaluationReport_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationReport_Get',
      method: 'GET',
      path: '/api/app/hr-evaluation-report/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return HrEvaluationReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/hr-evaluation-alert  (List the low average alerts of HR evaluations)
  Future<HrEvaluationAlertPagedResultDto> HrEvaluationAlert_GetList({
    String? employeeId,
    DateTime? from,
    DateTime? to,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationAlert_GetList',
      method: 'GET',
      path: '/api/app/hr-evaluation-alert',
      query: <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId,
        if (from != null) 'from': hrDateToJson(from),
        if (to != null) 'to': hrDateToJson(to),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrEvaluationAlertPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/hr-evaluation-alert/{id}  (Get a low average alert of HR evaluations)
  Future<HrEvaluationAlertDto> HrEvaluationAlert_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'HrEvaluationAlert_Get',
      method: 'GET',
      path: '/api/app/hr-evaluation-alert/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return HrEvaluationAlertDto.fromJson(data as Map<String, dynamic>);
  }

}
