#!/usr/bin/env python3
"""Generates the Dart API client from api-contract/openapi.yaml.

Run from the repository root:  python frontend/tool/generate_api.py
Requires Python 3.9+ and PyYAML (pip install pyyaml).

Output (do not edit by hand, re-run this script after any contract change):
  frontend/lib/api/generated/models.dart        every schema: enums and classes
  frontend/lib/api/generated/<tag>_api.dart     one class per tag, one method per operation,
                                                the method name is the full operationId
  frontend/lib/api/generated/hr_api.dart        HrApi aggregating every tag
  frontend/lib/api/generated/operations.dart    x-permission, x-allowed-roles, x-release,
                                                x-error-codes per operationId (UI gating)
"""
import os
import re
import sys

import yaml

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
CONTRACT = os.path.join(ROOT, 'api-contract', 'openapi.yaml')
OUT = os.path.join(ROOT, 'frontend', 'lib', 'api', 'generated')

RESERVED = set('''assert break case catch class const continue default do else enum extends false
final finally for if in is new null rethrow return super switch this throw true try var void while
with'''.split())
METHODS = ('get', 'post', 'put', 'patch', 'delete')

HEADER = ('// GENERATED CODE - DO NOT MODIFY BY HAND.\n'
          '// Source: api-contract/openapi.yaml, generator: frontend/tool/generate_api.py\n'
          '// ignore_for_file: type=lint, unused_import, unnecessary_non_null_assertion\n\n')


def camel(name):
    parts = re.split(r'[_\-\s]+', name)
    out = parts[0][:1].lower() + parts[0][1:] + ''.join(p[:1].upper() + p[1:] for p in parts[1:])
    return out


def ident(name):
    n = camel(name)
    if n in RESERVED:
        n = n + '_'
    return n


def snake(name):
    return re.sub(r'(?<!^)(?=[A-Z])', '_', name).lower()


def dstr(s):
    """Dart single-quoted string literal."""
    return "'" + str(s).replace('\\', '\\\\').replace("'", "\\'").replace('$', '\\$').replace('\n', '\\n') + "'"


def doc(text, indent):
    if not text:
        return ''
    text = ' '.join(str(text).split())
    if len(text) > 400:
        text = text[:400] + '...'
    return f'{indent}/// {text}\n'


