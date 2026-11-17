#!/usr/bin/env node
import { execFileSync } from "node:child_process";
import { readFileSync } from "node:fs";

const EXCUSED = new Map([["GHSA-fc5q-39xc-vjpf", "picomatch"]]);

function option(name) {
  const at = process.argv.indexOf(name);
  return at === -1 ? undefined : process.argv[at + 1];
}

function options(name) {
  return process.argv.flatMap((argument, at) => (argument === name ? [process.argv[at + 1]] : []));
}

function npm(args) {
  try {
    return execFileSync("npm", args, { encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] });
  } catch (error) {
    const printedAReport = typeof error.stdout === "string" && error.stdout.length > 0;
    if (printedAReport) {
      return error.stdout;
    }
    throw error;
  }
}

function readReport() {
  const file = option("--report");
  const text = file === undefined ? npm(["audit", "--json"]) : readFileSync(file, "utf8");
  const report = JSON.parse(text);
  if (report.auditReportVersion !== 2) {
    throw new Error(`npm audit printed no report of version 2: ${text.slice(0, 400)}`);
  }
  return report;
}

function advisoriesOf(report) {
  const found = new Map();
  for (const vulnerability of Object.values(report.vulnerabilities)) {
    for (const via of vulnerability.via) {
      if (typeof via === "object") {
        found.set(via.url.slice(via.url.lastIndexOf("/") + 1), via);
      }
    }
  }
  return found;
}

function installed(lock, name) {
  const entries = Object.entries(lock.packages).filter(
    ([path]) => path === `node_modules/${name}` || path.endsWith(`/node_modules/${name}`),
  );
  return [...new Set(entries.map(([, entry]) => entry.version))];
}

function latest(name) {
  const given = options("--latest").find((pair) => pair.slice(0, pair.lastIndexOf("@")) === name);
  return given === undefined ? npm(["view", name, "version"]).trim() : given.slice(given.lastIndexOf("@") + 1);
}

function excusedFinding(id, name, advisories, lock) {
  if (!advisories.has(id)) {
    return {
      problem: `${id} is no longer reported for ${name}: remove it from the excused advisories of scripts/npm-audit.mjs and from spec/REWRITE.md section 2.7`,
    };
  }
  const newest = latest(name);
  const held = installed(lock, name);
  const lockHoldsTheLatest = held.length > 0 && held.every((version) => version === newest);
  if (!lockHoldsTheLatest) {
    return {
      problem: `${id} is excused only while the lock holds the latest ${name}: ${name} ${newest} is published and the lock holds ${held.join(", ") || "none"}, so move the lock to ${newest}`,
    };
  }
  return { excused: `${id} (${name} ${newest}, the latest published)` };
}

const report = readReport();
const lock = JSON.parse(readFileSync(option("--lock") ?? "package-lock.json", "utf8"));
const advisories = advisoriesOf(report);
const problems = [...advisories]
  .filter(([id, advisory]) => EXCUSED.get(id) !== advisory.name)
  .map(([id, advisory]) => `${id} (${advisory.name} ${advisory.range}): ${advisory.title}`);
const findings = [...EXCUSED].map(([id, name]) => excusedFinding(id, name, advisories, lock));
problems.push(...findings.flatMap((finding) => (finding.problem === undefined ? [] : [finding.problem])));

if (problems.length > 0) {
  console.error(`[npm-audit] these findings fail the check:\n${problems.join("\n")}`);
  process.exit(1);
}
const excused = findings.map((finding) => finding.excused);
console.log(`[npm-audit] every finding is excused: ${excused.join("; ")}`);
