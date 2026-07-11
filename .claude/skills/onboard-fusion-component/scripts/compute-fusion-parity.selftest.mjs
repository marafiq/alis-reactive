#!/usr/bin/env node

// Proves compute-fusion-parity.mjs does honest arithmetic and fails loud below the
// threshold. Builds fixture surfaces in a temp dir and asserts exit codes + output.
//
// Run: node .claude/skills/onboard-fusion-component/scripts/compute-fusion-parity.selftest.mjs

import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const TOOL = fileURLToPath(new URL("./compute-fusion-parity.mjs", import.meta.url));
let failures = 0;
const roots = [];

function check(label, cond, detail) {
  if (cond) console.log(`  ok   ${label}`);
  else { failures++; console.log(`  FAIL ${label}${detail ? ` — ${detail}` : ""}`); }
}

function member(name, kind, builderCovered, decision) {
  return { name, kind, builder: { covered: builderCovered }, discoveryDecision: decision ?? "candidate" };
}

function fixture(members, accounting) {
  const root = mkdtempSync(join(tmpdir(), "parity-"));
  roots.push(root);
  const disc = join(root, "widget", "discovery");
  mkdirSync(disc, { recursive: true });
  writeFileSync(join(disc, "public-api-surface.json"), JSON.stringify({ component: "widget", members }));
  if (accounting) writeFileSync(join(disc, "parity-accounting.json"), JSON.stringify(accounting));
  return root;
}

function run(root, threshold = 95) {
  const r = spawnSync(process.execPath, [TOOL, "--component", "widget", "--root", root, "--threshold", String(threshold)], { encoding: "utf8" });
  return { code: r.status, out: `${r.stdout}\n${r.stderr}` };
}

try {
  console.log("100% when every member is builder-owned:");
  {
    const root = fixture([member("a", "property", true), member("b", "method", true)]);
    const r = run(root);
    check("exit 0", r.code === 0, `exit=${r.code}`);
    check("reports 100%", r.out.includes("2/2 = 100.0%"));
  }

  console.log("FAIL below threshold when members are unaccounted:");
  {
    const root = fixture([member("a", "property", true), member("b", "method", false), member("c", "method", false)]);
    const r = run(root);
    check("exit 1", r.code === 1, `exit=${r.code}`);
    check("reports 33.3%", r.out.includes("1/3 = 33.3%"));
    check("lists unaccounted", r.out.includes("unaccounted          : 2"));
  }

  console.log("onboarded-typed accounting lifts parity to PASS:");
  {
    const members = [member("a", "property", true), member("b", "method", false), member("c", "method", false)];
    const root = fixture(members, { onboarded: ["b", "c"] });
    const r = run(root);
    check("exit 0 once b,c onboarded", r.code === 0, `exit=${r.code}\n${r.out}`);
    check("reports 100%", r.out.includes("3/3 = 100.0%"));
  }

  console.log("excluded WITHOUT a reason is NOT honored; WITH a reason it is:");
  {
    const members = [member("a", "property", true), member("b", "method", false)];
    const noReason = run(fixture(members, { excluded: [{ name: "b" }] }));
    check("exit 1 when exclusion has no reason", noReason.code === 1, `exit=${noReason.code}`);
    const withReason = run(fixture(members, { excluded: [{ name: "b", reason: "builder covers the equivalent config" }] }));
    check("exit 0 when exclusion has a reason", withReason.code === 0, `exit=${withReason.code}\n${withReason.out}`);
  }

  console.log("a 'skip:' discovery decision counts as excluded-with-evidence:");
  {
    const members = [member("a", "property", true), member("b", "method", false, "skip: lifecycle cleanup, not plan behavior")];
    const r = run(fixture(members));
    check("exit 0", r.code === 0, `exit=${r.code}\n${r.out}`);
    check("excluded counts the skip", r.out.includes("excluded-with-evidence: 1"));
  }
} finally {
  for (const root of roots) rmSync(root, { recursive: true, force: true });
}

console.log("");
if (failures === 0) { console.log("compute-fusion-parity.selftest: ALL CHECKS PASSED"); process.exit(0); }
console.error(`compute-fusion-parity.selftest: ${failures} CHECK(S) FAILED`); process.exit(1);
