#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const repoRoot = resolve(args.root ?? ".");
const write = args.write === true || args.write === "true";
const check = args.check === true || args.check === "true";
const fusionRoot = join(repoRoot, "Alis.Reactive.Fusion/Components");
const artifactRoot = join(repoRoot, "tools/FusionOnboarding/wwwroot/onboarding/fusion");
const outputJson = join(artifactRoot, "_inventory/onboarding-status.json");
const outputMarkdown = join(artifactRoot, "_inventory/onboarding-status.md");

const requiredFiles = [
  "master-usecases-index.md",
  "discovery/source-inventory.md",
  "discovery/mvc-builder-coverage.md",
  "discovery/blazor-candidates.md",
  "discovery/public-api-surface.json",
  "discovery/event-payload-surface.json",
  "mapping/primitive-map.md",
  "mapping/csharp-name-decisions.md",
  "mapping/vertical-slice-plan.md",
  "proof/typed-api-coverage-matrix.md",
  "proof/playwright-proof.md",
  "proof/audit-report.md"
];

if (!existsSync(fusionRoot)) {
  console.error(`Fusion component root not found: ${fusionRoot}`);
  process.exit(1);
}

const components = readdirSync(fusionRoot)
  .filter(name => name.startsWith("Fusion"))
  .filter(name => statSync(join(fusionRoot, name)).isDirectory())
  .sort()
  .map(componentType => componentStatus(componentType));

const summary = summarize(components);
const report = {
  status: "fusion-onboarding-status",
  generatedBy: ".claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs",
  artifactRoot: toRepoPath(artifactRoot),
  summary,
  components
};

const jsonText = `${JSON.stringify(report, null, 2)}\n`;
const markdownText = markdown(report);

if (check) {
  assertCurrent(outputJson, jsonText);
  assertCurrent(outputMarkdown, markdownText);
  console.log(`${toRepoPath(outputMarkdown)} and ${toRepoPath(outputJson)} are current.`);
  process.exit(0);
}

if (write) {
  writeFile(outputJson, jsonText);
  writeFile(outputMarkdown, markdownText);
}

print(report, write);

function componentStatus(componentType) {
  const componentName = componentType.replace(/^Fusion/, "");
  const artifactName = kebab(componentName);
  const root = join(artifactRoot, artifactName);
  const master = readIfExists(join(root, "master-usecases-index.md"));
  const required = requiredFiles.map(path => {
    const fullPath = join(root, path);
    const exists = existsSync(fullPath);
    return {
      path,
      exists,
      linkedFromMaster: path === "master-usecases-index.md" || Boolean(master && master.includes(path))
    };
  });
  const missingRequired = required.filter(item => !item.exists).map(item => item.path);
  const unlinkedRequired = required
    .filter(item => item.exists && !item.linkedFromMaster)
    .map(item => item.path);
  const probes = listFiles(join(root, "probes"), /^raw-ej2-.+\.html$/);
  const traces = listFiles(join(root, "traces"), /^raw-ej2-.+\.trace\.json$/);
  const matrix = matrixStatus(join(root, "proof/typed-api-coverage-matrix.md"));
  const auditReport = readIfExists(join(root, "proof/audit-report.md"));
  const openAuditMarkers = Boolean(auditReport) &&
    /\b(pending|unproven|missing|todo|failed-closed|incomplete|not started|not complete)\b/i.test(auditReport);
  const status = mechanicalStatus({ missingRequired, unlinkedRequired, probes, traces, matrix, openAuditMarkers });
  const nextAction = nextActionFor({
    artifactName,
    missingRequired,
    unlinkedRequired,
    probes,
    traces,
    matrix,
    openAuditMarkers,
    status
  });

  return {
    componentType,
    componentName,
    artifactName,
    artifactRoot: toRepoPath(root),
    requiredPresent: required.filter(item => item.exists).length,
    requiredTotal: required.length,
    missingRequired,
    unlinkedRequired,
    probeCount: probes.length,
    traceCount: traces.length,
    matrix,
    openAuditMarkers,
    mechanicalStatus: status,
    nextAction
  };
}

function mechanicalStatus({ missingRequired, unlinkedRequired, probes, traces, matrix, openAuditMarkers }) {
  if (missingRequired.includes("master-usecases-index.md")) return "missing-master";
  if (missingRequired.length > 0) return "artifact-incomplete";
  if (unlinkedRequired.length > 0) return "artifact-unlinked";
  if (probes.length === 0) return "missing-raw-probe";
  if (traces.length === 0) return "missing-raw-trace";
  if (!matrix.exists) return "missing-matrix";
  if (matrix.unprovenRows > 0 || matrix.status !== "audited") return "matrix-fail-closed";
  if (openAuditMarkers) return "audit-report-open";
  return "audited";
}

