import 'package:file_picker/file_picker.dart';

import '../../api/runtime/api_client.dart';

/// Mobile/desktop: opens the platform save dialog with the file bytes.
Future<void> saveBinary(HrBinaryResponse file, {String fallbackName = 'file'}) async {
  await FilePicker.platform.saveFile(
    fileName: file.fileName ?? fallbackName,
    bytes: file.bytes,
  );
}
