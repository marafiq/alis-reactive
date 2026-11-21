#!/usr/bin/env node
import { existsSync, readFileSync } from "node:fs";

const modes = ["failed", "passed", "annotations", "summary", "flaky"];
const [trxPath, mode = "failed", rerunTrxPath] = process.argv.slice(2);
if (!trxPath || !modes.includes(mode) || (mode === "flaky") !== Boolean(rerunTrxPath)) {
  console.error(`usage: node poc/trx-failed-tests.mjs <file.trx> [${modes.filter(m => m !== "flaky").join("|")}]`);
  console.error("       node poc/trx-failed-tests.mjs <file.trx> flaky <rerun.trx>");
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
      console.log(annotation("warning", "Flaky test", `${fqn} failed, then passed on re-run`, failedFirst.get(fqn)));
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
const annotationsShownPerStep = 10;
if (mode === "annotations" && fqns.length > annotationsShownPerStep) {
  console.log(`::error title=${fqns.length} failed tests::${fqns.join("%0A")}`);
}
for (const fqn of fqns) {
  console.log(mode === "annotations" ? annotation("error", "Failed test", fqn, selected.get(fqn)) : fqn);
}

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

function failure(body) {
  const message = /<Message>([\s\S]*?)<\/Message>/.exec(body);
  const output = /<StdOut>([\s\S]*?)<\/StdOut>/.exec(body);
  return {
    message: message ? unescapeXml(message[1]).trim() : "",
    console: output ? pageConsole(unescapeXml(output[1])) : "",
  };
}

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

function annotation(level, title, subject, { message, console }) {
  const text = `${subject}${message ? ` — ${message.slice(0, 3000)}` : ""}${console ? `\n${console}` : ""}`;
  const escaped = text.replace(/%/g, "%25").replace(/\r/g, "%0D").replace(/\n/g, "%0A");
  return `::${level} title=${title}::${escaped}`;
}

function summary(passed) {
  const perFixture = new Map();
  for (const fqn of passed) {
    const fixture = fqn.slice(0, fqn.lastIndexOf("."));
    perFixture.set(fixture, (perFixture.get(fixture) ?? 0) + 1);
  }
  const lines = [...perFixture].map(([fixture, count]) => `${fixture.slice(fixture.lastIndexOf(".") + 1)} (${count})`);
  return `::notice title=${passed.length} passed::${lines.join("%0A")}`;
}