function matrixStatus(path) {
  if (!existsSync(path)) {
    return {
      exists: false,
      status: "missing",
      totalRows: 0,
      rowProvenRows: 0,
      unprovenRows: 0,
      firstUnprovenRow: null
    };
  }

  const text = readFileSync(path, "utf8");
  const statusLine = text.split(/\r?\n/).find(line => line.startsWith("Status:")) ?? "";
  const rows = text.split(/\r?\n/).filter(line => line.startsWith("| `"));
  const parsedRows = rows.map(parseMatrixRow).filter(Boolean);
  const unprovenRows = parsedRows.filter(row => row.status !== "row-proven");
  const rowProvenRows = parsedRows.length - unprovenRows.length;
  const firstUnprovenRow = unprovenRows[0] ?? null;
  return {
    exists: true,
    status: /^Status:\s+audited\./.test(statusLine) ? "audited" : "unproven",
    totalRows: parsedRows.length,
    rowProvenRows,
    unprovenRows: unprovenRows.length,
    firstUnprovenRow
  };
}

function parseMatrixRow(row) {
  const cells = row.split("|").map(cell => cell.trim()).filter(Boolean);
  if (cells.length < 8) return null;
  return {
    publicApi: cells[0].replace(/^`|`$/g, ""),
    kind: cells[1],
    source: cells[2].replace(/^`|`$/g, ""),
    rawTrace: cells[3],
    primitiveMap: cells[4],
    verticalSlice: cells[5],
    playwrightProof: cells[6],
    status: cells[7]
  };
}

function isAggregateMatrixRow(row) {
  const text = [
    row.rawTrace,
    row.primitiveMap,
    row.verticalSlice,
    row.playwrightProof
  ].join(" ");
  return /\bpending variant matrix\b/i.test(text) ||
    /\bpending complete\b/i.test(text) ||
    /\bbroad .+ remains open\b/i.test(text) ||
    /\brequires variant-scoped\b/i.test(text) ||
    /\bproperty proof must name\b/i.test(text) ||
    /\bshared .+ remains open\b/i.test(text);
}

function nextActionFor({ artifactName, missingRequired, unlinkedRequired, probes, traces, matrix, openAuditMarkers, status }) {
  if (status === "missing-master") {
    return {
      stage: "inventory",
      action: "write component inventory artifacts",
      command: "node .claude/skills/onboard-fusion-component/scripts/inventory-fusion-components.mjs --write",
      reason: "component has no master-usecases-index.md"
    };
  }

  if (missingRequired.length > 0) {
    const firstMissing = missingRequired[0];
    return {
      stage: stageForMissingArtifact(firstMissing),
      action: `create missing artifact ${firstMissing}`,
      command: commandForMissingArtifact(artifactName, firstMissing),
      reason: `${missingRequired.length} required artifact files are missing`
    };
  }

  if (unlinkedRequired.length > 0) {
    return {
      stage: "artifact-sync",
      action: `link existing artifact ${unlinkedRequired[0]} from master-usecases-index.md`,
      command: "manual artifact sync required",
      reason: `${unlinkedRequired.length} required artifacts exist but are not linked from the master index`
    };
  }

  if (probes.length === 0) {
    return {
      stage: "raw-ej2-probe",
      action: "create first raw EJ2 probe",
      command: `node .claude/skills/onboard-fusion-component/scripts/create-fusion-probe.mjs --component ${artifactName}`,
      reason: "no raw probe exists"
    };
  }

  if (traces.length === 0) {
    return {
      stage: "raw-ej2-trace",
      action: "run first raw EJ2 trace",
      command: `node .claude/skills/onboard-fusion-component/scripts/run-fusion-probe-trace.mjs --component ${artifactName} --api-set core`,
      reason: "raw probe exists but no trace exists"
    };
  }

  if (!matrix.exists) {
    return {
      stage: "typed-api-matrix",
      action: "generate typed API coverage matrix",
      command: `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component ${artifactName} --write`,
      reason: "no typed API coverage matrix exists"
    };
  }

  if (matrix.unprovenRows > 0 || matrix.status !== "audited") {
    const row = matrix.firstUnprovenRow;
    if (row && isAggregateMatrixRow(row)) {
      return {
        stage: "variant-discovery",
        action: `decompose aggregate matrix row ${row.publicApi} into the next missing variant or lane row`,
        command: "derive the missing variant/lane from raw EJ2/source evidence, then follow one-row proof chain: raw trace -> judgment -> primitive map -> vertical slice -> typed DSL Playwright -> matrix",
        reason: `${matrix.unprovenRows} matrix rows remain unproven; the first gap is aggregate and cannot be closed directly`
      };
    }
    return {
      stage: "row-proof",
      action: row
        ? `close matrix row ${row.publicApi}`
        : "close next unproven matrix row",
      command: "follow one-row proof chain: raw trace -> judgment -> primitive map -> vertical slice -> typed DSL Playwright -> matrix",
      reason: `${matrix.unprovenRows} matrix rows remain unproven`
    };
  }

  if (openAuditMarkers) {
    return {
      stage: "audit-closeout",
      action: "remove stale open markers from audit report only after evidence proves closure",
      command: `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component ${artifactName}`,
      reason: "matrix is audited but audit report still contains open markers"
    };
  }

  return {
    stage: "audited",
    action: "none",
    command: `node .claude/skills/onboard-fusion-component/scripts/verify-fusion-artifact-gates.mjs --component ${artifactName}`,
    reason: "component is mechanically audited"
  };
}

