#!/usr/bin/env node

// TASK ZERO 0b — the behavioral coverage gate (sufficiency).
//
// 0a (tools/FusionCoverage) proves the NEGATIVE: a slice member referenced
// nowhere in the sandbox assembly cannot be covered. This gate proves the
// POSITIVE and is the only artifact allowed to say a member is "covered":
//
//   every public matrix member  ->  names a Playwright test (the behavioral
//   coverage map)  ->  that test FQN EXISTS in the latest TRX AND its
//   Outcome="Passed"  ->  variant fan-out is bounded and declared.
//
// It reads run truth (the TRX), never prose. A `row-proven` string, a
// `Status: audited` line, or a markdown link is NOT proof here and is never
// consulted. That is the whole point: the matrix can lie; the TRX cannot.
//
// Closes the three documented loopholes in verify-fusion-artifact-gates.mjs:
//   1. row-proven self-declared  -> each member's test FQN resolved against the
//      latest TRX (exists + Passed), by exit code.
//   2. "partial"/"wip" prose slips the denylist -> prose status is irrelevant;
//      only the TRX decides. (The artifact verifier's denylist is hardened too.)
//   3. covered-by-variant inflation -> members-per-test is capped; fan-out above
//      the cap must be declared with a reason in `acceptedFanout`, or it FAILS.
//
// Deterministic: same (map, matrix, TRX) -> same verdict. Judgment (does the
// test really exercise the member?) stays with the author + blind reviewer; this
// gate forces every member to NAME its test and what breaks if the member breaks
// (`catches`), then proves that named test is green in the run that just ran.

import { existsSync, readFileSync, readdirSync } from "node:fs";
import { join, resolve } from "node:path";

const DEFAULT_TRX_DIR = "tests/Alis.Reactive.PlaywrightTests/TestResults/observable";
const DEFAULT_ARTIFACT_ROOT = "tools/FusionOnboarding/wwwroot/onboarding/fusion";
const DEFAULT_MAX_FANOUT = 4;

function main() {
  const args = parseArgs(process.argv.slice(2));
  const artifactRoot = resolve(args.root ?? DEFAULT_ARTIFACT_ROOT);
  const maxFanout = Number(args["max-fanout"] ?? DEFAULT_MAX_FANOUT);
  const trxPath = resolveTrxPath(args.trx, args["trx-dir"] ?? DEFAULT_TRX_DIR);

  if (!trxPath) {
    fail(`no TRX found under ${args["trx-dir"] ?? DEFAULT_TRX_DIR} (run scripts/playwright.sh first)`);
  }

  const trx = parseTrxOutcomes(trxPath);
  const scope = resolveScope(args, artifactRoot);

  const reports = scope.components.map(component =>
    verifyComponent({ component, artifactRoot, trx, trxPath, maxFanout }));

  printReports(reports, { trxPath, skipped: scope.skipped });

  const failed = reports.filter(report => report.problems.length > 0);
  process.exit(failed.length > 0 ? 1 : 0);
}

// --- scope resolution ------------------------------------------------------

function resolveScope(args, artifactRoot) {
  if (args.component) {
    return { components: [args.component], skipped: [] };
  }
  if (args.all === true || args.all === "true") {
    const mapped = [];
    const skipped = [];
    for (const name of listComponentDirs(artifactRoot)) {
      const hasMap = existsSync(mapPath(artifactRoot, name));
      (hasMap ? mapped : skipped).push(name);
    }
    return { components: mapped, skipped };
  }
  fail("specify --component <name> or --all");
  return { components: [], skipped: [] };
}

function listComponentDirs(artifactRoot) {
  if (!existsSync(artifactRoot)) return [];
  return readdirSync(artifactRoot, { withFileTypes: true })
    .filter(entry => entry.isDirectory() && !entry.name.startsWith("_"))
    .map(entry => entry.name)
    .sort();
}

// --- per-component verification --------------------------------------------

