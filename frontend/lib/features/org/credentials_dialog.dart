import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../../api/generated/models.dart';
import '../../core/l10n/strings.dart';
import '../../core/theme/tokens.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/motion_widgets.dart';

/// Shows the temporary credentials once (TemporaryCredentialsDto). HR hands them over;
/// the user must change the password at first sign-in.
Future<void> showCredentialsDialog(BuildContext context, TemporaryCredentialsDto c) {
  final s = S.of(context);
  return showHrDialog<void>(
    context: context,
    barrierDismissible: false,
    builder: (ctx) => AlertDialog(
      title: Text(s.t('بيانات الدخول المؤقتة', 'Temporary sign-in details')),
      content: SizedBox(
        width: 440,
        child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
          Text(
            s.t('تظهر مرة واحدة فقط. سلّمها للموظف، وسيُطلب منه تغيير كلمة السر عند أول دخول.',
                'Shown once only. Hand them to the employee, who must change the password at first sign-in.'),
            style: const TextStyle(color: HrColors.warning),
          ),
          const SizedBox(height: 12),
          Directionality(
            textDirection: TextDirection.ltr,
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              InfoRow(s.t('اسم المستخدم', 'User name'), c.userName),
              InfoRow(s.t('كلمة السر المؤقتة', 'Temporary password'), c.temporaryPassword),
            ]),
          ),
        ]),
      ),
      actions: [
        TextButton.icon(
          onPressed: () => Clipboard.setData(ClipboardData(text: '${c.userName}\n${c.temporaryPassword}')),
          icon: const Icon(Icons.copy),
          label: Text(s.t('نسخ', 'Copy')),
        ),
        FilledButton(onPressed: () => Navigator.pop(ctx), child: Text(s.done)),
      ],
    ),
  );
}
