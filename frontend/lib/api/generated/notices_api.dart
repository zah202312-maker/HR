// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Notices in the contract.
class NoticesApi {
  NoticesApi(this.client);
  final HrApiClient client;

  /// GET /api/app/notice  (List notices I received)
  Future<NoticeReceivedPagedResultDto> Notice_GetReceivedList({
    bool? isAcknowledged,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notice_GetReceivedList',
      method: 'GET',
      path: '/api/app/notice',
      query: <String, dynamic>{
        if (isAcknowledged != null) 'isAcknowledged': isAcknowledged,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return NoticeReceivedPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/notice  (Send a notice to chosen employees)
  Future<NoticeSentDto> Notice_Create({
    String? idempotencyKey,
    required NoticeCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notice_Create',
      method: 'POST',
      path: '/api/app/notice',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return NoticeSentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/notice/sent  (List notices I sent)
  Future<NoticeSentPagedResultDto> Notice_GetSentList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notice_GetSentList',
      method: 'GET',
      path: '/api/app/notice/sent',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return NoticeSentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/notice/{id}  (Get a received notice)
  Future<NoticeReceivedDto> Notice_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notice_Get',
      method: 'GET',
      path: '/api/app/notice/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return NoticeReceivedDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/notice/{id}/receipt  (List notice acknowledgements)
  Future<NoticeReceiptPagedResultDto> Notice_GetReceipts({
    required String id,
    bool? isAcknowledged,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notice_GetReceipts',
      method: 'GET',
      path: '/api/app/notice/${Uri.encodeComponent(id)}/receipt',
      query: <String, dynamic>{
        if (isAcknowledged != null) 'isAcknowledged': isAcknowledged,
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return NoticeReceiptPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/notice/{id}/acknowledge  (Acknowledge a notice)
  Future<NoticeReceivedDto> Notice_Acknowledge({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notice_Acknowledge',
      method: 'POST',
      path: '/api/app/notice/${Uri.encodeComponent(id)}/acknowledge',
      cancelToken: cancelToken,
    );
    return NoticeReceivedDto.fromJson(data as Map<String, dynamic>);
  }

}