function verifyComponent({ component, artifactRoot, trx, trxPath, maxFanout }) {
  const problems = [];
  const componentRoot = join(artifactRoot, component);
  const map = loadMap(mapPath(artifactRoot, component), problems);
  if (!map) {
    return { component, problems, summary: { mapped: 0, members: 0, tests: 0 } };
  }

  const matrixMembers = loadMatrixMembers(join(componentRoot, "proof/typed-api-coverage-matrix.md"), problems);
  const coverage = Array.isArray(map.coverage) ? map.coverage : [];
  const mappedMembers = new Set();
  const testsByName = new Map();

  for (const [index, entry] of coverage.entries()) {
    verifyEntryShape(entry, index, component, problems);
    if (typeof entry.member === "string") mappedMembers.add(entry.member);
    if (typeof entry.test === "string") {
      if (!testsByName.has(entry.test)) testsByName.set(entry.test, []);
      testsByName.get(entry.test).push(entry.member);
    }
    verifyEntryAgainstTrx(entry, component, trx, problems);
  }

  verifyCompleteness(matrixMembers, mappedMembers, component, problems);
  verifyNoStrayMapMembers(matrixMembers, mappedMembers, component, problems);
  verifyFanout(testsByName, map.acceptedFanout ?? [], maxFanout, component, problems);

  return {
    component,
    problems,
    summary: {
      members: matrixMembers.size,
      mapped: mappedMembers.size,
      tests: testsByName.size
    }
  };
}

function verifyEntryShape(entry, index, component, problems) {
  const where = `${component} coverage[${index}]`;
  if (typeof entry.member !== "string" || entry.member.trim() === "") {
    problems.push(`${where}: missing "member"`);
  }
  if (typeof entry.test !== "string" || entry.test.trim() === "") {
    problems.push(`${where}: missing "test" (FQN of the proving Playwright test)`);
  }
  // BDD Rule 3 made non-optional: every entry states what breaks if the member
  // breaks. An empty "catches" is a member nobody can prove fails-when-broken.
  if (typeof entry.catches !== "string" || entry.catches.trim() === "") {
    problems.push(`${where} (${entry.member ?? "?"}): missing "catches" — name what this test would catch if the member broke (BDD Rule 3)`);
  }
}

function verifyEntryAgainstTrx(entry, component, trx, problems) {
  if (typeof entry.test !== "string" || entry.test.trim() === "") return;
  const outcomes = trx.byFqn.get(entry.test);
  if (!outcomes) {
    problems.push(`${component}: test not found in latest TRX (deleted, renamed, or never ran): ${entry.test}`);
    return;
  }
  const notPassed = outcomes.filter(outcome => outcome !== "Passed");
  if (notPassed.length > 0) {
    problems.push(`${component}: test did not pass in latest TRX (outcome=${[...new Set(notPassed)].join(",")}): ${entry.test}`);
  }
}

function verifyCompleteness(matrixMembers, mappedMembers, component, problems) {
  const uncovered = [...matrixMembers].filter(member => !mappedMembers.has(member)).sort();
  if (uncovered.length > 0) {
    problems.push(`${component}: ${uncovered.length} matrix member(s) have NO behavioral coverage entry (not 100%):`);
    for (const member of uncovered) problems.push(`    - ${member}`);
  }
}

function verifyNoStrayMapMembers(matrixMembers, mappedMembers, component, problems) {
  if (matrixMembers.size === 0) return; // matrix problems already reported
  const stray = [...mappedMembers].filter(member => !matrixMembers.has(member)).sort();
  for (const member of stray) {
    problems.push(`${component}: coverage map names a member absent from the matrix (drift): ${member}`);
  }
}

function verifyFanout(testsByName, acceptedFanout, maxFanout, component, problems) {
  const declared = new Map();
  for (const item of acceptedFanout) {
    if (item && typeof item.test === "string" && typeof item.reason === "string" && item.reason.trim() !== "") {
      declared.set(item.test, item.reason);
    }
  }
  for (const [test, members] of testsByName) {
    if (members.length <= maxFanout) continue;
    if (!declared.has(test)) {
      problems.push(`${component}: one test covers ${members.length} members (cap ${maxFanout}) without an acceptedFanout reason — covered-by-variant inflation: ${test}`);
    }
  }
}

// --- coverage map + matrix loading -----------------------------------------

function mapPath(artifactRoot, component) {
  return join(artifactRoot, component, "proof/behavioral-coverage.json");
}

