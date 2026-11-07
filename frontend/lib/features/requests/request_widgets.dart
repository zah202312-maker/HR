import 'package:flutter/material.dart';

import '../../api/generated/models.dart';
import '../../core/widgets/common.dart';

Tone toneForStatus(RequestStatus s) => switch (s) {
      RequestStatus.approved || RequestStatus.completed => Tone.success,
      RequestStatus.rejected => Tone.danger,
      RequestStatus.cancelled => Tone.neutral,
      _ => Tone.warning,
    };

Tone toneForEmployeeStatus(RequestEmployeeStatus s) => switch (s) {
      RequestEmployeeStatus.approved || RequestEmployeeStatus.completed => Tone.success,
      RequestEmployeeStatus.rejected => Tone.danger,
      RequestEmployeeStatus.cancelled => Tone.neutral,
      _ => Tone.warning,
    };

IconData iconForStatusTone(Tone t) => switch (t) {
      Tone.success => Icons.check_circle_outline,
      Tone.danger => Icons.cancel_outlined,
      Tone.warning => Icons.hourglass_top,
      _ => Icons.remove_circle_outline,
    };
