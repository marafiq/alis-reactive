#!/usr/bin/env node
// Pins Syncfusion EJ2 to one version across npm and NuGet, and proves the pins agree.
//
//   node scripts/pin-syncfusion.mjs <version>   set every pin to <version> (needs network)
//   node scripts/pin-syncfusion.mjs --check     verify the pins agree (offline; scripts/test.sh runs it)
//
// The framework supports exactly one Syncfusion version. It is pinned in three places that must
// move together: the npm umbrella `@syncfusion/ej2` (Alis.Reactive.Assets/package.json, the browser
// bundle and CSS), and `Syncfusion.EJ2.AspNet.Core` + `Syncfusion.EJ2.MVC5` (Alis.Reactive.Fusion.csproj).
// The umbrella depends on ~50 control packages through `~` ranges, which npm resolves to the NEWEST
// patch of the line, not the set the umbrella was published with. So every control package is pinned
// exactly through root `overrides`, copied from the umbrella's own dependency list.
//
// npm ignores new `overrides` for packages an existing package-lock.json already resolved (observed
// with npm 11.17 during the 32.2.8 -> 33.1.47 upgrade: 22 control packages stayed on 33.1.49). So the
// Syncfusion part of the lock is resolved from scratch in a temporary directory and spliced into the
// real lock; every non-Syncfusion lock entry is left untouched. Afterwards run `npm ci` and --check.

