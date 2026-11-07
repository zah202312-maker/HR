// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Evaluations in the contract.
class EvaluationsApi {
  EvaluationsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/evaluation-cycle/current  (Get the current evaluation cycle)
  Future<EvaluationCycleDto> EvaluationCycle_GetCurrent({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationCycle_GetCurrent',
      method: 'GET',
      path: '/api/app/evaluation-cycle/current',
      cancelToken: cancelToken,
    );
    return EvaluationCycleDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation/team  (List team evaluations for the manager)
  Future<EvaluationManagerSummaryPagedResultDto> Evaluation_GetTeamList({
    String? cycleId,
    EvaluationStatus? status,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_GetTeamList',
      method: 'GET',
      path: '/api/app/evaluation/team',
      query: <String, dynamic>{
        if (cycleId != null) 'cycleId': cycleId,
        if (status != null) 'status': status.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationManagerSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation/{id}  (Get evaluation form for the manager)
  Future<EvaluationManagerDto> Evaluation_GetManagerForm({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_GetManagerForm',
      method: 'GET',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return EvaluationManagerDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/evaluation/{id}  (Save an evaluation draft)
  Future<EvaluationManagerDto> Evaluation_SaveDraft({
    required String id,
    required EvaluationSaveDraftDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_SaveDraft',
      method: 'PUT',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationManagerDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation/{id}/submit  (Submit an evaluation)
  Future<EvaluationManagerDto> Evaluation_Submit({
    required String id,
    required EvaluationSubmitInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_Submit',
      method: 'POST',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/submit',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationManagerDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation/my  (List my evaluations)
  Future<EvaluationEmployeeSummaryPagedResultDto> Evaluation_GetMyList({
    int? year,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_GetMyList',
      method: 'GET',
      path: '/api/app/evaluation/my',
      query: <String, dynamic>{
        if (year != null) 'year': year,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationEmployeeSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation/my/{id}  (Get my evaluation as employee)
  Future<EvaluationEmployeeDto> Evaluation_GetMyDetail({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_GetMyDetail',
      method: 'GET',
      path: '/api/app/evaluation/my/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return EvaluationEmployeeDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation/{id}/acknowledge  (Acknowledge an evaluation)
  Future<EvaluationEmployeeDto> Evaluation_Acknowledge({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_Acknowledge',
      method: 'POST',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/acknowledge',
      cancelToken: cancelToken,
    );
    return EvaluationEmployeeDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation/{id}/response  (Add employee comment or objection)
  Future<EvaluationResponseDto> Evaluation_AddResponse({
    required String id,
    String? idempotencyKey,
    required EvaluationResponseCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_AddResponse',
      method: 'POST',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/response',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationResponseDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation/{id}/response/{rid}/decide  (Decide an evaluation objection)
  Future<EvaluationHrDto> Evaluation_DecideObjection({
    required String id,
    required String rid,
    required EvaluationObjectionDecideDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_DecideObjection',
      method: 'POST',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/response/${Uri.encodeComponent(rid)}/decide',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationHrDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation  (List evaluations for HR)
  Future<EvaluationHrSummaryPagedResultDto> Evaluation_GetHrList({
    String? cycleId,
    EvaluationStatus? status,
    EvaluationObjectionStatus? objectionStatus,
    String? departmentId,
    String? employeeId,
    bool? isLate,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_GetHrList',
      method: 'GET',
      path: '/api/app/evaluation',
      query: <String, dynamic>{
        if (cycleId != null) 'cycleId': cycleId,
        if (status != null) 'status': status.toJson(),
        if (objectionStatus != null) 'objectionStatus': objectionStatus.toJson(),
        if (departmentId != null) 'departmentId': departmentId,
        if (employeeId != null) 'employeeId': employeeId,
        if (isLate != null) 'isLate': isLate,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationHrSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation/{id}/hr-view  (Get evaluation as HR)
  Future<EvaluationHrDto> Evaluation_GetHrView({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Evaluation_GetHrView',
      method: 'GET',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/hr-view',
      cancelToken: cancelToken,
    );
    return EvaluationHrDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation/{id}/internal-note  (List internal notes)
  Future<EvaluationInternalNotePagedResultDto> EvaluationInternalNote_GetList({
    required String id,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationInternalNote_GetList',
      method: 'GET',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/internal-note',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationInternalNotePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/evaluation/{id}/internal-note  (Append an internal note after submit)
  Future<EvaluationInternalNoteDto> EvaluationInternalNote_Add({
    required String id,
    String? idempotencyKey,
    required EvaluationInternalNoteCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationInternalNote_Add',
      method: 'POST',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/internal-note',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationInternalNoteDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/evaluation/{id}/internal-note/draft  (Save the internal note draft before submit)
  Future<EvaluationInternalNoteDto> EvaluationInternalNote_SaveDraft({
    required String id,
    required EvaluationInternalNoteDraftDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationInternalNote_SaveDraft',
      method: 'PUT',
      path: '/api/app/evaluation/${Uri.encodeComponent(id)}/internal-note/draft',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return EvaluationInternalNoteDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation-annual-summary/my  (Get my annual evaluation summary)
  Future<EvaluationAnnualSummaryDto> EvaluationAnnualSummary_GetMy({
    required int year,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationAnnualSummary_GetMy',
      method: 'GET',
      path: '/api/app/evaluation-annual-summary/my',
      query: <String, dynamic>{
        'year': year,
      },
      cancelToken: cancelToken,
    );
    return EvaluationAnnualSummaryDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation-annual-summary/employee/{employeeId}  (Get an employee annual evaluation summary)
  Future<EvaluationAnnualSummaryDto> EvaluationAnnualSummary_GetForEmployee({
    required String employeeId,
    required int year,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationAnnualSummary_GetForEmployee',
      method: 'GET',
      path: '/api/app/evaluation-annual-summary/employee/${Uri.encodeComponent(employeeId)}',
      query: <String, dynamic>{
        'year': year,
      },
      cancelToken: cancelToken,
    );
    return EvaluationAnnualSummaryDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/evaluation-cycle  (List evaluation cycles)
  Future<EvaluationCyclePagedResultDto> EvaluationCycle_GetList({
    int? year,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EvaluationCycle_GetList',
      method: 'GET',
      path: '/api/app/evaluation-cycle',
      query: <String, dynamic>{
        if (year != null) 'year': year,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationCyclePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

}
