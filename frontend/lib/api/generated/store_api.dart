// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Store in the contract.
class StoreApi {
  StoreApi(this.client);
  final HrApiClient client;

  /// GET /api/app/item  (List items with stock)
  Future<ItemPagedResultDto> Item_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    ItemKind? kind,
    bool? isActive,
    bool? belowMinimum,
    String? categoryId,
    StockStatus? stockStatus,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Item_GetList',
      method: 'GET',
      path: '/api/app/item',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (kind != null) 'kind': kind.toJson(),
        if (isActive != null) 'isActive': isActive,
        if (belowMinimum != null) 'belowMinimum': belowMinimum,
        if (categoryId != null) 'categoryId': categoryId,
        if (stockStatus != null) 'stockStatus': stockStatus.toJson(),
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return ItemPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/item  (Create an item)
  Future<ItemDto> Item_Create({
    String? idempotencyKey,
    required ItemCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Item_Create',
      method: 'POST',
      path: '/api/app/item',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ItemDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/item/lookup  (Look up active items)
  Future<ItemRefPagedResultDto> Item_Lookup({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    ItemKind? kind,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Item_Lookup',
      method: 'GET',
      path: '/api/app/item/lookup',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (kind != null) 'kind': kind.toJson(),
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return ItemRefPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/item/{id}  (Get an item)
  Future<ItemDto> Item_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Item_Get',
      method: 'GET',
      path: '/api/app/item/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ItemDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/item/{id}  (Update an item)
  Future<ItemDto> Item_Update({
    required String id,
    required ItemUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Item_Update',
      method: 'PUT',
      path: '/api/app/item/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ItemDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/item/{id}  (Delete an item)
  Future<void> Item_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'Item_Delete',
      method: 'DELETE',
      path: '/api/app/item/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

  /// POST /api/app/stock/receive  (Receive stock)
  Future<StockMovementDto> Stock_Receive({
    String? idempotencyKey,
    required StockReceiveInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Stock_Receive',
      method: 'POST',
      path: '/api/app/stock/receive',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StockMovementDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/stock/adjust  (Correct stock with a reversing movement)
  Future<StockMovementDto> Stock_Adjust({
    String? idempotencyKey,
    required StockAdjustInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Stock_Adjust',
      method: 'POST',
      path: '/api/app/stock/adjust',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StockMovementDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/stock/movements  (List stock movements)
  Future<StockMovementPagedResultDto> Stock_GetMovements({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? itemId,
    StockMovementType? type,
    DateTime? fromDate,
    DateTime? toDate,
    String? teamId,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Stock_GetMovements',
      method: 'GET',
      path: '/api/app/stock/movements',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (itemId != null) 'itemId': itemId,
        if (type != null) 'type': type.toJson(),
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (teamId != null) 'teamId': teamId,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return StockMovementPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/item-category  (List item categories)
  Future<ItemCategoryPagedResultDto> ItemCategory_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ItemCategory_GetList',
      method: 'GET',
      path: '/api/app/store/item-category',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return ItemCategoryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/item-category  (Create an item category)
  Future<ItemCategoryDto> ItemCategory_Create({
    String? idempotencyKey,
    required ItemCategoryCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ItemCategory_Create',
      method: 'POST',
      path: '/api/app/store/item-category',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ItemCategoryDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/store/item-category/{id}  (Update an item category)
  Future<ItemCategoryDto> ItemCategory_Update({
    required String id,
    required ItemCategoryUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ItemCategory_Update',
      method: 'PUT',
      path: '/api/app/store/item-category/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ItemCategoryDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/item-unit  (List units of measure)
  Future<ItemUnitPagedResultDto> ItemUnit_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ItemUnit_GetList',
      method: 'GET',
      path: '/api/app/store/item-unit',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return ItemUnitPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/item-unit  (Create a unit of measure)
  Future<ItemUnitDto> ItemUnit_Create({
    String? idempotencyKey,
    required ItemUnitCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ItemUnit_Create',
      method: 'POST',
      path: '/api/app/store/item-unit',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ItemUnitDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/store/item-unit/{id}  (Update a unit of measure)
  Future<ItemUnitDto> ItemUnit_Update({
    required String id,
    required ItemUnitUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ItemUnit_Update',
      method: 'PUT',
      path: '/api/app/store/item-unit/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ItemUnitDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/purchase  (List purchases)
  Future<StorePurchaseSummaryPagedResultDto> StorePurchase_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    StoreDocumentStatus? status,
    DateTime? fromDate,
    DateTime? toDate,
    String? itemId,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_GetList',
      method: 'GET',
      path: '/api/app/store/purchase',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (itemId != null) 'itemId': itemId,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return StorePurchaseSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/purchase  (Create a purchase draft)
  Future<StorePurchaseDto> StorePurchase_Create({
    String? idempotencyKey,
    required StorePurchaseCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_Create',
      method: 'POST',
      path: '/api/app/store/purchase',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StorePurchaseDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/purchase/{id}  (Get a purchase)
  Future<StorePurchaseDto> StorePurchase_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_Get',
      method: 'GET',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return StorePurchaseDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/store/purchase/{id}  (Update a purchase draft)
  Future<StorePurchaseDto> StorePurchase_Update({
    required String id,
    required StorePurchaseUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_Update',
      method: 'PUT',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StorePurchaseDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/store/purchase/{id}  (Delete a purchase draft)
  Future<void> StorePurchase_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'StorePurchase_Delete',
      method: 'DELETE',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

  /// POST /api/app/store/purchase/{id}/approve  (Approve a purchase)
  Future<StorePurchaseDto> StorePurchase_Approve({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_Approve',
      method: 'POST',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}/approve',
      cancelToken: cancelToken,
    );
    return StorePurchaseDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/purchase/{id}/reverse  (Reverse an approved purchase)
  Future<StorePurchaseDto> StorePurchase_Reverse({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_Reverse',
      method: 'POST',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}/reverse',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StorePurchaseDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/purchase/{id}/attachment  (Upload the purchase attachment)
  Future<AttachmentDto> StorePurchase_UploadAttachment({
    required String id,
    required StoreAttachmentUploadInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_UploadAttachment',
      method: 'POST',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}/attachment',
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AttachmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/store/purchase/{id}/attachment  (Delete the purchase attachment)
  Future<void> StorePurchase_DeleteAttachment({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'StorePurchase_DeleteAttachment',
      method: 'DELETE',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}/attachment',
      cancelToken: cancelToken,
    );
  }

  /// GET /api/app/store/purchase/{id}/attachment-download-url  (Get the purchase attachment download URL)
  Future<FileDownloadUrlDto> StorePurchase_GetAttachmentDownloadUrl({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StorePurchase_GetAttachmentDownloadUrl',
      method: 'GET',
      path: '/api/app/store/purchase/${Uri.encodeComponent(id)}/attachment-download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/team  (List store teams)
  Future<StoreTeamPagedResultDto> StoreTeam_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isActive,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreTeam_GetList',
      method: 'GET',
      path: '/api/app/store/team',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isActive != null) 'isActive': isActive,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return StoreTeamPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/team  (Create a store team)
  Future<StoreTeamDto> StoreTeam_Create({
    String? idempotencyKey,
    required StoreTeamCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreTeam_Create',
      method: 'POST',
      path: '/api/app/store/team',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StoreTeamDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/store/team/{id}  (Update a store team)
  Future<StoreTeamDto> StoreTeam_Update({
    required String id,
    required StoreTeamUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreTeam_Update',
      method: 'PUT',
      path: '/api/app/store/team/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StoreTeamDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/issue-voucher  (List issue vouchers)
  Future<StoreIssueVoucherSummaryPagedResultDto> StoreIssueVoucher_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    StoreDocumentStatus? status,
    String? teamId,
    String? itemId,
    DateTime? fromDate,
    DateTime? toDate,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_GetList',
      method: 'GET',
      path: '/api/app/store/issue-voucher',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (teamId != null) 'teamId': teamId,
        if (itemId != null) 'itemId': itemId,
        if (fromDate != null) 'fromDate': hrDateToJson(fromDate),
        if (toDate != null) 'toDate': hrDateToJson(toDate),
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return StoreIssueVoucherSummaryPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/issue-voucher  (Create an issue voucher draft)
  Future<StoreIssueVoucherDto> StoreIssueVoucher_Create({
    String? idempotencyKey,
    required StoreIssueVoucherCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_Create',
      method: 'POST',
      path: '/api/app/store/issue-voucher',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StoreIssueVoucherDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/issue-voucher/{id}  (Get an issue voucher)
  Future<StoreIssueVoucherDto> StoreIssueVoucher_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_Get',
      method: 'GET',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return StoreIssueVoucherDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/store/issue-voucher/{id}  (Update an issue voucher draft)
  Future<StoreIssueVoucherDto> StoreIssueVoucher_Update({
    required String id,
    required StoreIssueVoucherUpdateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_Update',
      method: 'PUT',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StoreIssueVoucherDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/store/issue-voucher/{id}  (Delete an issue voucher draft)
  Future<void> StoreIssueVoucher_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'StoreIssueVoucher_Delete',
      method: 'DELETE',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

  /// POST /api/app/store/issue-voucher/{id}/approve  (Approve an issue voucher)
  Future<StoreIssueVoucherDto> StoreIssueVoucher_Approve({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_Approve',
      method: 'POST',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}/approve',
      cancelToken: cancelToken,
    );
    return StoreIssueVoucherDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/issue-voucher/{id}/reverse  (Reverse an approved issue voucher)
  Future<StoreIssueVoucherDto> StoreIssueVoucher_Reverse({
    required String id,
    required ReasonInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_Reverse',
      method: 'POST',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}/reverse',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StoreIssueVoucherDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/store/issue-voucher/{id}/attachment  (Upload the issue voucher attachment)
  Future<AttachmentDto> StoreIssueVoucher_UploadAttachment({
    required String id,
    required StoreAttachmentUploadInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_UploadAttachment',
      method: 'POST',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}/attachment',
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AttachmentDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/store/issue-voucher/{id}/attachment  (Delete the issue voucher attachment)
  Future<void> StoreIssueVoucher_DeleteAttachment({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'StoreIssueVoucher_DeleteAttachment',
      method: 'DELETE',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}/attachment',
      cancelToken: cancelToken,
    );
  }

  /// GET /api/app/store/issue-voucher/{id}/attachment-download-url  (Get the issue voucher attachment download URL)
  Future<FileDownloadUrlDto> StoreIssueVoucher_GetAttachmentDownloadUrl({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreIssueVoucher_GetAttachmentDownloadUrl',
      method: 'GET',
      path: '/api/app/store/issue-voucher/${Uri.encodeComponent(id)}/attachment-download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/dashboard  (Get the store dashboard)
  Future<StoreDashboardDto> StoreDashboard_Get({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreDashboard_Get',
      method: 'GET',
      path: '/api/app/store/dashboard',
      cancelToken: cancelToken,
    );
    return StoreDashboardDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/report/purchases  (Purchases report)
  Future<StorePurchaseReportDto> StoreReport_GetPurchases({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required DateTime from,
    required DateTime to,
    String? itemId,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreReport_GetPurchases',
      method: 'GET',
      path: '/api/app/store/report/purchases',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (itemId != null) 'itemId': itemId,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return StorePurchaseReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/report/stock  (Current stock report)
  Future<StoreStockReportDto> StoreReport_GetStock({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    StockStatus? stockStatus,
    String? categoryId,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreReport_GetStock',
      method: 'GET',
      path: '/api/app/store/report/stock',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (stockStatus != null) 'stockStatus': stockStatus.toJson(),
        if (categoryId != null) 'categoryId': categoryId,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return StoreStockReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/report/issues-by-team  (Issues by team report)
  Future<StoreTeamIssueReportDto> StoreReport_GetIssuesByTeam({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required DateTime from,
    required DateTime to,
    String? teamId,
    String? itemId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreReport_GetIssuesByTeam',
      method: 'GET',
      path: '/api/app/store/report/issues-by-team',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
        if (teamId != null) 'teamId': teamId,
        if (itemId != null) 'itemId': itemId,
      },
      cancelToken: cancelToken,
    );
    return StoreTeamIssueReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/report/item-movement  (Item movement report)
  Future<StoreItemMovementReportDto> StoreReport_GetItemMovement({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    required String itemId,
    required DateTime from,
    required DateTime to,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StoreReport_GetItemMovement',
      method: 'GET',
      path: '/api/app/store/report/item-movement',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        'itemId': itemId,
        'from': hrDateToJson(from),
        'to': hrDateToJson(to),
      },
      cancelToken: cancelToken,
    );
    return StoreItemMovementReportDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/store/report/{code}/export  (Export a store report to PDF or Excel)
  Future<HrBinaryResponse> StoreReport_Export({
    required StoreReportCode code,
    required ReportExportFormat format,
    DateTime? from,
    DateTime? to,
    String? itemId,
    String? teamId,
    String? categoryId,
    String? filter,
    CancelToken? cancelToken,
  }) async {
    final data = await client.sendBinary(
      operationId: 'StoreReport_Export',
      method: 'GET',
      path: '/api/app/store/report/${Uri.encodeComponent(code.toJson())}/export',
      query: <String, dynamic>{
        'format': format.toJson(),
        if (from != null) 'from': hrDateToJson(from),
        if (to != null) 'to': hrDateToJson(to),
        if (itemId != null) 'itemId': itemId,
        if (teamId != null) 'teamId': teamId,
        if (categoryId != null) 'categoryId': categoryId,
        if (filter != null) 'filter': filter,
      },
      cancelToken: cancelToken,
    );
    return data;
  }

  /// GET /api/app/stock-issue  (List stock issue requests)
  Future<StockIssuePagedResultDto> StockIssue_GetList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    StockIssueStatus? status,
    String? itemId,
    String? employeeId,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StockIssue_GetList',
      method: 'GET',
      path: '/api/app/stock-issue',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (status != null) 'status': status.toJson(),
        if (itemId != null) 'itemId': itemId,
        if (employeeId != null) 'employeeId': employeeId,
      },
      cancelToken: cancelToken,
    );
    return StockIssuePagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/stock-issue/{id}  (Get a stock issue request)
  Future<StockIssueDto> StockIssue_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StockIssue_Get',
      method: 'GET',
      path: '/api/app/stock-issue/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return StockIssueDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/stock-issue/{id}/acknowledge  (Mark a stock issue as acknowledged)
  Future<StockIssueDto> StockIssue_Acknowledge({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StockIssue_Acknowledge',
      method: 'POST',
      path: '/api/app/stock-issue/${Uri.encodeComponent(id)}/acknowledge',
      cancelToken: cancelToken,
    );
    return StockIssueDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/stock-issue/{id}/mark-purchasing  (Mark a stock issue as waiting for purchase)
  Future<StockIssueDto> StockIssue_MarkPurchasing({
    required String id,
    NoteInputDto? body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StockIssue_MarkPurchasing',
      method: 'POST',
      path: '/api/app/stock-issue/${Uri.encodeComponent(id)}/mark-purchasing',
      body: body?.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StockIssueDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/stock-issue/{id}/deliver  (Deliver the item)
  Future<StockIssueDto> StockIssue_Deliver({
    required String id,
    String? idempotencyKey,
    StockIssueDeliverInputDto? body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'StockIssue_Deliver',
      method: 'POST',
      path: '/api/app/stock-issue/${Uri.encodeComponent(id)}/deliver',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body?.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return StockIssueDto.fromJson(data as Map<String, dynamic>);
  }

}
