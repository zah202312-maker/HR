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
      },
      cancelToken: cancelToken,
    );
    return StockMovementPagedResultDto.fromJson(data as Map<String, dynamic>);
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
