#!/usr/bin/env node
// Prints, one per line, the fully qualified name of every test in a TRX whose outcome is a
// failure (Failed, Error, Timeout, Aborted, ...). With a second argument "passed" it prints the
// tests that passed instead. Skipped (NotExecuted) and Inconclusive tests are neither.
// With "annotations" it prints one GitHub Actions ::error command per failed test, carrying the
// start of its failure message, so a red shard names its failures on the pull request's checks.
//
// Used by scripts/playwright.sh --retry-failed to re-run only what failed, to report which tests
// then passed (the flaky list), and to annotate what still failed.
//
//   node scripts/trx-failed-tests.mjs <file.trx> [failed|passed|annotations]
//
// A missing file prints nothing and exits 0: a crashed or hung run has no results to re-run.
import { existsSync, readFileSync } from "node:fs";

const modes = ["failed", "passed", "annotations"];
const [trxPath, mode = "failed"] = process.argv.slice(2);
if (!trxPath || !modes.includes(mode)) {
  console.error(`usage: node scripts/trx-failed-tests.mjs <file.trx> [${modes.join("|")}]`);
  process.exit(2);
}
if (!existsSync(trxPath)) process.exit(0);

const xml = readFileSync(trxPath, "utf8");

const idToFqn = new Map();
const unitTest = /<UnitTest\b[^>]*\bid="([^"]+)"[^>]*>([\s\S]*?)<\/UnitTest>/g;
let match;
while ((match = unitTest.exec(xml)) !== null) {
  const method = /<TestMethod\b[^>]*\bclassName="([^"]+)"[^>]*\bname="([^"]+)"/.exec(match[2]);
  if (method) idToFqn.set(match[1], `${method[1]}.${method[2]}`);
}

const neitherPassedNorFailed = new Set(["NotExecuted", "Inconclusive"]);
const wanted = mode === "passed"
  ? outcome => outcome === "Passed"
  : outcome => outcome !== "Passed" && !neitherPassedNorFailed.has(outcome);

const selected = new Map();
const result = /<UnitTestResult\b([^>]*?)(?:\/>|>([\s\S]*?)<\/UnitTestResult>)/g;
while ((match = result.exec(xml)) !== null) {
  const testId = /\btestId="([^"]+)"/.exec(match[1]);
  const outcome = /\boutcome="([^"]+)"/.exec(match[1]);
  const fqn = testId && idToFqn.get(testId[1]);
  if (fqn && outcome && wanted(outcome[1])) selected.set(fqn, failureMessage(match[2] ?? ""));
}

const fqns = [...selected.keys()].sort();
// GitHub shows ten error annotations per step, so a shard with more failures leads with one that
// names them all; the detailed ones that follow are cut at ten.
const annotationsShownPerStep = 10;
if (mode === "annotations" && fqns.length > annotationsShownPerStep) {
  console.log(`::error title=${fqns.length} failed tests::${fqns.join("%0A")}`);
}
for (const fqn of fqns) {
  console.log(mode === "annotations" ? annotation(fqn, selected.get(fqn)) : fqn);
}

function failureMessage(body) {
  const message = /<Message>([\s\S]*?)<\/Message>/.exec(body);
  return message ? unescapeXml(message[1]).trim() : "";
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
// message is enough to name the defect, and the artifact carries the rest.
function annotation(fqn, message) {
  const text = `${fqn}${message ? ` — ${message.slice(0, 4000)}` : ""}`;
  const escaped = text.replace(/%/g, "%25").replace(/\r/g, "%0D").replace(/\n/g, "%0A");
  return `::error title=Failed test::${escaped}`;
}
