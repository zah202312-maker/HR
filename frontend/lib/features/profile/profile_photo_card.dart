import 'dart:typed_data';

import 'package:dio/dio.dart' show DioMediaType, MultipartFile;
import 'package:file_picker/file_picker.dart';
import 'package:flutter/foundation.dart' show compute;
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:image/image.dart' as img;

import '../../api/generated/hr_api.dart';
import '../../core/l10n/strings.dart';
import '../../core/network/api_providers.dart';
import '../../core/session/session_controller.dart';
import '../../core/widgets/common.dart';
import '../../core/widgets/person_avatar.dart';
import '../../core/widgets/pickers.dart';

/// The longest side of an uploaded photo. The server accepts up to 1 MB; a 512 px JPEG is far below that.
const _photoSize = 512;

/// Square-crops from the centre, shrinks to [_photoSize] and re-encodes as JPEG. Null when the bytes are not an image.
Uint8List? _shrink(Uint8List bytes) {
  final decoded = img.decodeImage(bytes);
  if (decoded == null) return null;
  final upright = img.bakeOrientation(decoded);
  final side = upright.width < upright.height ? upright.width : upright.height;
  final square = img.copyCrop(upright, x: (upright.width - side) ~/ 2, y: (upright.height - side) ~/ 2, width: side, height: side);
  final small = side > _photoSize ? img.copyResize(square, width: _photoSize, height: _photoSize, interpolation: img.Interpolation.average) : square;
  return Uint8List.fromList(img.encodeJpg(small, quality: 85));
}

/// «صورتي» on the account screen (decision 2026-11-11): only the owner changes or removes their photo. It then shows
/// next to their name everywhere. Mobile and web.
class ProfilePhotoCard extends ConsumerStatefulWidget {
  const ProfilePhotoCard({super.key});

  @override
  ConsumerState<ProfilePhotoCard> createState() => _ProfilePhotoCardState();
}

class _ProfilePhotoCardState extends ConsumerState<ProfilePhotoCard> {
  bool _busy = false;

  Future<void> _change() async {
    final s = S.of(context);
    final picked = await FilePicker.platform.pickFiles(withData: true, type: FileType.image);
    final file = picked?.files.single;
    if (file?.bytes == null || !mounted) return;

    setState(() => _busy = true);
    try {
      final small = await compute(_shrink, file!.bytes!);
      if (!mounted) return;
      if (small == null) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(s.t('الملف ليس صورة.', 'The file is not an image.'))));
        return;
      }

      await runAction(context, () async {
        await ref.read(hrApiProvider).currentUser.Me_UploadPhoto(
              body: MyPhotoUploadDto(file: MultipartFile.fromBytes(small, filename: 'photo.jpg', contentType: DioMediaType('image', 'jpeg'))),
            );
        await ref.read(sessionProvider.notifier).reload();
      }, done: s.t('تم تحديث الصورة', 'Photo updated'));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  Future<void> _remove() async {
    final s = S.of(context);
    if (!await confirmAction(context, title: s.t('حذف الصورة', 'Remove photo'), message: s.t('ترجع الأحرف بدل الصورة.', 'Your initial shows instead.'), destructive: true)) {
      return;
    }
    if (!mounted) return;
    setState(() => _busy = true);
    try {
      await runAction(context, () async {
        await ref.read(hrApiProvider).currentUser.Me_DeletePhoto();
        await ref.read(sessionProvider.notifier).reload();
      }, done: s.t('تم حذف الصورة', 'Photo removed'));
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final s = S.of(context);
    final me = ref.watch(sessionProvider).me;
    if (me == null) return const SizedBox.shrink();
    return HrCard(
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        SectionTitle(s.t('صورتي', 'My photo')),
        Row(children: [
          PersonAvatar(name: me.displayName, photoId: me.photoId, radius: 40),
          const SizedBox(width: 16),
          Expanded(
            child: Wrap(spacing: 8, runSpacing: 8, children: [
              FilledButton.icon(
                onPressed: _busy ? null : _change,
                icon: const Icon(Icons.photo_camera_outlined),
                label: Text(me.photoId == null ? s.t('إضافة صورة', 'Add photo') : s.t('تغيير الصورة', 'Change photo')),
              ),
              if (me.photoId != null)
                OutlinedButton.icon(
                  onPressed: _busy ? null : _remove,
                  icon: const Icon(Icons.delete_outline),
                  label: Text(s.t('حذف', 'Remove')),
                ),
            ]),
          ),
        ]),
        const SizedBox(height: 8),
        Text(
          s.t('تظهر صورتك بجانب اسمك لكل من يرى اسمك في النظام.', 'Your photo shows next to your name to everyone who sees your name.'),
          style: Theme.of(context).textTheme.labelSmall,
        ),
      ]),
    );
  }
}