class Gen:
    def __init__(self, spec):
        self.spec = spec
        self.schemas = spec['components']['schemas']
        self.params = spec['components'].get('parameters', {})

    # ---------- types ----------
    def ref_name(self, ref):
        return ref.split('/')[-1]

    def unwrap(self, s):
        """allOf with a single $ref and a sibling nullable is a nullable reference."""
        if 'allOf' in s and len(s['allOf']) == 1 and '$ref' in s['allOf'][0]:
            return {'$ref': s['allOf'][0]['$ref']}, bool(s.get('nullable'))
        return s, bool(s.get('nullable'))

    def is_enum(self, name):
        return 'enum' in self.schemas[name]

    def type_of(self, s):
        """Returns (dartType, fromJson(expr), toJson(expr)) for a non-null value."""
        s, _ = self.unwrap(s)
        if '$ref' in s:
            n = self.ref_name(s['$ref'])
            if self.is_enum(n):
                return n, f'{n}.fromJson({{}})', '{}.toJson()'
            return n, f'{n}.fromJson(({{}}) as Map<String, dynamic>)', '{}.toJson()'
        t, f = s.get('type'), s.get('format')
        if 'enum' in s and t == 'string':
            return 'String', '({}) as String', '{}'
        if t == 'string':
            if f == 'date-time':
                return 'DateTime', 'DateTime.parse(({}) as String)', 'hrDateTimeToJson({})'
            if f == 'date':
                return 'DateTime', 'hrDateFromJson(({}) as String)', 'hrDateToJson({})'
            if f == 'binary':
                return 'MultipartFile', '({}) as MultipartFile', '{}'
            return 'String', '({}) as String', '{}'
        if t == 'integer':
            return 'int', '(({}) as num).toInt()', '{}'
        if t == 'number':
            return 'double', '(({}) as num).toDouble()', '{}'
        if t == 'boolean':
            return 'bool', '({}) as bool', '{}'
        if t == 'array':
            it, ifr, ito = self.type_of(s.get('items', {}))
            ifr_e = ifr.format('e')
            ito_e = ito.format('e')
            fr = '(({}) as List).map((e) => ' + ifr_e.replace('{', '{{').replace('}', '}}') + ').toList()'
            to = ('{}' if ito_e == 'e' else '{}.map((e) => ' + ito_e.replace('{', '{{').replace('}', '}}') + ').toList()')
            return f'List<{it}>', fr, to
        if t == 'object' or t is None:
            ap = s.get('additionalProperties')
            if isinstance(ap, dict) and ap:
                vt, vfr, vto = self.type_of(ap)
                vnull = bool(ap.get('nullable'))
                vtype = vt + ('?' if vnull else '')
                vfr_v = vfr.format('v')
                if vnull:
                    vfr_v = f'v == null ? null : {vfr_v}'
                fr = ('(({}) as Map).map((k, v) => MapEntry(k as String, '
                      + vfr_v.replace('{', '{{').replace('}', '}}') + '))')
                vto_v = vto.format('v')
                to = '{}' if vto_v == 'v' else ('{}.map((k, v) => MapEntry(k, ' + vto_v.replace('{', '{{').replace('}', '}}') + '))')
                return f'Map<String, {vtype}>', fr, to
            return 'Map<String, dynamic>', '(({}) as Map).cast<String, dynamic>()', '{}'
        return 'Object', '({}) as Object', '{}'

    # ---------- models ----------
    def gen_enum(self, name, s):
        out = doc(s.get('description'), '')
        out += f'enum {name} {{\n'
        for v in s['enum']:
            out += f'  {ident(str(v))}({dstr(v)}),\n'
        out += "\n  /// A value this client does not know yet (newer server).\n  $unknown('');\n\n"
        out += f'  const {name}(this.value);\n  final String value;\n\n'
        out += f'  static {name} fromJson(Object? json) {{\n'
        out += f'    for (final v in {name}.values) {{\n      if (v.value == json) return v;\n    }}\n'
        out += f'    return {name}.$unknown;\n  }}\n\n'
        out += '  String toJson() => value;\n}\n\n'
        return out

    def fields(self, s):
        req = set(s.get('required', []))
        res = []
        for k, p in s.get('properties', {}).items():
            inner, nullable = self.unwrap(p)
            t, fr, to = self.type_of(p)
            optional = nullable or k not in req
            res.append(dict(json=k, name=ident(k), type=t, fr=fr, to=to, required=k in req,
                            optional=optional, desc=p.get('description'), schema=inner))
        return res

    def gen_class(self, name, s, multipart):
        fs = self.fields(s)
        out = doc(s.get('description'), '')
        out += f'class {name} {{\n'
        if fs:
            out += f'  const {name}({{\n'
            for f in fs:
                out += f"    {'' if f['optional'] else 'required '}this.{f['name']},\n"
            out += '  });\n\n'
        else:
            out += f'  const {name}();\n\n'
        for f in fs:
            out += doc(f['desc'], '  ')
            out += f"  final {f['type']}{'?' if f['optional'] else ''} {f['name']};\n"
        # fromJson (request-only multipart bodies have no JSON form)
        has_file = any('MultipartFile' in f['type'] for f in fs)
        if not has_file:
            out += f'\n  factory {name}.fromJson(Map<String, dynamic> json) => {name}(\n'
            for f in fs:
                acc = f"json[{dstr(f['json'])}]"
                conv = f['fr'].format(acc)
                if f['optional']:
                    conv = f'{acc} == null ? null : {conv}'
                out += f"        {f['name']}: {conv},\n"
            out += '      );\n'
        out += '\n'
        # toJson
        out += '  Map<String, dynamic> toJson() => <String, dynamic>{\n'
        for f in fs:
            if f['type'] == 'MultipartFile' or f['type'] == 'List<MultipartFile>':
                continue
            v = f['name'] + ('!' if f['optional'] else '')
            conv = f['to'].format(v)
            if not f['optional']:
                out += f"        {dstr(f['json'])}: {conv},\n"
            elif f['required']:
                out += f"        {dstr(f['json'])}: {f['name']} == null ? null : {conv},\n"
            else:
                out += f"        if ({f['name']} != null) {dstr(f['json'])}: {conv},\n"
        out += '      };\n'
        if multipart:
            out += '\n  /// multipart/form-data body: null parts omitted, invariant numbers, ISO dates,\n'
            out += '  /// one part per file for arrays (x-server-requirements.multipart).\n'
            out += '  FormData toFormData() {\n    final form = FormData();\n'
            for f in fs:
                n = f['name']
                v = n + ('!' if f['optional'] else '')
                guard = f'if ({n} != null) ' if f['optional'] else ''
                if f['type'] == 'MultipartFile':
                    out += f"    {guard}form.files.add(MapEntry({dstr(f['json'])}, {v}));\n"
                elif f['type'] == 'List<MultipartFile>':
                    out += f"    {guard}for (final file in {v}) {{\n      form.files.add(MapEntry({dstr(f['json'])}, file));\n    }}\n"
                elif f['type'].startswith('List<'):
                    conv = f['to'].format(v)
                    out += f"    {guard}for (final item in ({conv} as List)) {{\n      form.fields.add(MapEntry({dstr(f['json'])}, hrFormValue(item)));\n    }}\n"
                else:
                    conv = f['to'].format(v)
                    out += f"    {guard}form.fields.add(MapEntry({dstr(f['json'])}, hrFormValue({conv})));\n"
            out += '    return form;\n  }\n'
        out += '}\n\n'
        return out

    def multipart_schemas(self):
        res = set()
        for p, ops in self.spec['paths'].items():
            for m, o in ops.items():
                if m in METHODS:
                    c = (o.get('requestBody') or {}).get('content', {})
                    if 'multipart/form-data' in c:
                        res.add(self.ref_name(c['multipart/form-data']['schema']['$ref']))
        return res

    def gen_models(self):
        mp = self.multipart_schemas()
        out = HEADER + "import 'package:dio/dio.dart';\n\nimport '../runtime/json.dart';\n\n"
        for name, s in self.schemas.items():
            if 'enum' in s:
                out += self.gen_enum(name, s)
            else:
                out += self.gen_class(name, s, name in mp)
        return out

    # ---------- operations ----------
    def operations(self):
        for path, ops in self.spec['paths'].items():
            for m, o in ops.items():
                if m in METHODS:
                    yield path, m, o

    def resolve_param(self, p):
        if '$ref' in p:
            return self.params[self.ref_name(p['$ref'])]
        return p

    def param_value(self, schema, expr):
        t, fr, to = self.type_of(schema)
        if t in ('DateTime',) or '$ref' in self.unwrap(schema)[0]:
            return to.format(expr)
        return expr

    def gen_method(self, path, method, o):
        op = o['operationId']
        args = []          # (decl, sortkey)
        path_expr = path
        query = []
        headers = []
        security = o.get('security')
        for raw in o.get('parameters', []):
            p = self.resolve_param(raw)
            if p['in'] == 'header' and p['name'] == 'Accept-Language':
                continue
            t, _, _ = self.type_of(p.get('schema', {}))
            name = ident(p['name'])
            req = p.get('required', False)
            if p['in'] == 'path':
                args.append(f'required {t} {name}')
                pv = self.param_value(p.get('schema', {}), name)
                if t == 'int':
                    pv = f'{name}.toString()'
                path_expr = path_expr.replace('{' + p['name'] + '}', '${Uri.encodeComponent(' + pv + ')}')
            elif p['in'] == 'query':
                args.append(f"{'required ' if req else ''}{t}{'' if req else '?'} {name}")
                val = self.param_value(p.get('schema', {}), name)
                if req:
                    query.append(f"      {dstr(p['name'])}: {val},")
                else:
                    query.append(f"      if ({name} != null) {dstr(p['name'])}: {val},")
            elif p['in'] == 'header':
                args.append(f"{'required ' if req else ''}{t}{'' if req else '?'} {name}")
                if req:
                    headers.append(f"      {dstr(p['name'])}: {name},")
                else:
                    headers.append(f"      if ({name} != null) {dstr(p['name'])}: {name},")
        if security and any('deviceKey' in sreq for sreq in security):
            args.append('required String deviceKey')
            headers.append("      'X-Device-Key': deviceKey,")
        body_expr = 'null'
        content_type = None
        rb = o.get('requestBody')
        if rb:
            ct, c = next(iter(rb['content'].items()))
            n = self.ref_name(c['schema']['$ref'])
            args.append(f"{'required ' if rb.get('required', True) else ''}{n}{'' if rb.get('required', True) else '?'} body")
            if ct == 'multipart/form-data':
                body_expr = 'body.toFormData()'
                content_type = 'multipart/form-data'
            else:
                body_expr = 'body.toJson()' if rb.get('required', True) else 'body?.toJson()'
                content_type = ct
        # response
        ret, conv, binary = 'void', None, False
        for code in sorted(o['responses']):
            if not str(code).startswith('2'):
                continue
            content = o['responses'][code].get('content') or {}
            if not content:
                continue
            ct, c = next(iter(content.items()))
            sc = c.get('schema', {})
            if sc.get('format') == 'binary':
                ret, binary = 'HrBinaryResponse', True
            elif '$ref' in sc:
                ret = self.ref_name(sc['$ref'])
                conv = f'{ret}.fromJson(data as Map<String, dynamic>)'
            break
        args.append('CancelToken? cancelToken')
        skip_auth = security == []
        out = f"  /// {method.upper()} {path}  ({o.get('summary', '')})\n"
        out += f'  Future<{ret}> {op}({{\n'
        for a in args:
            out += f'    {a},\n'
        out += '  }) async {\n'
        call = 'sendBinary' if binary else 'send'
        out += ('    await client.' if ret == 'void' else '    final data = await client.') + f'{call}(\n'
        out += f"      operationId: {dstr(op)},\n"
        out += f"      method: {dstr(method.upper())},\n"
        out += f"      path: '{path_expr}',\n"
        if query:
            out += '      query: <String, dynamic>{\n' + '\n'.join('  ' + q for q in query) + '\n      },\n'
        if headers:
            out += '      headers: <String, dynamic>{\n' + '\n'.join('  ' + h for h in headers) + '\n      },\n'
        if body_expr != 'null':
            out += f'      body: {body_expr},\n'
        if content_type:
            out += f"      contentType: {dstr(content_type)},\n"
        if skip_auth:
            out += '      skipAuth: true,\n'
        out += '      cancelToken: cancelToken,\n    );\n'
        if binary:
            out += '    return data;\n'
        elif conv:
            out += f'    return {conv};\n'
        out += '  }\n\n'
        return out

    def gen_apis(self):
        by_tag = {}
        for path, m, o in self.operations():
            by_tag.setdefault(o['tags'][0], []).append((path, m, o))
        files = {}
        for tag, ops in by_tag.items():
            cls = tag + 'Api'
            out = HEADER + "import 'package:dio/dio.dart';\n\nimport '../runtime/api_client.dart';\nimport '../runtime/json.dart';\nimport 'models.dart';\n\n"
            out += f'/// Operations tagged {tag} in the contract.\nclass {cls} {{\n  {cls}(this.client);\n  final HrApiClient client;\n\n'
            for path, m, o in ops:
                out += self.gen_method(path, m, o)
            out += '}\n'
            files[snake(tag) + '_api.dart'] = out
        # aggregate
        agg = HEADER + "import '../runtime/api_client.dart';\n"
        for tag in by_tag:
            agg += f"import '{snake(tag)}_api.dart';\n"
        for tag in by_tag:
            agg += f"export '{snake(tag)}_api.dart';\n"
        agg += "export 'models.dart';\nexport 'operations.dart';\n\n"
        agg += '/// Every API of the contract, one property per tag.\nclass HrApi {\n  HrApi(this.client);\n  final HrApiClient client;\n\n'
        for tag in by_tag:
            agg += f'  late final {tag}Api {camel(tag)} = {tag}Api(client);\n'
        agg += '}\n'
        files['hr_api.dart'] = agg
        return files

    def gen_operations(self):
        out = HEADER + "/// Contract metadata per operation, used to show or hide UI.\n"
        out += "/// x-permission is the server check; x-allowed-roles is the UI contract.\n"
        out += 'class HrOperationMeta {\n  const HrOperationMeta({required this.method, required this.path, required this.permission, required this.roles, required this.release, required this.errorCodes});\n'
        out += '  final String method;\n  final String path;\n  final String? permission;\n  final List<String> roles;\n  final String release;\n  final List<String> errorCodes;\n}\n\n'
        out += 'class HrOps {\n'
        for path, m, o in self.operations():
            out += f"  static const String {o['operationId']} = {dstr(o['operationId'])};\n"
        out += '}\n\nconst Map<String, HrOperationMeta> hrOperations = <String, HrOperationMeta>{\n'
        for path, m, o in self.operations():
            perm = o.get('x-permission')
            roles = ', '.join(dstr(r) for r in (o.get('x-allowed-roles') or []))
            codes = ', '.join(dstr(c) for c in (o.get('x-error-codes') or []))
            out += (f"  {dstr(o['operationId'])}: HrOperationMeta(method: {dstr(m.upper())}, path: {dstr(path)}, "
                    f"permission: {dstr(perm) if perm else 'null'}, roles: <String>[{roles}], "
                    f"release: {dstr(o.get('x-release', ''))}, errorCodes: <String>[{codes}]),\n")
        out += '};\n'
        return out


def main():
    with open(CONTRACT, encoding='utf-8') as f:
        spec = yaml.safe_load(f)
    g = Gen(spec)
    os.makedirs(OUT, exist_ok=True)
    files = {'models.dart': g.gen_models(), 'operations.dart': g.gen_operations()}
    files.update(g.gen_apis())
    for old in os.listdir(OUT):
        if old.endswith('.dart') and old not in files:
            os.remove(os.path.join(OUT, old))
    for name, text in files.items():
        with open(os.path.join(OUT, name), 'w', encoding='utf-8', newline='\n') as f:
            f.write(text)
    ops = sum(1 for _ in g.operations())
    print(f'Generated {len(files)} files: {len(g.schemas)} schemas, {ops} operations -> {OUT}')


if __name__ == '__main__':
    sys.exit(main())
