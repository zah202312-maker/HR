import 'dart:async';

import 'package:audioplayers/audioplayers.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';

/// How often the unread count is read. The live SignalR hub is not part of the contract, so a new notification
/// is noticed on the next read (decision 2026-11-11: every 20 seconds, so the sound comes soon after it arrives).
const unreadPollInterval = Duration(seconds: 20);

/// Unread badge, read every [unreadPollInterval].
final unreadCountProvider = StreamNotifierProvider.autoDispose<UnreadCount, int>(UnreadCount.new);

class UnreadCount extends AutoDisposeStreamNotifier<int> {
  @override
  Stream<int> build() async* {
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
      await Future<void>.delayed(unreadPollInterval);
    }
  }

  /// A notification was just marked read (decision 2026-11-11: the number goes down at once with each tap), then the
  /// server's count is read again so the badge never drifts from it.
  void markedOneRead() {
    final current = state.valueOrNull;
    if (current != null && current > 0) state = AsyncData(current - 1);
    ref.invalidateSelf();
  }
}

/// Plays a short chime when a new notification arrives (decision 2026-11-11: every new notification, so circulars,
/// notices and reminders get noticed). Mobile and web. Only while the app is open: a closed app needs push (Q8).
class NotificationSound {
  NotificationSound._();

  static final AudioPlayer _player = AudioPlayer()..setReleaseMode(ReleaseMode.stop);

  static Future<void> play() async {
    try {
      await _player.stop();
      await _player.play(AssetSource('sounds/notification.wav'), volume: 1.0);
    } catch (_) {
      // A browser that blocks sound before the first click, or a device with no audio, must not break the app.
    }
  }

  /// Chimes when the count goes up between two reads. The first read after sign-in and a drop after reading
  /// notifications stay silent.
  static void onUnreadChanged(AsyncValue<int>? previous, AsyncValue<int> next) {
    final before = previous?.valueOrNull;
    final after = next.valueOrNull;
    if (before != null && after != null && after > before) {
      play();
    }
  }
}