import { execFileSync } from "node:child_process";
import { cpSync, mkdtempSync, mkdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";

const ASSETS_PACKAGE = "Alis.Reactive.Assets/package.json";
const FUSION_CSPROJ = "Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj";
const NUGET_IDS = ["Syncfusion.EJ2.AspNet.Core", "Syncfusion.EJ2.MVC5"];
const UMBRELLA = "@syncfusion/ej2";
const SYNCFUSION_LOCK_ENTRY = /node_modules\/(@syncfusion\/[^/]+)$/;

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

function lockedSyncfusionVersions(lock) {
  const versions = new Map();
  for (const [key, meta] of Object.entries(lock.packages)) {
    const name = SYNCFUSION_LOCK_ENTRY.exec(key)?.[1];
    if (!name) continue;
    if (!versions.has(name)) versions.set(name, new Set());
    versions.get(name).add(meta.version);
  }
  return versions;
}

function check() {
  const problems = [];
  const npmPin = readJson(ASSETS_PACKAGE).devDependencies?.[UMBRELLA];
  const csproj = readFileSync(FUSION_CSPROJ, "utf8");
  const pins = { [`${ASSETS_PACKAGE} ${UMBRELLA}`]: npmPin };
  for (const id of NUGET_IDS) pins[`${FUSION_CSPROJ} ${id}`] = csprojVersion(csproj, id);
  const distinct = new Set(Object.values(pins));
  if (distinct.size !== 1 || distinct.has(null) || distinct.has(undefined)) {
    problems.push(`the three Syncfusion pins disagree: ${Object.entries(pins).map(([where, v]) => `${where}=${v}`).join(", ")}`);
  }

  const overrides = Object.fromEntries(Object.entries(readJson("package.json").overrides ?? {})
    .filter(([name]) => name.startsWith("@syncfusion/")));
  const expected = { ...overrides, [UMBRELLA]: npmPin };
  const locked = lockedSyncfusionVersions(readJson("package-lock.json"));
  for (const [name, versions] of locked) {
    const got = [...versions];
    if (!(name in expected)) problems.push(`${name} is in package-lock.json but has no pin in package.json overrides`);
    else if (got.length !== 1 || got[0] !== expected[name]) problems.push(`${name} is locked at ${got.join(",")} but pinned to ${expected[name]}`);
  }
  for (const name of Object.keys(expected)) {
    if (!locked.has(name)) problems.push(`${name} is pinned but absent from package-lock.json`);
  }

  if (problems.length > 0) {
    console.error("pin-syncfusion --check: FAILED");
    for (const problem of problems) console.error(`  - ${problem}`);
    console.error("Fix: node scripts/pin-syncfusion.mjs <version>, then npm ci.");
    process.exit(1);
  }
  console.log(`pin-syncfusion --check: Syncfusion ${npmPin} pinned in npm and NuGet; ${locked.size} npm packages locked at their pins`);
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

// Returns the fresh lock entries this script owns: every @syncfusion package, plus each workspace's
// own entry (it records the workspace's devDependencies, including the @syncfusion/ej2 pin).
function resolveSyncfusionLockEntries(rootPackage) {
  const scratch = mkdtempSync(join(tmpdir(), "pin-syncfusion-"));
  try {
    writeJson(join(scratch, "package.json"), rootPackage);
    for (const workspace of rootPackage.workspaces ?? []) {
      mkdirSync(join(scratch, workspace), { recursive: true });
      cpSync(join(workspace, "package.json"), join(scratch, workspace, "package.json"));
    }
    execFileSync("npm", ["install", "--package-lock-only", "--ignore-scripts", "--no-audit", "--no-fund"],
      { cwd: scratch, stdio: "ignore" });
    const fresh = readJson(join(scratch, "package-lock.json"));
    const workspaces = new Set(rootPackage.workspaces ?? []);
    return Object.fromEntries(Object.entries(fresh.packages)
      .filter(([key]) => SYNCFUSION_LOCK_ENTRY.test(key) || workspaces.has(key)));
  } finally {
    rmSync(scratch, { recursive: true, force: true });
  }
}

async function pin(version) {
  if (!/^\d+\.\d+\.\d+$/.test(version)) throw new Error(`expected a version like 33.1.47, got '${version}'`);
  if (npmView(`${UMBRELLA}@${version}`, "version") !== version) throw new Error(`${UMBRELLA}@${version} is not on npm`);
  for (const id of NUGET_IDS) await assertNugetVersionExists(id, version);

  const assets = readJson(ASSETS_PACKAGE);
  assets.devDependencies[UMBRELLA] = version;
  writeJson(ASSETS_PACKAGE, assets);

  let csproj = readFileSync(FUSION_CSPROJ, "utf8");
  for (const id of NUGET_IDS) {
    const pattern = new RegExp(`(Include="${id.replace(/\./g, "\\.")}" Version=")[^"]+(")`);
    if (!pattern.test(csproj)) throw new Error(`${FUSION_CSPROJ} has no PackageReference for ${id}`);
    csproj = csproj.replace(pattern, `$1${version}$2`);
  }
  writeFileSync(FUSION_CSPROJ, csproj);

  const controls = npmView(`${UMBRELLA}@${version}`, "dependencies");
  const root = readJson("package.json");
  const kept = Object.entries(root.overrides ?? {}).filter(([name]) => !name.startsWith("@syncfusion/"));
  const pinned = Object.entries(controls).map(([name, range]) => [name, range.replace(/^[~^]/, "")]);
  root.overrides = Object.fromEntries([...kept, ...pinned].sort(([a], [b]) => a.localeCompare(b)));
  writeJson("package.json", root);

  const lock = readJson("package-lock.json");
  const entries = resolveSyncfusionLockEntries(root);
  const packages = { "": lock.packages[""] };
  const keys = [...Object.keys(lock.packages).filter(key => key !== "" && !SYNCFUSION_LOCK_ENTRY.test(key) && !(key in entries)),
    ...Object.keys(entries)].sort();
  for (const key of keys) packages[key] = entries[key] ?? lock.packages[key];
  lock.packages = packages;
  writeJson("package-lock.json", lock);

  const syncfusionEntries = Object.keys(entries).filter(key => SYNCFUSION_LOCK_ENTRY.test(key)).length;
  console.log(`pinned Syncfusion ${version}: ${UMBRELLA}, ${NUGET_IDS.join(", ")}, ${pinned.length} control packages; ${syncfusionEntries} lock entries re-resolved`);
  console.log("next: npm ci && node scripts/pin-syncfusion.mjs --check && scripts/test.sh --parallel");
}

process.chdir(join(dirname(new URL(import.meta.url).pathname), ".."));
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
