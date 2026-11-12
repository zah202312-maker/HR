import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/date_symbol_data_local.dart';

import 'app/app.dart';
import 'core/network/dev_cert_override.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  installDevCertOverride();
  await initializeDateFormatting('ar');
  await initializeDateFormatting('en');
  runApp(const ProviderScope(child: HrApp()));
}
