// Compact view of api-contract/openapi.yaml, because the file is too large to read whole. Run from the repository root.
// node tools/contract-digest.mjs ops Tag1,Tag2      Operations of these tags, or these operationIds
// node tools/contract-digest.mjs refs Tag1,Tag2     Every schema those operations use
// node tools/contract-digest.mjs schemas Name1,Name2
// node tools/contract-digest.mjs tags               Every tag with its operation count and releases
import fs from 'fs';
const raw = fs.readFileSync('api-contract/openapi.yaml', 'utf8');
const L = raw.split(/\r?\n/);
const unq = s => s.trim().replace(/^['"]|['"]$/g, '');
const ops = [];
{
  const start = L.indexOf('paths:'), end = L.indexOf('components:');
  let path = null, op = null, key = null, inResp = false;
  for (let i = start + 1; i < end; i++) {
    const l = L[i]; let m;
    if ((m = l.match(/^  (\/\S*|'\/.*'):\s*$/))) { path = unq(m[1]); op = null; continue; }
    if ((m = l.match(/^    (get|post|put|patch|delete):\s*$/))) { op = { path, method: m[1].toUpperCase(), line: i + 1, resp: [] }; ops.push(op); key = null; inResp = false; continue; }
    if (!op) continue;
    if ((m = l.match(/^      ([A-Za-z-]+):\s*(.*)$/))) {
      key = m[1]; inResp = key === 'responses'; const v = m[2].trim();
      if (v === '') op[key] = []; else if (v.startsWith('[')) op[key] = v.slice(1, -1).split(',').map(unq).filter(Boolean); else if (v === 'null') op[key] = null; else op[key] = unq(v);
      continue;
    }
    if (inResp && (m = l.match(/^        ['"]?(\d{3})['"]?:/))) { op.resp.push(m[1]); continue; }
    if ((m = l.match(/^        - (.*)$/)) && Array.isArray(op[key])) op[key].push(unq(m[1]));
  }
}
const [mode, arg] = process.argv.slice(2);
const names = (arg || '').split(',');
function opBlock(o) { let j = o.line; while (j < L.length && (/^      /.test(L[j]) || L[j].trim() === '')) j++; return L.slice(o.line, j); }
function describe(o) {
  const b = opBlock(o);
  const i = b.findIndex(l => /^      description:/.test(l));
  let d = '';
  if (i >= 0) { const first = b[i].replace(/^      description:\s*(>-)?\s*/, ''); let j = i + 1; const parts = [first]; while (j < b.length && (/^        /.test(b[j]) || b[j].trim() === '')) { parts.push(b[j].trim()); j++; } d = parts.filter(Boolean).join(' '); }
  const params = []; let inP = false;
  for (let k = 0; k < b.length; k++) {
    if (/^      parameters:/.test(b[k])) { inP = true; continue; }
    if (inP && /^      \S/.test(b[k])) inP = false;
    if (inP) { let m; if ((m = b[k].match(/^        - \$ref: '#\/components\/parameters\/(\w+)'/)) && !['AcceptLanguage'].includes(m[1])) params.push(m[1]); if ((m = b[k].match(/^        - name: (\S+)/))) { const seg = b.slice(k, k + 12).join(' '); const t = (seg.match(/\$ref: '#\/components\/schemas\/(\w+)'/) || seg.match(/format: (\S+)/) || seg.match(/type: (\S+)/) || [])[1]; const req = /required: true/.test(b.slice(k, k + 3).join(' ')); params.push(m[1] + (req ? '!' : '') + ':' + t); } }
  }
  const body = b.join('\n');
  const rb = body.match(/requestBody:[\s\S]*?(?=\n      responses:)/); const req = rb ? (rb[0].match(/schemas\/(\w+)/) || [])[1] + (/multipart/.test(rb[0]) ? ' (multipart)' : '') : '';
  const rs = body.slice(body.indexOf('\n      responses:'));
  const m2 = rs.match(/'(2\d\d)':[\s\S]*?(?=\n        '\d\d\d'|\n      x-module|$)/);
  const resp = m2 ? (m2[0].match(/schemas\/(\w+)/) || [])[1] || (m2[0].match(/application\/\S+|text\/\S+/) || [''])[0] : '';
  const codes = (o['x-error-codes'] || []).map(c => c.replace('Hr:', '')).join(',');
  const extra = [...body.matchAll(/^      (x-data-scope-by-role|x-complaint-handler-assignee):(.*)$/gm)].map(m => m[1] + m[2]).join(' ');
  return `### ${o.operationId} ${o.method} ${o.path} -> ${o.resp.filter(r => r[0] === '2')} ${resp}\nroles=${(o['x-allowed-roles'] || []).join(',')} perm=${o['x-permission']} scope=${o['x-data-scope']} ${extra}\nreq=${req} params=${params.join(' ')}\ncodes=${codes}\n${d}\n`;
}
function schema(n) {
  const i = L.indexOf(`    ${n}:`); if (i < 0) return `?? ${n}`;
  let j = i + 1; while (j < L.length && (/^      /.test(L[j]) || L[j].trim() === '')) j++;
  const b = L.slice(i + 1, j);
  const e = b.findIndex(l => /^      enum:/.test(l));
  if (e >= 0) { const vals = []; for (let k = e + 1; k < b.length && /^        - /.test(b[k]); k++) vals.push(b[k].trim().slice(2)); return `${n} = enum ${vals.join('|')}`; }
  const req = []; const r = b.findIndex(l => /^      required:/.test(l)); if (r >= 0) for (let k = r + 1; k < b.length && /^        - /.test(b[k]); k++) req.push(b[k].trim().slice(2));
  const out = []; const p = b.findIndex(l => /^      properties:/.test(l));
  if (p >= 0) for (let k = p + 1; k < b.length; k++) {
    const m = b[k].match(/^        (\w+):/); if (!m) { if (/^      \S/.test(b[k])) break; continue; }
    let q = k + 1; while (q < b.length && !/^        \w+:/.test(b[q]) && !/^      \S/.test(b[q])) q++;
    const seg = b.slice(k + 1, q).join('\n');
    const ref = (seg.match(/schemas\/(\w+)/) || [])[1]; const type = (seg.match(/^          type: (\S+)/m) || [])[1]; const fmt = (seg.match(/^          format: (\S+)/m) || [])[1];
    const items = /^          type: array/m.test(seg) ? ((seg.match(/items:[\s\S]*?schemas\/(\w+)/) || [])[1] || (seg.match(/items:\s*\n\s+type: (\S+)/) || [])[1]) : null;
    const lim = ['minLength', 'maxLength', 'minimum', 'maximum', 'minItems', 'maxItems', 'pattern'].map(a => { const v = (seg.match(new RegExp(`^ +${a}: (.*)$`, 'm')) || [])[1]; return v ? `${a}=${v}` : ''; }).filter(Boolean).join(' ');
    const nul = /nullable: true/.test(seg) ? '?' : '';
    const dsc = (seg.match(/^          description: (.*)$/m) || [])[1];
    out.push(`  ${req.includes(m[1]) ? '*' : ''}${m[1]}${nul}: ${items ? items + '[]' : ref || (fmt ? type + '/' + fmt : type)}${lim ? ' ' + lim : ''}${dsc ? ' // ' + dsc.slice(0, 110) : ''}`);
    k = q - 1;
  }
  return `${n}:\n${out.join('\n')}`;
}
if (mode === 'ops') for (const o of ops.filter(o => names.includes(o.tags[0]) || names.includes(o.operationId))) console.log(describe(o));
if (mode === 'schemas') for (const n of names) console.log(schema(n));
if (mode === 'refs') {
  const seen = new Set(); const q = [];
  for (const o of ops.filter(o => names.includes(o.tags[0]) || names.includes(o.operationId))) for (const m of opBlock(o).join('\n').matchAll(/schemas\/(\w+)/g)) q.push(m[1]);
  while (q.length) { const n = q.pop(); if (seen.has(n)) continue; seen.add(n); const i = L.indexOf(`    ${n}:`); let j = i + 1; while (j < L.length && (/^      /.test(L[j]) || L[j].trim() === '')) j++; for (const m of L.slice(i, j).join('\n').matchAll(/schemas\/(\w+)/g)) q.push(m[1]); }
  for (const n of [...seen].sort()) console.log(schema(n));
}
if (mode === 'tags') { const t = {}; for (const o of ops) { const k = o.tags[0]; (t[k] ??= {})[o['x-release']] = (t[k][o['x-release']] || 0) + 1; } for (const k in t) console.log(k, JSON.stringify(t[k])); }
