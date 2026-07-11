#!/usr/bin/env node
// THE 100% GUARANTEE GATE. A component is "onboarded" if and only if this exits 0.
// It cannot be faked: it composes the deterministic gates and requires ALL to pass at
// 100%. This is what "done" means — nothing else marks a component complete.
//
//   node verify-onboarding-complete.mjs --component switch [--fusion-type FusionSwitch]
//   node verify-onboarding-complete.mjs --all
//
// Gates (all must pass):
//   1. PRIMITIVE MAP   audit-primitive-coverage  -> every typed slice member maps to a
//                      core DSL primitive (Set/Call/Read/Event), 100%, and is present in
//                      the behavioral-coverage map.  (no member silently dropped)
//   2. BEHAVIOR PROOF  verify-behavioral-coverage (0b) -> every covered member's Playwright
//                      test EXISTS and shows Outcome=Passed in the latest TRX, fan-out capped,
//                      fails-when-broken declared.  (no fake/missing/weak proof)
//   3. PARITY          compute-fusion-parity -> vendor surface accounted >= threshold.
//   4. ARTIFACTS       verify-fusion-artifact-gates -> the artifact tree + inventory fresh.
//
// Exit 0 only when 1-4 all pass. Otherwise exit 1 and print exactly which gate failed and
// which members are unmapped/uncovered. This is the tooling that guarantees correctness for
// onboard / onboard-all / upgrade — the driver loops authoring until THIS passes.

import { existsSync, readdirSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const S = dirname(fileURLToPath(import.meta.url));
const ROOT = resolve("tools/FusionOnboarding/wwwroot/onboarding/fusion");
const args = parseArgs(process.argv.slice(2));

const components = args.all
  ? readdirSync(ROOT, { withFileTypes: true }).filter(e => e.isDirectory() && !e.name.startsWith("_")).map(e => e.name).sort()
  : [args.component || fail("--component <name> or --all required")];

let failed = 0;
for (const c of components) {
  const ft = (!args.all && args["fusion-type"]) ? args["fusion-type"] : "Fusion" + pascal(c);
  const r = verifyOne(c, ft);
  const tag = r.ok ? "DONE ✓" : "INCOMPLETE";
  console.log(`${pad(c, 22)} ${tag}` + (r.ok ? "" : `  — ${r.reason}`));
  if (!r.ok) failed++;
}
console.log("");
console.log(args.all
  ? `${components.length - failed}/${components.length} components pass the 100% guarantee gate.`
  : (failed ? "INCOMPLETE — not onboarded." : "DONE — 100% onboarded, guaranteed."));
process.exit(failed ? 1 : 0);

function verifyOne(component, fusionType) {
  // 1. primitive map: 100% mapped + every member present in coverage map
  const audit = run(join(S, "audit-primitive-coverage.mjs"), ["--component", component, "--fusion-type", fusionType]);
  if (audit.status !== 0) {
    const why = (audit.stdout.match(/UN(MAPPED|TESTED):.*/g) || ["audit gate failed"]).join(" | ");
    return { ok: false, reason: `primitive/coverage: ${why}` };
  }
  // 2. behavior proof: 0b against the latest TRX (existence + Passed + fails-when-broken)
  if (existsSync(join(ROOT, component, "proof/behavioral-coverage.json"))) {
    const zerob = run(join(S, "verify-behavioral-coverage.mjs"), ["--component", component]);
    if (zerob.status !== 0) return { ok: false, reason: `0b behavior proof: ${(zerob.stdout.match(/\[FAIL\].*/) || ["tests not all passing in latest TRX"])[0]}` };
  } else {
    return { ok: false, reason: "no behavioral-coverage map (behavior leg not authored)" };
  }
  // 3. parity
  if (existsSync(join(ROOT, component, "discovery/public-api-surface.json"))) {
    const parity = run(join(S, "compute-fusion-parity.mjs"), ["--component", component]);
    if (parity.status !== 0) return { ok: false, reason: "parity below threshold" };
  } else {
    return { ok: false, reason: "no discovery (run discovery first)" };
  }
  // 4. artifacts + inventory freshness
  const art = run(join(S, "verify-fusion-artifact-gates.mjs"), ["--component", component]);
  if (art.status !== 0) return { ok: false, reason: "artifact/inventory gate failed" };

  return { ok: true };
}

function run(script, a) { return spawnSync(process.execPath, [script, ...a], { encoding: "utf8" }); }
function parseArgs(a) { const o = {}; for (let i = 0; i < a.length; i++) if (a[i].startsWith("--")) o[a[i].slice(2)] = (a[i + 1] && !a[i + 1].startsWith("--")) ? a[++i] : true; return o; }
function pascal(s) { return s.split("-").map(p => p[0].toUpperCase() + p.slice(1)).join(""); }
function pad(s, n) { s = String(s); return s.length >= n ? s : s + " ".repeat(n - s.length); }
function fail(m) { console.error(m); process.exit(2); }
