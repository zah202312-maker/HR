// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged ReviewRooms in the contract.
class ReviewRoomsApi {
  ReviewRoomsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/review-room/candidates  (List department managers and their linked employees for building rooms)
  Future<List<ReviewRoomCandidateDto>> ReviewRoom_GetCandidates({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewRoom_GetCandidates',
      method: 'GET',
      path: '/api/app/review-room/candidates',
      cancelToken: cancelToken,
    );
    return (data as List).map((e) => ReviewRoomCandidateDto.fromJson((e) as Map<String, dynamic>)).toList();
  }

  /// GET /api/app/review-room  (List the reviewer's rooms)
  Future<ReviewRoomPagedResultDto> ReviewRoom_GetList({
    int? skipCount,
    int? maxResultCount,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewRoom_GetList',
      method: 'GET',
      path: '/api/app/review-room',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
      },
      cancelToken: cancelToken,
    );
    return ReviewRoomPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/review-room  (Create a room)
  Future<ReviewRoomDto> ReviewRoom_Create({
    required ReviewRoomCreateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewRoom_Create',
      method: 'POST',
      path: '/api/app/review-room',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewRoomDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/review-room/{id}  (Get a room)
  Future<ReviewRoomDto> ReviewRoom_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewRoom_Get',
      method: 'GET',
      path: '/api/app/review-room/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return ReviewRoomDto.fromJson(data as Map<String, dynamic>);
  }

  /// PUT /api/app/review-room/{id}  (Update a room)
  Future<ReviewRoomDto> ReviewRoom_Update({
    required String id,
    required ReviewRoomUpdateDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'ReviewRoom_Update',
      method: 'PUT',
      path: '/api/app/review-room/${Uri.encodeComponent(id)}',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
    return ReviewRoomDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/review-room/{id}  (Delete a room)
  Future<void> ReviewRoom_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'ReviewRoom_Delete',
      method: 'DELETE',
      path: '/api/app/review-room/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

}
