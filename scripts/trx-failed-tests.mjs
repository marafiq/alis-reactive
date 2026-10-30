#!/usr/bin/env node
// Prints, one per line, the fully qualified name of every test in a TRX whose outcome is a
// failure (Failed, Error, Timeout, Aborted, ...). With a second argument "passed" it prints the
// tests that passed instead. Skipped (NotExecuted) and Inconclusive tests are neither.
// With "annotations" it prints one GitHub Actions ::error command per failed test, carrying the
// start of its failure message, so a red shard names its failures on the pull request's checks.
// With "summary" it prints one ::notice command naming each fixture and how many of its tests
// passed, so a green shard says what it proved.
// With "flaky" and the TRX of the re-run it prints one ::warning command per test that failed in
// the first file and passed in the re-run, carrying the start of the failure the re-run hid, so a
// flaky test's check says why it failed (the failure artifacts are uploaded only for a red job).
// A failure's annotation also carries the end of the page console the test wrote on failure
// (PlaywrightTestBase's "Page Console Output"), and every error line in it, so the check shows what
// the page said without the job's artifacts.
//
// Used by scripts/playwright.sh --retry-failed to re-run only what failed, to report which tests
// then passed (the flaky list) and why they failed, and to annotate what still failed.
//
//   node scripts/trx-failed-tests.mjs <file.trx> [failed|passed|annotations|summary]
//   node scripts/trx-failed-tests.mjs <file.trx> flaky <rerun.trx>
//
// A missing file prints nothing and exits 0: a crashed or hung run has no results to re-run.
import { existsSync, readFileSync } from "node:fs";

const modes = ["failed", "passed", "annotations", "summary", "flaky"];
const [trxPath, mode = "failed", rerunTrxPath] = process.argv.slice(2);
if (!trxPath || !modes.includes(mode) || (mode === "flaky") !== Boolean(rerunTrxPath)) {
  console.error(`usage: node scripts/trx-failed-tests.mjs <file.trx> [${modes.filter(m => m !== "flaky").join("|")}]`);
  console.error("       node scripts/trx-failed-tests.mjs <file.trx> flaky <rerun.trx>");
  process.exit(2);
}
if (!existsSync(trxPath)) process.exit(0);

const neitherPassedNorFailed = new Set(["NotExecuted", "Inconclusive"]);
const passed = outcome => outcome === "Passed";
const failed = outcome => outcome !== "Passed" && !neitherPassedNorFailed.has(outcome);

if (mode === "flaky") {
  const failedFirst = results(trxPath, failed);
  const passedOnRerun = results(rerunTrxPath, passed);
  for (const fqn of [...failedFirst.keys()].sort()) {
    if (passedOnRerun.has(fqn)) {
      console.log(annotation("warning", "Flaky test", `${fqn} failed, then passed on re-run (docs/CI.md, "Flaky tests")`, failedFirst.get(fqn)));
    }
  }
  process.exit(0);
}

const selected = results(trxPath, mode === "passed" || mode === "summary" ? passed : failed);
const fqns = [...selected.keys()].sort();
if (mode === "summary") {
  console.log(summary(fqns));
  process.exit(0);
}
// GitHub shows ten error annotations per step, so a shard with more failures leads with one that
// names them all; the detailed ones that follow are cut at ten.
const annotationsShownPerStep = 10;
if (mode === "annotations" && fqns.length > annotationsShownPerStep) {
  console.log(`::error title=${fqns.length} failed tests::${fqns.join("%0A")}`);
}
for (const fqn of fqns) {
  console.log(mode === "annotations" ? annotation("error", "Failed test", fqn, selected.get(fqn)) : fqn);
}

// Each test in the TRX whose outcome `wanted` accepts, with the message it failed with ("" if none).
function results(path, wanted) {
  const selected = new Map();
  if (!existsSync(path)) return selected;
  const xml = readFileSync(path, "utf8");

  const idToFqn = new Map();
  const unitTest = /<UnitTest\b[^>]*\bid="([^"]+)"[^>]*>([\s\S]*?)<\/UnitTest>/g;
  let match;
  while ((match = unitTest.exec(xml)) !== null) {
    const method = /<TestMethod\b[^>]*\bclassName="([^"]+)"[^>]*\bname="([^"]+)"/.exec(match[2]);
    if (method) idToFqn.set(match[1], `${method[1]}.${method[2]}`);
  }

  const result = /<UnitTestResult\b([^>]*?)(?:\/>|>([\s\S]*?)<\/UnitTestResult>)/g;
  while ((match = result.exec(xml)) !== null) {
    const testId = /\btestId="([^"]+)"/.exec(match[1]);
    const outcome = /\boutcome="([^"]+)"/.exec(match[1]);
    const fqn = testId && idToFqn.get(testId[1]);
    if (fqn && outcome && wanted(outcome[1])) selected.set(fqn, failure(match[2] ?? ""));
  }
  return selected;
}

// The failure message, and the page console the test wrote to its output when it failed.
function failure(body) {
  const message = /<Message>([\s\S]*?)<\/Message>/.exec(body);
  const output = /<StdOut>([\s\S]*?)<\/StdOut>/.exec(body);
  return {
    message: message ? unescapeXml(message[1]).trim() : "",
    console: output ? pageConsole(unescapeXml(output[1])) : "",
  };
}

// The page console's error lines, then its last thirty lines.
function pageConsole(output) {
  const section = /=== Page Console Output ===\r?\n([\s\S]*?)=== End Console Output ===/.exec(output);
  if (!section) return "";
  const lines = section[1].split(/\r?\n/).filter(line => line.trim() !== "");
  const errors = lines.filter(line => /^\[(error|PAGE ERROR)\]/.test(line)).join("\n").slice(0, 1500);
  const last = lines.slice(-30).join("\n").slice(-2500);
  return `Page console errors:\n${errors || "none"}\nPage console, last lines:\n${last}`;
}

function unescapeXml(text) {
  return text
    .replace(/&lt;/g, "<")
    .replace(/&gt;/g, ">")
    .replace(/&quot;/g, "\"")
    .replace(/&apos;/g, "'")
    .replace(/&#xD;/gi, "\r")
    .replace(/&#xA;/gi, "\n")
    .replace(/&amp;/g, "&");
}

// GitHub Actions workflow commands escape %, CR and LF in the message; the start of the failure
// message names the defect, the end of the page console shows what the page did, and the artifact
// carries the rest.
function annotation(level, title, subject, { message, console }) {
  const text = `${subject}${message ? ` — ${message.slice(0, 3000)}` : ""}${console ? `\n${console}` : ""}`;
  const escaped = text.replace(/%/g, "%25").replace(/\r/g, "%0D").replace(/\n/g, "%0A");
  return `::${level} title=${title}::${escaped}`;
}

// "312 passed" and, one per line, each fixture with its passed count: WhenCoordinatorReviewsMoveIn (9).
function summary(passed) {
  const perFixture = new Map();
  for (const fqn of passed) {
    const fixture = fqn.slice(0, fqn.lastIndexOf("."));
    perFixture.set(fixture, (perFixture.get(fixture) ?? 0) + 1);
  }
  const lines = [...perFixture].map(([fixture, count]) => `${fixture.slice(fixture.lastIndexOf(".") + 1)} (${count})`);
  return `::notice title=${passed.length} passed::${lines.join("%0A")}`;
}
