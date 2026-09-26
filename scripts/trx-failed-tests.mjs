#!/usr/bin/env node
// Prints, one per line, the fully qualified name of every test in a TRX whose outcome is a
// failure (Failed, Error, Timeout, Aborted, ...). With a second argument "passed" it prints the
// tests that passed instead. Skipped (NotExecuted) and Inconclusive tests are neither.
//
// Used by scripts/playwright.sh --retry-failed to re-run only what failed and to report which
// tests then passed (the flaky list). The TRX regexes mirror the repo's proven parser in
// .claude/skills/onboard-fusion-component/scripts/verify-behavioral-coverage.mjs.
//
//   node scripts/trx-failed-tests.mjs <file.trx> [failed|passed]
//
// A missing file prints nothing and exits 0: a crashed or hung run has no results to re-run.
import { existsSync, readFileSync } from "node:fs";

const [trxPath, mode = "failed"] = process.argv.slice(2);
if (!trxPath || !["failed", "passed"].includes(mode)) {
  console.error("usage: node scripts/trx-failed-tests.mjs <file.trx> [failed|passed]");
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

const selected = new Set();
const result = /<UnitTestResult\b[^>]*\btestId="([^"]+)"[^>]*\boutcome="([^"]+)"/g;
while ((match = result.exec(xml)) !== null) {
  const fqn = idToFqn.get(match[1]);
  if (fqn && wanted(match[2])) selected.add(fqn);
}

for (const fqn of [...selected].sort()) console.log(fqn);