function stageForMissingArtifact(path) {
  if (path === "discovery/source-inventory.md") return "inventory";
  if (path.startsWith("discovery/")) return "static-discovery";
  if (path.startsWith("mapping/")) return "mapping";
  if (path === "proof/typed-api-coverage-matrix.md") return "typed-api-matrix";
  if (path.startsWith("proof/")) return "behavior-proof";
  return "artifact-tree";
}

function commandForMissingArtifact(component, path) {
  if (path === "discovery/source-inventory.md") {
    return "node .claude/skills/onboard-fusion-component/scripts/inventory-fusion-components.mjs --write";
  }
  if (path === "discovery/public-api-surface.json" || path === "discovery/event-payload-surface.json" || path === "discovery/mvc-builder-coverage.md" || path === "discovery/blazor-candidates.md") {
    return `node .claude/skills/onboard-fusion-component/scripts/write-fusion-discovery-artifacts.mjs --component ${component} --class <SyncfusionClass> --namespace <ej.namespace> --dts <path> --xml <path> --write`;
  }
  if (path === "proof/typed-api-coverage-matrix.md") {
    return `node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component ${component} --write`;
  }
  return "complete the previous gate before writing this artifact";
}

function summarize(items) {
  const byStatus = {};
  for (const item of items) {
    byStatus[item.mechanicalStatus] = (byStatus[item.mechanicalStatus] ?? 0) + 1;
  }
  const byNextStage = {};
  for (const item of items) {
    byNextStage[item.nextAction.stage] = (byNextStage[item.nextAction.stage] ?? 0) + 1;
  }
  return {
    componentCount: items.length,
    audited: items.filter(item => item.mechanicalStatus === "audited").length,
    withTypedMatrix: items.filter(item => item.matrix.exists).length,
    totalMatrixRows: sum(items, item => item.matrix.totalRows),
    rowProvenMatrixRows: sum(items, item => item.matrix.rowProvenRows),
    unprovenMatrixRows: sum(items, item => item.matrix.unprovenRows),
    totalRawProbes: sum(items, item => item.probeCount),
    totalRawTraces: sum(items, item => item.traceCount),
    byStatus,
    byNextStage
  };
}

