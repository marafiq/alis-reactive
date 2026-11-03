#!/usr/bin/env node
// Pins Syncfusion EJ2 to one version across NuGet and npm, and proves the pins agree.
//
//   node scripts/pin-syncfusion.mjs <version>   set every pin to <version> (needs network)
//   node scripts/pin-syncfusion.mjs --check     verify the pins agree (offline; scripts/test.sh runs it)
//
// The framework supports exactly one Syncfusion version: Alis.Reactive.Fusion.csproj pins
// `Syncfusion.EJ2.AspNet.Core` and `Syncfusion.EJ2.MVC5` as [version] (exact; a bare version is only a
// minimum). No framework package ships Syncfusion's browser files: each app that renders Fusion
// components depends on the npm umbrella `@syncfusion/ej2` at the same version and copies
// dist/ej2.min.js and tailwind3.css into its wwwroot (its copy-vendor.mjs). The apps in this
// repository are listed below with the lock that resolves each; nothing imports a control package,
// so none is pinned.

import { execFileSync } from "node:child_process";
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const FUSION_CSPROJ = "Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj";
const NUGET_IDS = ["Syncfusion.EJ2.AspNet.Core", "Syncfusion.EJ2.MVC5"];
const UMBRELLA = "@syncfusion/ej2";
const APPS = [
  { manifest: "Alis.Reactive.SandboxApp/package.json", lock: "package-lock.json" },
];

function readJson(path) {
  return JSON.parse(readFileSync(path, "utf8"));
}

function writeJson(path, value) {
  writeFileSync(path, JSON.stringify(value, null, 2) + "\n");
}

function csprojVersion(csproj, id) {
  const match = new RegExp(`Include="${id.replace(/\./g, "\\.")}" Version="([^"]+)"`).exec(csproj);
  return match ? match[1] : null;
}

function check() {
  const problems = [];
  const csproj = readFileSync(FUSION_CSPROJ, "utf8");
  const pin = csprojVersion(csproj, NUGET_IDS[0])?.replace(/^\[|\]$/g, "");
  if (!/^\d+\.\d+\.\d+$/.test(pin ?? "")) {
    problems.push(`${FUSION_CSPROJ} ${NUGET_IDS[0]} must pin an exact [version], got ${csprojVersion(csproj, NUGET_IDS[0])}`);
  }
  for (const id of NUGET_IDS) {
    const nugetPin = csprojVersion(csproj, id);
    if (nugetPin !== `[${pin}]`) problems.push(`${FUSION_CSPROJ} ${id} must use [${pin}], got ${nugetPin}`);
  }
  for (const app of APPS) {
    const npmPin = readJson(app.manifest).dependencies?.[UMBRELLA];
    if (npmPin !== pin) problems.push(`${app.manifest} ${UMBRELLA} must be ${pin}, got ${npmPin}`);
    const locked = readJson(app.lock).packages[`node_modules/${UMBRELLA}`]?.version;
    if (locked !== pin) problems.push(`${app.lock} resolves ${UMBRELLA} to ${locked}, pinned to ${pin}`);
  }

  if (problems.length > 0) {
    console.error("pin-syncfusion --check: FAILED");
    for (const problem of problems) console.error(`  - ${problem}`);
    console.error("Fix: node scripts/pin-syncfusion.mjs <version>, then npm ci.");
    process.exit(1);
  }
  console.log(`pin-syncfusion --check: Syncfusion ${pin} pinned in NuGet and in ${APPS.length} app manifest(s) and lock(s)`);
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

  let csproj = readFileSync(FUSION_CSPROJ, "utf8");
  for (const id of NUGET_IDS) {
    const pattern = new RegExp(`(Include="${id.replace(/\./g, "\\.")}" Version=")[^"]+(")`);
    if (!pattern.test(csproj)) throw new Error(`${FUSION_CSPROJ} has no PackageReference for ${id}`);
    csproj = csproj.replace(pattern, `$1[${version}]$2`);
  }
  writeFileSync(FUSION_CSPROJ, csproj);

  for (const app of APPS) {
    const manifest = readJson(app.manifest);
    manifest.dependencies[UMBRELLA] = version;
    writeJson(app.manifest, manifest);
    execFileSync("npm", ["install", "--package-lock-only", "--ignore-scripts", "--no-audit", "--no-fund"],
      { cwd: dirname(app.lock), stdio: "inherit" });
  }

  console.log(`pinned Syncfusion ${version}: ${NUGET_IDS.join(", ")}; ${UMBRELLA} in ${APPS.map(app => app.manifest).join(", ")}`);
  console.log("next: npm ci && node scripts/pin-syncfusion.mjs --check && scripts/test.sh --parallel");
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
