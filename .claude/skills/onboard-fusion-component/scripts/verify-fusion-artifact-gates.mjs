#!/usr/bin/env node

import { existsSync, readFileSync } from "node:fs";
import { join, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const component = requireArg(args, "component");
const root = resolve(args.root ?? `tools/FusionOnboarding/wwwroot/onboarding/fusion/${component}`);

const required = [
  ["master", "master-usecases-index.md"],
  ["source inventory", "discovery/source-inventory.md"],
  ["MVC builder coverage", "discovery/mvc-builder-coverage.md"],
  ["Blazor candidates", "discovery/blazor-candidates.md"],
  ["public API surface", "discovery/public-api-surface.json"],
  ["event payload surface", "discovery/event-payload-surface.json"],
  ["primitive map", "mapping/primitive-map.md"],
  ["C# name decisions", "mapping/csharp-name-decisions.md"],
  ["vertical slice plan", "mapping/vertical-slice-plan.md"],
  ["typed API coverage matrix", "proof/typed-api-coverage-matrix.md"],
  ["Playwright proof", "proof/playwright-proof.md"],
  ["audit report", "proof/audit-report.md"]
];

const files = required.map(([label, path]) => ({
  label,
  path,
  exists: existsSync(join(root, path))
}));
const master = readIfExists(join(root, "master-usecases-index.md"));
const problems = [];

for (const file of files) {
  if (!file.exists) problems.push(`missing ${file.label}: ${file.path}`);
  if (file.exists && !master.includes(file.path) && file.path !== "master-usecases-index.md") {
    problems.push(`master-usecases-index.md does not link ${file.path}`);
  }
}

const probeLinks = linksWithPrefix(master, "probes/raw-ej2-");
const traceLinks = linksWithPrefix(master, "traces/raw-ej2-");
if (probeLinks.length === 0) problems.push("master-usecases-index.md links no raw EJ2 probe");
if (traceLinks.length === 0) problems.push("master-usecases-index.md links no raw EJ2 trace");

for (const link of probeLinks) {
  if (!existsSync(join(root, link))) problems.push(`missing linked raw EJ2 probe: ${link}`);
}
for (const link of traceLinks) {
  if (!existsSync(join(root, link))) problems.push(`missing linked raw EJ2 trace: ${link}`);
}

const publicSurface = parseJsonIfExists(join(root, "discovery/public-api-surface.json"));
const eventSurface = parseJsonIfExists(join(root, "discovery/event-payload-surface.json"));
if (publicSurface && publicSurface.status !== "static-discovery") {
  problems.push(`unexpected public-api-surface status: ${publicSurface.status}`);
}
if (eventSurface && eventSurface.status !== "static-discovery") {
  problems.push(`unexpected event-payload-surface status: ${eventSurface.status}`);
}

if (problems.length > 0) {
  console.error(`# Fusion artifact gate check failed for ${component}`);
  console.error("");
  for (const problem of problems) console.error(`- ${problem}`);
  process.exit(1);
}

console.log(`# Fusion artifact gate check passed for ${component}`);
for (const file of files) console.log(`- ${file.path}`);
for (const link of [...probeLinks, ...traceLinks]) console.log(`- ${link}`);

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

function requireArg(values, name) {
  const value = values[name];
  if (typeof value === "string" && value.trim().length > 0) return value.trim();
  console.error(`Missing --${name}`);
  process.exit(1);
}

function readIfExists(path) {
  return existsSync(path) ? readFileSync(path, "utf8") : "";
}

function parseJsonIfExists(path) {
  if (!existsSync(path)) return null;
  return JSON.parse(readFileSync(path, "utf8"));
}

function linksWithPrefix(markdown, prefix) {
  const links = new Set();
  for (const line of markdown.split(/\r?\n/)) {
    const markdownLinks = /\[[^\]]+\]\(([^)]+)\)/g;
    let markdownMatch;
    while ((markdownMatch = markdownLinks.exec(line)) !== null) {
      const link = markdownMatch[1] || "";
      if (link.startsWith(prefix)) links.add(link);
    }

    const inlineCodeLinks = /`([^`\n]+)`/g;
    let inlineMatch;
    while ((inlineMatch = inlineCodeLinks.exec(line)) !== null) {
      const link = inlineMatch[1] || "";
      if (link.startsWith(prefix)) links.add(link);
    }
  }
  return Array.from(links).sort();
}
