import 'dart:async';

/// Events raised by the transport layer and handled by the session controller.
enum SessionSignal { signedOut, passwordChangeRequired, mfaEnrollmentRequired }

class SessionSignals {
  final _controller = StreamController<SessionSignal>.broadcast();
  Stream<SessionSignal> get stream => _controller.stream;
  void emit(SessionSignal signal) => _controller.add(signal);
}
