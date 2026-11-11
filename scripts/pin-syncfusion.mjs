#!/usr/bin/env node
import { execFileSync } from "node:child_process";
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const NUGET_PINS = "Directory.Packages.props";
const NUGET_IDS = ["Syncfusion.EJ2.AspNet.Core", "Syncfusion.EJ2.MVC5"];
const UMBRELLA = "@syncfusion/ej2";
const MANIFEST = "package.json";
const LOCK = "package-lock.json";

function readJson(path) {
  return JSON.parse(readFileSync(path, "utf8"));
}

function writeJson(path, value) {
  writeFileSync(path, JSON.stringify(value, null, 2) + "\n");
}

function nugetPin(pins, id) {
  const match = new RegExp(`<PackageVersion Include="${id.replace(/\./g, "\\.")}" Version="([^"]+)"`).exec(pins);
  return match ? match[1] : null;
}

function check() {
  const problems = [];
  const pins = readFileSync(NUGET_PINS, "utf8");
  const pin = nugetPin(pins, NUGET_IDS[0])?.replace(/^\[|\]$/g, "");
  if (!/^\d+\.\d+\.\d+$/.test(pin ?? "")) {
    problems.push(`${NUGET_PINS} ${NUGET_IDS[0]} must pin an exact [version], got ${nugetPin(pins, NUGET_IDS[0])}`);
  }
  for (const id of NUGET_IDS) {
    const pinned = nugetPin(pins, id);
    if (pinned !== `[${pin}]`) problems.push(`${NUGET_PINS} ${id} must use [${pin}], got ${pinned}`);
  }
  const npmPin = readJson(MANIFEST).devDependencies?.[UMBRELLA];
  if (npmPin !== pin) problems.push(`${MANIFEST} ${UMBRELLA} must be ${pin}, got ${npmPin}`);
  const locked = readJson(LOCK).packages[`node_modules/${UMBRELLA}`]?.version;
  if (locked !== pin) problems.push(`${LOCK} resolves ${UMBRELLA} to ${locked}, pinned to ${pin}`);

  if (problems.length > 0) {
    console.error("pin-syncfusion --check: FAILED");
    for (const problem of problems) console.error(`  - ${problem}`);
    console.error("Fix: node scripts/pin-syncfusion.mjs <version>, then npm ci.");
    process.exit(1);
  }
  console.log(`pin-syncfusion --check: Syncfusion ${pin} pinned in ${NUGET_PINS}, ${MANIFEST} and ${LOCK}`);
}

function npmView(spec, field) {
  return JSON.parse(execFileSync("npm", ["view", spec, field, "--json"], { encoding: "utf8" }));
}

async function assertNugetVersionExists(id, version) {
  const response = await fetch(`https://api.nuget.org/v3-flatcontainer/${id.toLowerCase()}/index.json`);
  if (!response.ok) throw new Error(`nuget.org lookup for ${id} failed: HTTP ${response.status}`);
  const { versions } = await response.json();
  if (!versions.includes(version)) throw new Error(`${id} ${version} is not on nuget.org`);
}

async function pin(version) {
  if (!/^\d+\.\d+\.\d+$/.test(version)) throw new Error(`expected a version like 33.1.47, got '${version}'`);
  if (npmView(`${UMBRELLA}@${version}`, "version") !== version) throw new Error(`${UMBRELLA}@${version} is not on npm`);
  for (const id of NUGET_IDS) await assertNugetVersionExists(id, version);

  let pins = readFileSync(NUGET_PINS, "utf8");
  for (const id of NUGET_IDS) {
    const pattern = new RegExp(`(<PackageVersion Include="${id.replace(/\./g, "\\.")}" Version=")[^"]+(")`);
    if (!pattern.test(pins)) throw new Error(`${NUGET_PINS} has no PackageVersion for ${id}`);
    pins = pins.replace(pattern, `$1[${version}]$2`);
  }
  writeFileSync(NUGET_PINS, pins);

  const manifest = readJson(MANIFEST);
  manifest.devDependencies[UMBRELLA] = version;
  writeJson(MANIFEST, manifest);
  execFileSync("npm", ["install", "--package-lock-only", "--ignore-scripts", "--no-audit", "--no-fund"], { stdio: "inherit" });

  console.log(`pinned Syncfusion ${version}: ${NUGET_IDS.join(", ")} in ${NUGET_PINS}; ${UMBRELLA} in ${MANIFEST}`);
  console.log("next: npm ci && node scripts/pin-syncfusion.mjs --check, then python3 spec/tools/check.py, whose probes read the new version");
}

process.chdir(join(dirname(fileURLToPath(import.meta.url)), ".."));
const [argument] = process.argv.slice(2);
if (argument === "--check") {
  check();
} else if (argument) {
  pin(argument).catch(error => {
    console.error(`pin-syncfusion: ${error.message}`);
    process.exit(2);
  });
} else {
  console.error("usage: node scripts/pin-syncfusion.mjs <version> | --check");
  process.exit(2);
}
