#!/usr/bin/env node

import { existsSync, mkdirSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const write = Boolean(args.write);
const repoRoot = resolve(args.root ?? ".");
const artifactRoot = join(repoRoot, "tools/FusionOnboarding/wwwroot/onboarding/fusion");

const components = listCurrentComponents(repoRoot);
const inventory = components.map(component => describeComponent(repoRoot, component));

if (write) {
  writeInventoryArtifacts(artifactRoot, inventory);
}

printSummary(inventory, write);

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

function listCurrentComponents(root) {
  const componentRoot = join(root, "Alis.Reactive.Fusion/Components");
  return readdirSync(componentRoot)
    .filter(name => name.startsWith("Fusion"))
    .filter(name => statSync(join(componentRoot, name)).isDirectory())
    .sort();
}

function describeComponent(root, componentType) {
  const componentName = componentType.replace(/^Fusion/, "");
  const artifactName = kebab(componentName);
  const componentRoot = join(root, "Alis.Reactive.Fusion/Components", componentType);
  const viewRoot = join(root, "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion");
  const controllerRoot = join(root, "Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion");
  const modelRoot = join(root, "Alis.Reactive.SandboxApp/Areas/Sandbox/Models/Components/Fusion");
  const playwrightRoot = join(root, "tests/Alis.Reactive.PlaywrightTests/Components/Fusion");

  return {
    componentType,
    componentName,
    artifactName,
    fusionFiles: relativeFiles(root, componentRoot),
    sandboxViews: matchingFiles(root, viewRoot, viewFolderCandidates(componentType, componentName)),
    sandboxControllers: matchingControllerFiles(root, controllerRoot, componentType, componentName),
    sandboxModels: matchingFiles(root, modelRoot, viewFolderCandidates(componentType, componentName)),
    playwrightTests: matchingFiles(root, playwrightRoot, playwrightFolderCandidates(componentType, componentName)),
    status: "inventory-only"
  };
}

function viewFolderCandidates(componentType, componentName) {
  const candidates = new Set([componentName, componentType]);
  if (componentName === "Grid") candidates.add("ArrayGrid");
  if (componentName === "ChipList") candidates.add("ChipFilter");
  if (componentName === "DatePicker") candidates.add("FusionDatePicker");
  if (componentName === "InPlaceEditor") candidates.add("FusionInPlaceEditor");
  if (componentName === "SmartPasteButton" || componentName === "SmartTextArea") candidates.add("SmartComponents");
  return [...candidates];
}

function playwrightFolderCandidates(componentType, componentName) {
  const candidates = new Set([componentName, componentType]);
  if (componentName === "SmartPasteButton" || componentName === "SmartTextArea") candidates.add("SmartComponents");
  return [...candidates];
}

function matchingControllerFiles(root, controllerRoot, componentType, componentName) {
  if (!existsSync(controllerRoot)) return [];
  const prefixes = new Set([
    `${componentName}Controller`,
    `${componentType}Controller`
  ]);
  if (componentName === "Grid") prefixes.add("GridController");
  if (componentName === "CheckBox") prefixes.add("FusionCheckBoxController");
  if (componentName === "DatePicker") prefixes.add("FusionDatePickerController");
  if (componentName === "InPlaceEditor") prefixes.add("FusionInPlaceEditorController");
  if (componentName === "SmartPasteButton" || componentName === "SmartTextArea") prefixes.add("SmartComponentsController");

  return walk(controllerRoot)
    .filter(file => file.endsWith(".cs"))
    .filter(file => {
      const name = file.split(/[\\/]/).pop().replace(/\.cs$/, "");
      return [...prefixes].some(prefix => name === prefix || name.startsWith(`${prefix}.`));
    })
    .map(file => toRepoPath(root, file))
    .sort();
}

function matchingFiles(root, parent, folderNames) {
  if (!existsSync(parent)) return [];
  const files = [];
  for (const folderName of folderNames) {
    const folder = join(parent, folderName);
    if (existsSync(folder) && statSync(folder).isDirectory()) {
      files.push(...relativeFiles(root, folder));
    }
  }
  return [...new Set(files)].sort();
}

function relativeFiles(root, folder) {
  if (!existsSync(folder)) return [];
  return walk(folder).map(file => toRepoPath(root, file)).sort();
}

function walk(folder) {
  const output = [];
  for (const entry of readdirSync(folder)) {
    const path = join(folder, entry);
    const stats = statSync(path);
    if (stats.isDirectory()) {
      output.push(...walk(path));
      continue;
    }
    output.push(path);
  }
  return output;
}

function writeInventoryArtifacts(root, inventory) {
  writeFile(join(root, "_inventory/current-components.json"), `${JSON.stringify(inventory, null, 2)}\n`);
  writeFile(join(root, "_inventory/current-components.md"), inventoryMarkdown(inventory));

  for (const item of inventory) {
    const componentRoot = join(root, item.artifactName);
    writeFile(join(componentRoot, "master-usecases-index.md"), masterIndex(item));
    writeFile(join(componentRoot, "discovery/source-inventory.md"), sourceInventory(item));
  }
}

function writeFile(file, content) {
  mkdirSync(dirname(file), { recursive: true });
  writeFileSync(file, content, "utf8");
}

function inventoryMarkdown(inventory) {
  return `# Current Fusion Component Inventory

Generated by:

\`\`\`bash
node .claude/skills/onboard-fusion-component/scripts/inventory-fusion-components.mjs --write
\`\`\`

This is Stage 1 inventory evidence only. It does not prove any Syncfusion EJ2 API
member, C# Fusion API, payload shape, primitive mapping, or Playwright behavior.

| Component | Artifact | Fusion Files | Sandbox Views | Controllers | Models | Playwright Tests | Status |
|---|---|---:|---:|---:|---:|---:|---|
${inventory.map(item => `| ${item.componentType} | \`${item.artifactName}/\` | ${item.fusionFiles.length} | ${item.sandboxViews.length} | ${item.sandboxControllers.length} | ${item.sandboxModels.length} | ${item.playwrightTests.length} | ${item.status} |`).join("\n")}
`;
}

function masterIndex(item) {
  return `# ${item.componentType} Master Use Cases

