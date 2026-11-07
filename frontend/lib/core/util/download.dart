// Saves a binary response (export, attachment).
// Web: a real browser download (Blob + <a download>).
// Mobile/desktop: the platform "save as" dialog via file_picker.
export 'download_io.dart' if (dart.library.js_interop) 'download_web.dart';
