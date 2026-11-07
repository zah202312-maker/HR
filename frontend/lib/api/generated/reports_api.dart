// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Reports in the contract.
class ReportsApi {
  ReportsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/report/team-attendance  (Team attendance report)
  Future<TeamAttendanceRowPagedResultDto> Report_GetTeamAttendance({
    required DateTime from,
    required DateTime to,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetTeamAttendance',
      method: 'GET',
      path: '/api/app/report/team-attendance',
      query: <String, dynamic>{
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return TeamAttendanceRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/team-lateness-absence  (Team lateness and absence report)
  Future<TeamLatenessAbsenceRowPagedResultDto> Report_GetTeamLatenessAbsence({
    required DateTime from,
    required DateTime to,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetTeamLatenessAbsence',
      method: 'GET',
      path: '/api/app/report/team-lateness-absence',
      query: <String, dynamic>{
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return TeamLatenessAbsenceRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/team-leaves  (Team leaves report)
  Future<TeamLeaveRowPagedResultDto> Report_GetTeamLeaves({
    required DateTime from,
    required DateTime to,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetTeamLeaves',
      method: 'GET',
      path: '/api/app/report/team-leaves',
      query: <String, dynamic>{
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return TeamLeaveRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/team-pending  (Team pending requests report)
  Future<TeamPendingRequestRowPagedResultDto> Report_GetTeamPending({
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetTeamPending',
      method: 'GET',
      path: '/api/app/report/team-pending',
      query: <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return TeamPendingRequestRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/team-custody  (Team custody report)
  Future<TeamCustodyRowPagedResultDto> Report_GetTeamCustody({
    String? employeeId,
    ReportCustodyKind? kind,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetTeamCustody',
      method: 'GET',
      path: '/api/app/report/team-custody',
      query: <String, dynamic>{
        if (employeeId != null) 'employeeId': employeeId,
        if (kind != null) 'kind': kind.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return TeamCustodyRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/team-evaluations  (Team evaluations report)
  Future<TeamEvaluationRowPagedResultDto> Report_GetTeamEvaluations({
    String? cycleId,
    int? year,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetTeamEvaluations',
      method: 'GET',
      path: '/api/app/report/team-evaluations',
      query: <String, dynamic>{
        if (cycleId != null) 'cycleId': cycleId,
        if (year != null) 'year': year,
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return TeamEvaluationRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/review-dashboard  (Reviewer dashboard)
  Future<ReviewDashboardDto> Report_GetReviewDashboard({
    int? year,
    int? month,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetReviewDashboard',
      method: 'GET',
      path: '/api/app/report/review-dashboard',
      query: <String, dynamic>{
        if (year != null) 'year': year,
        if (month != null) 'month': month,
      },
      cancelToken: cancelToken,
    );
    return ReviewDashboardDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/evaluation-consistency  (Evaluation consistency report)
  Future<EvaluationConsistencyRowPagedResultDto> Report_GetEvaluationConsistency({
    String? cycleId,
    String? departmentId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetEvaluationConsistency',
      method: 'GET',
      path: '/api/app/report/evaluation-consistency',
      query: <String, dynamic>{
        if (cycleId != null) 'cycleId': cycleId,
        if (departmentId != null) 'departmentId': departmentId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EvaluationConsistencyRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/hr-attendance  (HR attendance report)
  Future<HrAttendanceReportRowPagedResultDto> Report_GetHrAttendance({
    required DateTime from,
    required DateTime to,
    String? departmentId,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetHrAttendance',
      method: 'GET',
      path: '/api/app/report/hr-attendance',
      query: <String, dynamic>{
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (departmentId != null) 'departmentId': departmentId,
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrAttendanceReportRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/hr-leave-balances  (HR leave balances report)
  Future<HrLeaveBalanceReportRowPagedResultDto> Report_GetHrLeaveBalances({
    String? departmentId,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetHrLeaveBalances',
      method: 'GET',
      path: '/api/app/report/hr-leave-balances',
      query: <String, dynamic>{
        if (departmentId != null) 'departmentId': departmentId,
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrLeaveBalanceReportRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/hr-requests  (HR requests report)
  Future<HrRequestReportRowPagedResultDto> Report_GetHrRequests({
    required DateTime from,
    required DateTime to,
    String? departmentId,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetHrRequests',
      method: 'GET',
      path: '/api/app/report/hr-requests',
      query: <String, dynamic>{
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (departmentId != null) 'departmentId': departmentId,
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrRequestReportRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/hr-contracts  (HR contracts report)
  Future<HrContractReportRowPagedResultDto> Report_GetHrContracts({
    DateTime? endFrom,
    DateTime? endTo,
    String? departmentId,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetHrContracts',
      method: 'GET',
      path: '/api/app/report/hr-contracts',
      query: <String, dynamic>{
        if (endFrom != null) 'endFrom': hrDateToJson(endFrom),
        if (endTo != null) 'endTo': hrDateToJson(endTo),
        if (departmentId != null) 'departmentId': departmentId,
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrContractReportRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/hr-warnings  (HR warnings report)
  Future<HrWarningReportRowPagedResultDto> Report_GetHrWarnings({
    required DateTime from,
    required DateTime to,
    String? departmentId,
    String? employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Report_GetHrWarnings',
      method: 'GET',
      path: '/api/app/report/hr-warnings',
      query: <String, dynamic>{
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (departmentId != null) 'departmentId': departmentId,
        if (employeeId != null) 'employeeId': employeeId,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return HrWarningReportRowPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/report/{code}/export  (Export a report to PDF or Excel)
  Future<HrBinaryResponse> Report_Export({
    required ReportCode code,
    required ReportExportFormat format,
    DateTime? from,
    DateTime? to,
    String? employeeId,
    String? departmentId,
    String? cycleId,
    int? year,
    int? month,
    ReportCustodyKind? kind,
    DateTime? endFrom,
    DateTime? endTo,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'Report_Export',
      method: 'GET',
      path: '/api/app/report/${Uri.encodeComponent(code.toJson())}/export',
      query: <String, dynamic>{
        'format': format.toJson(),
        if (from != null) 'from': hrDateToJson(from),
        if (to != null) 'to': hrDateToJson(to),
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (cycleId != null) 'cycleId': cycleId,
        if (year != null) 'year': year,
        if (month != null) 'month': month,
        if (kind != null) 'kind': kind.toJson(),
        if (endFrom != null) 'endFrom': hrDateToJson(endFrom),
        if (endTo != null) 'endTo': hrDateToJson(endTo),
      },
      cancelToken: cancelToken,
    );
    return data;
  }

}