function markdown(report) {
  return `# Fusion Onboarding Automation Status

Generated by:

\`\`\`bash
node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write
\`\`\`

Status: mechanical inventory. This report does not prove onboarding completion;
it makes the current cross-component gap measurable and fail-closed.

| Metric | Count |
|---|---:|
| Components | ${report.summary.componentCount} |
| Audited components | ${report.summary.audited} |
| Components with typed matrix | ${report.summary.withTypedMatrix} |
| Total matrix rows | ${report.summary.totalMatrixRows} |
| Row-proven matrix rows | ${report.summary.rowProvenMatrixRows} |
| Unproven matrix rows | ${report.summary.unprovenMatrixRows} |
| Raw probes | ${report.summary.totalRawProbes} |
| Raw traces | ${report.summary.totalRawTraces} |

## Status Counts

| Status | Components |
|---|---:|
${Object.entries(report.summary.byStatus).sort(([left], [right]) => left.localeCompare(right)).map(([status, count]) => `| ${status} | ${count} |`).join("\n")}

## Next Action Counts

| Next Stage | Components |
|---|---:|
${Object.entries(report.summary.byNextStage).sort(([left], [right]) => left.localeCompare(right)).map(([stage, count]) => `| ${stage} | ${count} |`).join("\n")}

## Components

| Component | Artifact | Status | Next Stage | Next Action | Required | Probes | Traces | Matrix | Row-Proven | Unproven |
|---|---|---|---|---|---:|---:|---:|---:|---:|---:|
${report.components.map(item => `| ${item.componentType} | \`${item.artifactName}/\` | ${item.mechanicalStatus} | ${item.nextAction.stage} | ${escapePipe(item.nextAction.action)} | ${item.requiredPresent}/${item.requiredTotal} | ${item.probeCount} | ${item.traceCount} | ${item.matrix.totalRows} | ${item.matrix.rowProvenRows} | ${item.matrix.unprovenRows} |`).join("\n")}
`;
}

function print(report, wrote) {
  console.log("# Fusion Onboarding Automation Status");
  console.log("");
  console.log(`Components: ${report.summary.componentCount}`);
  console.log(`Audited components: ${report.summary.audited}`);
  console.log(`Components with typed matrix: ${report.summary.withTypedMatrix}`);
  console.log(`Matrix rows: ${report.summary.totalMatrixRows}`);
  console.log(`Row-proven matrix rows: ${report.summary.rowProvenMatrixRows}`);
  console.log(`Unproven matrix rows: ${report.summary.unprovenMatrixRows}`);
  console.log(`Raw probes: ${report.summary.totalRawProbes}`);
  console.log(`Raw traces: ${report.summary.totalRawTraces}`);
  console.log(`Artifacts ${wrote ? "written" : "not written"}: ${toRepoPath(outputMarkdown)}`);
  console.log("");
  console.log("| Status | Components |");
  console.log("|---|---:|");
  for (const [status, count] of Object.entries(report.summary.byStatus).sort(([left], [right]) => left.localeCompare(right))) {
    console.log(`| ${status} | ${count} |`);
  }
  console.log("");
  console.log("| Next Stage | Components |");
  console.log("|---|---:|");
  for (const [stage, count] of Object.entries(report.summary.byNextStage).sort(([left], [right]) => left.localeCompare(right))) {
    console.log(`| ${stage} | ${count} |`);
  }
}

function listFiles(root, pattern) {
  if (!existsSync(root)) return [];
  return readdirSync(root)
    .filter(name => pattern.test(name))
    .sort();
}

function readIfExists(path) {
  return existsSync(path) ? readFileSync(path, "utf8") : "";
}

function writeFile(file, content) {
  mkdirSync(dirname(file), { recursive: true });
  writeFileSync(file, content, "utf8");
}

function assertCurrent(file, expected) {
  if (!existsSync(file)) {
    console.error(`${toRepoPath(file)} is missing. Regenerate it with:`);
    console.error("node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write");
    process.exit(1);
  }

  const current = readFileSync(file, "utf8");
  if (current !== expected) {
    const difference = firstDifferentLine(current, expected);
    console.error(`${toRepoPath(file)} is stale against current Fusion onboarding status.`);
    console.error(`First differing line: ${difference.line}`);
    console.error(`Current : ${difference.current}`);
    console.error(`Expected: ${difference.expected}`);
    console.error("Regenerate it with:");
    console.error("node .claude/skills/onboard-fusion-component/scripts/report-fusion-onboarding-status.mjs --write");
    process.exit(1);
  }
}

function firstDifferentLine(current, expected) {
  const currentLines = current.split(/\r?\n/);
  const expectedLines = expected.split(/\r?\n/);
  const count = Math.max(currentLines.length, expectedLines.length);
  for (let index = 0; index < count; index++) {
    if (currentLines[index] !== expectedLines[index]) {
      return {
        line: index + 1,
        current: currentLines[index] ?? "(missing)",
        expected: expectedLines[index] ?? "(missing)"
      };
    }
  }
  return {
    line: 0,
    current: "(no difference)",
    expected: "(no difference)"
  };
}

function sum(items, selector) {
  return items.reduce((total, item) => total + selector(item), 0);
}

function escapePipe(value) {
  return String(value).replace(/\|/g, "\\|");
}

function toRepoPath(path) {
  return relative(repoRoot, path).replace(/\\/g, "/");
}

function kebab(value) {
  return value
    .replace(/([A-Z]+)([A-Z][a-z])/g, "$1-$2")
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .toLowerCase();
}

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
