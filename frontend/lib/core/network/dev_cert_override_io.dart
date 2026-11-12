import 'dart:io';

import 'package:flutter/foundation.dart';

/// Debug builds only: accept the ASP.NET Core self-signed dev certificate for
/// localhost (physical device via `adb reverse`) and 10.0.2.2 (emulator).
/// Release builds keep full certificate validation.
void installDevCertOverride() {
  if (kDebugMode) HttpOverrides.global = _DevHttpOverrides();
}

class _DevHttpOverrides extends HttpOverrides {
  static const _devHosts = {'localhost', '127.0.0.1', '10.0.2.2'};

  @override
  HttpClient createHttpClient(SecurityContext? context) => super.createHttpClient(context)
    ..badCertificateCallback = (cert, host, port) => _devHosts.contains(host);
}
