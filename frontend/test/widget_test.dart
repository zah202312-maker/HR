import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/core/l10n/strings.dart';
import 'package:hr_app/core/widgets/common.dart';

void main() {
  testWidgets('StatusPill shows its label (status never by colour alone)', (tester) async {
    await tester.pumpWidget(const MaterialApp(
      home: Scaffold(body: StatusPill('موافق عليه', tone: Tone.success)),
    ));
    expect(find.text('موافق عليه'), findsOneWidget);
  });

  test('Arabic is the default language of the strings', () {
    expect(const S(true).signIn, 'تسجيل الدخول');
    expect(const S(false).signIn, 'Sign in');
  });
}
