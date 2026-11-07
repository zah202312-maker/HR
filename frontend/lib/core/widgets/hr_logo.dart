import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

import '../theme/motion.dart';
import 'motion_widgets.dart';

/// The project mark, from the brand sheets (frontend/public/images/HR-Light.png and HR-Dark.png),
/// with its full geometry and colors. Light theme: the version made for light backgrounds;
/// dark theme: the version made for dark backgrounds. Never recolored, stretched or rotated.
class HrMark extends StatelessWidget {
  const HrMark({super.key, required this.height, this.semanticLabel = 'HR'});
  final double height;
  final String? semanticLabel;

  static const onLight = 'assets/images/hr_logo_on_light.png';
  static const onDark = 'assets/images/hr_logo_on_dark.png';

  @override
  Widget build(BuildContext context) {
    final dark = Theme.of(context).brightness == Brightness.dark;
    return Image.asset(
      dark ? onDark : onLight,
      height: height,
      fit: BoxFit.contain,
      filterQuality: FilterQuality.high,
      semanticLabel: semanticLabel,
      excludeFromSemantics: semanticLabel == null,
    );
  }
}

/// Mark + "HR" wordmark. The symbol alone is used where space is small (collapsed sidebar).
/// [clearSpace] keeps at least 25% of the mark height free on every side.
class HrLogo extends StatelessWidget {
  const HrLogo({super.key, this.size = 36, this.showWordmark = true, this.clearSpace = true, this.animateIn = false});

  final double size;
  final bool showWordmark;
  final bool clearSpace;

  /// One light entrance (opacity + shift), then the logo stays still.
  final bool animateIn;

  @override
  Widget build(BuildContext context) {
    final color = Theme.of(context).colorScheme.onSurface;
    Widget logo = Row(
      mainAxisSize: MainAxisSize.min,
      textDirection: TextDirection.ltr,
      children: [
        HrMark(height: size),
        if (showWordmark) ...[
          SizedBox(width: size * .3),
          Text('HR',
              style: GoogleFonts.inter(fontSize: size * .62, fontWeight: FontWeight.w700, letterSpacing: 1.5, color: color)),
        ],
      ],
    );
    if (clearSpace) logo = Padding(padding: EdgeInsets.all(size * .25), child: logo);
    if (animateIn) {
      logo = FadeSlideIn(duration: HrMotion.pageIn, offset: const Offset(0, 8), child: logo);
    }
    return logo;
  }
}
