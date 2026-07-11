#!/usr/bin/env node

// Proves verify-behavioral-coverage.mjs BITES both ways. The gate is only worth
// wiring blocking if it is RED on each real gap and GREEN on a clean tree, so
// every failure mode is exercised here with a fixture and its exit code asserted.
//
// Run: node .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.selftest.mjs

import { mkdtempSync, mkdirSync, writeFileSync, rmSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const GATE = fileURLToPath(new URL("./verify-behavioral-coverage.mjs", import.meta.url));

let failures = 0;
function check(label, condition, detail) {
  if (condition) {
    console.log(`  ok   ${label}`);
  } else {
    failures++;
    console.log(`  FAIL ${label}${detail ? ` — ${detail}` : ""}`);
  }
}

// A minimal TRX. `tests` is { fqn -> outcome }. We split fqn into className.method
// and emit matching <UnitTest>/<TestMethod> and <UnitTestResult> with a shared id.
function trx(tests) {
  const defs = [];
  const results = [];
  let n = 0;
  for (const [fqn, outcome] of Object.entries(tests)) {
    const id = `00000000-0000-0000-0000-${String(++n).padStart(12, "0")}`;
    const dot = fqn.lastIndexOf(".");
    const className = fqn.slice(0, dot);
    const name = fqn.slice(dot + 1);
    defs.push(`<UnitTest name="${name}" id="${id}"><TestMethod codeBase="x.dll" className="${className}" name="${name}" /></UnitTest>`);
    results.push(`<UnitTestResult executionId="${id}" testId="${id}" testName="${name}" outcome="${outcome}" />`);
  }
  return `<?xml version="1.0" encoding="UTF-8"?>
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
${results.join("\n")}
  </Results>
  <TestDefinitions>
${defs.join("\n")}
  </TestDefinitions>
</TestRun>`;
}

function matrix(members) {
  const header = `# Matrix\n\nStatus: audited.\n\n| Public API | Kind | Status |\n|---|---|---|\n`;
  const rows = members.map(member => `| \`${member}\` | member | row-proven |`).join("\n");
  return header + rows + "\n";
}

// Builds a fixture component tree under a fresh temp artifact root and returns
// the root + trx path. `map`/`matrixMembers`/`trxTests` are fully controllable.
function fixture({ matrixMembers, map, trxTests }) {
  const root = mkdtempSync(join(tmpdir(), "behavcov-"));
  const component = "widget";
  const proof = join(root, component, "proof");
  mkdirSync(proof, { recursive: true });
  writeFileSync(join(proof, "typed-api-coverage-matrix.md"), matrix(matrixMembers));
  writeFileSync(join(proof, "behavioral-coverage.json"), JSON.stringify(map, null, 2));
  const trxPath = join(root, "run.trx");
  writeFileSync(trxPath, trx(trxTests));
  return { root, trxPath };
}

function runGate({ root, trxPath, extra = [] }) {
  return spawnSync(process.execPath, [GATE, "--component", "widget", "--root", root, "--trx", trxPath, ...extra], {
    encoding: "utf8"
  });
}

const FQN_A = "Alis.Reactive.PlaywrightTests.Components.Fusion.Widget.WhenA.behavior_a";
const FQN_B = "Alis.Reactive.PlaywrightTests.Components.Fusion.Widget.WhenB.behavior_b";

const cleanMap = {
  component: "widget",
  coverage: [
    { member: "FusionWidget.Foo", test: FQN_A, catches: "Foo no longer writes the value" },
    { member: "FusionWidget.Bar", test: FQN_B, catches: "Bar no longer reads the value" }
  ]
};

const roots = [];
function build(spec) {
  const f = fixture(spec);
  roots.push(f.root);
  return f;
}

try {
  console.log("clean tree is GREEN (exit 0):");
  {
    const f = build({
      matrixMembers: ["FusionWidget.Foo", "FusionWidget.Bar"],
      map: cleanMap,
      trxTests: { [FQN_A]: "Passed", [FQN_B]: "Passed" }
    });
    const r = runGate(f);
    check("exit 0 when every member maps to a passing test", r.status === 0, `exit=${r.status}\n${r.stdout}${r.stderr}`);
  }

  console.log("RED when a covered test is missing from the TRX (deleted/renamed):");
  {
    const f = build({
      matrixMembers: ["FusionWidget.Foo", "FusionWidget.Bar"],
      map: cleanMap,
      trxTests: { [FQN_A]: "Passed" } // FQN_B absent
    });
    const r = runGate(f);
    check("exit 1", r.status === 1, `exit=${r.status}`);
    check("names the missing test", r.stdout.includes("not found in latest TRX") && r.stdout.includes(FQN_B));
  }

  console.log("RED when a covered test FAILED in the TRX:");
  {
    const f = build({
      matrixMembers: ["FusionWidget.Foo", "FusionWidget.Bar"],
      map: cleanMap,
      trxTests: { [FQN_A]: "Passed", [FQN_B]: "Failed" }
    });
    const r = runGate(f);
    check("exit 1", r.status === 1, `exit=${r.status}`);
    check("reports outcome != Passed", r.stdout.includes("did not pass") && r.stdout.includes(FQN_B));
  }

  console.log("RED when a matrix member has NO coverage entry (not 100%):");
  {
    const f = build({
      matrixMembers: ["FusionWidget.Foo", "FusionWidget.Bar", "FusionWidget.Baz"],
      map: cleanMap, // Baz unmapped
      trxTests: { [FQN_A]: "Passed", [FQN_B]: "Passed" }
    });
    const r = runGate(f);
    check("exit 1", r.status === 1, `exit=${r.status}`);
    check("names the uncovered member", r.stdout.includes("NO behavioral coverage entry") && r.stdout.includes("FusionWidget.Baz"));
  }

  console.log("RED when one test is inflated over the fan-out cap with no declared reason:");
  {
    const members = ["m1", "m2", "m3", "m4", "m5"].map(m => `FusionWidget.${m}`);
    const f = build({
      matrixMembers: members,
      map: {
        component: "widget",
        coverage: members.map(member => ({ member, test: FQN_A, catches: `${member} breaks` }))
      },
      trxTests: { [FQN_A]: "Passed" }
    });
    const r = runGate(f); // default cap 4, 5 members on one test
    check("exit 1", r.status === 1, `exit=${r.status}`);
    check("flags covered-by-variant inflation", r.stdout.includes("inflation"));
  }

  console.log("GREEN when over-cap fan-out is DECLARED with a reason:");
  {
    const members = ["m1", "m2", "m3", "m4", "m5"].map(m => `FusionWidget.${m}`);
    const f = build({
      matrixMembers: members,
      map: {
        component: "widget",
        coverage: members.map(member => ({ member, test: FQN_A, catches: `${member} breaks` })),
        acceptedFanout: [{ test: FQN_A, reason: "one event-payload variant test gathers all five fields in one POST body" }]
      },
      trxTests: { [FQN_A]: "Passed" }
    });
    const r = runGate(f);
    check("exit 0 once declared", r.status === 0, `exit=${r.status}\n${r.stdout}`);
  }

  console.log("RED when an entry omits `catches` (BDD Rule 3 not articulated):");
  {
    const f = build({
      matrixMembers: ["FusionWidget.Foo"],
      map: { component: "widget", coverage: [{ member: "FusionWidget.Foo", test: FQN_A }] },
      trxTests: { [FQN_A]: "Passed" }
    });
    const r = runGate(f);
    check("exit 1", r.status === 1, `exit=${r.status}`);
    check("requires catches", r.stdout.includes("missing \"catches\""));
  }
} finally {
  for (const root of roots) rmSync(root, { recursive: true, force: true });
}

console.log("");
if (failures === 0) {
  console.log("verify-behavioral-coverage.selftest: ALL CHECKS PASSED");
  process.exit(0);
} else {
  console.error(`verify-behavioral-coverage.selftest: ${failures} CHECK(S) FAILED`);
  process.exit(1);
}
