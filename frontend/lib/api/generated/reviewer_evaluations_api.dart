// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged ReviewerEvaluations in the contract.
class ReviewerEvaluationsApi {
  ReviewerEvaluationsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/reviewer-evaluation-criterion  (List the reviewer evaluation criteria)
  Future<ReviewerEvaluationCriterionPagedResultDto> ReviewerEvaluationCriterion_GetList({
    bool? isActive,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluationCriterion_GetList',
      method: 'GET',
      path: '/api/app/reviewer-evaluation-criterion',
      query: <String, dynamic>{
        if (isActive != null) 'isActive': isActive,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationCriterionPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/reviewer-evaluation-criterion  (Add a reviewer evaluation criterion)
  Future<ReviewerEvaluationCriterionDto> ReviewerEvaluationCriterion_Create({
    required ReviewerEvaluationCriterionCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluationCriterion_Create',
      method: 'POST',
      path: '/api/app/reviewer-evaluation-criterion',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationCriterionDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/reviewer-evaluation-criterion/{id}  (Update a reviewer evaluation criterion)
  Future<ReviewerEvaluationCriterionDto> ReviewerEvaluationCriterion_Update({
    required String id,
    required ReviewerEvaluationCriterionUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluationCriterion_Update',
      method: 'PUT',
      path: '/api/app/reviewer-evaluation-criterion/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationCriterionDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/reviewer-evaluation  (List reviewer evaluations)
  Future<ReviewerEvaluationPagedResultDto> ReviewerEvaluation_GetList({
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
      operationId: 'ReviewerEvaluation_GetList',
      method: 'GET',
      path: '/api/app/reviewer-evaluation',
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
    return ReviewerEvaluationPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/reviewer-evaluation  (Evaluate an employee (reviewer))
  Future<ReviewerEvaluationDto> ReviewerEvaluation_Create({
    String? idempotencyKey,
    required ReviewerEvaluationCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluation_Create',
      method: 'POST',
      path: '/api/app/reviewer-evaluation',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/reviewer-evaluation/candidate  (Employees the reviewer may evaluate)
  Future<EmployeeLookupPagedResultDto> ReviewerEvaluation_GetCandidates({
    String? search,
    int? skipCount,
    int? maxResultCount,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluation_GetCandidates',
      method: 'GET',
      path: '/api/app/reviewer-evaluation/candidate',
      query: <String, dynamic>{
        if (search != null) 'search': search,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
      },
      cancelToken: cancelToken,
    );
    return EmployeeLookupPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/reviewer-evaluation/{id}  (Get a reviewer evaluation)
  Future<ReviewerEvaluationDto> ReviewerEvaluation_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluation_Get',
      method: 'GET',
      path: '/api/app/reviewer-evaluation/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/reviewer-evaluation/{id}/send  (Send one reviewer evaluation to the executive director)
  Future<ReviewerEvaluationReportDto> ReviewerEvaluation_Send({
    required String id,
    String? idempotencyKey,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluation_Send',
      method: 'POST',
      path: '/api/app/reviewer-evaluation/${Uri.encodeComponent(id)}/send',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/reviewer-evaluation-report  (List the reviewer evaluation reports)
  Future<ReviewerEvaluationReportPagedResultDto> ReviewerEvaluationReport_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluationReport_GetList',
      method: 'GET',
      path: '/api/app/reviewer-evaluation-report',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationReportPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/reviewer-evaluation-report  (Send the unsent reviewer evaluations to the executive director)
  Future<ReviewerEvaluationReportDto> ReviewerEvaluationReport_Send({
    String? idempotencyKey,
    required ReviewerEvaluationReportCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluationReport_Send',
      method: 'POST',
      path: '/api/app/reviewer-evaluation-report',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/reviewer-evaluation-report/{id}  (Get a reviewer evaluation report)
  Future<ReviewerEvaluationReportDto> ReviewerEvaluationReport_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewerEvaluationReport_Get',
      method: 'GET',
      path: '/api/app/reviewer-evaluation-report/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ReviewerEvaluationReportDto.fromJson(data as Map<String, dynamic>);
  }

}
