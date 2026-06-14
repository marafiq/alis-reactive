#!/usr/bin/env node
// OBSERVATION PLAN (end-to-end deterministic onboarding, step 3 of: discover -> audit
// -> observe -> prove). For each typed member (with its core-DSL primitive from
// audit-primitive-coverage), this names HOW its behavior is observed THROUGH THE DSL
// and asserted in Playwright. It is grounded in the proven slices (numeric-text-box,
// rating, switch-family read-only props), NOT invented: the observation wiring below
// is the exact pattern those committed views/tests use.
//
//   node observe-plan.mjs --component switch --fusion-type FusionSwitch
//
// Output: per member -> { primitive, dslWiring, playwrightGesture, playwrightAssert }.
// This is the spec a vertical-slice generator (or an author) fills in; "covered" is
// ONLY ever true when the generated Playwright test that drives this DSL wiring PASSES
// and fails-when-broken. Nothing here marks coverage — verify-behavioral-coverage (0b)
// against a real TRX does. This file makes the behavior leg deterministic, not faked.

import { spawnSync } from "node:child_process";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const S = dirname(fileURLToPath(import.meta.url));
const args = parseArgs(process.argv.slice(2));
const component = args.component || fail("--component <name> required");
const fusionType = args["fusion-type"] || "Fusion" + pascal(component);

// reuse the deterministic, slice-grounded member->primitive classification
const audit = spawnSync(process.execPath,
  [join(S, "audit-primitive-coverage.mjs"), "--component", component, "--fusion-type", fusionType],
  { encoding: "utf8" });
const members = parseAuditMembers(audit.stdout || "");
if (!members.length) fail(`no typed members from audit for ${component} (${fusionType}); run discovery first`);

console.log(`# Observation plan: ${component} (${fusionType}) — ${members.length} typed members`);
console.log(`# covered := a Playwright test driving the DSL wiring below PASSES + fails-when-broken (0b verifies; this does not).`);
console.log("");
for (const m of members) console.log(planFor(m));

// Observation patterns — grounded in committed proven slices. Each says: how the member
// is wired into a VISIBLE outcome via the DSL, and how Playwright proves it.
function planFor(m) {
  const { name, primitive } = m;
  switch (primitive) {
    case "Read": // value source -> route into a visible status (scalar) or a branch (boolean)
      return block(name, "Read", [
        `DSL wiring: inside the owning event's .Reactive(...), route the read into a visible status:`,
        `  scalar -> p.Element("<status>").SetText(args, x => x.${leaf(name)});`,
        `  boolean -> p.When(args, x => x.${leaf(name)}).Truthy().Then(SetText("A")).Else(SetText("B"));`,
        `  component read (Value()) -> feed a gather body or a condition off comp.${leaf(name)}.`,
        `Playwright: perform the real gesture that gives the member each reachable value`,
        `  (e.g. typed vs SetValue, on vs off), assert the status text / branch flips — fails-when-broken`,
        `  because a broken read takes the wrong branch / shows the wrong value.`,
      ]);
    case "Set": // property write -> a NativeButton reaction emits it; assert the component reflects it
      return block(name, "Set", [
        `DSL wiring: Html.NativeButton("do-${kebab(leaf(name))}",...).Reactive(plan, evt => evt.Click,`,
        `  (a,p) => p.Component<${fusionType},TModel>("<id>").${leaf(name)}(<value>));`,
        `Playwright: click the button, assert the component DOM reflects <value> (ToHaveValue / checked /`,
        `  aria-valuenow / a status the view reads back) — fails-when-broken if the set is dropped.`,
      ]);
    case "Call": // method -> a NativeButton reaction invokes it; assert the visible effect
      return block(name, "Call", [
        `DSL wiring: Html.NativeButton("do-${kebab(leaf(name))}",...).Reactive(plan, evt => evt.Click,`,
        `  (a,p) => p.Component<${fusionType},TModel>("<id>").${leaf(name)}());`,
        `Playwright: click the button, assert the method's visible effect (focus moved, value reset,`,
        `  popup opened, item enabled...) — fails-when-broken if the call no-ops.`,
      ]);
    case "Event": // trigger -> .Reactive(plan, evt => evt.<Event>, ...); proven by the reaction's visible effect
      return block(name, "Event", [
        `DSL wiring: .Reactive(plan, evt => evt.${leaf(name)}, (args,p) => { /* observe payload reads here */ });`,
        `Playwright: perform the real gesture that fires ${leaf(name)} (trusted click/type),`,
        `  assert the reaction's visible outcome — fails-when-broken if the event never fires the handler.`,
      ]);
    default:
      return block(name, primitive, [`UNMAPPED — no core-DSL primitive; this must not happen (audit says 100% mapped). Re-run audit.`]);
  }
}

function block(name, prim, lines) { return `## ${prim}  ${name}\n` + lines.map(l => "  " + l).join("\n") + "\n"; }
function leaf(n) { const base = n.replace(/\(.*$/, ""); return base.includes(".") ? base.split(".").pop() : base; }
function kebab(s) { return s.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase(); }
function parseAuditMembers(out) {
  const members = [];
  for (const line of out.split(/\r?\n/)) {
    const m = line.match(/^\s{2}(Set|Call|Read|Event|UNMAPPED)\s{2,}(\S.*?)(\s+<--.*)?$/);
    if (m) members.push({ primitive: m[1], name: m[2].trim() });
  }
  return members;
}
function parseArgs(a) { const o = {}; for (let i = 0; i < a.length; i++) if (a[i].startsWith("--")) o[a[i].slice(2)] = (a[i + 1] && !a[i + 1].startsWith("--")) ? a[++i] : true; return o; }
function pascal(s) { return s.split("-").map(p => p[0].toUpperCase() + p.slice(1)).join(""); }
function fail(m) { console.error(m); process.exit(2); }
