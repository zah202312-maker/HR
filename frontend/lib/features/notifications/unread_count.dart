import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';

/// Unread badge. Polls every minute; the live SignalR hub is not part of the contract.
final unreadCountProvider = StreamProvider.autoDispose<int>((ref) async* {
  final session = ref.watch(sessionProvider);
  if (!session.can(HrOps.Notification_GetUnreadCount)) {
    yield 0;
    return;
  }
  final api = ref.watch(hrApiProvider);
  while (true) {
    try {
      yield (await api.notifications.Notification_GetUnreadCount()).count;
    } catch (_) {
      // keep the last value
    }
    await Future<void>.delayed(const Duration(seconds: 60));
  }
});
