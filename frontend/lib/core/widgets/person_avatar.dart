import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../api/generated/hr_api.dart';
import '../network/api_providers.dart';
import '../theme/tokens.dart';

/// The bytes of one profile photo. A photo id never changes content (a new upload is a new id), so each id is read
/// once per session and kept. Null when it cannot be read; the avatar then shows the initial.
final profilePhotoProvider = FutureProvider.family<Uint8List?, String>((ref, photoId) async {
  try {
    final bytes = (await ref.watch(hrApiProvider).currentUser.ProfilePhoto_Get(id: photoId)).bytes;
    return bytes.isEmpty ? null : bytes;
  } catch (_) {
    return null;
  }
});

/// A person's account photo next to their name (decision 2026-11-11), or the first letter of the name when there is
/// none. Mobile and web.
class PersonAvatar extends ConsumerWidget {
  const PersonAvatar({super.key, required this.name, this.photoId, this.radius = 18});

  PersonAvatar.employee(EmployeeRefDto e, {Key? key, double radius = 18}) : this(key: key, name: e.fullName, photoId: e.photoId, radius: radius);

  PersonAvatar.user(UserRefDto u, {Key? key, double radius = 18}) : this(key: key, name: u.displayName, photoId: u.photoId, radius: radius);

  final String name;
  final String? photoId;
  final double radius;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final bytes = photoId == null ? null : ref.watch(profilePhotoProvider(photoId!)).valueOrNull;
    final initial = name.trim().isEmpty ? '?' : name.trim().characters.first;
    return CircleAvatar(
      radius: radius,
      backgroundColor: context.hr.glow,
      foregroundImage: bytes == null ? null : MemoryImage(bytes),
      child: Text(
        initial,
        style: TextStyle(color: context.hr.textPrimary, fontWeight: FontWeight.w700, fontSize: radius * 0.9),
      ),
    );
  }
}

/// The avatar with the name beside it, for rows that show a person.
class PersonName extends StatelessWidget {
  const PersonName({super.key, required this.name, this.photoId, this.radius = 12, this.style});

  PersonName.employee(EmployeeRefDto e, {Key? key, double radius = 12, TextStyle? style})
      : this(key: key, name: e.fullName, photoId: e.photoId, radius: radius, style: style);

  PersonName.user(UserRefDto u, {Key? key, double radius = 12, TextStyle? style})
      : this(key: key, name: u.displayName, photoId: u.photoId, radius: radius, style: style);

  final String name;
  final String? photoId;
  final double radius;
  final TextStyle? style;

  @override
  Widget build(BuildContext context) => Row(mainAxisSize: MainAxisSize.min, children: [
        PersonAvatar(name: name, photoId: photoId, radius: radius),
        const SizedBox(width: 8),
        Flexible(child: Text(name, style: style, overflow: TextOverflow.ellipsis)),
      ]);
}
