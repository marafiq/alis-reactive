#!/usr/bin/env node

import { existsSync, readFileSync } from "node:fs";
import { spawnSync } from "node:child_process";
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

// Status markers that mean "not closed". Checked on a file's `Status:` line ONLY
// (never the whole file) so legitimate prose like "partial injection" or "some
// residents" is never a false positive. The lie this catches is goal loophole 2:
// a component whose proof STATUS still says partial/wip/stub while the matrix
// claims audited. matrix-status (must be `audited`) AND proof-status (must be
// closed) together force agreement — they cannot disagree and still pass.
const OPEN_STATUS = /\b(partial|wip|stub|todo[- ]?later|some|pending|unproven|incomplete|in progress|not (?:started|complete)|draft)\b/i;

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
if (eventSurface) {
  const unresolvedPayloads = findUnresolvedPayloads(eventSurface);
  for (const payload of unresolvedPayloads) {
    problems.push(`event-payload-surface.json has unresolved payload type ${payload}`);
  }
}

for (const path of [
  "mapping/primitive-map.md",
  "mapping/csharp-name-decisions.md",
  "mapping/vertical-slice-plan.md",
  "proof/typed-api-coverage-matrix.md",
  "proof/playwright-proof.md",
  "proof/audit-report.md"
]) {
  const fullPath = join(root, path);
  if (!existsSync(fullPath)) continue;
  const text = readFileSync(fullPath, "utf8");
  if (path === "proof/typed-api-coverage-matrix.md") {
    const matrixProblems = matrixOpenMarkers(text);
    problems.push(...matrixProblems.map(problem => `${path} ${problem}`));
    continue;
  }
  if (/\b(pending|unproven|missing|todo|failed-closed|incomplete|not started|not complete)\b/i.test(text)) {
    problems.push(`${path} contains open, failed-closed, incomplete, pending, unproven, missing, or todo markers`);
  }
  const openStatus = statusLineOpenMarker(text);
  if (openStatus) {
    problems.push(`${path} status line declares open work ("${openStatus}"); a closed component states an audited/proven status`);
  }
}

const auditReport = readIfExists(join(root, "proof/audit-report.md"));
if (auditReport && !auditReport.includes("_skill/pattern-map.md")) {
  problems.push("proof/audit-report.md does not link the skill pattern map");
}

