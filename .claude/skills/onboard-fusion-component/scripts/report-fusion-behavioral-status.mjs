#!/usr/bin/env node

// TERMINAL EXIT 3: a cross-component status reporter at the PER-COMPONENT BEHAVIORAL
// bar — parity (exit b) + behavioral coverage (exit c, 0b) — NOT the documentation
// gate. The old report-fusion-onboarding-status.mjs reports the documentation gate
// (it called Grid "audited"); this one reports run truth, so "onboarded" here means
// the deterministic gates actually pass.
//
// A component counts as behaviorally-onboarded here only when BOTH hold:
//   parity     : compute-fusion-parity.mjs exits 0 (>= threshold)
//   behavioral : verify-behavioral-coverage.mjs --component x exits 0 (every matrix
//                member maps to a test that passed in the latest TRX)
// Components missing discovery or a behavioral map are reported as below-bar, not
// failed — they simply have not started that gate.
//
// Cheap by construction: it shells the parity tool only where discovery exists and
// the 0b gate only where a behavioral-coverage.json exists; everywhere else it is a
// file check. No builds, no full gate.

import { existsSync, readdirSync } from "node:fs";
import { join, resolve, dirname } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const SKILL_SCRIPTS = dirname(fileURLToPath(import.meta.url));
const ARTIFACT_ROOT = resolve("tools/FusionOnboarding/wwwroot/onboarding/fusion");

function main() {
  const components = listComponents();
  const rows = components.map(componentStatus);

  const passing = rows.filter(r => r.verdict === "det-gates-pass");
  console.log(`# Fusion onboarding status — deterministic gates (parity + 0b + artifacts)`);
  console.log(`# "det-gates-pass" is NOT full onboarding: blind-review (e) and a green full gate (g) are also required.`);
  console.log("");
  console.log(`Components                  : ${rows.length}`);
  console.log(`Deterministic gates passing : ${passing.length}/${rows.length}`);
  console.log("");
  console.log(`${pad("component", 22)} ${pad("parity", 20)} ${pad("0b", 8)} ${pad("artifacts", 11)} verdict`);
  console.log("-".repeat(86));
  for (const r of rows) {
    console.log(`${pad(r.component, 22)} ${pad(r.parity, 20)} ${pad(r.behavioral, 8)} ${pad(r.artifacts, 11)} ${r.verdict}`);
  }
  console.log("");
  console.log(`${passing.length}/${rows.length} pass the deterministic gates.` +
    (passing.length < rows.length ? " The rest are below the bar (no discovery / no behavioral map / a gate failing)." : ""));

  // Exit non-zero until every component passes (so this can gate too).
  process.exit(passing.length === rows.length ? 0 : 1);
}

function listComponents() {
  if (!existsSync(ARTIFACT_ROOT)) return [];
  return readdirSync(ARTIFACT_ROOT, { withFileTypes: true })
    .filter(e => e.isDirectory() && !e.name.startsWith("_"))
    .map(e => e.name)
    .sort();
}

function componentStatus(component) {
  const parity = parityStatus(component);
  const behavioral = behavioralStatus(component);
  // The artifact verifier is heavier (spawns generators), so only run it once the
  // cheap gates pass — otherwise the component is already below-bar.
  const artifacts = (parity.pass && behavioral.pass) ? artifactStatus(component) : { pass: false, label: "-" };
  // "det-gates-pass" = the DETERMINISTIC gates (parity + 0b + artifacts) all pass.
  // It is NOT full onboarding: the blind-review verdict (e) and a green full gate (g)
  // are additional, judgment/expensive gates this cheap reporter does not run.
  const verdict = parity.pass && behavioral.pass && artifacts.pass ? "det-gates-pass" : "below-bar";
  return { component, parity: parity.label, behavioral: behavioral.label, artifacts: artifacts.label, verdict };
}

function artifactStatus(component) {
  const r = spawnSync(process.execPath, [join(SKILL_SCRIPTS, "verify-fusion-artifact-gates.mjs"), "--component", component], { encoding: "utf8" });
  return { pass: r.status === 0, label: r.status === 0 ? "PASS" : "FAIL" };
}

function parityStatus(component) {
  const surface = join(ARTIFACT_ROOT, component, "discovery/public-api-surface.json");
  if (!existsSync(surface)) return { pass: false, label: "no-discovery" };
  const r = spawnSync(process.execPath, [join(SKILL_SCRIPTS, "compute-fusion-parity.mjs"), "--component", component], { encoding: "utf8" });
  const line = (r.stdout ?? "").split(/\r?\n/).find(l => l.startsWith("parity"));
  const pct = line ? (line.match(/=\s*([\d.]+)%/)?.[1] ?? "?") : "?";
  return { pass: r.status === 0, label: `${pct}%${r.status === 0 ? " PASS" : " FAIL"}` };
}

function behavioralStatus(component) {
  const map = join(ARTIFACT_ROOT, component, "proof/behavioral-coverage.json");
  if (!existsSync(map)) return { pass: false, label: "no-map" };
  const r = spawnSync(process.execPath, [join(SKILL_SCRIPTS, "verify-behavioral-coverage.mjs"), "--component", component], { encoding: "utf8" });
  return { pass: r.status === 0, label: r.status === 0 ? "PASS" : "FAIL" };
}

function pad(value, width) {
  const s = String(value);
  return s.length >= width ? s : s + " ".repeat(width - s.length);
}

main();
