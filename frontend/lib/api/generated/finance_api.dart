// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Finance in the contract.
class FinanceApi {
  FinanceApi(this.client);
  final HrApiClient client;

  /// GET /api/app/finance/salary-profile/overview  (List employees with their base salary (executive director))
  Future<EmployeeSalaryOverviewPagedResultDto> SalaryProfile_GetOverview({
    int? skipCount,
    int? maxResultCount,
    String? search,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SalaryProfile_GetOverview',
      method: 'GET',
      path: '/api/app/finance/salary-profile/overview',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (search != null) 'search': search,
      },
      cancelToken: cancelToken,
    );
    return EmployeeSalaryOverviewPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/salary-profile  (List employees with salary status)
  Future<SalaryProfileListItemPagedResultDto> SalaryProfile_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? departmentId,
    bool? hasProfile,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SalaryProfile_List',
      method: 'GET',
      path: '/api/app/finance/salary-profile',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (departmentId != null) 'departmentId': departmentId,
        if (hasProfile != null) 'hasProfile': hasProfile,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return SalaryProfileListItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/salary-profile/{employeeId}  (Get a salary profile)
  Future<SalaryProfileDto> SalaryProfile_Get({
    required String employeeId,
    DateTime? asOf,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SalaryProfile_Get',
      method: 'GET',
      path: '/api/app/finance/salary-profile/${Uri.encodeComponent(employeeId)}',
      query: <String, dynamic>{
        if (asOf != null) 'asOf': hrDateToJson(asOf),
      },
      cancelToken: cancelToken,
    );
    return SalaryProfileDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/finance/salary-profile/{employeeId}  (Create a new salary profile version)
  Future<SalaryProfileDto> SalaryProfile_Update({
    required String employeeId,
    required SalaryProfileUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SalaryProfile_Update',
      method: 'PUT',
      path: '/api/app/finance/salary-profile/${Uri.encodeComponent(employeeId)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return SalaryProfileDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/salary-profile/{employeeId}/history  (List salary profile versions)
  Future<SalaryProfilePagedResultDto> SalaryProfile_GetHistory({
    required String employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'SalaryProfile_GetHistory',
      method: 'GET',
      path: '/api/app/finance/salary-profile/${Uri.encodeComponent(employeeId)}/history',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return SalaryProfilePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/adjustment  (List additions and deductions)
  Future<AdjustmentPagedResultDto> Adjustment_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? employeeId,
    String? departmentId,
    String? period,
    AdjustmentType? type,
    AdjustmentSource? source,
    AdjustmentStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Adjustment_List',
      method: 'GET',
      path: '/api/app/finance/adjustment',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (period != null) 'period': period,
        if (type != null) 'type': type.toJson(),
        if (source != null) 'source': source.toJson(),
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return AdjustmentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/adjustment  (Create a manual addition or deduction)
  Future<AdjustmentDto> Adjustment_Create({
    String? idempotencyKey,
    required AdjustmentCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Adjustment_Create',
      method: 'POST',
      path: '/api/app/finance/adjustment',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AdjustmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/adjustment/{id}/reverse  (Reverse an adjustment)
  Future<AdjustmentDto> Adjustment_Reverse({
    required String id,
    required AdjustmentReverseInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Adjustment_Reverse',
      method: 'POST',
      path: '/api/app/finance/adjustment/${Uri.encodeComponent(id)}/reverse',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AdjustmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/adjustment/{id}/attachment-download-url  (Get an adjustment attachment download URL)
  Future<FileDownloadUrlDto> Adjustment_GetAttachmentDownloadUrl({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Adjustment_GetAttachmentDownloadUrl',
      method: 'GET',
      path: '/api/app/finance/adjustment/${Uri.encodeComponent(id)}/attachment-download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/adjustment/suggestions  (List attendance-based suggestions)
  Future<AdjustmentSuggestionPagedResultDto> AdjustmentSuggestion_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? period,
    String? employeeId,
    String? departmentId,
    AdjustmentSuggestionStatus? status,
    AdjustmentSuggestionBasis? basis,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AdjustmentSuggestion_List',
      method: 'GET',
      path: '/api/app/finance/adjustment/suggestions',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (period != null) 'period': period,
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (status != null) 'status': status.toJson(),
        if (basis != null) 'basis': basis.toJson(),
      },
      cancelToken: cancelToken,
    );
    return AdjustmentSuggestionPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/adjustment/suggestions/{id}/approve  (Approve a suggestion into an adjustment)
  Future<AdjustmentDto> AdjustmentSuggestion_Approve({
    required String id,
    required AdjustmentSuggestionApproveInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AdjustmentSuggestion_Approve',
      method: 'POST',
      path: '/api/app/finance/adjustment/suggestions/${Uri.encodeComponent(id)}/approve',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AdjustmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/adjustment/suggestions/{id}/reject  (Reject a suggestion)
  Future<AdjustmentSuggestionDto> AdjustmentSuggestion_Reject({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AdjustmentSuggestion_Reject',
      method: 'POST',
      path: '/api/app/finance/adjustment/suggestions/${Uri.encodeComponent(id)}/reject',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return AdjustmentSuggestionDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/payroll-run  (List payroll runs)
  Future<PayrollRunSummaryPagedResultDto> PayrollRun_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    PayrollRunStatus? status,
    String? periodFrom,
    String? periodTo,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_List',
      method: 'GET',
      path: '/api/app/finance/payroll-run',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (periodFrom != null) 'periodFrom': periodFrom,
        if (periodTo != null) 'periodTo': periodTo,
      },
      cancelToken: cancelToken,
    );
    return PayrollRunSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/payroll-run  (Create a monthly payroll run)
  Future<PayrollRunDto> PayrollRun_Create({
    String? idempotencyKey,
    required PayrollRunCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_Create',
      method: 'POST',
      path: '/api/app/finance/payroll-run',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/payroll-run/{id}  (Get a payroll run)
  Future<PayrollRunDto> PayrollRun_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_Get',
      method: 'GET',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/payroll-run/{id}/recalculate  (Recalculate a draft payroll run)
  Future<PayrollRunDto> PayrollRun_Recalculate({
    required String id,
    required PayrollRunActionInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_Recalculate',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/recalculate',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/payroll-run/{id}/request-approval  (Send a draft payroll run for approval)
  Future<PayrollRunDto> PayrollRun_RequestApproval({
    required String id,
    required PayrollRunActionInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_RequestApproval',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/request-approval',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/payroll-run/{id}/approve  (Approve a payroll run)
  Future<PayrollRunDto> PayrollRun_Approve({
    required String id,
    required PayrollRunActionInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_Approve',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/approve',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/payroll-run/{id}/close  (Close a payroll run)
  Future<PayrollRunDto> PayrollRun_Close({
    required String id,
    required PayrollRunActionInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_Close',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/close',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/payroll-run/{id}/export  (Export a payroll run)
  Future<HrBinaryResponse> PayrollRun_Export({
    required String id,
    required FinanceExportFormat format,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'PayrollRun_Export',
      method: 'GET',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/export',
      query: <String, dynamic>{
        'format': format.toJson(),
      },
      cancelToken: cancelToken,
    );
    return data;
  }

  /// GET /api/app/finance/payslip/my  (List my payslips)
  Future<PayslipSummaryPagedResultDto> Payslip_GetMyList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Payslip_GetMyList',
      method: 'GET',
      path: '/api/app/finance/payslip/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return PayslipSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/payslip/my/{id}  (Get my payslip)
  Future<PayslipDto> Payslip_GetMy({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Payslip_GetMy',
      method: 'GET',
      path: '/api/app/finance/payslip/my/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return PayslipDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/payslip/my/{id}/pdf  (Download my payslip as PDF)
  Future<HrBinaryResponse> Payslip_DownloadMyPdf({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'Payslip_DownloadMyPdf',
      method: 'GET',
      path: '/api/app/finance/payslip/my/${Uri.encodeComponent(id)}/pdf',
      cancelToken: cancelToken,
    );
    return data;
  }

  /// POST /api/app/finance/payroll-run/{id}/deliver  (Mark salaries as delivered)
  Future<PayrollRunDto> PayrollRun_Deliver({
    required String id,
    required PayrollDeliveryInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_Deliver',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/deliver',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/payroll-run/{id}/undo-delivery  (Undo a salary delivery mark)
  Future<PayrollRunDto> PayrollRun_UndoDelivery({
    required String id,
    required PayrollUndoDeliveryInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_UndoDelivery',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/undo-delivery',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/petty-cash-type  (List petty cash types)
  Future<PettyCashTypePagedResultDto> PettyCashType_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashType_GetList',
      method: 'GET',
      path: '/api/app/finance/petty-cash-type',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
      },
      cancelToken: cancelToken,
    );
    return PettyCashTypePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/petty-cash-type  (Create a petty cash type)
  Future<PettyCashTypeDto> PettyCashType_Create({
    String? idempotencyKey,
    required PettyCashTypeCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashType_Create',
      method: 'POST',
      path: '/api/app/finance/petty-cash-type',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashTypeDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/finance/petty-cash-type/{id}  (Update a petty cash type)
  Future<PettyCashTypeDto> PettyCashType_Update({
    required String id,
    required PettyCashTypeUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashType_Update',
      method: 'PUT',
      path: '/api/app/finance/petty-cash-type/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashTypeDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/petty-cash-assignment  (List monthly petty cash)
  Future<PettyCashAssignmentPagedResultDto> PettyCashAssignment_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? employeeId,
    String? typeId,
    String? activeIn,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashAssignment_GetList',
      method: 'GET',
      path: '/api/app/finance/petty-cash-assignment',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeId != null) 'employeeId': employeeId,
        if (typeId != null) 'typeId': typeId,
        if (activeIn != null) 'activeIn': activeIn,
      },
      cancelToken: cancelToken,
    );
    return PettyCashAssignmentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/petty-cash-assignment  (Give an employee monthly petty cash)
  Future<PettyCashAssignmentDto> PettyCashAssignment_Create({
    String? idempotencyKey,
    required PettyCashAssignmentCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashAssignment_Create',
      method: 'POST',
      path: '/api/app/finance/petty-cash-assignment',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashAssignmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/finance/petty-cash-assignment/{id}  (Update or stop monthly petty cash)
  Future<PettyCashAssignmentDto> PettyCashAssignment_Update({
    required String id,
    required PettyCashAssignmentUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashAssignment_Update',
      method: 'PUT',
      path: '/api/app/finance/petty-cash-assignment/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashAssignmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/petty-cash/month  (Petty cash of a month)
  Future<PettyCashMonthDto> PettyCashEntry_GetMonth({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required String period,
    PettyCashEntryStatus? status,
    String? employeeId,
    String? typeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashEntry_GetMonth',
      method: 'GET',
      path: '/api/app/finance/petty-cash/month',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'period': period,
        if (status != null) 'status': status.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
        if (typeId != null) 'typeId': typeId,
      },
      cancelToken: cancelToken,
    );
    return PettyCashMonthDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/finance/petty-cash/entry/{id}  (Change one month amount)
  Future<PettyCashEntryDto> PettyCashEntry_Update({
    required String id,
    required PettyCashEntryUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashEntry_Update',
      method: 'PUT',
      path: '/api/app/finance/petty-cash/entry/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashEntryDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/petty-cash/deliver  (Mark petty cash as delivered)
  Future<PettyCashEntryListDto> PettyCashEntry_Deliver({
    required PettyCashDeliveryInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashEntry_Deliver',
      method: 'POST',
      path: '/api/app/finance/petty-cash/deliver',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashEntryListDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/petty-cash/entry/{id}/undo-delivery  (Undo a petty cash delivery)
  Future<PettyCashEntryDto> PettyCashEntry_UndoDelivery({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PettyCashEntry_UndoDelivery',
      method: 'POST',
      path: '/api/app/finance/petty-cash/entry/${Uri.encodeComponent(id)}/undo-delivery',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PettyCashEntryDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/expense  (List expenses)
  Future<ExpensePagedResultDto> Expense_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    ExpenseSource? source,
    String? category,
    DateTime? dateFrom,
    DateTime? dateTo,
    ExpenseStatus? status,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Expense_List',
      method: 'GET',
      path: '/api/app/finance/expense',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (source != null) 'source': source.toJson(),
        if (category != null) 'category': category,
        if (dateFrom != null) 'dateFrom': hrDateToJson(dateFrom),
        if (dateTo != null) 'dateTo': hrDateToJson(dateTo),
        if (status != null) 'status': status.toJson(),
      },
      cancelToken: cancelToken,
    );
    return ExpensePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/expense  (Create a general expense)
  Future<ExpenseDto> Expense_Create({
    String? idempotencyKey,
    required ExpenseCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Expense_Create',
      method: 'POST',
      path: '/api/app/finance/expense',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return ExpenseDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/expense/export  (Export expenses)
  Future<HrBinaryResponse> Expense_Export({
    required FinanceExportFormat format,
    ExpenseSource? source,
    String? category,
    DateTime? dateFrom,
    DateTime? dateTo,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'Expense_Export',
      method: 'GET',
      path: '/api/app/finance/expense/export',
      query: <String, dynamic>{
        'format': format.toJson(),
        if (source != null) 'source': source.toJson(),
        if (category != null) 'category': category,
        if (dateFrom != null) 'dateFrom': hrDateToJson(dateFrom),
        if (dateTo != null) 'dateTo': hrDateToJson(dateTo),
      },
      cancelToken: cancelToken,
    );
    return data;
  }

  /// GET /api/app/finance/expense/{id}  (Get an expense)
  Future<ExpenseDto> Expense_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Expense_Get',
      method: 'GET',
      path: '/api/app/finance/expense/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ExpenseDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/finance/expense/{id}/reverse  (Reverse an expense)
  Future<ExpenseDto> Expense_Reverse({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Expense_Reverse',
      method: 'POST',
      path: '/api/app/finance/expense/${Uri.encodeComponent(id)}/reverse',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ExpenseDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/expense/{id}/attachment-download-url  (Get an expense attachment download URL)
  Future<FileDownloadUrlDto> Expense_GetAttachmentDownloadUrl({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Expense_GetAttachmentDownloadUrl',
      method: 'GET',
      path: '/api/app/finance/expense/${Uri.encodeComponent(id)}/attachment-download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/accountant-request  (List requests routed to the accountant)
  Future<AccountantRequestSummaryPagedResultDto> AccountantRequest_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    AccountantRequestStatus? status,
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AccountantRequest_List',
      method: 'GET',
      path: '/api/app/finance/accountant-request',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return AccountantRequestSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/accountant-request/{id}  (Get a request routed to the accountant)
  Future<AccountantRequestDto> AccountantRequest_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AccountantRequest_Get',
      method: 'GET',
      path: '/api/app/finance/accountant-request/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return AccountantRequestDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/report/monthly-statement  (Get the monthly statement report)
  Future<FinanceMonthlyStatementReportDto> FinanceReport_GetMonthlyStatement({
    required String period,
    String? departmentId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'FinanceReport_GetMonthlyStatement',
      method: 'GET',
      path: '/api/app/finance/report/monthly-statement',
      query: <String, dynamic>{
        'period': period,
        if (departmentId != null) 'departmentId': departmentId,
      },
      cancelToken: cancelToken,
    );
    return FinanceMonthlyStatementReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/report/adjustments  (Get the additions and deductions report)
  Future<FinanceAdjustmentReportPagedResultDto> FinanceReport_GetAdjustments({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required String periodFrom,
    required String periodTo,
    String? employeeId,
    String? departmentId,
    AdjustmentType? type,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'FinanceReport_GetAdjustments',
      method: 'GET',
      path: '/api/app/finance/report/adjustments',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'periodFrom': periodFrom,
        'periodTo': periodTo,
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (type != null) 'type': type.toJson(),
      },
      cancelToken: cancelToken,
    );
    return FinanceAdjustmentReportPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/report/store-expenses  (Get the store expenses report)
  Future<FinanceStoreExpenseReportPagedResultDto> FinanceReport_GetStoreExpenses({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required DateTime dateFrom,
    required DateTime dateTo,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'FinanceReport_GetStoreExpenses',
      method: 'GET',
      path: '/api/app/finance/report/store-expenses',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'dateFrom': hrDateToJson(dateFrom),
        'dateTo': hrDateToJson(dateTo),
      },
      cancelToken: cancelToken,
    );
    return FinanceStoreExpenseReportPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/finance/report/{code}/export  (Export an accountant report)
  Future<HrBinaryResponse> FinanceReport_Export({
    required FinanceReportCode code,
    required FinanceExportFormat format,
    String? period,
    String? periodFrom,
    String? periodTo,
    DateTime? dateFrom,
    DateTime? dateTo,
    String? employeeId,
    String? departmentId,
    AdjustmentType? type,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'FinanceReport_Export',
      method: 'GET',
      path: '/api/app/finance/report/${Uri.encodeComponent(code.toJson())}/export',
      query: <String, dynamic>{
        'format': format.toJson(),
        if (period != null) 'period': period,
        if (periodFrom != null) 'periodFrom': periodFrom,
        if (periodTo != null) 'periodTo': periodTo,
        if (dateFrom != null) 'dateFrom': hrDateToJson(dateFrom),
        if (dateTo != null) 'dateTo': hrDateToJson(dateTo),
        if (employeeId != null) 'employeeId': employeeId,
        if (departmentId != null) 'departmentId': departmentId,
        if (type != null) 'type': type.toJson(),
      },
      cancelToken: cancelToken,
    );
    return data;
  }

  /// POST /api/app/finance/payroll-run/{id}/return  (Return a payroll run to the accountant)
  Future<PayrollRunDto> PayrollRun_ReturnForChanges({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'PayrollRun_ReturnForChanges',
      method: 'POST',
      path: '/api/app/finance/payroll-run/${Uri.encodeComponent(id)}/return',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return PayrollRunDto.fromJson(data as Map<String, dynamic>);
  }

}
