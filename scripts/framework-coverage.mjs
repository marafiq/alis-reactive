// What the framework's own tests run of the framework: the runtime (runtime/**, through the bundle's
// source map) and the core's C# (Alis.Reactive, Alis.Reactive.FluentValidator, from Cobertura).
//
// usage: node scripts/framework-coverage.mjs <js coverage dir> <cobertura file> <report.md>
//
// A runtime segment counts as run when any test ran it: V8's block coverage gives each range a count,
// an inner range overriding the one around it. A C# line counts as run when its hits are above zero.
// The framework's own component code (components/**) is a consumer and is not counted. Neither is
// runtime/shared/assert-never.ts: it runs only for a node a switch's union has no case for, which the
// generated contract makes a type error, so no page can run it.
import { readFileSync, readdirSync, writeFileSync } from "node:fs";
import path from "node:path";
import { SourceMapConsumer } from "source-map-js";

const [jsDir, cobertura, reportPath] = process.argv.slice(2);
if (!jsDir || !cobertura || !reportPath) {
  console.error("usage: node scripts/framework-coverage.mjs <js coverage dir> <cobertura file> <report.md>");
  process.exit(2);
}
const repo = path.resolve(path.dirname(new URL(import.meta.url).pathname), "..");

// ── runtime ──────────────────────────────────────────────────────────────────────────────────
const bundlePath = path.join(repo, "Alis.Reactive.Assets/dist/scripts/alis-reactive.dev.js");
const bundle = readFileSync(bundlePath, "utf8");
const map = new SourceMapConsumer(JSON.parse(readFileSync(bundlePath + ".map", "utf8")));

const run = new Uint8Array(bundle.length);
const files = readdirSync(jsDir).filter(name => name.endsWith(".json"));
for (const name of files) {
  const scripts = JSON.parse(readFileSync(path.join(jsDir, name), "utf8"));
  for (const functions of scripts) {
    // A function reported without block detail (V8 gives none for a page opened a second time in one test)
    // says only that it was entered, not which of its blocks ran: it counts as not run, never as all run.
    const ranges = functions.flatMap(fn => fn.isBlockCoverage ? fn.ranges : fn.ranges.map(range => ({ ...range, count: 0 })))
      .sort((a, b) => a.startOffset - b.startOffset || b.endOffset - a.endOffset);
    const counts = new Int32Array(bundle.length).fill(-1);
    for (const range of ranges) counts.fill(range.count, range.startOffset, range.endOffset);
    for (let i = 0; i < counts.length; i++) if (counts[i] > 0) run[i] = 1;
  }
}

const lineStarts = [0];
for (let i = 0; i < bundle.length; i++) if (bundle[i] === "\n") lineStarts.push(i + 1);
const offsetOf = (line, column) => lineStarts[line - 1] + column;

