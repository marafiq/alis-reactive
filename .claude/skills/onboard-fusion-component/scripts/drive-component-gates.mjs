#!/usr/bin/env node

// The ONE command (terminal-exit #2). Drives a chosen Fusion component through
// every deterministic onboarding gate and FAILS LOUD on any gap. Run from the
// repo root:
//
//   node .claude/skills/onboard-fusion-component/scripts/drive-component-gates.mjs \
//     --component schedule [--fusion-type FusionSchedule] [--full] [--no-build]
//
// It runs the cheap gates first, then the full gate (only with --full, and only
// if the cheap gates passed). Every gate is one of:
//   PASS  — proven green
//   FAIL  — ran and a real gap was found
//   GAP   — the gate's tool is not built yet (e.g. parity) — counts as not-done
//   SKIP  — deliberately not run this invocation (e.g. full gate without --full)
//
// Exit 0 ONLY when every required gate is PASS. A SKIP/GAP/FAIL on any required
// gate is a non-zero exit, because a component is not onboarded until ALL of the
// per-component exit conditions hold (see GOAL-deterministic-onboarding-automation.md).
//
// Maps to the per-component exit letters:
//   a ALIS009 typed gate         -> build the slice; the analyzer is Error-severity
//   0a no-sandbox-usage signal   -> tools/FusionCoverage, filtered to this component
//   b parity >= 95%              -> GAP until a parity tool exists (named, not hidden)
//   c 100% behavioral coverage   -> verify-behavioral-coverage.mjs (0b)
//   e blind-review verdict       -> the verdict artifact must exist (quality is judged by a human/agent)
//   f artifacts complete         -> verify-fusion-artifact-gates.mjs
//   g full gate green            -> scripts/test.sh (only with --full)

import { existsSync } from "node:fs";
import { spawnSync } from "node:child_process";
import { resolve, join } from "node:path";

const SKILL_SCRIPTS = ".claude/skills/onboard-fusion-component/scripts";
const SANDBOX_CSProj = "Alis.Reactive.SandboxApp/Alis.Reactive.SandboxApp.csproj";
const SANDBOX_BIN = "Alis.Reactive.SandboxApp/bin/Debug/net10.0";

function main() {
  const args = parseArgs(process.argv.slice(2));
  const component = requireArg(args, "component");
  const fusionType = args["fusion-type"] ?? `Fusion${pascal(component)}`;
  const full = flag(args, "full");
  const build = !flag(args, "no-build");

  console.log(`# Onboarding gate driver — ${component} (${fusionType})`);
  console.log("");

  const gates = [];
  const record = (letter, name, result) => {
    gates.push({ letter, name, ...result });
    const detail = result.detail ? ` — ${result.detail}` : "";
    console.log(`[${result.status}] (${letter}) ${name}${detail}`);
  };

  // a + the dlls 0a needs: one slice build. ALIS009 is Error-severity, so a clean
  // build IS the typed-gate proof.
  if (build) {
    const buildResult = run("dotnet", ["build", SANDBOX_CSProj, "-c", "Debug", "--nologo", "-v", "q"]);
    const alis009 = /ALIS009/.test(buildResult.output);
    if (buildResult.code === 0) {
      record("a", "ALIS009 typed gate (slice builds clean)", { status: "PASS" });
    } else {
      record("a", "ALIS009 typed gate", {
        status: "FAIL",
        detail: alis009 ? "build failed with ALIS009 (untyped public API)" : `build failed (exit ${buildResult.code})`
      });
    }
  } else {
    record("a", "ALIS009 typed gate", { status: "SKIP", detail: "--no-build (using existing bin; ALIS009 unverified)" });
  }

  // 0a — members of THIS component with zero sandbox references. Necessary, not
  // sufficient; a non-zero count is a real gap (cannot be Playwright-covered).
  record("0a", "no-sandbox-usage signal (FusionCoverage)", coverageGate(fusionType));

  // b — parity. No parity tool is wired yet; name the gap, never hide it.
  record("b", "parity >= 95% (vendor surface vs typed C#)", {
    status: "GAP",
    detail: "parity tool not implemented — build it; do not claim parity by hand"
  });

  // c — 0b behavioral coverage gate.
  record("c", "100% behavioral coverage (0b, TRX-verified)",
    nodeGate([join(SKILL_SCRIPTS, "verify-behavioral-coverage.mjs"), "--component", component]));

  // f — artifact completeness.
  record("f", "artifacts complete (artifact-gate verifier)",
    nodeGate([join(SKILL_SCRIPTS, "verify-fusion-artifact-gates.mjs"), "--component", component]));

  // e — blind-review verdict artifact must exist (its quality is judged by reading it).
  record("e", "blind-review verdict present", blindReviewGate(component));

  // g — the full gate, the ultimate proof. Expensive; only with --full, and only
  // worth running once the cheap gates are green.
  const cheapAllPass = gates.every(gate => gate.status === "PASS");
  if (full && cheapAllPass) {
    record("g", "full gate green (scripts/test.sh)", shellGate("scripts/test.sh", []));
  } else if (full) {
    record("g", "full gate green (scripts/test.sh)", { status: "SKIP", detail: "earlier gates not all PASS — fix them first" });
  } else {
    record("g", "full gate green (scripts/test.sh)", { status: "SKIP", detail: "pass --full to run the 57-min full gate" });
  }

  console.log("");
  const blocking = gates.filter(gate => gate.status !== "PASS");
  if (blocking.length === 0) {
    console.log(`${component}: ALL gates PASS — onboarded to the bar.`);
    process.exit(0);
  }
  console.error(`${component}: ${blocking.length} gate(s) not PASS: ${blocking.map(g => `${g.letter}=${g.status}`).join(", ")}`);
  console.error("A component is onboarded only when every gate is PASS. Close each gap above.");
  process.exit(1);
}

