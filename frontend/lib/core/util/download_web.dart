import 'dart:js_interop';

import 'package:web/web.dart' as web;

import '../../api/runtime/api_client.dart';

/// Web: triggers a browser download of the bytes (no FilePicker on web).
Future<void> saveBinary(HrBinaryResponse file, {String fallbackName = 'file'}) async {
  final blob = web.Blob(
    [file.bytes.toJS].toJS,
    web.BlobPropertyBag(type: file.contentType ?? 'application/octet-stream'),
  );
  final url = web.URL.createObjectURL(blob);
  final anchor = web.HTMLAnchorElement()
    ..href = url
    ..download = file.fileName ?? fallbackName
    ..style.display = 'none';
  web.document.body?.appendChild(anchor);
  anchor.click();
  anchor.remove();
  // Give the browser time to start the download before releasing the blob.
  Future<void>.delayed(const Duration(seconds: 2), () => web.URL.revokeObjectURL(url));
}
