import 'package:flutter/material.dart';

import '../../../core/theme/tokens.dart';
import 'org_network.dart';

/// Welcome hero: diagonal brand gradient, chip (company · date), gradient H1, subtitle,
/// one or two actions, and the organizational network beside it (below it on small screens).
class WelcomeHero extends StatelessWidget {
  const WelcomeHero({super.key, required this.chip, required this.title, required this.subtitle, this.actions = const []});
  final String chip;
  final String title;
  final String subtitle;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) {
    final c = context.hr;
    final dark = Theme.of(context).brightness == Brightness.dark;
    final width = MediaQuery.sizeOf(context).width;
    final small = width < 768;
    final twoColumns = width >= 900;

    final h1 = Theme.of(context).textTheme.headlineSmall!.copyWith(
          fontSize: small ? 32 : 36,
          fontWeight: FontWeight.w700,
          height: 1.2,
          color: Colors.white, // replaced by the gradient shader
        );
    final text = Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
          decoration: BoxDecoration(
            color: c.surface.withValues(alpha: dark ? .35 : .7),
            borderRadius: BorderRadius.circular(HrColors.radiusBadge),
            border: Border.all(color: c.accent.withValues(alpha: .35)),
          ),
          child: Row(mainAxisSize: MainAxisSize.min, children: [
            Icon(Icons.apartment_rounded, size: 16, color: c.accent),
            const SizedBox(width: 6),
            Text(chip, style: TextStyle(color: c.textPrimary, fontSize: 13, fontWeight: FontWeight.w600)),
          ]),
        ),
        const SizedBox(height: 16),
        Semantics(
          header: true,
          child: ShaderMask(
            blendMode: BlendMode.srcIn,
            shaderCallback: (r) => LinearGradient(
              begin: AlignmentDirectional.centerStart.resolve(Directionality.of(context)),
              end: AlignmentDirectional.centerEnd.resolve(Directionality.of(context)),
              colors: [c.textPrimary, c.accent, c.textSecondary],
              stops: const [0, .65, 1],
            ).createShader(r),
            child: Text(title, style: h1),
          ),
        ),
        const SizedBox(height: 12),
        Text(subtitle, style: TextStyle(fontSize: 15, height: 1.6, color: c.textSecondary)),
        if (actions.isNotEmpty) ...[
          const SizedBox(height: 20),
          Wrap(spacing: 12, runSpacing: 12, children: actions),
        ],
      ],
    );

    final network = Center(child: OrgNetwork(static: small, maxWidth: small ? 300 : 480));

    final gradient = LinearGradient(
      begin: AlignmentDirectional.topStart,
      end: AlignmentDirectional.bottomEnd, // 135°
      colors: dark
          ? [c.accent.withValues(alpha: .26), const Color(0xB80F172A), HrColors.turquoise.withValues(alpha: .16)]
          : [c.accent.withValues(alpha: .16), c.surface.withValues(alpha: .9), HrColors.turquoise.withValues(alpha: .12)],
      stops: const [0, .55, 1],
    );

    // Glass is optional (brief §4); no backdrop blur so the page always paints on every GPU.
    return ClipRRect(
      borderRadius: BorderRadius.circular(HrColors.radiusCard),
      child: Container(
          decoration: BoxDecoration(
            gradient: gradient,
            borderRadius: BorderRadius.circular(HrColors.radiusCard),
            border: Border.all(color: dark ? Colors.white.withValues(alpha: .08) : c.accent.withValues(alpha: .18)),
          ),
          child: Stack(children: [
            // soft inner highlight along the top edge
            Positioned(
              left: 0,
              right: 0,
              top: 0,
              height: 48,
              child: IgnorePointer(
                child: DecoratedBox(
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      colors: [Colors.white.withValues(alpha: dark ? .06 : .5), Colors.white.withValues(alpha: 0)],
                    ),
                  ),
                ),
              ),
            ),
            Padding(
              padding: EdgeInsets.all(small ? 20 : 32),
              child: twoColumns
                  ? Row(crossAxisAlignment: CrossAxisAlignment.center, children: [
                      Expanded(flex: 105, child: text),
                      const SizedBox(width: 24),
                      Expanded(flex: 95, child: network),
                    ])
                  : Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [text, const SizedBox(height: 24), network]),
            ),
          ]),
      ),
    );
  }
}
