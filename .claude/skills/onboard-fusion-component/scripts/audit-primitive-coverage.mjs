#!/usr/bin/env node
// DETERMINISTIC primitive-coverage audit (no invention, source-grounded).
//
// Goal contract: every typed PUBLIC member of a Fusion component slice maps to a
// core DSL primitive — Set (property write), Call (method), Read (value source),
// or Event (a trigger whose payload fields are Reads) — and each is covered by a
// behavior test. This script reads the SLICE SOURCE as the authority for the
// member -> primitive map (the slice's own EmitSet/EmitCall/Read/TypedEvent), then
// cross-checks the behavioral-coverage map. It invents no taxonomy: the primitive
// is whatever the slice actually emits.
//
//   node audit-primitive-coverage.mjs --component switch --fusion-type FusionSwitch
//
// Exit 0 only when every typed member maps to a primitive AND is covered; else 1.
// This is the audit backbone (re-run = audit) and the upgrade anchor (a new vendor
// member with no slice mapping shows up here as UNMAPPED).

import { existsSync, readFileSync, readdirSync } from "node:fs";
import { join, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const component = args.component || fail("--component <name> required");
const fusionType = args["fusion-type"] || "Fusion" + pascal(component);
const sliceDir = resolve(`Alis.Reactive.Fusion/Components/${fusionType}`);
const artifactDir = resolve(`tools/FusionOnboarding/wwwroot/onboarding/fusion/${component}`);

if (!existsSync(sliceDir)) fail(`slice dir not found: ${sliceDir}`);

const members = extractSliceMembers(sliceDir, fusionType);
const covered = loadCoveredMembers(join(artifactDir, "proof/behavioral-coverage.json"));

const rows = members.map(m => ({ ...m, tested: covered.has(normalize(m.name)) }));
report(rows);

const unmapped = rows.filter(r => r.primitive === "UNMAPPED");
const untested = rows.filter(r => r.primitive !== "UNMAPPED" && !r.tested);
process.exit(unmapped.length === 0 && untested.length === 0 ? 0 : 1);

// --- slice parsing: the slice IS the member -> primitive map -------------------
function extractSliceMembers(dir, fusionType) {
  const out = [];
  for (const file of readdirSync(dir).filter(f => f.endsWith(".cs"))) {
    const src = readFileSync(join(dir, file), "utf8");

    // extension methods: public static ... Name<TModel>(this ComponentRef<...> self, ...)
    for (const m of src.matchAll(/public\s+static\s+[A-Za-z0-9_<>,.\s]+?\s+([A-Z][A-Za-z0-9]*)\s*<[^>]*>\s*\(\s*this\s+ComponentRef<[^>]*>\s+self/g)) {
      const name = m[1];
      const body = bodyAfter(src, m.index);
      let primitive = "UNMAPPED";
      if (/\bEmitSet\b/.test(body)) primitive = "Set";
      else if (/\bEmitCall\b/.test(body)) primitive = "Call";
      else if (/\.Read\b|TypedComponentSource/.test(body)) primitive = "Read";
      out.push({ name: `${name}()`, primitive, file, source: "extension" });
    }

    // events: public TypedEvent<TArgs> Name => ...  -> Event; its payload props are Reads
    for (const e of src.matchAll(/public\s+TypedEvent<([A-Za-z0-9_]+)>\s+([A-Z][A-Za-z0-9]*)\s*=>/g)) {
      out.push({ name: e[2], primitive: "Event", file, source: "event", argsType: e[1] });
    }
  }

  // event payload properties: each public prop on a Fusion*Args is a payload Read
  for (const file of readdirSync(dir).flatMap(sub => walkCs(join(dir, sub)).concat(join(dir, sub)))) {
    if (!file.endsWith(".cs")) continue;
    const src = readFileSync(file, "utf8");
    for (const c of src.matchAll(/public\s+class\s+(Fusion[A-Za-z0-9]*Args)\b/g)) {
      const argsBody = bodyAfter(src, c.index);
      for (const p of argsBody.matchAll(/public\s+[A-Za-z0-9_<>?]+\s+([A-Z][A-Za-z0-9]*)\s*\{\s*get/g)) {
        out.push({ name: `${c[1]}.${p[1]}`, primitive: "Read", file: file.split("/").pop(), source: "payload" });
      }
    }
  }
  // de-dupe by name
  const seen = new Map();
  for (const m of out) if (!seen.has(m.name)) seen.set(m.name, m);
  return [...seen.values()].sort((a, b) => a.name.localeCompare(b.name));
}

function walkCs(p) {
  try { if (!readdirSync(p)) return []; } catch { return []; }
  try { return readdirSync(p).flatMap(c => { const f = join(p, c); try { readdirSync(f); return walkCs(f); } catch { return [f]; } }); }
  catch { return []; }
}

function bodyAfter(src, idx) {
  // exact brace-matched body from the first '{' after idx, so a class/method body
  // never bleeds into the next declaration (the breadcrumb false-positive bug).
  const open = src.indexOf("{", idx);
  if (open === -1) return src.slice(idx, idx + 400); // expression-bodied member
  let depth = 0;
  for (let i = open; i < src.length; i++) {
    if (src[i] === "{") depth++;
    else if (src[i] === "}" && --depth === 0) return src.slice(idx, i + 1);
  }
  return src.slice(idx);
}

function loadCoveredMembers(path) {
  const set = new Set();
  if (!existsSync(path)) return set;
  try {
    const j = JSON.parse(readFileSync(path, "utf8"));
    for (const c of (j.coverage || [])) set.add(normalize(c.member));
  } catch { /* no map yet */ }
  return set;
}

function normalize(name) {
  // match slice member names against coverage 'member' strings loosely
  return String(name).replace(/\(this[^)]*\)/, "()").replace(/\s+/g, "").toLowerCase();
}

function report(rows) {
  const byPrim = rows.reduce((a, r) => ((a[r.primitive] = (a[r.primitive] || 0) + 1), a), {});
  console.log(`# Primitive-coverage audit: ${component} (${fusionType})`);
  console.log(`typed members: ${rows.length}  |  ${Object.entries(byPrim).map(([k, v]) => `${k}:${v}`).join("  ")}`);
  console.log("");
  for (const r of rows) {
    const flag = r.primitive === "UNMAPPED" ? "  <-- NO CORE-DSL PRIMITIVE" : (r.tested ? "" : "  <-- UNTESTED");
    console.log(`  ${r.primitive.padEnd(8)} ${r.name}${flag}`);
  }
  const unmapped = rows.filter(r => r.primitive === "UNMAPPED");
  const untested = rows.filter(r => r.primitive !== "UNMAPPED" && !r.tested);
  console.log("");
  console.log(`mapped-to-primitive: ${rows.length - unmapped.length}/${rows.length}   covered-by-test: ${rows.filter(r => r.tested).length}/${rows.length}`);
  if (unmapped.length) console.log(`UNMAPPED (no core-DSL primitive): ${unmapped.map(r => r.name).join(", ")}`);
  if (untested.length) console.log(`UNTESTED: ${untested.map(r => r.name).join(", ")}`);
}

function parseArgs(a) { const o = {}; for (let i = 0; i < a.length; i++) if (a[i].startsWith("--")) o[a[i].slice(2)] = (a[i + 1] && !a[i + 1].startsWith("--")) ? a[++i] : true; return o; }
function pascal(s) { return s.split("-").map(p => p[0].toUpperCase() + p.slice(1)).join(""); }
function fail(m) { console.error(m); process.exit(2); }
