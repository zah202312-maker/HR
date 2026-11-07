// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Circulars in the contract.
class CircularsApi {
  CircularsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/circular  (List circulars I received)
  Future<CircularReceivedPagedResultDto> Circular_GetReceivedList({
    bool? isAcknowledged,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Circular_GetReceivedList',
      method: 'GET',
      path: '/api/app/circular',
      query: <String, dynamic>{
        if (isAcknowledged != null) 'isAcknowledged': isAcknowledged,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return CircularReceivedPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/circular  (Send a circular)
  Future<CircularSentDto> Circular_Create({
    String? idempotencyKey,
    required CircularCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Circular_Create',
      method: 'POST',
      path: '/api/app/circular',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return CircularSentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/circular/sent  (List circulars I sent)
  Future<CircularSentPagedResultDto> Circular_GetSentList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Circular_GetSentList',
      method: 'GET',
      path: '/api/app/circular/sent',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return CircularSentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/circular/{id}  (Get a received circular)
  Future<CircularReceivedDto> Circular_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Circular_Get',
      method: 'GET',
      path: '/api/app/circular/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return CircularReceivedDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/circular/{id}/receipt  (List circular acknowledgements)
  Future<CircularReceiptPagedResultDto> Circular_GetReceipts({
    required String id,
    bool? isAcknowledged,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Circular_GetReceipts',
      method: 'GET',
      path: '/api/app/circular/${Uri.encodeComponent(id)}/receipt',
      query: <String, dynamic>{
        if (isAcknowledged != null) 'isAcknowledged': isAcknowledged,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return CircularReceiptPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/circular/{id}/acknowledge  (Acknowledge a circular)
  Future<CircularReceivedDto> Circular_Acknowledge({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Circular_Acknowledge',
      method: 'POST',
      path: '/api/app/circular/${Uri.encodeComponent(id)}/acknowledge',
      cancelToken: cancelToken,
    );
    return CircularReceivedDto.fromJson(data as Map<String, dynamic>);
  }

}
