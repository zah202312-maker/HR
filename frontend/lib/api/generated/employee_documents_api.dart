// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged EmployeeDocuments in the contract.
class EmployeeDocumentsApi {
  EmployeeDocumentsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/employee/{id}/documents  (List documents of an employee)
  Future<EmployeeDocumentPagedResultDto> EmployeeDocument_List({
    required String id,
    EmployeeDocumentType? type,
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EmployeeDocument_List',
      method: 'GET',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/documents',
      query: <String, dynamic>{
        if (type != null) 'type': type.toJson(),
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
      },
      cancelToken: cancelToken,
    );
    return EmployeeDocumentPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/employee/{id}/documents  (Upload an employee document)
  Future<EmployeeDocumentDto> EmployeeDocument_Upload({
    required String id,
    required EmployeeDocumentUploadDto body,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EmployeeDocument_Upload',
      method: 'POST',
      path: '/api/app/employee/${Uri.encodeComponent(id)}/documents',
      body: body.toFormData(),
      contentType: 'multipart/form-data',
      cancelToken: cancelToken,
    );
    return EmployeeDocumentDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/document/{id}/download-url  (Get a short-lived download link)
  Future<FileDownloadUrlDto> EmployeeDocument_GetDownloadUrl({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'EmployeeDocument_GetDownloadUrl',
      method: 'GET',
      path: '/api/app/document/${Uri.encodeComponent(id)}/download-url',
      cancelToken: cancelToken,
    );
    return FileDownloadUrlDto.fromJson(data as Map<String, dynamic>);
  }

  /// DELETE /api/app/document/{id}  (Remove a document from the employee file)
  Future<void> EmployeeDocument_Delete({
    required String id,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'EmployeeDocument_Delete',
      method: 'DELETE',
      path: '/api/app/document/${Uri.encodeComponent(id)}',
      cancelToken: cancelToken,
    );
  }

}
