// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged EvaluationAdmin in the contract.
class EvaluationAdminApi {
  EvaluationAdminApi(this.client);
  final HrApiClient client;

  /// GET /api/app/evaluation-criterion  (List evaluation criteria)
  Future<EvaluationCriterionPagedResultDto> EvaluationCriterion_GetList({
    bool? isActive,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationCriterion_GetList',
      method: 'GET',
      path: '/api/app/evaluation-criterion',
      query: <String, dynamic>{
        if (isActive != null) 'isActive': isActive,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationCriterionPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation-criterion  (Create an evaluation criterion)
  Future<EvaluationCriterionDto> EvaluationCriterion_Create({
    required EvaluationCriterionCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationCriterion_Create',
      method: 'POST',
      path: '/api/app/evaluation-criterion',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationCriterionDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/evaluation-criterion/{id}  (Update an evaluation criterion)
  Future<EvaluationCriterionDto> EvaluationCriterion_Update({
    required String id,
    required EvaluationCriterionUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationCriterion_Update',
      method: 'PUT',
      path: '/api/app/evaluation-criterion/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationCriterionDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation-grade  (List the verbal grade scale)
  Future<EvaluationGradePagedResultDto> EvaluationGrade_GetList({
    bool? isActive,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationGrade_GetList',
      method: 'GET',
      path: '/api/app/evaluation-grade',
      query: <String, dynamic>{
        if (isActive != null) 'isActive': isActive,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationGradePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation-grade  (Create a verbal grade)
  Future<EvaluationGradeDto> EvaluationGrade_Create({
    required EvaluationGradeCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationGrade_Create',
      method: 'POST',
      path: '/api/app/evaluation-grade',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationGradeDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/evaluation-grade/{id}  (Update a verbal grade)
  Future<EvaluationGradeDto> EvaluationGrade_Update({
    required String id,
    required EvaluationGradeUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationGrade_Update',
      method: 'PUT',
      path: '/api/app/evaluation-grade/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationGradeDto.fromJson(data as Map<String, dynamic>);
  }

}
