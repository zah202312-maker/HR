// Generates the Dart API client from api-contract/openapi.yaml. A line-for-line port of generate_api.py,
// for machines without Python. Same output.
//
// Run from the repository root:  node frontend/tool/generate_api.mjs <dir containing node_modules/js-yaml>
// (or install js-yaml next to this file). Output: see generate_api.py.
import fs from 'fs';
import path from 'path';
import { createRequire } from 'module';
import { fileURLToPath } from 'url';

const here = path.dirname(fileURLToPath(import.meta.url));
const ROOT = path.resolve(here, '..', '..');
const CONTRACT = path.join(ROOT, 'api-contract', 'openapi.yaml');
const OUT = path.join(ROOT, 'frontend', 'lib', 'api', 'generated');
const deps = process.argv[2] ?? here;
const yaml = createRequire(path.join(deps, 'package.json'))('js-yaml');

const RESERVED = new Set(`assert break case catch class const continue default do else enum extends false
final finally for if in is new null rethrow return super switch this throw true try var void while
with`.split(/\s+/));
const METHODS = ['get', 'post', 'put', 'patch', 'delete'];
const HEADER = '// GENERATED CODE - DO NOT MODIFY BY HAND.\n'
  + '// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py\n'
  + '// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion\n\n';

const camel = (name) => {
  const parts = String(name).split(/[_\-\s]+/);
  return parts[0].slice(0, 1).toLowerCase() + parts[0].slice(1) + parts.slice(1).map((p) => p.slice(0, 1).toUpperCase() + p.slice(1)).join('');
};
const ident = (name) => { const n = camel(name); return RESERVED.has(n) ? n + '_' : n; };
const snake = (name) => name.replace(/(?<!^)(?=[A-Z])/g, '_').toLowerCase();
const dstr = (s) => "'" + String(s).replace(/\\/g, '\\\\').replace(/'/g, "\\'").replace(/\$/g, '\\$').replace(/\n/g, '\\n') + "'";
const doc = (text, indent) => {
  if (!text) return '';
  text = String(text).split(/\s+/).filter(Boolean).join(' ');
  if ([...text].length > 400) text = [...text].slice(0, 400).join('') + '...';
  return `${indent}/// ${text}\n`;
};
const refName = (ref) => ref.split('/').pop();

class Gen {
  constructor(spec) {
    this.spec = spec;
    this.schemas = spec.components.schemas;
    this.params = spec.components.parameters ?? {};
  }

  unwrap(s) {
    if (s.allOf && s.allOf.length === 1 && s.allOf[0].$ref) return [{ $ref: s.allOf[0].$ref }, !!s.nullable];
    return [s, !!s.nullable];
  }

  isEnum(name) { return 'enum' in this.schemas[name]; }

  /** [dartType, fromJson(expr), toJson(expr)] for a non-null value. */
  typeOf(s0) {
    const [s] = this.unwrap(s0);
    if (s.$ref) {
      const n = refName(s.$ref);
      if (this.isEnum(n)) return [n, (e) => `${n}.fromJson(${e})`, (e) => `${e}.toJson()`];
      return [n, (e) => `${n}.fromJson((${e}) as Map<String, dynamic>)`, (e) => `${e}.toJson()`];
    }
    const t = s.type, f = s.format;
    const id = (e) => e;
    if ('enum' in s && t === 'string') return ['String', (e) => `(${e}) as String`, id];
    if (t === 'string') {
      if (f === 'date-time') return ['DateTime', (e) => `DateTime.parse((${e}) as String)`, (e) => `hrDateTimeToJson(${e})`];
      if (f === 'date') return ['DateTime', (e) => `hrDateFromJson((${e}) as String)`, (e) => `hrDateToJson(${e})`];
      if (f === 'binary') return ['MultipartFile', (e) => `(${e}) as MultipartFile`, id];
      return ['String', (e) => `(${e}) as String`, id];
    }
    if (t === 'integer') return ['int', (e) => `((${e}) as num).toInt()`, id];
    if (t === 'number') return ['double', (e) => `((${e}) as num).toDouble()`, id];
    if (t === 'boolean') return ['bool', (e) => `(${e}) as bool`, id];
    if (t === 'array') {
      const [it, ifr, ito] = this.typeOf(s.items ?? {});
      const itoE = ito('e');
      return [`List<${it}>`, (e) => `((${e}) as List).map((e) => ${ifr('e')}).toList()`, itoE === 'e' ? id : (e) => `${e}.map((e) => ${itoE}).toList()`];
    }
    if (t === 'object' || t === undefined) {
      const ap = s.additionalProperties;
      if (ap && typeof ap === 'object' && Object.keys(ap).length) {
        const [vt, vfr, vto] = this.typeOf(ap);
        const vnull = !!ap.nullable;
        let vfrV = vfr('v');
        if (vnull) vfrV = `v == null ? null : ${vfrV}`;
        const vtoV = vto('v');
        return [`Map<String, ${vt}${vnull ? '?' : ''}>`, (e) => `((${e}) as Map).map((k, v) => MapEntry(k as String, ${vfrV}))`, vtoV === 'v' ? id : (e) => `${e}.map((k, v) => MapEntry(k, ${vtoV}))`];
      }
      return ['Map<String, dynamic>', (e) => `((${e}) as Map).cast<String, dynamic>()`, id];
    }
    return ['Object', (e) => `(${e}) as Object`, id];
  }

  genEnum(name, s) {
    let out = doc(s.description, '');
    out += `enum ${name} {\n`;
    for (const v of s.enum) out += `  ${ident(String(v))}(${dstr(v)}),\n`;
    out += "\n  /// A value this client does not know yet (newer server).\n  $unknown('');\n\n";
    out += `  const ${name}(this.value);\n  final String value;\n\n`;
    out += `  static ${name} fromJson(Object? json) {\n`;
    out += `    for (final v in ${name}.values) {\n      if (v.value == json) return v;\n    }\n`;
    out += `    return ${name}.$unknown;\n  }\n\n`;
    out += '  String toJson() => value;\n}\n\n';
    return out;
  }

  fields(s) {
    const req = new Set(s.required ?? []);
    return Object.entries(s.properties ?? {}).map(([k, p]) => {
      const [inner, nullable] = this.unwrap(p);
      const [t, fr, to] = this.typeOf(p);
      return { json: k, name: ident(k), type: t, fr, to, required: req.has(k), optional: nullable || !req.has(k), desc: p.description, schema: inner };
    });
  }

  genClass(name, s, multipart) {
    const fs_ = this.fields(s);
    let out = doc(s.description, '');
    out += `class ${name} {\n`;
    if (fs_.length) {
      out += `  const ${name}({\n`;
      for (const f of fs_) out += `    ${f.optional ? '' : 'required '}this.${f.name},\n`;
      out += '  });\n\n';
    } else {
      out += `  const ${name}();\n\n`;
    }
    for (const f of fs_) {
      out += doc(f.desc, '  ');
      out += `  final ${f.type}${f.optional ? '?' : ''} ${f.name};\n`;
    }
    const hasFile = fs_.some((f) => f.type.includes('MultipartFile'));
    if (!hasFile) {
      out += `\n  factory ${name}.fromJson(Map<String, dynamic> json) => ${name}(\n`;
      for (const f of fs_) {
        const acc = `json[${dstr(f.json)}]`;
        let conv = f.fr(acc);
        if (f.optional) conv = `${acc} == null ? null : ${conv}`;
        out += `        ${f.name}: ${conv},\n`;
      }
      out += '      );\n';
    }
    out += '\n';
    out += '  Map<String, dynamic> toJson() => <String, dynamic>{\n';
    for (const f of fs_) {
      if (f.type === 'MultipartFile' || f.type === 'List<MultipartFile>') continue;
      const v = f.name + (f.optional ? '!' : '');
      const conv = f.to(v);
      if (!f.optional) out += `        ${dstr(f.json)}: ${conv},\n`;
      else if (f.required) out += `        ${dstr(f.json)}: ${f.name} == null ? null : ${conv},\n`;
      else out += `        if (${f.name} != null) ${dstr(f.json)}: ${conv},\n`;
    }
    out += '      };\n';
    if (multipart) {
      out += '\n  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,\n';
      out += '  /// one part per file for arrays (x-server-requirements.multipart).\n';
      out += '  FormData toFormData() {\n    final form = FormData();\n';
      for (const f of fs_) {
        const n = f.name;
        const v = n + (f.optional ? '!' : '');
        const guard = f.optional ? `if (${n} != null) ` : '';
        if (f.type === 'MultipartFile') out += `    ${guard}form.files.add(MapEntry(${dstr(f.json)}, ${v}));\n`;
        else if (f.type === 'List<MultipartFile>') out += `    ${guard}for (final file in ${v}) {\n      form.files.add(MapEntry(${dstr(f.json)}, file));\n    }\n`;
        else if (f.type.startsWith('List<')) out += `    ${guard}for (final item in (${f.to(v)} as List)) {\n      form.fields.add(MapEntry(${dstr(f.json)}, hrFormValue(item)));\n    }\n`;
        else out += `    ${guard}form.fields.add(MapEntry(${dstr(f.json)}, hrFormValue(${f.to(v)})));\n`;
      }
      out += '    return form;\n  }\n';
    }
    out += '}\n\n';
    return out;
  }

  multipartSchemas() {
    const res = new Set();
    for (const [, o] of this.operations()) {
      const c = (o.requestBody ?? {}).content ?? {};
      if (c['multipart/form-data']) res.add(refName(c['multipart/form-data'].schema.$ref));
    }
    return res;
  }

  genModels() {
    const mp = this.multipartSchemas();
    let out = HEADER + "import 'package:dio/dio.dart';\n\nimport '../runtime/json.dart';\n\n";
    for (const [name, s] of Object.entries(this.schemas)) out += 'enum' in s ? this.genEnum(name, s) : this.genClass(name, s, mp.has(name));
    return out;
  }

  *operationsWithPath() {
    for (const [p, ops] of Object.entries(this.spec.paths)) for (const [m, o] of Object.entries(ops)) if (METHODS.includes(m)) yield [p, m, o];
  }

  *operations() { for (const [p, m, o] of this.operationsWithPath()) yield [m, o, p]; }

  resolveParam(p) { return p.$ref ? this.params[refName(p.$ref)] : p; }

  paramValue(schema, expr) {
    const [t, , to] = this.typeOf(schema);
    if (t === 'DateTime' || this.unwrap(schema)[0].$ref) return to(expr);
    return expr;
  }

  genMethod(p, method, o) {
    const op = o.operationId;
    const args = [], query = [], headers = [];
    let pathExpr = p;
    const security = o.security;
    for (const raw of o.parameters ?? []) {
      const prm = this.resolveParam(raw);
      if (prm.in === 'header' && prm.name === 'Accept-Language') continue;
      const [t] = this.typeOf(prm.schema ?? {});
      const name = ident(prm.name);
      const req = prm.required ?? false;
      if (prm.in === 'path') {
        args.push(`required ${t} ${name}`);
        let pv = this.paramValue(prm.schema ?? {}, name);
        if (t === 'int') pv = `${name}.toString()`;
        pathExpr = pathExpr.replace('{' + prm.name + '}', '${Uri.encodeComponent(' + pv + ')}');
      } else if (prm.in === 'query') {
        args.push(`${req ? 'required ' : ''}${t}${req ? '' : '?'} ${name}`);
        const val = this.paramValue(prm.schema ?? {}, name);
        query.push(req ? `      ${dstr(prm.name)}: ${val},` : `      if (${name} != null) ${dstr(prm.name)}: ${val},`);
      } else if (prm.in === 'header') {
        args.push(`${req ? 'required ' : ''}${t}${req ? '' : '?'} ${name}`);
        headers.push(req ? `      ${dstr(prm.name)}: ${name},` : `      if (${name} != null) ${dstr(prm.name)}: ${name},`);
      }
    }
    if (security && security.some((r) => 'deviceKey' in r)) {
      args.push('required String deviceKey');
      headers.push("      'X-Device-Key': deviceKey,");
    }
    let bodyExpr = 'null', contentType = null;
    const rb = o.requestBody;
    if (rb) {
      const [ct, c] = Object.entries(rb.content)[0];
      const n = refName(c.schema.$ref);
      const required = rb.required ?? true;
      args.push(`${required ? 'required ' : ''}${n}${required ? '' : '?'} body`);
      if (ct === 'multipart/form-data') { bodyExpr = 'body.toFormData()'; contentType = 'multipart/form-data'; }
      else { bodyExpr = required ? 'body.toJson()' : 'body?.toJson()'; contentType = ct; }
    }
    let ret = 'void', conv = null, binary = false;
    for (const code of Object.keys(o.responses).sort()) {
      if (!String(code).startsWith('2')) continue;
      const content = o.responses[code].content ?? {};
      if (!Object.keys(content).length) continue;
      const [, c] = Object.entries(content)[0];
      const sc = c.schema ?? {};
      if (sc.format === 'binary') { ret = 'HrBinaryResponse'; binary = true; }
      else if (sc.$ref) { ret = refName(sc.$ref); conv = `${ret}.fromJson(data as Map<String, dynamic>)`; }
      // Extension over generate_api.py: an array of a schema returns a typed list instead of void.
      else if (sc.type === 'array' && sc.items?.$ref) { const it = refName(sc.items.$ref); ret = `List<${it}>`; conv = `(data as List).map((e) => ${it}.fromJson((e) as Map<String, dynamic>)).toList()`; }
      break;
    }
    args.push('CancelToken? cancelToken');
    const skipAuth = Array.isArray(security) && security.length === 0;
    let out = `  /// ${method.toUpperCase()} ${p}  (${o.summary ?? ''})\n`;
    out += `  Future<${ret}> ${op}({\n`;
    for (const a of args) out += `    ${a},\n`;
    out += '  }) async {\n';
    out += (ret === 'void' ? '    await client.' : '    final data = await client.') + `${binary ? 'sendBinary' : 'send'}(\n`;
    out += `      operationId: ${dstr(op)},\n`;
    out += `      method: ${dstr(method.toUpperCase())},\n`;
    out += `      path: '${pathExpr}',\n`;
    if (query.length) out += '      query: <String, dynamic>{\n' + query.map((q) => '  ' + q).join('\n') + '\n      },\n';
    if (headers.length) out += '      headers: <String, dynamic>{\n' + headers.map((h) => '  ' + h).join('\n') + '\n      },\n';
    if (bodyExpr !== 'null') out += `      body: ${bodyExpr},\n`;
    if (contentType) out += `      contentType: ${dstr(contentType)},\n`;
    if (skipAuth) out += '      skipAuth: true,\n';
    out += '      cancelToken: cancelToken,\n    );\n';
    if (binary) out += '    return data;\n';
    else if (conv) out += `    return ${conv};\n`;
    out += '  }\n\n';
    return out;
  }

  genApis() {
    const byTag = new Map();
    for (const [p, m, o] of this.operationsWithPath()) {
      const tag = o.tags[0];
      if (!byTag.has(tag)) byTag.set(tag, []);
      byTag.get(tag).push([p, m, o]);
    }
    const files = {};
    for (const [tag, ops] of byTag) {
      const cls = tag + 'Api';
      let out = HEADER + "import 'package:dio/dio.dart';\n\nimport '../runtime/api_client.dart';\nimport '../runtime/json.dart';\nimport 'models.dart';\n\n";
      out += `/// Operations tagged ${tag} in the contract.\nclass ${cls} {\n  ${cls}(this.client);\n  final HrApiClient client;\n\n`;
      for (const [p, m, o] of ops) out += this.genMethod(p, m, o);
      out += '}\n';
      files[snake(tag) + '_api.dart'] = out;
    }
    let agg = HEADER + "import '../runtime/api_client.dart';\n";
    for (const tag of byTag.keys()) agg += `import '${snake(tag)}_api.dart';\n`;
    for (const tag of byTag.keys()) agg += `export '${snake(tag)}_api.dart';\n`;
    agg += "export 'models.dart';\nexport 'operations.dart';\n\n";
    agg += '/// Every API of the contract, one property per tag.\nclass HrApi {\n  HrApi(this.client);\n  final HrApiClient client;\n\n';
    for (const tag of byTag.keys()) agg += `  late final ${tag}Api ${camel(tag)} = ${tag}Api(client);\n`;
    agg += '}\n';
    files['hr_api.dart'] = agg;
    return files;
  }

  genOperations() {
    let out = HEADER + '/// Contract metadata per operation, used to show or hide UI.\n';
    out += '/// x-permission is the server check; x-allowed-roles is the UI contract.\n';
    out += 'class HrOperationMeta {\n  const HrOperationMeta({required this.method, required this.path, required this.permission, required this.roles, required this.release, required this.errorCodes});\n';
    out += '  final String method;\n  final String path;\n  final String? permission;\n  final List<String> roles;\n  final String release;\n  final List<String> errorCodes;\n}\n\n';
    out += 'class HrOps {\n';
    for (const [, , o] of this.operationsWithPath()) out += `  static const String ${o.operationId} = ${dstr(o.operationId)};\n`;
    out += '}\n\nconst Map<String, HrOperationMeta> hrOperations = <String, HrOperationMeta>{\n';
    for (const [p, m, o] of this.operationsWithPath()) {
      const perm = o['x-permission'];
      const roles = (o['x-allowed-roles'] ?? []).map(dstr).join(', ');
      const codes = (o['x-error-codes'] ?? []).map(dstr).join(', ');
      out += `  ${dstr(o.operationId)}: HrOperationMeta(method: ${dstr(m.toUpperCase())}, path: ${dstr(p)}, `
        + `permission: ${perm ? dstr(perm) : 'null'}, roles: <String>[${roles}], `
        + `release: ${dstr(o['x-release'] ?? '')}, errorCodes: <String>[${codes}]),\n`;
    }
    out += '};\n';
    return out;
  }
}

const spec = yaml.load(fs.readFileSync(CONTRACT, 'utf8'), { schema: yaml.JSON_SCHEMA });
const g = new Gen(spec);
fs.mkdirSync(OUT, { recursive: true });
const files = { 'models.dart': g.genModels(), 'operations.dart': g.genOperations(), ...g.genApis() };
for (const old of fs.readdirSync(OUT)) if (old.endsWith('.dart') && !(old in files)) fs.rmSync(path.join(OUT, old));
for (const [name, text] of Object.entries(files)) fs.writeFileSync(path.join(OUT, name), text, 'utf8');
console.log(`Generated ${Object.keys(files).length} files: ${Object.keys(g.schemas).length} schemas, ${[...g.operationsWithPath()].length} operations -> ${OUT}`);
