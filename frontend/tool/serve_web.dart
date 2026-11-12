// Serves frontend/build/web on http://localhost:<port> (default 5000) for local testing.
// Dart twin of serve_web.mjs for machines without Node. Unknown paths fall back to index.html
// so go_router deep links work.
import 'dart:io';

const _types = {
  '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript',
  '.json': 'application/json', '.css': 'text/css', '.png': 'image/png',
  '.svg': 'image/svg+xml', '.ico': 'image/x-icon', '.wasm': 'application/wasm',
  '.otf': 'font/otf', '.ttf': 'font/ttf', '.wav': 'audio/wav',
  '.bin': 'application/octet-stream', '.frag': 'text/plain',
};

Future<void> main(List<String> args) async {
  final port = args.isNotEmpty ? int.parse(args.first) : 5000;
  final root = Directory.fromUri(Platform.script.resolve('../build/web/')).absolute.path;
  final server = await HttpServer.bind(InternetAddress.loopbackIPv4, port);
  stdout.writeln('Serving $root on http://localhost:$port');
  await for (final req in server) {
    var rel = Uri.decodeComponent(req.uri.path).replaceAll('/', Platform.pathSeparator);
    var file = File('$root$rel');
    if (!file.absolute.path.startsWith(root) || !file.existsSync()) {
      file = File('${root}index.html');
    }
    final dot = file.path.lastIndexOf('.');
    final ext = dot >= 0 ? file.path.substring(dot) : '';
    req.response.headers
      ..set('Content-Type', _types[ext] ?? 'application/octet-stream')
      ..set('Cache-Control', 'no-store');
    await req.response.addStream(file.openRead());
    await req.response.close();
  }
}
