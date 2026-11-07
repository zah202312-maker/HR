// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged AppreciationLetters in the contract.
class AppreciationLettersApi {
  AppreciationLettersApi(this.client);
  final HrApiClient client;

  /// POST /api/app/appreciation-letter  (Issue an appreciation letter)
  Future<AppreciationLetterDto> AppreciationLetter_Create({
    String? idempotencyKey,
    required AppreciationLetterCreateInputDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_Create',
      method: 'POST',
      path: '/api/app/appreciation-letter',
      headers: <String, dynamic>{
        if (idempotencyKey != null) 'Idempotency-Key': idempotencyKey,
      },
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return AppreciationLetterDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/appreciation-letter  (List appreciation letters)
  Future<AppreciationLetterPagedResultDto> AppreciationLetter_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    String? employeeId,
    bool? isAcknowledged,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_List',
      method: 'GET',
      path: '/api/app/appreciation-letter',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (employeeId != null) 'employeeId': employeeId,
        if (isAcknowledged != null) 'isAcknowledged': isAcknowledged,
      },
      cancelToken: cancelToken,
    );
    return AppreciationLetterPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/appreciation-letter/my  (List my appreciation letters)
  Future<AppreciationLetterPagedResultDto> AppreciationLetter_GetMyList({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_GetMyList',
      method: 'GET',
      path: '/api/app/appreciation-letter/my',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return AppreciationLetterPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/appreciation-letter/employee/{employeeId}  (List letters in an employee file)
  Future<AppreciationLetterPagedResultDto> AppreciationLetter_GetEmployeeFile({
    required String employeeId,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_GetEmployeeFile',
      method: 'GET',
      path: '/api/app/appreciation-letter/employee/${Uri.encodeComponent(employeeId)}',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return AppreciationLetterPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/appreciation-letter/{id}  (Get an appreciation letter)
  Future<AppreciationLetterDto> AppreciationLetter_Get({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_Get',
      method: 'GET',
      path: '/api/app/appreciation-letter/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
    return AppreciationLetterDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/appreciation-letter/{id}/download-url  (Get a letter download URL)
  Future<FileDownloadUrlDto> AppreciationLetter_GetDownloadUrl({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_GetDownloadUrl',
      method: 'GET',
      path: '/api/app/appreciation-letter/${Uri.encodeComponent(id)}/download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/appreciation-letter/{id}/acknowledge  (Acknowledge an appreciation letter)
  Future<AppreciationLetterDto> AppreciationLetter_Acknowledge({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'AppreciationLetter_Acknowledge',
      method: 'POST',
      path: '/api/app/appreciation-letter/${Uri.encodeComponent(id)}/acknowledge',
      cancelToken: cancelToken,
    );
    return AppreciationLetterDto.fromJson(data as Map<String, dynamic>);
  }

}