function coverageGate(fusionType) {
  if (!existsSync(join(SANDBOX_BIN, "Alis.Reactive.SandboxApp.dll"))) {
    return { status: "FAIL", detail: `${SANDBOX_BIN} not built — run without --no-build` };
  }
  const result = run("dotnet", ["run", "--project", "tools/FusionCoverage", "-c", "Debug", "--", SANDBOX_BIN]);
  if (result.code !== 0) {
    return { status: "FAIL", detail: `FusionCoverage exited ${result.code}` };
  }
  const prefix = `Alis.Reactive.Fusion.Components.${fusionType}.`;
  const uncovered = result.stdout
    .split(/\r?\n/)
    .filter(line => line.startsWith(prefix));
  if (uncovered.length === 0) {
    return { status: "PASS", detail: "every slice member is referenced in the sandbox assembly" };
  }
  return { status: "FAIL", detail: `${uncovered.length} member(s) with NO sandbox reference (cannot be covered): ${uncovered[0]}${uncovered.length > 1 ? ", ..." : ""}` };
}

function blindReviewGate(component) {
  const path = join("tools/FusionOnboarding/wwwroot/onboarding/fusion", component, "proof/blind-review.md");
  if (!existsSync(path)) {
    return { status: "GAP", detail: `missing ${path} (a quoted blind-reviewer verdict that could REJECT)` };
  }
  return { status: "PASS", detail: path };
}

function nodeGate(scriptArgs) {
  const result = run(process.execPath, scriptArgs);
  return result.code === 0
    ? { status: "PASS" }
    : { status: "FAIL", detail: firstProblemLine(result.output) };
}

function shellGate(script, scriptArgs) {
  const result = run("bash", [script, ...scriptArgs]);
  return result.code === 0
    ? { status: "PASS" }
    : { status: "FAIL", detail: `exit ${result.code} — see output above` };
}

function run(command, commandArgs) {
  const result = spawnSync(command, commandArgs, { cwd: process.cwd(), encoding: "utf8" });
  const stdout = result.stdout ?? "";
  const stderr = result.stderr ?? "";
  return { code: result.status ?? 1, stdout, stderr, output: `${stdout}\n${stderr}` };
}

function firstProblemLine(output) {
  const line = output.split(/\r?\n/).find(item => item.trim().startsWith("- ") || /not found|did not pass|NO behavioral|inflation|missing/.test(item));
  return (line ?? "see output").trim().replace(/^- /, "");
}

function parseArgs(items) {
  const result = {};
  for (let i = 0; i < items.length; i++) {
    const item = items[i];
    if (!item.startsWith("--")) continue;
    const key = item.slice(2);
    const value = items[i + 1];
    if (value === undefined || value.startsWith("--")) {
      result[key] = true;
      continue;
    }
    result[key] = value;
    i++;
  }
  return result;
}

function requireArg(values, name) {
  const value = values[name];
  if (typeof value === "string" && value.trim().length > 0) return value.trim();
  console.error(`Missing --${name}`);
  process.exit(2);
}

function flag(values, name) {
  return values[name] === true || values[name] === "true";
}

function pascal(value) {
  return value
    .split(/[-_]/)
    .filter(Boolean)
    .map(part => part[0].toUpperCase() + part.slice(1))
    .join("");
}

main();