Status: inventory-only.

This file is the entry point for deterministic Fusion onboarding or audit of
\`${item.componentType}\`. Existing C#, sandbox, tests, docs, and memory are
evidence only after raw EJ2 discovery and primitive mapping prove them.

No API member is accepted until the row is proven end to end:

\`\`\`text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
\`\`\`

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| component inventory | pending discovery | pending discovery | pending discovery | pending mapping | pending design | inventory-only |

## Linked Artifacts

- [Source inventory](discovery/source-inventory.md)
- \`discovery/public-api-surface.json\` pending raw EJ2 and shipped source discovery
- \`discovery/event-payload-surface.json\` pending event payload discovery
- \`mapping/primitive-map.md\` pending authoritative primitive mapping
- \`mapping/csharp-name-decisions.md\` pending Blazor candidate review
- \`mapping/vertical-slice-plan.md\` pending vertical slice design
- \`proof/typed-api-coverage-matrix.md\` pending implementation inventory
- \`proof/playwright-proof.md\` pending behavior proof
- \`proof/audit-report.md\` pending audit closeout
`;
}

function sourceInventory(item) {
  return `# ${item.componentType} Source Inventory

Generated by:

\`\`\`bash
node .claude/skills/onboard-fusion-component/scripts/inventory-fusion-components.mjs --write
\`\`\`

Status: inventory-only. This file records current repo surfaces before raw EJ2
discovery. It does not prove any API member, payload shape, primitive mapping,
or Playwright behavior.

## Component

- Component type: \`${item.componentType}\`
- Component name: \`${item.componentName}\`
- Artifact folder: \`${item.artifactName}\`
- Workflow mode: audit existing component unless a future pass declares new onboarding

## Fusion Slice Files

${fileList(item.fusionFiles)}

## Sandbox Views

${fileList(item.sandboxViews)}

## Sandbox Controllers

${fileList(item.sandboxControllers)}

## Sandbox Models

${fileList(item.sandboxModels)}

## Playwright Tests

${fileList(item.playwrightTests)}

## Next Required Stage

Run raw Syncfusion EJ2 discovery and write:

- \`probes/raw-ej2-{api-set}.html\`
- \`traces/raw-ej2-{api-set}.trace.json\`
- \`discovery/public-api-surface.json\`
- \`discovery/event-payload-surface.json\`
`;
}

function fileList(files) {
  if (files.length === 0) return "- none found\n";
  return files.map(file => `- \`${file}\``).join("\n") + "\n";
}

function toRepoPath(root, file) {
  return relative(root, file).replace(/\\/g, "/");
}

function kebab(value) {
  return value
    .replace(/([A-Z]+)([A-Z][a-z])/g, "$1-$2")
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .toLowerCase();
}

function printSummary(inventory, wrote) {
  console.log(`# Fusion Component Inventory`);
  console.log("");
  console.log(`Components: ${inventory.length}`);
  console.log(`Artifacts ${wrote ? "written" : "not written"}: ${toRepoPath(process.cwd(), artifactRoot)}`);
  console.log("");
  console.log("| Component | Artifact | Fusion | Views | Controllers | Models | Playwright |");
  console.log("|---|---|---:|---:|---:|---:|---:|");
  for (const item of inventory) {
    console.log(`| ${item.componentType} | \`${item.artifactName}\` | ${item.fusionFiles.length} | ${item.sandboxViews.length} | ${item.sandboxControllers.length} | ${item.sandboxModels.length} | ${item.playwrightTests.length} |`);
  }
}