function loadMap(path, problems) {
  if (!existsSync(path)) {
    problems.push(`missing behavioral coverage map: ${path}`);
    return null;
  }
  try {
    return JSON.parse(readFileSync(path, "utf8"));
  } catch (error) {
    problems.push(`behavioral coverage map is not valid JSON: ${path} (${error.message})`);
    return null;
  }
}

// The matrix is the authoritative "one row per public member" list. We read it
// only for the member SET (scope/completeness) — never for its self-declared
// status. Aggregate rows (slash-keyed variant contracts, remote-data:,
// data-source:) are scope summaries, not individual members, so they are not
// required to carry their own coverage entry.
function loadMatrixMembers(path, problems) {
  const members = new Set();
  if (!existsSync(path)) {
    problems.push(`missing typed API coverage matrix: ${path}`);
    return members;
  }
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (!line.startsWith("| `")) continue;
    const name = firstCell(line);
    if (isAggregateRow(name)) continue;
    members.add(name);
  }
  return members;
}

function firstCell(row) {
  const cells = row.split("|").map(cell => cell.trim()).filter(Boolean);
  return (cells[0] ?? "").replace(/^`|`$/g, "");
}

function isAggregateRow(name) {
  return name.includes("/") || name.startsWith("remote-data:") || name.startsWith("data-source:");
}

// --- TRX parsing -----------------------------------------------------------

function resolveTrxPath(explicit, dir) {
  if (explicit) return existsSync(explicit) ? resolve(explicit) : null;
  if (!existsSync(dir)) return null;
  const trx = readdirSync(dir)
    .filter(name => name.endsWith(".trx"))
    .sort(); // filenames are timestamped: lexical sort == chronological
  return trx.length > 0 ? resolve(join(dir, trx[trx.length - 1])) : null;
}

// Builds FQN -> [outcome,...]. A test that appears more than once (retry, data
// rows) keeps every outcome; the caller requires all of them to be Passed.
function parseTrxOutcomes(trxPath) {
  const xml = readFileSync(trxPath, "utf8");
  const idToFqn = new Map();
  const unitTest = /<UnitTest\b[^>]*\bid="([^"]+)"[^>]*>([\s\S]*?)<\/UnitTest>/g;
  let match;
  while ((match = unitTest.exec(xml)) !== null) {
    const id = match[1];
    const body = match[2];
    const method = /<TestMethod\b[^>]*\bclassName="([^"]+)"[^>]*\bname="([^"]+)"/.exec(body);
    if (method) idToFqn.set(id, `${method[1]}.${method[2]}`);
  }

  const byFqn = new Map();
  const result = /<UnitTestResult\b[^>]*\btestId="([^"]+)"[^>]*\boutcome="([^"]+)"/g;
  while ((match = result.exec(xml)) !== null) {
    const fqn = idToFqn.get(match[1]);
    if (!fqn) continue;
    if (!byFqn.has(fqn)) byFqn.set(fqn, []);
    byFqn.get(fqn).push(match[2]);
  }
  return { byFqn };
}

// --- reporting -------------------------------------------------------------

function printReports(reports, { trxPath, skipped }) {
  const failed = reports.filter(report => report.problems.length > 0);
  console.log(`# Behavioral coverage gate (0b)`);
  console.log(`TRX: ${trxPath}`);
  console.log("");
  for (const report of reports) {
    const mark = report.problems.length === 0 ? "PASS" : "FAIL";
    const { mapped, members, tests } = report.summary;
    console.log(`[${mark}] ${report.component} — ${mapped}/${members} members mapped, ${tests} proving test(s)`);
    for (const problem of report.problems) console.log(`  - ${problem}`);
  }
  if (skipped.length > 0) {
    console.log("");
    console.log(`Not behaviorally audited (no behavioral-coverage.json — below bar, not failed): ${skipped.length}`);
    console.log(`  ${skipped.join(", ")}`);
  }
  console.log("");
  if (failed.length === 0) {
    console.log(`All ${reports.length} behaviorally-audited component(s) green.`);
  } else {
    console.error(`${failed.length} component(s) FAILED the behavioral coverage gate.`);
  }
}

// --- utilities -------------------------------------------------------------

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

function fail(message) {
  console.error(`verify-behavioral-coverage: ${message}`);
  process.exit(2);
}

main();