const mappings = [];
map.eachMapping(m => { if (m.source) mappings.push(m); });
mappings.sort((a, b) => offsetOf(a.generatedLine, a.generatedColumn) - offsetOf(b.generatedLine, b.generatedColumn));
const runtime = new Map();
const unreachable = new Set(["runtime/shared/assert-never.ts"]);
for (let i = 0; i < mappings.length; i++) {
  const m = mappings[i];
  const source = m.source.replace(/^(\.\.\/)+/, "").replace(/^Alis\.Reactive\.Assets\//, "");
  if (!source.startsWith("runtime/") || unreachable.has(source)) continue;
  const start = offsetOf(m.generatedLine, m.generatedColumn);
  const end = i + 1 < mappings.length ? offsetOf(mappings[i + 1].generatedLine, mappings[i + 1].generatedColumn) : bundle.length;
  if (end <= start) continue;
  let wasRun = false;
  for (let j = start; j < end; j++) if (run[j]) { wasRun = true; break; }
  const file = runtime.get(source) ?? { segments: 0, run: 0, unrunLines: new Set() };
  file.segments += 1;
  if (wasRun) file.run += 1; else file.unrunLines.add(m.originalLine);
  runtime.set(source, file);
}

// ── C# ───────────────────────────────────────────────────────────────────────────────────────
const xml = readFileSync(cobertura, "utf8");
const csharp = new Map();
for (const klass of xml.matchAll(/<class [^>]*filename="([^"]+)"[^>]*>([\s\S]*?)<\/class>/g)) {
  const relative = path.relative(repo, klass[1]).replaceAll("\\", "/");
  if (!/^Alis\.Reactive(\.FluentValidator)?\//.test(relative)) continue;
  const file = csharp.get(relative) ?? { lines: new Map() };
  for (const line of klass[2].matchAll(/<line number="(\d+)" hits="(\d+)"/g)) {
    const number = Number(line[1]);
    file.lines.set(number, Math.max(file.lines.get(number) ?? 0, Number(line[2])));
  }
  csharp.set(relative, file);
}

// ── report ───────────────────────────────────────────────────────────────────────────────────
const percent = (part, whole) => (whole === 0 ? 100 : (100 * part) / whole).toFixed(1);
const ranges = numbers => {
  const sorted = [...numbers].sort((a, b) => a - b);
  const out = [];
  for (const n of sorted) {
    const last = out[out.length - 1];
    if (last && n === last[1] + 1) last[1] = n; else out.push([n, n]);
  }
  return out.map(([a, b]) => (a === b ? `${a}` : `${a}-${b}`)).join(", ");
};

const jsRows = [...runtime].sort(([a], [b]) => a.localeCompare(b));
const jsTotal = jsRows.reduce((sum, [, f]) => sum + f.segments, 0);
const jsRun = jsRows.reduce((sum, [, f]) => sum + f.run, 0);
const csRows = [...csharp].sort(([a], [b]) => a.localeCompare(b)).map(([file, f]) => {
  const total = f.lines.size;
  const run = [...f.lines.values()].filter(hits => hits > 0).length;
  const unrun = [...f.lines].filter(([, hits]) => hits === 0).map(([n]) => n);
  return { file, total, run, unrun };
});
const csTotal = csRows.reduce((sum, r) => sum + r.total, 0);
const csRunTotal = csRows.reduce((sum, r) => sum + r.run, 0);

// A run that recorded nothing measured nothing: no share is written for it.
if (files.length === 0 || csTotal === 0) {
  console.error(
    `[coverage] nothing was measured: ${files.length} page tests recorded runtime coverage and the ` +
    `Cobertura file names ${csTotal} lines of the core. The tests did not run; no report is written.`);
  process.exit(1);
}

const lines = [
  "# What the framework's tests run of the framework",
  "",
  `${files.length} page tests recorded.`,
  "",
  "| Code | Run | Of | Share |",
  "|---|---|---|---|",
  `| Runtime (runtime/**, source-map segments) | ${jsRun} | ${jsTotal} | ${percent(jsRun, jsTotal)}% |`,
  `| Core C# (Alis.Reactive, Alis.Reactive.FluentValidator; coverable lines) | ${csRunTotal} | ${csTotal} | ${percent(csRunTotal, csTotal)}% |`,
  "",
  "## Runtime, by file",
  "",
  "| File | Share | Lines not run |",
  "|---|---|---|",
  ...jsRows.map(([file, f]) => `| ${file} | ${percent(f.run, f.segments)}% | ${ranges(f.unrunLines)} |`),
  "",
  "## Core C#, by file",
  "",
  "| File | Share | Lines not run |",
  "|---|---|---|",
  ...csRows.map(r => `| ${r.file} | ${percent(r.run, r.total)}% | ${ranges(r.unrun)} |`),
  "",
];
writeFileSync(reportPath, lines.join("\n"));
console.log(`runtime: ${jsRun}/${jsTotal} segments run (${percent(jsRun, jsTotal)}%)`);
console.log(`core C#: ${csRunTotal}/${csTotal} lines run (${percent(csRunTotal, csTotal)}%)`);
console.log(`report: ${reportPath}`);
