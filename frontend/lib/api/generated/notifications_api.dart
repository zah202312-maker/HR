// GENERATED CODE - DO NOT MODIFY BY HAND.
// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py
// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion

import 'package:dio/dio.dart';

import '../runtime/api_client.dart';
import '../runtime/json.dart';
import 'models.dart';

/// Operations tagged Notifications in the contract.
class NotificationsApi {
  NotificationsApi(this.client);
  final HrApiClient client;

  /// GET /api/app/notification  (List my notifications)
  Future<NotificationPagedResultDto> Notification_List({
    int? skipCount,
    int? maxResultCount,
    String? sorting,
    bool? isRead,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notification_List',
      method: 'GET',
      path: '/api/app/notification',
      query: <String, dynamic>{
        if (skipCount != null) 'skipCount': skipCount,
        if (maxResultCount != null) 'maxResultCount': maxResultCount,
        if (sorting != null) 'sorting': sorting,
        if (isRead != null) 'isRead': isRead,
      },
      cancelToken: cancelToken,
    );
    return NotificationPagedResultDto.fromJson(data as Map<String, dynamic>);
  }

  /// GET /api/app/notification/unread-count  (Get my unread notification count)
  Future<NotificationUnreadCountDto> Notification_GetUnreadCount({
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notification_GetUnreadCount',
      method: 'GET',
      path: '/api/app/notification/unread-count',
      cancelToken: cancelToken,
    );
    return NotificationUnreadCountDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/notification/{id}/read  (Mark a notification as read)
  Future<NotificationDto> Notification_MarkRead({
    required String id,
    CancelToken? cancelToken,
  }) async {
    final data = await client.send(
      operationId: 'Notification_MarkRead',
      method: 'POST',
      path: '/api/app/notification/${Uri.encodeComponent(id)}/read',
      cancelToken: cancelToken,
    );
    return NotificationDto.fromJson(data as Map<String, dynamic>);
  }

  /// POST /api/app/device-token  (Register a device token)
  Future<void> DeviceToken_Register({
    required DeviceTokenRegisterInputDto body,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'DeviceToken_Register',
      method: 'POST',
      path: '/api/app/device-token',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
  }

  /// POST /api/app/device-token/unregister  (Unregister a device token)
  Future<void> DeviceToken_Unregister({
    required DeviceTokenUnregisterInputDto body,
    CancelToken? cancelToken,
  }) async {
    await client.send(
      operationId: 'DeviceToken_Unregister',
      method: 'POST',
      path: '/api/app/device-token/unregister',
      body: body.toJson(),
      contentType: 'application/json',
      cancelToken: cancelToken,
    );
  }

}