const matrix = readIfExists(join(root, "proof/typed-api-coverage-matrix.md"));
if (matrix) {
  const stats = matrixStats(matrix);
  verifyGeneratedMatrixIsCurrent(problems);
  verifyInventoryStatusIsCurrent(problems);
  verifyEditActionExclusionProofNarrative(
    matrix,
    readIfExists(join(root, "mapping/vertical-slice-plan.md")),
    problems);
  verifySummaryCounts("master-usecases-index.md", master, stats, problems);
  verifySummaryCounts("proof/audit-report.md", auditReport, stats, problems);
  verifyReferencedProofFilesExist("proof/playwright-proof.md", readIfExists(join(root, "proof/playwright-proof.md")), problems);
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

// Returns the open-status word found on the file's `Status:` line, or null.
function statusLineOpenMarker(text) {
  const line = text.split(/\r?\n/).find(item => /^\s*Status:/i.test(item));
  if (!line) return null;
  const match = line.match(OPEN_STATUS);
  return match ? match[0] : null;
}

function matrixOpenMarkers(markdown) {
  const output = [];
  const statusLine = markdown.split(/\r?\n/).find(line => line.startsWith("Status:")) ?? "";
  if (!/^Status:\s+audited\./.test(statusLine)) {
    output.push(`status is not audited: ${statusLine || "(missing status)"}`);
  }

  const rows = markdown
    .split(/\r?\n/)
    .filter(line => line.startsWith("| `"));
  const openRows = rows.filter(row => {
    const cells = row.split("|").map(cell => cell.trim()).filter(Boolean);
    const status = cells[cells.length - 1] ?? "";
    return status !== "row-proven";
  });
  if (openRows.length > 0) {
    output.push(`has ${openRows.length} matrix rows without row-proven status`);
  }
  return output;
}

function matrixStats(markdown) {
  const rows = markdown
    .split(/\r?\n/)
    .filter(line => line.startsWith("| `"));
  const supplementalRows = rows.filter(row => {
    const name = firstMatrixCell(row);
    return name.includes("/") || name.startsWith("remote-data:") || name.startsWith("data-source:");
  });
  const provenRows = rows.filter(row => {
    const cells = row.split("|").map(cell => cell.trim()).filter(Boolean);
    return cells[cells.length - 1] === "row-proven";
  });
  return {
    typed: rows.length - supplementalRows.length,
    supplemental: supplementalRows.length,
    total: rows.length,
    proven: provenRows.length,
    unproven: rows.length - provenRows.length
  };
}

function verifyEditActionExclusionProofNarrative(matrixMarkdown, verticalSlicePlan, output) {
  if (!verticalSlicePlan) return;

  const grouped = new Map();
  for (const row of parseMatrixRows(matrixMarkdown)) {
    if (row.kind !== "event-payload-variant-exclusion" || row.status !== "row-proven") continue;
    const match = row.name.match(/^(actionBegin|actionComplete)\/save-edit:\s+FusionGridEditActionArgs\.([A-Za-z0-9_]+)$/);
    if (!match) continue;
    const [, eventName, member] = match;
    if (!grouped.has(eventName)) grouped.set(eventName, new Set());
    grouped.get(eventName).add(member);
  }

  for (const [eventName, members] of grouped) {
    const section = proofSection(verticalSlicePlan, `${capitalize(eventName)} save/edit variant proof:`);
    if (!section) {
      output.push(`mapping/vertical-slice-plan.md missing ${eventName} save/edit variant proof section for row-proven exclusion rows`);
      continue;
    }

    for (const member of members) {
      if (!section.includes(`\`${member}\``)) {
        output.push(`mapping/vertical-slice-plan.md ${eventName} save/edit proof does not name row-proven excluded member ${member}`);
      }
    }
  }
}

function parseMatrixRows(markdown) {
  return markdown
    .split(/\r?\n/)
    .filter(line => line.startsWith("| `"))
    .map(line => {
      const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
      return {
        name: stripBackticks(cells[0] ?? ""),
        kind: cells[1] ?? "",
        status: cells[cells.length - 1] ?? ""
      };
    });
}

function proofSection(markdown, heading) {
  const start = markdown.indexOf(heading);
  if (start < 0) return "";
  const rest = markdown.slice(start);
  const next = rest.slice(heading.length).search(/\n[A-Z][^\n]+:\n/);
  return next < 0 ? rest : rest.slice(0, heading.length + next);
}

function stripBackticks(value) {
  return value.replace(/^`|`$/g, "");
}

function capitalize(value) {
  return value.length === 0 ? value : value[0].toUpperCase() + value.slice(1);
}

function firstMatrixCell(row) {
  const cells = row.split("|").map(cell => cell.trim()).filter(Boolean);
  return (cells[0] ?? "").replace(/^`|`$/g, "");
}

function verifySummaryCounts(label, text, stats, output) {
  if (!text) return;
  const typed = readCount(text, "Typed C# API rows");
  const supplemental = readCount(text, "Supplemental audit rows");
  const total = readCount(text, "Total typed coverage matrix rows");
  if (label === "master-usecases-index.md") {
    if (typed === null) output.push(`${label} missing count row: Typed C# API rows`);
    if (supplemental === null) output.push(`${label} missing count row: Supplemental audit rows`);
    if (total === null) output.push(`${label} missing count row: Total typed coverage matrix rows`);
  }
  if (typed !== null && typed !== stats.typed) {
    output.push(`${label} count drift: Typed C# API rows is ${typed}, generated matrix has ${stats.typed}`);
  }
  if (supplemental !== null && supplemental !== stats.supplemental) {
    output.push(`${label} count drift: Supplemental audit rows is ${supplemental}, generated matrix has ${stats.supplemental}`);
  }
  if (total !== null && total !== stats.total) {
    output.push(`${label} count drift: Total typed coverage matrix rows is ${total}, generated matrix has ${stats.total}`);
  }

  const auditMatch = text.match(/Current generated matrix count:\s+(\d+)\s+typed C# API rows,\s+(\d+)\s+supplemental audit\s+rows,\s+(\d+)\s+total rows\.[\s\S]*?shows\s+(\d+)\s+row-proven\s+matrix rows and\s+(\d+)\s+matrix rows without `row-proven` status\./);
  if (label === "proof/audit-report.md" && !auditMatch) {
    output.push(`${label} missing current generated matrix count summary`);
  }
  if (auditMatch) {
    const [, auditTyped, auditSupplemental, auditTotal, auditProven, auditUnproven] = auditMatch.map(Number);
    if (auditTyped !== stats.typed) output.push(`${label} count drift: audit typed count is ${auditTyped}, generated matrix has ${stats.typed}`);
    if (auditSupplemental !== stats.supplemental) output.push(`${label} count drift: audit supplemental count is ${auditSupplemental}, generated matrix has ${stats.supplemental}`);
    if (auditTotal !== stats.total) output.push(`${label} count drift: audit total count is ${auditTotal}, generated matrix has ${stats.total}`);
    if (auditProven !== stats.proven) output.push(`${label} count drift: audit row-proven count is ${auditProven}, generated matrix has ${stats.proven}`);
    if (auditUnproven !== stats.unproven) output.push(`${label} count drift: audit unproven count is ${auditUnproven}, generated matrix has ${stats.unproven}`);
  }
}

function verifyGeneratedMatrixIsCurrent(output) {
  const script = resolve(".claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs");
  const result = spawnSync(
    process.execPath,
    [
      script,
      "--component",
      component,
      "--root",
      root,
      "--check"
    ],
    {
      cwd: process.cwd(),
      encoding: "utf8"
    });
  if (result.status === 0) return;

  const details = [result.stderr, result.stdout]
    .filter(Boolean)
    .join("\n")
    .trim()
    .split(/\r?\n/)
    .filter(Boolean)
    .slice(0, 6)
    .join(" ");
  output.push(`proof/typed-api-coverage-matrix.md is stale or not generated from current source${details ? `: ${details}` : ""}`);
}

function verifyInventoryStatusIsCurrent(output) {
  const script = resolve(".claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs");
  const result = spawnSync(
    process.execPath,
    [
      script,
      "--check"
    ],
    {
      cwd: process.cwd(),
      encoding: "utf8"
    });
  if (result.status === 0) return;

  const details = [result.stderr, result.stdout]
    .filter(Boolean)
    .join("\n")
    .trim()
    .split(/\r?\n/)
    .filter(Boolean)
    .slice(0, 6)
    .join(" ");
  output.push(`_inventory/onboarding-status.* is stale or not generated from current source${details ? `: ${details}` : ""}`);
}

function readCount(text, label) {
  const escaped = label.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const match = text.match(new RegExp(`\\|\\s*${escaped}\\s*\\|\\s*(\\d+)\\s*\\|`));
  return match ? Number(match[1]) : null;
}

function verifyReferencedProofFilesExist(label, text, output) {
  if (!text) return;
  const expression = /`(tests\/Alis\.Reactive\.PlaywrightTests\/TestResults\/observable\/playwright-[^`]+\.trx)`/g;
  let match;
  while ((match = expression.exec(text)) !== null) {
    const proofPath = match[1];
    if (!existsSync(resolve(proofPath))) {
      output.push(`${label} references missing proof file: ${proofPath}`);
    }
  }
}

function findUnresolvedPayloads(eventSurface) {
  const output = [];
  for (const event of eventSurface.events ?? []) {
    for (const payload of event.payloadTypes ?? []) {
      if (["ambiguous", "not-found", "cycle"].includes(payload.status)) {
        const candidates = (payload.candidates ?? []).join(", ");
        output.push(`${event.event}.${payload.name} status=${payload.status}${candidates ? ` (${candidates})` : ""}`);
      }
    }
  }
  return output;
}
