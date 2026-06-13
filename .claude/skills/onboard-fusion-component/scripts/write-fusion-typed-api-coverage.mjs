#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const component = requireArg(args, "component");
const fusionType = args["fusion-type"] ?? `Fusion${pascal(component)}`;
const sourceRoot = resolve(args["source-root"] ?? `Alis.Reactive.Fusion/Components/${fusionType}`);
const artifactRoot = resolve(args.root ?? `tools/FusionOnboarding/wwwroot/onboarding/fusion/${component}`);
const write = args.write === true || args.write === "true";
const check = args.check === true || args.check === "true";

if (!existsSync(sourceRoot)) {
  console.error(`Fusion component source root not found: ${sourceRoot}`);
  process.exit(1);
}

const members = extractMembers(sourceRoot)
  .filter(member => includeMember(member, fusionType))
  .sort((left, right) => left.file.localeCompare(right.file) || left.name.localeCompare(right.name) || left.id.localeCompare(right.id));
const supplementalRows = supplementalMatrixRows(component, artifactRoot);

const matrix = coverageMatrixMarkdown({ component, fusionType, sourceRoot, artifactRoot, members });
const target = join(artifactRoot, "proof/typed-api-coverage-matrix.md");

if (check) {
  if (!existsSync(target)) {
    console.error(`${relativePath(target)} is missing. Regenerate it with:`);
    console.error(`node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component ${component} --fusion-type ${fusionType} --write`);
    process.exit(1);
  }

  const current = readFileSync(target, "utf8");
  if (current !== matrix) {
    const difference = firstDifferentLine(current, matrix);
    console.error(`${relativePath(target)} is stale against current Fusion source and artifact decisions.`);
    console.error(`First differing line: ${difference.line}`);
    console.error(`Current : ${difference.current}`);
    console.error(`Expected: ${difference.expected}`);
    console.error("Regenerate it with:");
    console.error(`node .claude/skills/onboard-fusion-component/scripts/write-fusion-typed-api-coverage.mjs --component ${component} --fusion-type ${fusionType} --write`);
    process.exit(1);
  }

  console.log(`${relativePath(target)} is current.`);
  process.exit(0);
}

if (!write) {
  console.log(`# ${fusionType} typed API coverage preview`);
  console.log("");
  console.log(`Members: ${members.length}`);
  for (const member of members) console.log(`- ${member.kind}: ${member.name} (${member.file})`);
  console.log("");
  console.log(`Target: ${target}`);
  process.exit(0);
}

mkdirSync(dirname(target), { recursive: true });
writeFileSync(target, matrix, "utf8");
console.log(target);
console.log(`Typed API rows: ${members.length}`);
console.log(`Supplemental audit rows: ${supplementalRows.length}`);
console.log(`Total matrix rows: ${members.length + supplementalRows.length}`);

function extractMembers(root) {
  const output = [];
  for (const file of walk(root).filter(path => path.endsWith(".cs"))) {
    const text = readFileSync(file, "utf8");
    const relative = file.replace(`${process.cwd()}/`, "");
    output.push(...extractMethods(text, relative));
    output.push(...extractTypedEvents(text, relative));
    output.push(...extractPublicClasses(text, relative));
  }
  return output;
}

function extractMethods(text, file) {
  const members = [];
  const expression = /public\s+(?:static\s+)?(?:[\w<>\[\],\s.?]+\s+)+([A-Z][A-Za-z0-9_]*)\s*(?:<[^>{;]+>)?\s*\(([^)]*)\)/g;
  let match;
  while ((match = expression.exec(stripComments(text))) !== null) {
    const name = match[1];
    const prefix = text.slice(Math.max(0, match.index - 160), match.index);
    const signature = compact(match[0]);
    const kind = match[0].includes("TypedEvent<") || prefix.includes("TypedEvent<") ? "event-selector" : "method";
    const extensionOwner = file.includes("/Events/") ? extensionReceiverOwner(match[2]) : "";
    members.push({
      kind,
      name: kind === "event-selector" ? name : (extensionOwner ? `${extensionOwner}.${name}()` : methodDisplayName(name, match[2])),
      methodName: name,
      id: `${file}:${signature}`,
      owner: extensionOwner,
      signature,
      file
    });
  }
  return members;
}

function extractTypedEvents(text, file) {
  const members = [];
  const expression = /public\s+TypedEvent<[^>]+>\s+([A-Z][A-Za-z0-9_]*)\s*=>/g;
  let match;
  while ((match = expression.exec(stripComments(text))) !== null) {
    members.push({
      kind: "event-selector",
      name: match[1],
      methodName: match[1],
      id: `${file}:${compact(match[0])}`,
      signature: compact(match[0]),
      file
    });
  }
  return members;
}

function extractPublicClasses(text, file) {
    const members = [];
    if (!file.includes("/Events/")) return members;
  const stripped = stripComments(text);
  const expression = /public\s+(?:sealed\s+)?class\s+([A-Z][A-Za-z0-9_]*)(?:<[^>{]+>)?[^{}]*\{/g;
  let match;
  while ((match = expression.exec(stripped)) !== null) {
    const className = match[1];
    const open = stripped.indexOf("{", match.index);
    const body = readBalancedBody(stripped, open);
    members.push({
      kind: "event-payload-contract",
      name: className,
      id: `${file}:${className}`,
      signature: compact(match[0]),
      file
    });
    members.push(...extractPublicProperties(body, className, file));
  }
  return members;
}

function extractPublicProperties(body, className, file) {
  const members = [];
  const expression = /public\s+([\w<>\[\],\s.?]+?)\s+([A-Z][A-Za-z0-9_]*)\s*\{\s*get;\s*set;\s*\}/g;
  let match;
  while ((match = expression.exec(body)) !== null) {
    const [, type, propertyName] = match;
    members.push({
      kind: "event-payload-property",
      name: `${className}.${propertyName}`,
      id: `${file}:${className}.${propertyName}`,
      owner: className,
      propertyName,
      signature: compact(match[0]),
      file,
      propertyType: compact(type)
    });
  }
  return members;
}

function includeMember(member, fusionType) {
  if (member.name === "WriteTo") return false;
  if (member.name === "Instance") return false;
  if (member.name === fusionType) return false;
  if (member.file.endsWith(`${fusionType}.cs`)) return false;
  if (member.file.endsWith(`${fusionType}Builder.cs`)) return false;
  return true;
}

function coverageMatrixMarkdown({ component, fusionType, sourceRoot, artifactRoot, members }) {
  const supplemental = supplementalMatrixRows(component, artifactRoot);
  const variantIndex = buildVariantCoverageIndex(supplemental);
  const allRows = [...members, ...supplemental];
  const rows = allRows.map(member => matrixRow(component, member, artifactRoot, variantIndex));
  const audited = rows.every(row => row.endsWith("| row-proven |"));
  return `# ${fusionType} Typed API Coverage Matrix

Status: ${audited ? "audited" : "unproven"}.

Generated from current public typed Fusion API under:

\`\`\`text
${relativePath(sourceRoot)}
\`\`\`

This matrix is fail-closed. A row with \`unproven\`, \`pending\`, or a missing
trace/mapping/Playwright link means the component is not audited.

| Public API | Kind | Source | Raw Trace Row | Primitive Map Row | Vertical Slice Row | Playwright DSL Proof | Status |
|---|---|---|---|---|---|---|---|
${rows.join("\n")}
`;
}

function matrixRow(component, member, artifactRoot, variantIndex) {
  const coverage = member.coverage ?? rowCoverage(component, member, variantIndex);
  return `| \`${member.name}\` | ${member.kind} | \`${relativePath(member.file)}\` | ${coverage.rawTrace} | ${coverage.primitiveMap} | ${coverage.verticalSlice} | ${coverage.playwrightProof} | ${coverage.status} |`;
}

function supplementalMatrixRows(component, artifactRoot) {
  if (component !== "grid") return [];

  return [
    ...gridEditActionVariantRows(artifactRoot),
    ...gridBeginEditVariantRows(artifactRoot),
    ...gridCellSaveVariantRows(artifactRoot),
    ...gridCellSavedVariantRows(artifactRoot),
    ...gridBeforeBatchSaveVariantRows(artifactRoot),
    ...gridDataStateVariantRows(artifactRoot),
    ...gridRemoteDataRows()
  ];
}

function gridEditActionVariantRows(artifactRoot) {
  const source = "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs";
  const rows = [];

  const variants = [
    {
      event: "actionBegin",
      name: "actionBegin/save-edit",
      judgment: "discovery/judgment-calls-action-begin-save-edit.md",
      trace: "[actionBegin save/edit](../traces/raw-ej2-action-begin-save-edit.trace.json)",
      proof: "[actionBegin save/edit proof](playwright-proof.md)"
    },
    {
      event: "actionComplete",
      name: "actionComplete/save-edit",
      judgment: "discovery/judgment-calls-action-complete-save-edit.md",
      trace: "[actionComplete save/edit](../traces/raw-ej2-action-complete-save-edit.trace.json)",
      proof: "[actionComplete save/edit proof](playwright-proof.md)"
    }
  ];

  for (const variant of variants) {
    const decisions = readEditActionDecisions(artifactRoot, variant.judgment);
    for (const decision of decisions) {
      const member = editActionMemberName(decision.path);
      if (!member) continue;
      const isAccepted = decision.kind === "accepted";
      rows.push({
        kind: isAccepted ? "event-payload-variant" : "event-payload-variant-exclusion",
        name: `${variant.name}: ${member}`,
        id: `supplemental:${variant.name}:${member}:${decision.kind}`,
        file: source,
        coverage: editActionVariantDecisionCoverage(variant, member, decision, artifactRoot)
      });
    }
  }

  return rows;
}

function gridBeginEditVariantRows(artifactRoot) {
  const source = "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs";
  const variant = {
    name: "beginEdit/normal-edit",
    judgment: "discovery/judgment-calls-begin-edit-normal.md",
    trace: "[beginEdit normal edit](../traces/raw-ej2-begin-edit-normal.trace.json)",
    proof: "[beginEdit normal edit proof](playwright-proof.md)"
  };
  const rows = [];

  for (const decision of readBeginEditDecisions(artifactRoot, variant.judgment)) {
    const member = beginEditMemberName(decision.path);
    if (!member) {
      fail(`Unmapped beginEdit judgment payload '${decision.path}' in ${relativePath(join(artifactRoot, variant.judgment))}`);
    }
    const isAccepted = decision.kind === "accepted";
    rows.push({
      kind: isAccepted ? "event-payload-variant" : "event-payload-variant-exclusion",
      name: `${variant.name}: ${member}`,
      id: `supplemental:${variant.name}:${member}:${decision.kind}`,
      file: source,
      coverage: beginEditVariantDecisionCoverage(variant, member, decision, artifactRoot)
    });
  }

  return rows;
}

function gridCellSaveVariantRows(artifactRoot) {
  const source = "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs";
  const variant = {
    name: "cellSave/batch-edit",
    judgment: "discovery/judgment-calls-cell-save-batch-edit.md",
    trace: "[cellSave batch edit](../traces/raw-ej2-cell-save-batch-edit.trace.json)",
    proof: "[cellSave batch edit proof](playwright-proof.md)"
  };
  const rows = [];

  for (const decision of readCellSaveDecisions(artifactRoot, variant.judgment)) {
    const member = cellSaveMemberName(decision.path);
    if (!member) {
      fail(`Unmapped cellSave judgment payload '${decision.path}' in ${relativePath(join(artifactRoot, variant.judgment))}`);
    }
    const isAccepted = decision.kind === "accepted";
    rows.push({
      kind: isAccepted ? "event-payload-variant" : "event-payload-variant-exclusion",
      name: `${variant.name}: ${member}`,
      id: `supplemental:${variant.name}:${member}:${decision.kind}`,
      file: source,
      coverage: cellSaveVariantDecisionCoverage(variant, member, decision, artifactRoot)
    });
  }

  return rows;
}

function readCellSaveDecisions(artifactRoot, relativeFile) {
  const path = join(artifactRoot, relativeFile);
  if (!existsSync(path)) {
    fail(`Required cellSave judgment artifact is missing: ${relativePath(path)}`);
  }

  const rows = [];
  let section = "";
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (line.startsWith("## ")) {
      section = line.toLowerCase();
      continue;
    }
    if (!line.startsWith("| `") && !line.startsWith("| writable `")) continue;
    const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
    const payloadPath = stripBackticks(cells[0] ?? "");
    if (section.includes("accepted public c# surface")) {
      rows.push({ path: payloadPath, kind: "accepted", decision: cells[2] ?? "" });
    } else if (section.includes("excluded from public c# surface")) {
      rows.push({ path: payloadPath, kind: "excluded", decision: cells[1] ?? "" });
    }
  }
  if (rows.length === 0) {
    fail(`Required cellSave judgment artifact has no accepted/excluded rows: ${relativePath(path)}`);
  }
  return rows;
}

function cellSaveMemberName(path) {
  const map = {
    "rowData": "FusionGridCellSaveArgs.RowData",
    "columnName": "FusionGridCellSaveArgs.ColumnName",
    "value": "FusionGridCellSaveArgs.Value",
    "previousValue": "FusionGridCellSaveArgs.PreviousValue",
    "cancel": "FusionGridCellSaveArgs.Cancel",
    "writable cancel": "FusionGridCellSaveArgs.Cancel()",
    "cell": "FusionGridCellSaveArgs.Cell",
    "column": "FusionGridCellSaveArgs.Column",
    "columnObject": "FusionGridCellSaveArgs.ColumnObject",
    "isForeignKey": "FusionGridCellSaveArgs.IsForeignKey",
    "name": "FusionGridCellSaveArgs.Name"
  };
  return map[path] ?? "";
}

function gridCellSavedVariantRows(artifactRoot) {
  const source = "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs";
  const variant = {
    name: "cellSaved/batch-edit",
    judgment: "discovery/judgment-calls-cell-saved-batch-edit.md",
    trace: "[cellSaved batch edit](../traces/raw-ej2-cell-saved-batch-edit.trace.json)",
    proof: "[cellSaved batch edit proof](playwright-proof.md)"
  };
  const rows = [];

  for (const decision of readCellSavedDecisions(artifactRoot, variant.judgment)) {
    const member = cellSavedMemberName(decision.path);
    if (!member) {
      fail(`Unmapped cellSaved judgment payload '${decision.path}' in ${relativePath(join(artifactRoot, variant.judgment))}`);
    }
    const isAccepted = decision.kind === "accepted";
    rows.push({
      kind: isAccepted ? "event-payload-variant" : "event-payload-variant-exclusion",
      name: `${variant.name}: ${member}`,
      id: `supplemental:${variant.name}:${member}:${decision.kind}`,
      file: source,
      coverage: cellSavedVariantDecisionCoverage(variant, member, decision, artifactRoot)
    });
  }

  return rows;
}

function readCellSavedDecisions(artifactRoot, relativeFile) {
  const path = join(artifactRoot, relativeFile);
  if (!existsSync(path)) {
    fail(`Required cellSaved judgment artifact is missing: ${relativePath(path)}`);
  }

  const rows = [];
  let section = "";
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (line.startsWith("## ")) {
      section = line.toLowerCase();
      continue;
    }
    if (!line.startsWith("| `") && !line.startsWith("| writable `")) continue;
    const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
    const payloadPath = stripBackticks(cells[0] ?? "");
    if (section.includes("accepted public c# surface")) {
      rows.push({ path: payloadPath, kind: "accepted", decision: cells[2] ?? "" });
    } else if (section.includes("excluded from public c# surface")) {
      rows.push({ path: payloadPath, kind: "excluded", decision: cells[1] ?? "" });
    }
  }
  if (rows.length === 0) {
    fail(`Required cellSaved judgment artifact has no accepted/excluded rows: ${relativePath(path)}`);
  }
  return rows;
}

function cellSavedMemberName(path) {
  const map = {
    "rowData": "FusionGridCellSavedArgs.RowData",
    "columnName": "FusionGridCellSavedArgs.ColumnName",
    "value": "FusionGridCellSavedArgs.Value",
    "previousValue": "FusionGridCellSavedArgs.PreviousValue",
    "cancel": "FusionGridCellSavedArgs.Cancel",
    "writable cancel": "FusionGridCellSavedArgs.Cancel()",
    "cell": "FusionGridCellSavedArgs.Cell",
    "column": "FusionGridCellSavedArgs.Column",
    "columnObject": "FusionGridCellSavedArgs.ColumnObject",
    "isForeignKey": "FusionGridCellSavedArgs.IsForeignKey",
    "name": "FusionGridCellSavedArgs.Name"
  };
  return map[path] ?? "";
}

function gridBeforeBatchSaveVariantRows(artifactRoot) {
  const source = "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnEditing.cs";
  const variant = {
    name: "beforeBatchSave/batch-edit",
    judgment: "discovery/judgment-calls-before-batch-save-batch-edit.md",
    trace: "[beforeBatchSave batch edit](../traces/raw-ej2-before-batch-save-batch-edit.trace.json)",
    proof: "[beforeBatchSave batch edit proof](playwright-proof.md)"
  };
  const rows = [];

  for (const decision of readBeforeBatchSaveDecisions(artifactRoot, variant.judgment)) {
    const member = beforeBatchSaveMemberName(decision.path);
    if (!member) {
      fail(`Unmapped beforeBatchSave judgment payload '${decision.path}' in ${relativePath(join(artifactRoot, variant.judgment))}`);
    }
    const isAccepted = decision.kind === "accepted";
    rows.push({
      kind: isAccepted ? "event-payload-variant" : "event-payload-variant-exclusion",
      name: `${variant.name}: ${member}`,
      id: `supplemental:${variant.name}:${member}:${decision.kind}`,
      file: source,
      coverage: beforeBatchSaveVariantDecisionCoverage(variant, member, decision, artifactRoot)
    });
  }

  return rows;
}

function readBeforeBatchSaveDecisions(artifactRoot, relativeFile) {
  const path = join(artifactRoot, relativeFile);
  if (!existsSync(path)) {
    fail(`Required beforeBatchSave judgment artifact is missing: ${relativePath(path)}`);
  }

  const rows = [];
  let section = "";
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (line.startsWith("## ")) {
      section = line.toLowerCase();
      continue;
    }
    if (!line.startsWith("| `") && !line.startsWith("| writable `")) continue;
    const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
    const payloadPath = stripBackticks(cells[0] ?? "");
    if (section.includes("accepted public c# surface")) {
      rows.push({ path: payloadPath, kind: "accepted", decision: cells[2] ?? "" });
    } else if (section.includes("excluded from public c# surface")) {
      rows.push({ path: payloadPath, kind: "excluded", decision: cells[1] ?? "" });
    }
  }
  if (rows.length === 0) {
    fail(`Required beforeBatchSave judgment artifact has no accepted/excluded rows: ${relativePath(path)}`);
  }
  return rows;
}

function beforeBatchSaveMemberName(path) {
  const map = {
    "batchChanges": "FusionGridBeforeBatchSaveArgs.BatchChanges",
    "cancel": "FusionGridBeforeBatchSaveArgs.Cancel",
    "writable cancel": "FusionGridBeforeBatchSaveArgs.Cancel()",
    "name": "FusionGridBeforeBatchSaveArgs.Name"
  };
  return map[path] ?? "";
}

function readBeginEditDecisions(artifactRoot, relativeFile) {
  const path = join(artifactRoot, relativeFile);
  if (!existsSync(path)) {
    fail(`Required beginEdit judgment artifact is missing: ${relativePath(path)}`);
  }

  const rows = [];
  let section = "";
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (line.startsWith("## ")) {
      section = line.toLowerCase();
      continue;
    }
    if (!line.startsWith("| `") && !line.startsWith("| writable `")) continue;
    const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
    const payloadPath = stripBackticks(cells[0] ?? "");
    if (section.includes("accepted public c# surface")) {
      rows.push({ path: payloadPath, kind: "accepted", decision: cells[2] ?? "" });
    } else if (section.includes("excluded from public c# surface")) {
      rows.push({ path: payloadPath, kind: "excluded", decision: cells[1] ?? "" });
    }
  }
  if (rows.length === 0) {
    fail(`Required beginEdit judgment artifact has no accepted/excluded rows: ${relativePath(path)}`);
  }
  return rows;
}

function beginEditMemberName(path) {
  const map = {
    "rowData": "FusionGridBeginEditArgs.RowData",
    "rowIndex": "FusionGridBeginEditArgs.RowIndex",
    "type": "FusionGridBeginEditArgs.Type",
    "cancel": "FusionGridBeginEditArgs.Cancel",
    "writable cancel": "FusionGridBeginEditArgs.Cancel()",
    "row": "FusionGridBeginEditArgs.Row",
    "foreignKeyData": "FusionGridBeginEditArgs.ForeignKeyData",
    "isScroll": "FusionGridBeginEditArgs.IsScroll",
    "name": "FusionGridBeginEditArgs.Name",
    "primaryKey": "FusionGridBeginEditArgs.PrimaryKey",
    "primaryKeyValue": "FusionGridBeginEditArgs.PrimaryKeyValue",
    "requestType": "FusionGridBeginEditArgs.RequestType",
    "target": "FusionGridBeginEditArgs.Target"
  };
  return map[path] ?? "";
}

function readEditActionDecisions(artifactRoot, relativeFile) {
  const path = join(artifactRoot, relativeFile);
  if (!existsSync(path)) return [];

  const rows = [];
  let section = "";
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (line.startsWith("## ")) {
      section = line.toLowerCase();
      continue;
    }
    if (!line.startsWith("| `")) continue;
    const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
    const payloadPath = stripBackticks(cells[0] ?? "");
    if (section.includes("accepted public c# surface")) {
      rows.push({ path: payloadPath, kind: "accepted", decision: cells[2] ?? "" });
    } else if (section.includes("excluded from public c# surface")) {
      rows.push({ path: payloadPath, kind: "excluded", decision: cells[1] ?? "" });
    }
  }
  return rows;
}

function editActionMemberName(path) {
  const map = {
    "name": "FusionGridEditActionArgs.Name",
    "requestType": "FusionGridEditActionArgs.RequestType",
    "action": "FusionGridEditActionArgs.Action",
    "type": "FusionGridEditActionArgs.Type",
    "cancel": "FusionGridEditActionArgs.Cancel",
    "data": "FusionGridEditActionArgs.Data",
    "previousData": "FusionGridEditActionArgs.PreviousData",
    "rowIndex": "FusionGridEditActionArgs.RowIndex",
    "selectedRow": "FusionGridEditActionArgs.SelectedRow",
    "row": "FusionGridEditActionArgs.Row",
    "form": "FusionGridEditActionArgs.Form",
    "target": "FusionGridEditActionArgs.Target",
    "foreignKeyData": "FusionGridEditActionArgs.ForeignKeyData",
    "isScroll": "FusionGridEditActionArgs.IsScroll",
    "primaryKey": "FusionGridEditActionArgs.PrimaryKey",
    "primaryKeyValue": "FusionGridEditActionArgs.PrimaryKeyValue",
    "rowData": "FusionGridEditActionArgs.RowData",
    "index": "FusionGridEditActionArgs.Index",
    "promise": "FusionGridEditActionArgs.Promise"
  };
  return map[path] ?? "";
}

function beginEditVariantDecisionCoverage(variant, member, decision, artifactRoot) {
  if (decision.kind === "accepted" && provenBeginEditAcceptedMember(member, artifactRoot)) {
    return acceptedVariantCoverage(variant);
  }
  if (decision.kind === "excluded" && provenBeginEditExcludedMember(member, artifactRoot)) {
    return excludedVariantCoverage(variant);
  }
  return pendingVariantDecisionCoverage(variant, decision);
}

function provenBeginEditAcceptedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridBeginEditArgs.RowData": ["#begin-edit-resident", 'ToHaveTextAsync("Amina Patel"'],
    "FusionGridBeginEditArgs.RowIndex": ["#begin-edit-row", 'ToHaveTextAsync("0"'],
    "FusionGridBeginEditArgs.Type": ["#begin-edit-type", 'ToHaveTextAsync("edit"'],
    "FusionGridBeginEditArgs.Cancel": ["#begin-edit-cancel", 'ToHaveTextAsync("false"'],
    "FusionGridBeginEditArgs.Cancel()": ["#begin-edit-cancelled", 'ToHaveTextAsync("edit cancelled"']
  };
  const snippets = memberToSnippet[member];
  if (!snippets) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBeginEditNormal.cs",
      method: "begin_edit_normal_reads_row_data_and_can_cancel_edit",
      snippets: [
        ...snippets
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`beginEdit` normal edit",
        "begin_edit_normal_reads_row_data_and_can_cancel_edit"
      ]
    }
  ]);
}

function provenBeginEditExcludedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridBeginEditArgs.Row": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "Row"),
    "FusionGridBeginEditArgs.ForeignKeyData": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "ForeignKeyData"),
    "FusionGridBeginEditArgs.IsScroll": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "IsScroll"),
    "FusionGridBeginEditArgs.Name": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "Name"),
    "FusionGridBeginEditArgs.PrimaryKey": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "PrimaryKey"),
    "FusionGridBeginEditArgs.PrimaryKeyValue": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "PrimaryKeyValue"),
    "FusionGridBeginEditArgs.RequestType": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "RequestType"),
    "FusionGridBeginEditArgs.Target": missingPropertyAssertionSnippet("FusionGridBeginEditArgs<ResidentDirectoryGridItem>", "Target")
  };
  const required = memberToSnippet[member];
  if (!required) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBeginEditNormal.cs",
      method: "begin_edit_normal_reads_row_data_and_can_cancel_edit",
      snippets: [
        required
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`beginEdit` normal edit",
        "begin_edit_normal_reads_row_data_and_can_cancel_edit"
      ]
    }
  ]);
}

function cellSaveVariantDecisionCoverage(variant, member, decision, artifactRoot) {
  if (decision.kind === "accepted" && provenCellSaveAcceptedMember(member, artifactRoot)) {
    return acceptedVariantCoverage(variant);
  }
  if (decision.kind === "excluded" && provenCellSaveExcludedMember(member, artifactRoot)) {
    return excludedVariantCoverage(variant);
  }
  return pendingVariantDecisionCoverage(variant, decision);
}

function cellSavedVariantDecisionCoverage(variant, member, decision, artifactRoot) {
  if (decision.kind === "accepted" && provenCellSavedAcceptedMember(member, artifactRoot)) {
    return acceptedVariantCoverage(variant);
  }
  if (decision.kind === "excluded" && provenCellSavedExcludedMember(member, artifactRoot)) {
    return excludedVariantCoverage(variant);
  }
  return pendingVariantDecisionCoverage(variant, decision);
}

function beforeBatchSaveVariantDecisionCoverage(variant, member, decision, artifactRoot) {
  if (decision.kind === "accepted" && provenBeforeBatchSaveAcceptedMember(member, artifactRoot)) {
    return acceptedVariantCoverage(variant);
  }
  if (decision.kind === "excluded" && provenBeforeBatchSaveExcludedMember(member, artifactRoot)) {
    return excludedVariantCoverage(variant);
  }
  return pendingVariantDecisionCoverage(variant, decision);
}

function provenCellSaveAcceptedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridCellSaveArgs.RowData": ["#batch-cell-save-resident", 'ToHaveTextAsync("Amina Patel"'],
    "FusionGridCellSaveArgs.ColumnName": ["#batch-cell-save-column", 'ToHaveTextAsync("openTasks"'],
    "FusionGridCellSaveArgs.Value": ["#batch-cell-save-value", 'ToHaveTextAsync("4"'],
    "FusionGridCellSaveArgs.PreviousValue": ["#batch-cell-save-previous", 'ToHaveTextAsync("0"'],
    "FusionGridCellSaveArgs.Cancel": ["#batch-cell-save-cancel", 'ToHaveTextAsync("false"'],
    "FusionGridCellSaveArgs.Cancel()": ["#batch-cell-save-cancelled", 'ToHaveTextAsync("blocked 99"', 'Not.ToContainTextAsync("99"']
  };
  const snippets = memberToSnippet[member];
  if (!snippets) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
      method: "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source",
      snippets: [
        ...snippets
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`cellSave` batch edit",
        "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"
      ]
    }
  ]);
}

function provenCellSavedAcceptedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridCellSavedArgs.RowData": ["#batch-cell-saved-resident", 'ToHaveTextAsync("Amina Patel"'],
    "FusionGridCellSavedArgs.ColumnName": ["#batch-cell-saved-column", 'ToHaveTextAsync("openTasks"'],
    "FusionGridCellSavedArgs.Value": ["#batch-cell-saved-value", 'ToHaveTextAsync("4"'],
    "FusionGridCellSavedArgs.PreviousValue": ["#batch-cell-saved-previous", 'ToHaveTextAsync("0"']
  };
  const snippets = memberToSnippet[member];
  if (!snippets) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
      method: "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source",
      snippets: [
        ...snippets
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`cellSaved` batch edit",
        "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"
      ]
    }
  ]);
}

function provenBeforeBatchSaveAcceptedMember(member, artifactRoot) {
  const postCancelGatherProof = `await ClickWhenStable(Page.Locator("#batch-gather-changes"));
        await Expect(Page.Locator("#batch-summary")).ToHaveTextAsync(
            "batch added 0, changed 1, deleted 0",
            new() { Timeout = 10000 });
        await Expect(Page.Locator("#batch-action-complete"))
            .ToHaveTextAsync("waiting after cancelled batch", new() { Timeout = 10000 });`;
  const memberToSnippet = {
    "FusionGridBeforeBatchSaveArgs.BatchChanges": ["#batch-before-save-tasks", 'ToHaveTextAsync("8"', "#batch-before-save-resident", 'ToHaveTextAsync("Amina Patel"'],
    "FusionGridBeforeBatchSaveArgs.Cancel": ["#batch-before-save-cancel", 'ToHaveTextAsync("false"'],
    "FusionGridBeforeBatchSaveArgs.Cancel()": ["#batch-before-save-cancelled", 'ToHaveTextAsync("blocked batch 8"', postCancelGatherProof]
  };
  const snippets = memberToSnippet[member];
  if (!snippets) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
      method: "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source",
      snippets: [
        ...snippets
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`beforeBatchSave` batch edit",
        "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"
      ]
    }
  ]);
}

function provenCellSaveExcludedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridCellSaveArgs.Cell": missingPropertyAssertionSnippet("FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>", "Cell"),
    "FusionGridCellSaveArgs.Column": missingPropertyAssertionSnippet("FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>", "Column"),
    "FusionGridCellSaveArgs.ColumnObject": missingPropertyAssertionSnippet("FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>", "ColumnObject"),
    "FusionGridCellSaveArgs.IsForeignKey": missingPropertyAssertionSnippet("FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>", "IsForeignKey"),
    "FusionGridCellSaveArgs.Name": missingPropertyAssertionSnippet("FusionGridCellSaveArgs<ResidentDirectoryGridItem, int>", "Name")
  };
  const required = memberToSnippet[member];
  if (!required) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
      method: "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source",
      snippets: [
        required
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`cellSave` batch edit",
        "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"
      ]
    }
  ]);
}

function provenBeforeBatchSaveExcludedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridBeforeBatchSaveArgs.Name": missingPropertyAssertionSnippet("FusionGridBeforeBatchSaveArgs<ResidentDirectoryGridItem>", "Name")
  };
  const required = memberToSnippet[member];
  if (!required) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
      method: "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source",
      snippets: [
        required
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`beforeBatchSave` batch edit",
        "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"
      ]
    }
  ]);
}

function provenCellSavedExcludedMember(member, artifactRoot) {
  const memberToSnippet = {
    "FusionGridCellSavedArgs.Cancel": missingPropertyAssertionSnippet("FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>", "Cancel"),
    "FusionGridCellSavedArgs.Cell": missingPropertyAssertionSnippet("FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>", "Cell"),
    "FusionGridCellSavedArgs.Column": missingPropertyAssertionSnippet("FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>", "Column"),
    "FusionGridCellSavedArgs.ColumnObject": missingPropertyAssertionSnippet("FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>", "ColumnObject"),
    "FusionGridCellSavedArgs.IsForeignKey": missingPropertyAssertionSnippet("FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>", "IsForeignKey"),
    "FusionGridCellSavedArgs.Name": missingPropertyAssertionSnippet("FusionGridCellSavedArgs<ResidentDirectoryGridItem, int>", "Name"),
    "FusionGridCellSavedArgs.Cancel()": "FusionGridCellSavedArgs\")),\n            Is.False"
  };
  const required = memberToSnippet[member];
  if (!required) return false;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
      method: "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source",
      snippets: [
        required
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: [
        "`cellSaved` batch edit",
        "batch_editing_exposes_cell_events_indexed_batch_payload_and_batch_changes_source"
      ]
    }
  ]);
}

function gridDataStateVariantRows(artifactRoot) {
  const source = "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs";
  const rows = [];

  const variants = [
    {
      name: "sorting",
      judgment: "discovery/judgment-calls-data-state-change-sorting.md",
      trace: "[sorting](../traces/raw-ej2-data-state-change-sorting.trace.json), [header-click sorting](../traces/raw-ej2-data-state-change-sorting-header-click.trace.json)",
      proof: "[sorting proof](playwright-proof.md)"
    },
    {
      name: "paging",
      judgment: "discovery/judgment-calls-data-state-change-paging.md",
      trace: "[paging method](../traces/raw-ej2-data-state-change-paging-method.trace.json), [pager click](../traces/raw-ej2-data-state-change-paging-pager-click.trace.json)",
      proof: "[paging proof](playwright-proof.md)"
    },
    {
      name: "filtering-method",
      judgment: "discovery/judgment-calls-data-state-change-filtering-method.md",
      trace: "[filtering](../traces/raw-ej2-data-state-change-filtering-method.trace.json)",
      proof: "[filtering proof](playwright-proof.md)"
    },
    {
      name: "filtering-filterbar",
      judgment: "discovery/judgment-calls-data-state-change-filtering-filterbar.md",
      trace: "[filterbar filtering](../traces/raw-ej2-data-state-change-filtering-filterbar.trace.json)",
      proof: "[filterbar filtering proof](playwright-proof.md)"
    },
    {
      name: "clear-filtering-method",
      judgment: "discovery/judgment-calls-data-state-change-clear-filtering-method.md",
      trace: "[clear filtering](../traces/raw-ej2-data-state-change-clear-filtering-method.trace.json)",
      proof: "[clear-filtering proof](playwright-proof.md)"
    },
    {
      name: "searching-method",
      judgment: "discovery/judgment-calls-data-state-change-searching-method.md",
      trace: "[searching](../traces/raw-ej2-data-state-change-searching-method.trace.json)",
      proof: "[searching proof](playwright-proof.md)"
    },
    {
      name: "clear-search-method",
      judgment: "discovery/judgment-calls-data-state-change-clear-search-method.md",
      trace: "[clear search](../traces/raw-ej2-data-state-change-clear-search-method.trace.json)",
      proof: "[clear-search proof](playwright-proof.md)"
    },
    {
      name: "clear-sorting-method",
      judgment: "discovery/judgment-calls-data-state-change-clear-sorting-method.md",
      trace: "[clear sorting](../traces/raw-ej2-data-state-change-clear-sorting-method.trace.json)",
      proof: "[clear-sorting proof](playwright-proof.md)"
    },
    {
      name: "grouping-method",
      judgment: "discovery/judgment-calls-data-state-change-grouping-method.md",
      trace: "[grouping](../traces/raw-ej2-data-state-change-grouping-method.trace.json)",
      proof: "[grouping proof](playwright-proof.md)"
    },
    {
      name: "ungrouping-method",
      judgment: "discovery/judgment-calls-data-state-change-ungrouping-method.md",
      trace: "[ungrouping](../traces/raw-ej2-data-state-change-ungrouping-method.trace.json)",
      proof: "[ungrouping proof](playwright-proof.md)"
    },
    {
      name: "clear-grouping-method",
      judgment: "discovery/judgment-calls-data-state-change-clear-grouping-method.md",
      trace: "[clear grouping](../traces/raw-ej2-data-state-change-clear-grouping-method.trace.json)",
      proof: "[clear-grouping proof](playwright-proof.md)"
    }
  ];

  for (const variant of variants) {
    const decisions = readVariantDecisions(artifactRoot, variant.judgment);
    for (const decision of decisions) {
      const member = dataStateMemberName(decision.path);
      if (!member) continue;
      const isAccepted = decision.kind === "accepted";
      rows.push({
        kind: isAccepted ? "event-payload-variant" : "event-payload-variant-exclusion",
        name: `dataStateChange/${variant.name}: ${member}`,
        id: `supplemental:${variant.name}:${member}:${decision.kind}`,
        file: source,
        coverage: variantDecisionCoverage(variant, member, decision, artifactRoot)
      });
    }
  }

  return rows;
}

function readVariantDecisions(artifactRoot, relativeFile) {
  const path = join(artifactRoot, relativeFile);
  if (!existsSync(path)) return [];

  const rows = [];
  for (const line of readFileSync(path, "utf8").split(/\r?\n/)) {
    if (!line.startsWith("| `")) continue;
    const cells = line.split("|").map(cell => cell.trim()).filter(Boolean);
    const payloadPath = cells[0]?.replace(/^`|`$/g, "") ?? "";
    const decision = (cells[2] ?? "").toLowerCase();
    const kind = decisionKind(decision);
    if (!kind) continue;
    rows.push({ path: payloadPath, kind, decision });
  }
  return rows;
}

function decisionKind(decision) {
  if (/\b(not accepted|excluded|removed)\b/.test(decision)) return "excluded";
  if (/\baccepted\b/.test(decision)) return "accepted";
  return "";
}

function dataStateMemberName(path) {
  const normalized = path
    .replace(/^grouping\s+/, "")
    .replace(/^`|`$/g, "");
  const map = {
    "name": "FusionGridDataStateChangeArgs.Name",
    "skip": "FusionGridDataStateChangeArgs.Skip",
    "take": "FusionGridDataStateChangeArgs.Take",
    "requiresCounts": "FusionGridDataStateChangeArgs.RequiresCounts",
    "sorted": "FusionGridDataStateChangeArgs.Sorted",
    "sorted[].name": "FusionGridSortColumn.Name",
    "sorted[].direction": "FusionGridSortColumn.Direction",
    "where": "FusionGridDataStateChangeArgs.Where",
    "where[].condition": "FusionGridTextFilterCriterion.Condition",
    "where[].ignoreCase": "FusionGridTextFilterCriterion.IgnoreCase",
    "where[].ignoreAccent": "FusionGridTextFilterCriterion.IgnoreAccent",
    "where[].predicates[]": "FusionGridTextFilterCriterion.Predicates",
    "where[].predicates[].field": "FusionGridTextFilterCriterion.Field",
    "where[].predicates[].operator": "FusionGridTextFilterCriterion.Operator",
    "where[].predicates[].value": "FusionGridTextFilterCriterion.Value",
    "where[].isComplex": "FusionGridTextFilterCriterion.IsComplex [where]",
    "where[].predicates[].isComplex": "FusionGridTextFilterCriterion.IsComplex [where.predicates]",
    "matchCase": "FusionGridTextFilterCriterion.MatchCase",
    "predicate": "FusionGridTextFilterCriterion.Predicate",
    "search": "FusionGridDataStateChangeArgs.Search",
    "search[].fields": "FusionGridSearchDescriptor.Fields",
    "search[].key": "FusionGridSearchDescriptor.Key",
    "search[].operator": "FusionGridSearchDescriptor.Operator",
    "search[].ignoreCase": "FusionGridSearchDescriptor.IgnoreCase",
    "search[].ignoreAccent": "FusionGridSearchDescriptor.IgnoreAccent",
    "group": "FusionGridDataStateChangeArgs.Group",
    "groups": "FusionGridDataStateChangeArgs.Groups",
    "aggregates": "FusionGridDataStateChangeArgs.Aggregates",
    "dataSource": "FusionGridDataStateChangeArgs.DataSource",
    "isLazyLoad": "FusionGridDataStateChangeArgs.IsLazyLoad",
    "onDemandGroupInfo": "FusionGridDataStateChangeArgs.OnDemandGroupInfo",
    "select": "FusionGridDataStateChangeArgs.Select",
    "table": "FusionGridDataStateChangeArgs.Table"
  };
  if (map[normalized]) return map[normalized];
  if (normalized.startsWith("action.")) return `FusionGridAction.${pascal(normalized.slice("action.".length))}`;
  return "";
}

function gridRemoteDataRows() {
  const rows = [
    {
      name: "initial builder-owned dataSource",
      source: "tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/mvc-builder-coverage.md",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "DataManager adaptor dataSource",
      source: "tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/public-api-surface.json",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "nested data-source property path",
      source: "tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/event-payload-surface.json",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "remote response shape { result, count }",
      source: "tools/FusionOnboarding/wwwroot/onboarding/fusion/grid/discovery/runtime-row-remote-response-shape.md",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "SetDataSource [event payload path] response refresh",
      source: "Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "SetDataSource [response path] response refresh",
      source: "Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "SetDataSource [typed array source] client rebind",
      source: "Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs",
      prefix: "data-source",
      kind: "data-source-lane"
    },
    {
      name: "SetDataSource [whole response body] response refresh",
      source: "Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs",
      prefix: "remote-data",
      kind: "remote-data-lane"
    },
    {
      name: "Data [component dataSource read]",
      source: "Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs",
      prefix: "data-source",
      kind: "data-source-lane"
    },
    {
      name: "Refresh [component refresh method]",
      source: "Alis.Reactive.Fusion/Components/FusionGrid/FusionGridDataSourceExtensions.cs",
      prefix: "data-source",
      kind: "data-source-lane"
    }
  ];

  return rows.map(row => ({
    kind: row.kind,
    name: `${row.prefix}: ${row.name}`,
    id: `supplemental:${row.prefix}:${row.name}`,
    file: row.source,
    coverage: dataSourceLaneCoverage(row.name) ?? pendingRemoteDataCoverage()
  }));
}

function dataSourceLaneCoverage(name) {
  if ([
    "remote response shape { result, count }",
    "SetDataSource [whole response body] response refresh"
  ].includes(name)) {
    return remoteWholeResponseCoverage();
  }

  // The response-path refresh lane is proven by the server-backed roster slice.
  if (name === "SetDataSource [response path] response refresh" && serverRosterProven()) {
    return serverRosterCoverage();
  }

  // The initial builder-owned data source lane is proven by the builder-roster slice.
  if (name === "initial builder-owned dataSource" && builderRosterProven()) {
    return builderRosterCoverage();
  }

  // The nested data-source property path lane is proven by the server-roster nested load.
  if (name === "nested data-source property path" && nestedPathProven()) {
    return serverRosterCoverage();
  }

  // The event-payload-path refresh lane is proven by the batch-change-review slice.
  if (name === "SetDataSource [event payload path] response refresh" && eventPayloadPathProven()) {
    return eventPayloadPathCoverage();
  }

  // The DataManager/adaptor lane is proven by the remote-adaptor roster slice.
  if (name === "DataManager adaptor dataSource" && remoteAdaptorProven()) {
    return remoteAdaptorCoverage();
  }

  if ([
    "SetDataSource [typed array source] client rebind",
    "Data [component dataSource read]",
    "Refresh [component refresh method]"
  ].includes(name)) {
    return typedArrayDataSourceCoverage();
  }

  return null;
}

function typedArrayDataSourceCoverage() {
  return {
    rawTrace: "[data-source read/rebind/refresh](../traces/raw-ej2-data-source-read-refresh.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[data-source typed-array proof](playwright-proof.md)",
    status: "row-proven"
  };
}

function remoteWholeResponseCoverage() {
  return {
    rawTrace: "[remote response shape](../traces/raw-ej2-remote-response-shape.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[remote whole-response proof](playwright-proof.md)",
    status: "row-proven"
  };
}

function pendingRemoteDataCoverage() {
  return {
    rawTrace: "pending remote/custom-binding trace",
    primitiveMap: "pending remote row in [primitive map](../mapping/primitive-map.md)",
    verticalSlice: "pending remote row in [vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "pending realistic remote-data Playwright proof",
    status: "unproven"
  };
}

function editActionVariantDecisionCoverage(variant, member, decision, artifactRoot) {
  if (decision.kind === "accepted" && provenEditActionAcceptedMember(variant.name, member, artifactRoot)) {
    return acceptedVariantCoverage(variant);
  }
  if (decision.kind === "excluded" && provenEditActionExcludedMember(variant.name, member, artifactRoot)) {
    return excludedVariantCoverage(variant);
  }
  return pendingVariantDecisionCoverage(variant, decision);
}

function provenEditActionAcceptedMember(variant, member, artifactRoot) {
  const accepted = [
    "FusionGridEditActionArgs.Name",
    "FusionGridEditActionArgs.RequestType",
    "FusionGridEditActionArgs.Action",
    "FusionGridEditActionArgs.Type",
    "FusionGridEditActionArgs.Cancel",
    "FusionGridEditActionArgs.Data",
    "FusionGridEditActionArgs.PreviousData",
    "FusionGridEditActionArgs.RowIndex",
    "FusionGridEditActionArgs.SelectedRow"
  ];
  if (!accepted.includes(member)) return false;
  if (variant === "actionBegin/save-edit") {
    const snippets = editActionAcceptedMemberSnippets("actionBegin", member);
    if (snippets.length === 0) return false;
    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs",
        method: "action_begin_save_edit_reads_typed_current_previous_and_action_fields",
        snippets: [
          ...snippets
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "`actionBegin` save/edit",
          "action_begin_save_edit_reads_typed_current_previous_and_action_fields"
        ]
      }
    ]);
  }
  if (variant === "actionComplete/save-edit") {
    const snippets = editActionAcceptedMemberSnippets("actionComplete", member);
    if (snippets.length === 0) return false;
    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridActionCompleteSaveEdit.cs",
        method: "action_complete_save_edit_reads_typed_current_previous_and_action_fields",
        snippets: [
          ...snippets
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "`actionComplete` save/edit",
          "action_complete_save_edit_reads_typed_current_previous_and_action_fields"
        ]
      }
    ]);
  }
  return false;
}

function editActionAcceptedMemberSnippets(eventName, member) {
  const prefix = eventName === "actionBegin" ? "#inline-action-begin" : "#ac";
  const snippets = {
    "FusionGridEditActionArgs.Name": [`${prefix}${eventName === "actionBegin" ? "-event" : "-name"}`, `ToHaveTextAsync("${eventName}"`],
    "FusionGridEditActionArgs.RequestType": [`${prefix}${eventName === "actionBegin" ? "" : "-request-type"}`, 'ToHaveTextAsync("save"'],
    "FusionGridEditActionArgs.Action": [`${prefix}-action`, 'ToHaveTextAsync("edit"'],
    "FusionGridEditActionArgs.Type": [`${prefix}-type`, `ToHaveTextAsync("${eventName}"`],
    "FusionGridEditActionArgs.Cancel": [`${prefix}-cancel`, 'ToHaveTextAsync("false"'],
    "FusionGridEditActionArgs.Data": [`${prefix}${eventName === "actionBegin" ? "-resident" : "-current-resident"}`, `ToHaveTextAsync("Amina ${eventName === "actionBegin" ? "ActionBegin" : "ActionComplete"}"`],
    "FusionGridEditActionArgs.PreviousData": [`${prefix}-previous-resident`, 'ToHaveTextAsync("Amina Patel"'],
    "FusionGridEditActionArgs.RowIndex": [`${prefix}${eventName === "actionBegin" ? "-row" : "-row-index"}`, 'ToHaveTextAsync("0"'],
    "FusionGridEditActionArgs.SelectedRow": [`${prefix}-selected-row`, 'ToHaveTextAsync("-1"']
  };
  return snippets[member] ?? [];
}

function provenEditActionExcludedMember(variant, member, artifactRoot) {
  const commonExcluded = {
    "FusionGridEditActionArgs.Row": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "Row"),
    "FusionGridEditActionArgs.Form": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "Form"),
    "FusionGridEditActionArgs.Target": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "Target"),
    "FusionGridEditActionArgs.ForeignKeyData": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "ForeignKeyData"),
    "FusionGridEditActionArgs.IsScroll": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "IsScroll"),
    "FusionGridEditActionArgs.PrimaryKey": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "PrimaryKey"),
    "FusionGridEditActionArgs.PrimaryKeyValue": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "PrimaryKeyValue"),
    "FusionGridEditActionArgs.RowData": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "RowData"),
    "FusionGridEditActionArgs.Index": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "Index")
  };
  const actionCompleteOnly = {
    "FusionGridEditActionArgs.Promise": missingPropertyAssertionSnippet("FusionGridEditActionArgs<ResidentDirectoryGridItem>", "Promise")
  };
  const memberToSnippet = variant === "actionBegin/save-edit"
    ? commonExcluded
    : variant === "actionComplete/save-edit"
      ? { ...commonExcluded, ...actionCompleteOnly }
      : {};
  const required = memberToSnippet[member];
  if (!required) return false;

  const testPath = variant === "actionBegin/save-edit"
    ? "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridEditing.cs"
    : "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridActionCompleteSaveEdit.cs";
  const testName = variant === "actionBegin/save-edit"
    ? "action_begin_save_edit_reads_typed_current_previous_and_action_fields"
    : "action_complete_save_edit_reads_typed_current_previous_and_action_fields";

  const proofSnippets = variant === "actionBegin/save-edit"
    ? [
        "`actionBegin` save/edit",
        "action_begin_save_edit_reads_typed_current_previous_and_action_fields"
      ]
    : [
        "`actionComplete` save/edit",
        "action_complete_save_edit_reads_typed_current_previous_and_action_fields"
      ];

  return filesContainRequiredEvidence([
    {
      path: testPath,
      method: testName,
      snippets: [
        required
      ]
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: proofSnippets
    }
  ]);
}

function variantDecisionCoverage(variant, member, decision, artifactRoot) {
  if (decision.kind === "accepted" && provenVariantAcceptedMember(variant.name, member, artifactRoot)) {
    return acceptedVariantCoverage(variant);
  }
  if (
    decision.kind === "excluded" &&
    provenVariantExcludedMember(variant.name, member) &&
    variantExclusionProofRequirementsMet(variant.name, member)
  ) {
    return excludedVariantCoverage(variant);
  }
  return pendingVariantDecisionCoverage(variant, decision);
}

function provenVariantAcceptedMember(variant, member, artifactRoot) {
  const proven = {
    "sorting": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.Cancel",
      "FusionGridAction.ColumnName",
      "FusionGridAction.Direction",
      "FusionGridSortColumn.Name",
      "FusionGridSortColumn.Direction"
    ],
    "paging": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.Cancel",
      "FusionGridAction.CurrentPage",
      "FusionGridAction.PreviousPage",
      "FusionGridAction.PageSize"
    ],
    "filtering-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridTextFilterCriterion.Condition",
      "FusionGridTextFilterCriterion.IgnoreCase",
      "FusionGridTextFilterCriterion.IgnoreAccent",
      "FusionGridTextFilterCriterion.Predicates",
      "FusionGridTextFilterCriterion.Field",
      "FusionGridTextFilterCriterion.Operator",
      "FusionGridTextFilterCriterion.Value",
      "FusionGridTextFilterCriterion.IsComplex [where]",
      "FusionGridTextFilterCriterion.IsComplex [where.predicates]",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.Cancel"
    ],
    "filtering-filterbar": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridTextFilterCriterion.Condition",
      "FusionGridTextFilterCriterion.IgnoreCase",
      "FusionGridTextFilterCriterion.IgnoreAccent",
      "FusionGridTextFilterCriterion.Predicates",
      "FusionGridTextFilterCriterion.Field",
      "FusionGridTextFilterCriterion.Operator",
      "FusionGridTextFilterCriterion.Value",
      "FusionGridTextFilterCriterion.IsComplex [where]",
      "FusionGridTextFilterCriterion.IsComplex [where.predicates]",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.Cancel"
    ],
    "clear-filtering-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name"
    ],
    "searching-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridSearchDescriptor.Fields",
      "FusionGridSearchDescriptor.Key",
      "FusionGridSearchDescriptor.Operator",
      "FusionGridSearchDescriptor.IgnoreCase",
      "FusionGridSearchDescriptor.IgnoreAccent",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type"
    ],
    "clear-search-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type"
    ],
    "clear-sorting-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type"
    ],
    "grouping-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridSortColumn.Name",
      "FusionGridSortColumn.Direction",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.ColumnName"
    ],
    "ungrouping-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.ColumnName"
    ],
    "clear-grouping-method": [
      "FusionGridDataStateChangeArgs.Name",
      "FusionGridDataStateChangeArgs.Skip",
      "FusionGridDataStateChangeArgs.Take",
      "FusionGridDataStateChangeArgs.RequiresCounts",
      "FusionGridAction.RequestType",
      "FusionGridAction.Name",
      "FusionGridAction.Type",
      "FusionGridAction.ColumnName"
    ]
  };
  if (proven[variant]?.includes(member) !== true) return false;
  return variantProofRequirementsMet(variant, member, artifactRoot);
}

function variantProofRequirementsMet(variant, member, artifactRoot) {
  if (
    (variant === "filtering-method" || variant === "filtering-filterbar") &&
    [
      "FusionGridTextFilterCriterion.IsComplex [where]",
      "FusionGridTextFilterCriterion.IsComplex [where.predicates]"
    ].includes(member)
  ) {
    return filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.Fusion/Components/FusionGrid/Events/FusionGridOnDataStateChange.cs",
        snippets: ["public bool IsComplex { get; set; }"]
      },
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Controllers/Components/Fusion/GridController.cs",
        snippets: ["filter.IsComplex && filter.Predicates is { Count: > 0 }"]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: variant === "filtering-filterbar"
          ? "filtering_filterbar_typing_sends_typed_where_payload_and_refreshes_grid"
          : "filtering_method_sends_typed_where_payload_and_refreshes_grid",
        snippets: filteringProofTestSnippets(variant)
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: filteringProofArtifactSnippets(variant)
      }
    ]);
  }

  if (variant === "ungrouping-method") {
    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "ungrouping_method_sends_typed_action_payload_and_refreshes_grid",
        snippets: ungroupingProofTestSnippets()
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ungroupingProofArtifactSnippets()
      }
    ]);
  }

  if (variant === "clear-grouping-method") {
    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_grouping_method_clears_all_active_groups_and_refreshes_grid",
        snippets: clearGroupingProofTestSnippets()
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: clearGroupingProofArtifactSnippets()
      }
    ]);
  }

  if (variant === "clear-sorting-method") {
    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_sorting_method_clears_active_sort_and_refreshes_grid",
        snippets: clearSortingProofTestSnippets()
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: clearSortingProofArtifactSnippets()
      }
    ]);
  }

  if (variant === "clear-search-method") {
    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_search_method_clears_active_search_and_refreshes_grid",
        snippets: clearSearchProofTestSnippets()
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: clearSearchProofArtifactSnippets()
      }
    ]);
  }

  if (variant === "clear-filtering-method") {
    return filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".ClearFiltering()",
          'p.Element("method-status").SetText("filters cleared")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_filtering_method_clears_active_filter_and_refreshes_grid",
        snippets: clearFilteringProofTestSnippets()
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: clearFilteringProofArtifactSnippets()
      }
    ]);
  }

  return true;
}

function ungroupingProofTestSnippets() {
  return [
    'ClickWhenStable(Page.Locator("#grid-ungroup-care"))',
    'root.TryGetProperty("requiresCounts", out _), Is.False',
    'root.TryGetProperty("group", out _), Is.False',
    'Page.Locator("#grid-action")).ToHaveTextAsync("ungrouping"',
    'Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange"',
    'Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true"',
    'Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#grid-column")).ToHaveTextAsync("careLevel"',
    'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
    'ToHaveCountAsync(0'
  ];
}

function clearGroupingProofTestSnippets() {
  return [
    'ClickWhenStable(Page.Locator("#grid-group-care"))',
    'ClickWhenStable(Page.Locator("#grid-group-wing"))',
    'ClickWhenStable(Page.Locator("#grid-clear-grouping"))',
    'root.TryGetProperty("requiresCounts", out _), Is.False',
    'root.TryGetProperty("group", out _), Is.False',
    'root.TryGetProperty("groups", out _), Is.False',
    'root.TryGetProperty("sorted", out _), Is.False',
    'Page.Locator("#grid-action")).ToHaveTextAsync("ungrouping"',
    'Page.Locator("#grid-column")).ToHaveTextAsync("wing"',
    'Page.Locator("#method-status")).ToHaveTextAsync("grouping cleared"',
    'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
    'ToHaveCountAsync(0',
    'ToContainTextAsync("Amina Patel"'
  ];
}

function clearSortingProofTestSnippets() {
  return [
    'ClickWhenStable(Page.Locator("#grid-sort-risk"))',
    'ClickWhenStable(Page.Locator("#grid-clear-sorting"))',
    'root.GetProperty("skip").GetInt32(), Is.EqualTo(0)',
    'root.GetProperty("take").GetInt32(), Is.EqualTo(8)',
    'root.TryGetProperty("sorted", out _), Is.False',
    'root.TryGetProperty("actionColumnName", out _), Is.False',
    'root.TryGetProperty("actionDirection", out _), Is.False',
    'root.TryGetProperty("actionTarget", out _), Is.False',
    'Page.Locator("#method-status")).ToHaveTextAsync("sorting cleared"',
    'Page.Locator("#grid-action")).ToHaveTextAsync("sorting"',
    'Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange"',
    'Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true"',
    'Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#grid-action-cancel")).Not.ToHaveTextAsync("false"',
    'Page.Locator("#grid-column")).Not.ToHaveTextAsync("riskLevel"',
    'Page.Locator("#grid-direction")).Not.ToHaveTextAsync("Descending"',
    'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
    'ToContainTextAsync("Grace Bennett"',
    'ToHaveTextAsync("Moderate"',
    'ToContainTextAsync("Amina Patel"',
    'ToHaveTextAsync("Low"'
  ];
}

function clearSearchProofTestSnippets() {
  return [
    'ClickWhenStable(Page.Locator("#grid-search-memory"))',
    'ClickWhenStable(Page.Locator("#grid-clear-search"))',
    'root.GetProperty("skip").GetInt32(), Is.EqualTo(0)',
    'root.GetProperty("take").GetInt32(), Is.EqualTo(8)',
    'root.TryGetProperty("search", out _), Is.False',
    'root.TryGetProperty("actionSearchString", out _), Is.False',
    'Page.Locator("#method-status")).ToHaveTextAsync("search cleared"',
    'Page.Locator("#grid-action")).ToHaveTextAsync("searching"',
    'Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange"',
    'Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true"',
    'Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#grid-action-type")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
    'ToContainTextAsync("Amina Patel"'
  ];
}

function clearFilteringProofTestSnippets() {
  return [
    'ClickWhenStable(Page.Locator("#grid-filter-north"))',
    'ClickWhenStable(Page.Locator("#grid-clear-filters"))',
    'root.GetProperty("skip").GetInt32(), Is.EqualTo(0)',
    'root.GetProperty("take").GetInt32(), Is.EqualTo(8)',
    'root.TryGetProperty("where", out _), Is.False',
    'root.TryGetProperty("actionCurrentFilterObject", out _), Is.False',
    'Page.Locator("#method-status")).ToHaveTextAsync("filters cleared"',
    'Page.Locator("#grid-action")).ToHaveTextAsync("refresh"',
    'Page.Locator("#grid-event")).ToHaveTextAsync("dataStateChange"',
    'Page.Locator("#grid-requires-counts")).ToHaveTextAsync("true"',
    'Page.Locator("#grid-action-name")).ToHaveTextAsync("actionBegin"',
    'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
    'ToContainTextAsync("Amina Patel"'
  ];
}

function ungroupingProofArtifactSnippets() {
  return [
    "Ungrouping method proof",
    "`group` is absent rather than an empty array",
    "`requiresCounts` is read from the typed event payload but is not gathered into the Directory request body",
    "`action.requestType` is `ungrouping`",
    "visible summary is `240 residents matched`",
    "group caption count is `0`"
  ];
}

function clearGroupingProofArtifactSnippets() {
  return [
    "ClearGrouping method proof",
    "typed `ClearGrouping()` starts from active `GroupBy` calls for care level and wing",
    "`group` is absent from the clear request body, not an empty array",
    "`action.requestType` is `ungrouping`",
    "`action.columnName` is the final active group column `wing`",
    "group caption count is `0`"
  ];
}

function clearSortingProofArtifactSnippets() {
  return [
    "ClearSorting method proof",
    "typed `ClearSorting()` starts from an active `SortBy` state",
    "`sorted` is absent from the clear request body, not an empty array",
    "`action.requestType` is `sorting`",
    "`action.columnName`, `action.direction`, and `action.target` remain excluded for this clear row",
    "visible summary is `240 residents matched`",
    "first visible row contains `Amina Patel`"
  ];
}

function clearSearchProofArtifactSnippets() {
  return [
    "ClearSearch method proof",
    "typed `ClearSearch()` starts from an active `Search(\"Memory\")` state",
    "`search` is absent from the clear request body, not an empty array",
    "`action.searchString` remains excluded from public C#",
    "visible summary is `240 residents matched`",
    "first visible row contains `Amina Patel`"
  ];
}

function clearFilteringProofArtifactSnippets() {
  return [
    "Clear-filtering method proof",
    "typed `ClearFiltering()` starts from an active `FilterTextBy",
    "`where` is absent from the clear request body, not an empty array",
    "visible `action.requestType=refresh`",
    "visible summary is `240 residents matched`",
    "first visible row contains `Amina Patel`"
  ];
}

function filteringProofTestSnippets(variant) {
  const shared = [
    'complex.GetProperty("isComplex").GetBoolean(), Is.True',
    'predicate.GetProperty("isComplex").GetBoolean(), Is.False',
    'ToHaveTextAsync("60 residents matched"'
  ];
  if (variant === "filtering-filterbar") {
    return [
      'input.PressAsync("Enter")',
      'predicate.GetProperty("operator").GetString(), Is.EqualTo("startswith")',
      'predicate.GetProperty("value").GetString(), Is.EqualTo("N")',
      ...shared
    ];
  }
  return shared;
}

function filteringProofArtifactSnippets(variant) {
  const shared = [
    "`isComplex` is present as the accepted typed predicate discriminator",
    "server checks `FusionGridTextFilterCriterion.IsComplex`"
  ];
  if (variant === "filtering-filterbar") {
    return [
      "FilterBar typing proof",
      "presses Enter",
      "`operator=startswith`",
      "`value=N`",
      ...shared
    ];
  }
  return shared;
}

function filesContainRequiredEvidence(requirements) {
  return requirements.every(requirement => {
    const path = resolve(requirement.path);
    if (!existsSync(path)) return false;
    const text = readFileSync(path, "utf8");
    const evidenceText = requirement.method
      ? csharpMethodBody(text, requirement.method)
      : text;
    if (evidenceText === null) return false;
    return requirement.snippets.every(snippet => evidenceText.includes(snippet));
  });
}

function csharpMethodBody(text, methodName) {
  const methodIndex = text.indexOf(`${methodName}(`);
  if (methodIndex < 0) return null;

  const bodyStart = text.indexOf("{", methodIndex);
  if (bodyStart < 0) return null;

  let depth = 0;
  for (let index = bodyStart; index < text.length; index += 1) {
    const char = text[index];
    if (char === "{") depth += 1;
    if (char === "}") {
      depth -= 1;
      if (depth === 0) return text.slice(bodyStart, index + 1);
    }
  }

  return null;
}

function missingPropertyAssertionSnippet(typeExpression, propertyName) {
  return `typeof(${typeExpression}).GetProperty("${propertyName}"),\n            Is.Null`;
}

function provenVariantExcludedMember(variant, member) {
  const proven = {
    "sorting": [
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridAction.Target"
    ],
    "paging": [
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridAction.PreviousPageSize",
      "FusionGridAction.Rows",
      "FusionGridAction.Target"
    ],
    "filtering-method": [
      "FusionGridAction.Action",
      "FusionGridAction.CurrentFilteringColumn",
      "FusionGridAction.CurrentFilterObject",
      "FusionGridAction.Columns",
      "FusionGridTextFilterCriterion.MatchCase",
      "FusionGridTextFilterCriterion.Predicate",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted"
    ],
    "searching-method": [
      "FusionGridAction.SearchString",
      "FusionGridAction.Cancel",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted"
    ],
    "clear-search-method": [
      "FusionGridAction.SearchString",
      "FusionGridAction.Cancel",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted"
    ],
    "clear-sorting-method": [
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Aggregates",
      "FusionGridAction.ColumnName",
      "FusionGridAction.Direction",
      "FusionGridAction.Cancel",
      "FusionGridAction.Target"
    ],
    "grouping-method": [
      "FusionGridDataStateChangeArgs.Groups",
      "FusionGridAction.PreventFocusOnGroup",
      "FusionGridAction.Cancel",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Aggregates"
    ],
    "ungrouping-method": [
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Aggregates",
      "FusionGridDataStateChangeArgs.Groups",
      "FusionGridAction.Cancel",
      "FusionGridAction.PreventFocusOnGroup"
    ],
    "clear-grouping-method": [
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Groups",
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Aggregates",
      "FusionGridAction.Cancel",
      "FusionGridAction.PreventFocusOnGroup"
    ],
    "filtering-filterbar": [
      "FusionGridAction.Action",
      "FusionGridAction.CurrentFilteringColumn",
      "FusionGridAction.CurrentFilterObject",
      "FusionGridAction.Columns",
      "FusionGridTextFilterCriterion.MatchCase",
      "FusionGridTextFilterCriterion.Predicate",
      "FusionGridDataStateChangeArgs.Aggregates",
      "FusionGridDataStateChangeArgs.DataSource",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.IsLazyLoad",
      "FusionGridDataStateChangeArgs.OnDemandGroupInfo",
      "FusionGridDataStateChangeArgs.Select",
      "FusionGridDataStateChangeArgs.Sorted",
      "FusionGridDataStateChangeArgs.Table"
    ],
    "clear-filtering-method": [
      "FusionGridAction.Type",
      "FusionGridAction.Cancel",
      "FusionGridAction.Action",
      "FusionGridAction.CurrentFilteringColumn",
      "FusionGridAction.CurrentFilterObject",
      "FusionGridAction.Columns",
      "FusionGridDataStateChangeArgs.Where",
      "FusionGridDataStateChangeArgs.Search",
      "FusionGridDataStateChangeArgs.Group",
      "FusionGridDataStateChangeArgs.Sorted"
    ]
  };
  return proven[variant]?.includes(member) === true;
}

function variantExclusionProofRequirementsMet(variant, member) {
  if (variant === "sorting") {
    const memberToSnippets = {
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridAction.Target": [
        missingPropertyAssertionSnippet("FusionGridAction", "Target")
      ]
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGrid.cs",
        method: "sorting_a_column_fetches_sorted_data_and_echoes_action",
        snippets: [
          ...required
        ]
      }
    ]);
  }

  if (variant === "paging") {
    const memberToSnippets = {
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False'],
      "FusionGridAction.PreviousPageSize": [
        missingPropertyAssertionSnippet("FusionGridAction", "PreviousPageSize"),
        'root.TryGetProperty("actionPreviousPageSize", out _), Is.False'
      ],
      "FusionGridAction.Rows": [
        missingPropertyAssertionSnippet("FusionGridAction", "Rows"),
        'root.TryGetProperty("actionRows", out _), Is.False'
      ],
      "FusionGridAction.Target": [
        missingPropertyAssertionSnippet("FusionGridAction", "Target"),
        'root.TryGetProperty("actionTarget", out _), Is.False'
      ]
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGrid.cs",
        method: "paging_fetches_next_page_with_correct_skip",
        snippets: [
          ...required
        ]
      }
    ]);
  }

  if (variant === "searching-method") {
    const memberToSnippets = {
      "FusionGridAction.SearchString": [
        missingPropertyAssertionSnippet("FusionGridAction", "SearchString"),
        'root.TryGetProperty("actionSearchString", out _), Is.False'
      ],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False']
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "searching_method_sends_typed_search_payload_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant === "clear-search-method") {
    const memberToSnippets = {
      "FusionGridAction.SearchString": [
        missingPropertyAssertionSnippet("FusionGridAction", "SearchString"),
        'root.TryGetProperty("actionSearchString", out _), Is.False'
      ],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False']
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_search_method_clears_active_search_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant === "clear-sorting-method") {
    const memberToSnippets = {
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Aggregates": ['root.TryGetProperty("aggregates", out _), Is.False'],
      "FusionGridAction.ColumnName": ['root.TryGetProperty("actionColumnName", out _), Is.False'],
      "FusionGridAction.Direction": ['root.TryGetProperty("actionDirection", out _), Is.False'],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridAction.Target": [
        missingPropertyAssertionSnippet("FusionGridAction", "Target"),
        'root.TryGetProperty("actionTarget", out _), Is.False'
      ]
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_sorting_method_clears_active_sort_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant === "clear-filtering-method") {
    const memberToSnippets = {
      "FusionGridAction.Type": ['root.TryGetProperty("actionType", out _), Is.False'],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridAction.Action": ['root.TryGetProperty("actionAction", out _), Is.False'],
      "FusionGridAction.CurrentFilteringColumn": ['root.TryGetProperty("actionCurrentFilteringColumn", out _), Is.False'],
      "FusionGridAction.CurrentFilterObject": ['root.TryGetProperty("actionCurrentFilterObject", out _), Is.False'],
      "FusionGridAction.Columns": ['root.TryGetProperty("actionColumns", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False']
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_filtering_method_clears_active_filter_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant === "grouping-method") {
    const memberToSnippets = {
      "FusionGridDataStateChangeArgs.Groups": [
        missingPropertyAssertionSnippet("FusionGridDataStateChangeArgs", "Groups"),
        'root.TryGetProperty("groups", out _), Is.False'
      ],
      "FusionGridAction.PreventFocusOnGroup": [
        missingPropertyAssertionSnippet("FusionGridAction", "PreventFocusOnGroup"),
        'root.TryGetProperty("actionPreventFocusOnGroup", out _), Is.False'
      ],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Aggregates": ['root.TryGetProperty("aggregates", out _), Is.False']
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "grouping_method_sends_typed_group_payload_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant === "ungrouping-method") {
    const memberToSnippets = {
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Aggregates": [
        missingPropertyAssertionSnippet("FusionGridDataStateChangeArgs", "Aggregates"),
        'root.TryGetProperty("aggregates", out _), Is.False'
      ],
      "FusionGridDataStateChangeArgs.Groups": [
        missingPropertyAssertionSnippet("FusionGridDataStateChangeArgs", "Groups"),
        'root.TryGetProperty("groups", out _), Is.False'
      ],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridAction.PreventFocusOnGroup": [
        missingPropertyAssertionSnippet("FusionGridAction", "PreventFocusOnGroup"),
        'root.TryGetProperty("actionPreventFocusOnGroup", out _), Is.False'
      ]
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "ungrouping_method_sends_typed_action_payload_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant === "clear-grouping-method") {
    const memberToSnippets = {
      "FusionGridDataStateChangeArgs.Group": ['root.TryGetProperty("group", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Groups": [
        missingPropertyAssertionSnippet("FusionGridDataStateChangeArgs", "Groups"),
        'root.TryGetProperty("groups", out _), Is.False'
      ],
      "FusionGridDataStateChangeArgs.Sorted": ['root.TryGetProperty("sorted", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Where": ['root.TryGetProperty("where", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Search": ['root.TryGetProperty("search", out _), Is.False'],
      "FusionGridDataStateChangeArgs.Aggregates": [
        missingPropertyAssertionSnippet("FusionGridDataStateChangeArgs", "Aggregates"),
        'root.TryGetProperty("aggregates", out _), Is.False'
      ],
      "FusionGridAction.Cancel": ['root.TryGetProperty("actionCancel", out _), Is.False'],
      "FusionGridAction.PreventFocusOnGroup": [
        missingPropertyAssertionSnippet("FusionGridAction", "PreventFocusOnGroup"),
        'root.TryGetProperty("actionPreventFocusOnGroup", out _), Is.False'
      ]
    };

    const required = memberToSnippets[member];
    if (!required) return true;

    return filesContainRequiredEvidence([
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_grouping_method_clears_all_active_groups_and_refreshes_grid",
        snippets: required
      }
    ]);
  }

  if (variant !== "filtering-filterbar") return true;

  const memberToSnippet = {
    "FusionGridDataStateChangeArgs.Aggregates": 'root.TryGetProperty("aggregates", out _), Is.False',
    "FusionGridDataStateChangeArgs.DataSource": 'root.TryGetProperty("dataSource", out _), Is.False',
    "FusionGridDataStateChangeArgs.Search": 'root.TryGetProperty("search", out _), Is.False',
    "FusionGridDataStateChangeArgs.Group": 'root.TryGetProperty("group", out _), Is.False',
    "FusionGridDataStateChangeArgs.IsLazyLoad": 'root.TryGetProperty("isLazyLoad", out _), Is.False',
    "FusionGridDataStateChangeArgs.OnDemandGroupInfo": 'root.TryGetProperty("onDemandGroupInfo", out _), Is.False',
    "FusionGridDataStateChangeArgs.Select": 'root.TryGetProperty("select", out _), Is.False',
    "FusionGridDataStateChangeArgs.Sorted": 'root.TryGetProperty("sorted", out _), Is.False',
    "FusionGridDataStateChangeArgs.Table": 'root.TryGetProperty("table", out _), Is.False'
  };

  const required = memberToSnippet[member];
  if (!required) return true;

  return filesContainRequiredEvidence([
    {
      path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
      method: "filtering_filterbar_typing_sends_typed_where_payload_and_refreshes_grid",
      snippets: [
        required
      ]
    }
  ]);
}

function acceptedVariantCoverage(variant) {
  return {
    rawTrace: variant.trace,
    primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
    verticalSlice: "[variant vertical slice rows](../mapping/vertical-slice-plan.md)",
    playwrightProof: variant.proof,
    status: "row-proven"
  };
}

function excludedVariantCoverage(variant) {
  return {
    rawTrace: variant.trace,
    primitiveMap: `[judgment row](../${variant.judgment})`,
    verticalSlice: "[variant exclusion rows](../mapping/vertical-slice-plan.md)",
    playwrightProof: variant.proof,
    status: "row-proven"
  };
}

function pendingVariantDecisionCoverage(variant, decision) {
  const label = decision.kind === "accepted" ? "accepted in judgment; pending typed DSL behavior proof" : "excluded in judgment; pending explicit exclusion proof";
  return {
    rawTrace: variant.trace,
    primitiveMap: `[judgment row](../${variant.judgment})`,
    verticalSlice: label,
    playwrightProof: `pending focused ${variant.name} proof for this member`,
    status: "unproven"
  };
}

function rowCoverage(component, member, variantIndex) {
  if (component !== "grid") return pendingCoverage();

  const gridDataStateRows = {
    rawTrace: "[sorting](../traces/raw-ej2-data-state-change-sorting.trace.json), [paging](../traces/raw-ej2-data-state-change-paging-method.trace.json), [filtering](../traces/raw-ej2-data-state-change-filtering-method.trace.json), [clear filtering](../traces/raw-ej2-data-state-change-clear-filtering-method.trace.json), [searching](../traces/raw-ej2-data-state-change-searching-method.trace.json), [clear search](../traces/raw-ej2-data-state-change-clear-search-method.trace.json), [clear sorting](../traces/raw-ej2-data-state-change-clear-sorting-method.trace.json), [grouping](../traces/raw-ej2-data-state-change-grouping-method.trace.json), [ungrouping](../traces/raw-ej2-data-state-change-ungrouping-method.trace.json), [clear grouping](../traces/raw-ej2-data-state-change-clear-grouping-method.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[proof](playwright-proof.md)",
    status: "row-proven"
  };

  const gridDataStatePayloadNames = ["FusionGridAction", "FusionGridDataStateChangeArgs", "FusionGridSearchDescriptor", "FusionGridSortColumn", "FusionGridTextFilterCriterion"];
  if (member.kind === "event-payload-contract" && gridDataStatePayloadNames.includes(member.name)) {
    if (allVariantsProvenForClass(variantIndex, member.name)) return variantCoveredCoverage();
    return variantPendingCoverage("dataStateChange payload requires variant-scoped property rows");
  }

  const gridDataStateProperties = {
    FusionGridAction: ["RequestType", "Name", "Type", "Cancel", "ColumnName", "Direction", "CurrentPage", "PreviousPage", "PageSize"],
    FusionGridDataStateChangeArgs: ["Name", "Skip", "Take", "RequiresCounts", "Sorted", "Group", "Where", "Search", "Action"],
    FusionGridSearchDescriptor: ["Fields", "Key", "Operator", "IgnoreCase", "IgnoreAccent"],
    FusionGridSortColumn: ["Name", "Direction"],
    FusionGridTextFilterCriterion: ["Field", "Operator", "Value", "Condition", "IsComplex", "Predicates", "IgnoreCase", "IgnoreAccent"]
  };

  // P023: a read-only object-valued payload property is covered when its
  // value-type class is fully proven across variants (every nested read
  // exercises the accessor). Writable members inside the object keep their own
  // P018 rows.
  const gridReadOnlyObjectProperties = {
    "FusionGridDataStateChangeArgs.Action": "FusionGridAction"
  };
  if (member.kind === "event-payload-property" && gridReadOnlyObjectProperties[member.name]) {
    if (allVariantsProvenForClass(variantIndex, gridReadOnlyObjectProperties[member.name])) {
      return variantCoveredCoverage();
    }
    return variantPendingCoverage("object-valued payload property resolves only when its value-type class is fully proven across variants");
  }

  if (isPayloadProperty(member, gridDataStateProperties)) {
    if (allVariantsProvenForMember(variantIndex, member.name)) return variantCoveredCoverage();
    return variantPendingCoverage("dataStateChange property proof must name accepted/absent/excluded status per trigger variant");
  }

  if (member.kind === "event-selector" && member.name === "DataStateChange") {
    return gridDataStateRows;
  }

  const gridRecordClickRows = {
    rawTrace: "[recordClick](../traces/raw-ej2-record-click-cell.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[recordClick proof](playwright-proof.md)",
    status: "row-proven"
  };

  if (member.kind === "event-payload-contract" && member.name === "FusionGridRecordClickArgs") {
    return gridRecordClickRows;
  }

  if (isPayloadProperty(member, {
    FusionGridRecordClickArgs: ["RowData", "RowIndex", "CellIndex", "Name"]
  })) {
    return gridRecordClickRows;
  }

  if (member.kind === "event-selector" && member.name === "RecordClick") {
    return gridRecordClickRows;
  }

  const gridRowSelectedRows = {
    rawTrace: "[rowSelected](../traces/raw-ej2-row-selected-click.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[rowSelected proof](playwright-proof.md)",
    status: "row-proven"
  };

  if (member.kind === "event-payload-contract" && member.name === "FusionGridRowSelectedArgs") {
    return gridRowSelectedRows;
  }

  if (isPayloadProperty(member, {
    FusionGridRowSelectedArgs: ["Data", "RowIndex", "PreviousRowIndex", "IsInteracted", "Name"]
  })) {
    return gridRowSelectedRows;
  }

  if (member.kind === "event-selector" && member.name === "RowSelected") {
    return gridRowSelectedRows;
  }

  const gridToolbarClickRows = {
    rawTrace: "[toolbarClick](../traces/raw-ej2-toolbar-click-custom.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[toolbarClick proof](playwright-proof.md)",
    status: "row-proven"
  };

  if (member.kind === "event-payload-contract" &&
      ["FusionGridToolbarClickArgs", "FusionGridToolbarItem"].includes(member.name)) {
    return gridToolbarClickRows;
  }

  if (isPayloadProperty(member, {
    FusionGridToolbarClickArgs: ["Item", "Cancel", "Name"],
    FusionGridToolbarItem: ["Id", "Text"]
  })) {
    return gridToolbarClickRows;
  }

  if (member.kind === "event-selector" && member.name === "ToolbarClick") {
    return gridToolbarClickRows;
  }

  if (member.kind === "event-selector" && member.name === "Reactive") {
    // .Reactive(evt => evt.X(), handler) is the event+Wire lane entry point, exercised
    // by every event proof. The batch-cell slice wires CellSave through it and asserts
    // the handler's visible output, so the wiring extension itself is proven.
    if (batchCellSaveProven()) {
      return {
        rawTrace: "[cellSave batch edit](../traces/raw-ej2-cell-save-batch-edit.trace.json)",
        primitiveMap: "[event+Wire](../mapping/primitive-map.md)",
        verticalSlice: "[batch cell edit slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[Batch cell save proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
    return variantPendingCoverage("Reactive wiring requires a proven event scenario");
  }

  if (member.kind === "event-selector" && member.name === "ActionBegin") {
    if (editActionProven()) return editActionCoverage();
    return {
      rawTrace: "[actionBegin save/edit](../traces/raw-ej2-action-begin-save-edit.trace.json)",
      primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
      verticalSlice: "broad ActionBegin selector remains open until ActionBegin variants beyond save/edit are discovered and proven",
      playwrightProof: "pending complete ActionBegin variant coverage",
      status: "unproven"
    };
  }

  if (member.kind === "event-selector" && member.name === "ActionComplete") {
    if (editActionProven()) return editActionCoverage();
    return {
      rawTrace: "[actionComplete save/edit](../traces/raw-ej2-action-complete-save-edit.trace.json)",
      primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
      verticalSlice: "broad ActionComplete selector remains open until actionComplete variants beyond save/edit are discovered and proven",
      playwrightProof: "pending complete ActionComplete variant coverage",
      status: "unproven"
    };
  }

  if (member.kind === "event-payload-contract" && member.name === "FusionGridEditActionArgs") {
    if (editActionProven()) return editActionCoverage();
    return variantPendingCoverage("edit-action payload requires save-edit and add/delete variant proof");
  }

  if (member.kind === "method" && member.name === "FusionGridEditActionArgs.Cancel()") {
    if (editActionAddDeleteProven()) return cancelMutationCoverage("Resident admission audit");
    return variantPendingCoverage("edit-action cancel mutation requires variant-scoped proof");
  }

  if (member.kind === "event-selector" && member.name === "BeginEdit") {
    if (beginEditEquivalenceProven() && beginEditCancelFlagProven()) {
      return beginEditEquivalenceCoverage();
    }
    return {
      rawTrace: "[beginEdit normal edit](../traces/raw-ej2-begin-edit-normal.trace.json)",
      primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
      verticalSlice: "broad BeginEdit selector remains open until beginEdit variants beyond normal edit are discovered and proven",
      playwrightProof: "pending complete BeginEdit variant coverage",
      status: "unproven"
    };
  }

  if (member.kind === "event-payload-contract" && member.name === "FusionGridBeginEditArgs") {
    if (beginEditEquivalenceProven()) return beginEditEquivalenceCoverage();
    return variantPendingCoverage("beginEdit payload requires variant-scoped property rows");
  }

  // Read-only reads resolve through normal+dialog equivalence (P022).
  if (isPayloadProperty(member, {
    FusionGridBeginEditArgs: ["RowData", "RowIndex", "Type"]
  })) {
    if (beginEditEquivalenceProven()) return beginEditEquivalenceCoverage();
    return variantPendingCoverage("beginEdit property proof must name accepted/absent/excluded status per trigger variant");
  }

  // The writable Cancel flag resolves only through mutation behavior (P018/P021).
  if (isPayloadProperty(member, { FusionGridBeginEditArgs: ["Cancel"] })) {
    if (beginEditCancelFlagProven()) return cancelMutationCoverage("BeginEdit Cancel");
    return variantPendingCoverage("beginEdit cancel flag requires mutation behavior proof");
  }

  if (member.kind === "method" && member.name === "FusionGridBeginEditArgs.Cancel()") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/LockedResidentEdit.cshtml",
        snippets: ["args.Cancel(t)", 'SetText("edit blocked for 6001")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridLockedEdit.cs",
        method: "canceling_begin_edit_blocks_the_locked_resident_but_allows_others",
        snippets: ['ClickWhenStable(Page.Locator("#edit-6001"))', "edit blocked for 6001", "ToHaveCountAsync(0"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["BeginEdit Cancel proof"] }
    ])) {
      return cancelMutationCoverage("BeginEdit Cancel");
    }
    return variantPendingCoverage("beginEdit cancel mutation requires variant-scoped proof");
  }

  if (member.kind === "event-selector" && member.name === "BeforeBatchSave") {
    if (beforeBatchSaveChangedProven()) return batchEventCoverage("beforeBatchSave batch edit", "raw-ej2-before-batch-save-batch-edit");
    return {
      rawTrace: "[beforeBatchSave batch edit](../traces/raw-ej2-before-batch-save-batch-edit.trace.json)",
      primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
      verticalSlice: "broad BeforeBatchSave selector remains open until beforeBatchSave variants beyond batch edit are discovered and proven",
      playwrightProof: "pending complete BeforeBatchSave variant coverage",
      status: "unproven"
    };
  }

  // The beforeBatchSave contract and its BatchChanges property stay open until the
  // BatchChanges value-type is fully proven (AddedRecords/DeletedRecords still need a
  // batch admit/discharge slice). The writable Cancel flag resolves on mutation (P018).
  if (member.kind === "event-payload-contract" && member.name === "FusionGridBeforeBatchSaveArgs") {
    if (batchChangesFullyProven() && beforeBatchSaveChangedProven()) return batchRosterCoverage();
    return variantPendingCoverage("beforeBatchSave payload requires AddedRecords/DeletedRecords proof via a batch admit/discharge slice");
  }

  if (isPayloadProperty(member, { FusionGridBeforeBatchSaveArgs: ["Cancel"] })) {
    if (beforeBatchSaveChangedProven()) return cancelMutationCoverage("Before batch save");
    return variantPendingCoverage("beforeBatchSave cancel flag requires mutation behavior proof");
  }

  // P023: the BatchChanges property is covered when its value-type class is fully proven.
  if (isPayloadProperty(member, { FusionGridBeforeBatchSaveArgs: ["BatchChanges"] })) {
    if (batchChangesFullyProven()) return batchRosterCoverage();
    return variantPendingCoverage("beforeBatchSave BatchChanges property requires its value-type fully proven (AddedRecords/DeletedRecords open)");
  }

  if (member.kind === "method" && member.name === "FusionGridBeforeBatchSaveArgs.Cancel()") {
    if (beforeBatchSaveChangedProven()) return cancelMutationCoverage("Before batch save");
    return variantPendingCoverage("beforeBatchSave cancel mutation requires variant-scoped proof");
  }

  // BatchChanges.ChangedRecords is proven by the beforeBatchSave read; AddedRecords/
  // DeletedRecords and the aggregate contract stay open (need batch admit/discharge).
  if (isPayloadProperty(member, { FusionGridBatchChanges: ["ChangedRecords"] })) {
    if (beforeBatchSaveChangedProven()) return batchEventCoverage("beforeBatchSave batch edit", "raw-ej2-before-batch-save-batch-edit");
    return variantPendingCoverage("batchChanges changedRecords requires beforeBatchSave proof");
  }

  if (isPayloadProperty(member, { FusionGridBatchChanges: ["AddedRecords", "DeletedRecords"] })) {
    if (batchRosterChangeProven()) return batchRosterCoverage();
    return variantPendingCoverage("batchChanges added/deleted records require a batch admit/discharge slice");
  }

  if (member.kind === "event-payload-contract" && member.name === "FusionGridBatchChanges") {
    if (batchChangesFullyProven()) return batchRosterCoverage();
    return variantPendingCoverage("batchChanges payload requires AddedRecords/DeletedRecords proof via a batch admit/discharge slice");
  }

  if (member.kind === "event-selector" && member.name === "CellSave") {
    if (batchCellSaveProven()) return batchEventCoverage("cellSave batch edit", "raw-ej2-cell-save-batch-edit");
    return {
      rawTrace: "[cellSave batch edit](../traces/raw-ej2-cell-save-batch-edit.trace.json)",
      primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
      verticalSlice: "broad CellSave selector remains open until cellSave variants beyond batch edit are discovered and proven",
      playwrightProof: "pending complete CellSave variant coverage",
      status: "unproven"
    };
  }

  if (member.kind === "event-payload-contract" && member.name === "FusionGridCellSaveArgs") {
    if (batchCellSaveProven()) return batchEventCoverage("cellSave batch edit", "raw-ej2-cell-save-batch-edit");
    return variantPendingCoverage("cellSave payload requires variant-scoped property rows");
  }

  // Read-only cell fields resolve through the batch-cell read proof.
  if (isPayloadProperty(member, {
    FusionGridCellSaveArgs: ["RowData", "ColumnName", "Value", "PreviousValue"]
  })) {
    if (batchCellSaveProven()) return batchEventCoverage("cellSave batch edit", "raw-ej2-cell-save-batch-edit");
    return variantPendingCoverage("cellSave property proof must name accepted/absent/excluded status per trigger variant");
  }

  // The writable Cancel flag resolves only through its mutation behavior (P018).
  if (isPayloadProperty(member, { FusionGridCellSaveArgs: ["Cancel"] })) {
    if (batchCellSaveProven()) return cancelMutationCoverage("Batch cell save");
    return variantPendingCoverage("cellSave cancel flag requires mutation behavior proof");
  }

  if (member.kind === "method" && member.name === "FusionGridCellSaveArgs.Cancel()") {
    if (batchCellSaveProven()) return cancelMutationCoverage("Batch cell save");
    return variantPendingCoverage("cellSave cancel mutation requires variant-scoped proof");
  }

  if (member.kind === "event-selector" && member.name === "CellSaved") {
    if (batchCellSavedProven()) return batchEventCoverage("cellSaved batch edit", "raw-ej2-cell-saved-batch-edit");
    return {
      rawTrace: "[cellSaved batch edit](../traces/raw-ej2-cell-saved-batch-edit.trace.json)",
      primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
      verticalSlice: "broad CellSaved selector remains open until cellSaved variants beyond batch edit are discovered and proven",
      playwrightProof: "pending complete CellSaved variant coverage",
      status: "unproven"
    };
  }

  if (member.kind === "event-payload-contract" && member.name === "FusionGridCellSavedArgs") {
    if (batchCellSavedProven()) return batchEventCoverage("cellSaved batch edit", "raw-ej2-cell-saved-batch-edit");
    return variantPendingCoverage("cellSaved payload requires variant-scoped property rows");
  }

  if (isPayloadProperty(member, {
    FusionGridCellSavedArgs: ["RowData", "ColumnName", "Value", "PreviousValue"]
  })) {
    if (batchCellSavedProven()) return batchEventCoverage("cellSaved batch edit", "raw-ej2-cell-saved-batch-edit");
    return variantPendingCoverage("cellSaved property proof must name accepted/absent/excluded status per trigger variant");
  }

  // Read-only edit-action members resolve across save-edit + add/delete variants.
  // RowIndex/SelectedRow/PreviousData are read by the save-edit slice (where the typed
  // surface applies) and proven-absent for add/delete by raw EJ2 evidence (P024).
  if (isPayloadProperty(member, {
    FusionGridEditActionArgs: ["Name", "RequestType", "Action", "Type", "Data", "PreviousData", "RowIndex", "SelectedRow"]
  })) {
    if (editActionProven()) return editActionCoverage();
    return variantPendingCoverage("shared edit-action property remains open until save-edit and add/delete variants are proven");
  }

  // The writable Cancel flag resolves only through the edit-action Cancel mutation (P018).
  if (isPayloadProperty(member, { FusionGridEditActionArgs: ["Cancel"] })) {
    if (editActionAddDeleteProven()) return cancelMutationCoverage("Resident admission audit");
    return variantPendingCoverage("edit-action cancel flag requires mutation behavior proof");
  }

  if (member.kind === "method" && member.methodName === "FilterTextBy") {
    return {
      rawTrace: "[filtering](../traces/raw-ej2-data-state-change-filtering-method.trace.json)",
      primitiveMap: "[primitive map](../mapping/primitive-map.md)",
      verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
      playwrightProof: "[filtering proof](playwright-proof.md)",
      status: "row-proven"
    };
  }

  if (member.kind === "method" && member.methodName === "ClearFiltering") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".ClearFiltering()",
          'p.Element("method-status").SetText("filters cleared")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_filtering_method_clears_active_filter_and_refreshes_grid",
        snippets: [
          'ClickWhenStable(Page.Locator("#grid-filter-north"))',
          'ClickWhenStable(Page.Locator("#grid-clear-filters"))',
          'root.TryGetProperty("where", out _), Is.False',
          'Page.Locator("#method-status")).ToHaveTextAsync("filters cleared"',
          'Page.Locator("#grid-action")).ToHaveTextAsync("refresh"',
          'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
          'ToContainTextAsync("Amina Patel"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "Clear-filtering method proof",
          "typed `ClearFiltering()` starts from an active `FilterTextBy",
          "`where` is absent from the clear request body, not an empty array"
        ]
      }
    ])) {
      return {
        rawTrace: "[clear filtering](../traces/raw-ej2-data-state-change-clear-filtering-method.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[clear-filtering proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.name.includes("UpdateCell(") && member.name.includes("Expression<Func<TRow, string>> field, string value")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchRiskReview.cshtml",
        snippets: ['UpdateCell(0, (ResidentDirectoryGridItem x) => x.RiskLevel, "Critical")', 'SetText("updateCell string called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchRisk.cs",
        method: "flagging_a_risk_cell_and_gathering_batch_changes_reports_the_pending_change",
        snippets: ['ClickWhenStable(Page.Locator("#batch-risk-flag"))', '"Critical"']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["UpdateCell string proof"] }
    ])) {
      return batchRiskMethodCoverage("UpdateCell string");
    }
  }

  if (member.kind === "method" && member.methodName === "BatchChanges") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchRiskReview.cshtml",
        snippets: ["BatchChanges<ResidentGridEditingModel, ResidentDirectoryGridItem>()", '"batchChanges"']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchRisk.cs",
        method: "flagging_a_risk_cell_and_gathering_batch_changes_reports_the_pending_change",
        snippets: ['ClickWhenStable(Page.Locator("#batch-risk-gather"))', "changed 1"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["BatchChanges proof"] }
    ])) {
      return batchRiskMethodCoverage("BatchChanges");
    }
  }

  if (member.kind === "method" && member.name.includes("FusionGrid(this IHtmlHelper")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareStaffColumns.cshtml",
        snippets: ["Html.FusionGrid<GridOperationsModel, ResidentDirectoryGridItem>(plan, GridId"]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridColumns.cs",
        method: "hiding_care_columns_removes_their_headers",
        snippets: ['"Resident", "Risk", "Primary Nurse", "Next Review", "Wing", "Care Level"']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["FusionGrid render helper proof"] }
    ])) {
      return {
        rawTrace: "[grid renders in every scenario](playwright-proof.md)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[FusionGrid render helper proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "AutoFitColumn") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ColumnFit.cshtml",
        snippets: [".AutoFitColumn((ResidentDirectoryGridItem x) => x.RiskLevel)", 'SetText("autoFitColumn called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridColumnFit.cs",
        method: "auto_fitting_one_column_then_all_columns_shrinks_them_to_content",
        snippets: ['ClickWhenStable(Page.Locator("#fit-risk"))', "autoFitColumn called"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["AutoFitColumn proof"] }
    ])) {
      return autoFitMethodCoverage("AutoFitColumn");
    }
  }

  if (member.kind === "method" && member.methodName === "AutoFitColumns") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ColumnFit.cshtml",
        snippets: [".AutoFitColumns()", 'SetText("autoFitColumns called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridColumnFit.cs",
        method: "auto_fitting_one_column_then_all_columns_shrinks_them_to_content",
        snippets: ['ClickWhenStable(Page.Locator("#fit-all"))', "autoFitColumns called"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["AutoFitColumns proof"] }
    ])) {
      return autoFitMethodCoverage("AutoFitColumns");
    }
  }

  if (member.kind === "method" && member.methodName === "CsvExport") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterExport.cshtml",
        snippets: [".CsvExport()", 'SetText("csvExport called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridExport.cs",
        method: "exporting_the_roster_downloads_csv_excel_and_pdf_files",
        snippets: ['ClickWhenStable(Page.Locator("#export-csv"))', 'EndWith(".csv")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["CsvExport proof"] }
    ])) {
      return exportMethodCoverage("CsvExport");
    }
  }

  if (member.kind === "method" && member.methodName === "ExcelExport") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterExport.cshtml",
        snippets: [".ExcelExport()", 'SetText("excelExport called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridExport.cs",
        method: "exporting_the_roster_downloads_csv_excel_and_pdf_files",
        snippets: ['ClickWhenStable(Page.Locator("#export-excel"))', 'EndWith(".xlsx")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["ExcelExport proof"] }
    ])) {
      return exportMethodCoverage("ExcelExport");
    }
  }

  if (member.kind === "method" && member.methodName === "PdfExport") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterExport.cshtml",
        snippets: [".PdfExport()", 'SetText("pdfExport called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridExport.cs",
        method: "exporting_the_roster_downloads_csv_excel_and_pdf_files",
        snippets: ['ClickWhenStable(Page.Locator("#export-pdf"))', 'EndWith(".pdf")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["PdfExport proof"] }
    ])) {
      return exportMethodCoverage("PdfExport");
    }
  }

  if (member.kind === "method" && member.methodName === "GoToPage") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/GridTooling.cshtml",
        snippets: [".GoToPage(2)", 'SetText("goToPage called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridTooling.cs",
        method: "going_to_a_page_and_opening_the_column_chooser_updates_the_grid_tooling",
        snippets: ['ClickWhenStable(Page.Locator("#tooling-go-page"))', "e-numericitem.e-active"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["GoToPage proof"] }
    ])) {
      return {
        rawTrace: "[dataStateChange paging](../traces/raw-ej2-data-state-change-paging-method.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[GoToPage proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "ShowColumnChooser") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/GridTooling.cshtml",
        snippets: [".ShowColumnChooser()", 'SetText("showColumnChooser called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridTooling.cs",
        method: "going_to_a_page_and_opening_the_column_chooser_updates_the_grid_tooling",
        snippets: ['ClickWhenStable(Page.Locator("#tooling-column-chooser"))', "e-ccdlg"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["ShowColumnChooser proof"] }
    ])) {
      return {
        rawTrace: "[column chooser dialog proven in Playwright](playwright-proof.md)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[ShowColumnChooser proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "CurrentViewRecords") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml",
        snippets: ["CurrentViewRecords<GridOperationsModel, ResidentDirectoryGridItem>()", '"records"']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridReview.cs",
        method: "reviewing_current_view_row_index_and_selected_residents_gathers_typed_sources",
        snippets: ['ClickWhenStable(Page.Locator("#review-current-view"))', "current view has 12 residents"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["CurrentViewRecords proof"] }
    ])) {
      return reviewMethodCoverage("CurrentViewRecords");
    }
  }

  if (member.kind === "method" && member.methodName === "RowIndexByPrimaryKey") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml",
        snippets: ["RowIndexByPrimaryKey(6005)", '"rowIndex"']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridReview.cs",
        method: "reviewing_current_view_row_index_and_selected_residents_gathers_typed_sources",
        snippets: ['ClickWhenStable(Page.Locator("#review-row-index"))', "row index 5"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["RowIndexByPrimaryKey proof"] }
    ])) {
      return reviewMethodCoverage("RowIndexByPrimaryKey");
    }
  }

  if (member.kind === "method" && member.methodName === "SelectedRecords") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml",
        snippets: ["SelectedRecords<GridOperationsModel, ResidentDirectoryGridItem>()", '"selectedRecords"']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridReview.cs",
        method: "reviewing_current_view_row_index_and_selected_residents_gathers_typed_sources",
        snippets: ['ClickWhenStable(Page.Locator("#review-selected"))', '"selected records:"']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SelectedRecords proof"] }
    ])) {
      return reviewMethodCoverage("SelectedRecords");
    }
  }

  if (member.kind === "method" && member.methodName === "SelectedRowIndexes") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml",
        snippets: ["SelectedRowIndexes<GridOperationsModel>()", '"selectedRowIndexes"']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridReview.cs",
        method: "reviewing_current_view_row_index_and_selected_residents_gathers_typed_sources",
        snippets: ['ClickWhenStable(Page.Locator("#review-indexes"))', "selected row indexes: 0, 1"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SelectedRowIndexes proof"] }
    ])) {
      return reviewMethodCoverage("SelectedRowIndexes");
    }
  }

  if (member.kind === "method" && member.name.includes("SetCellValue(") && member.name.includes("Expression<Func<TRow, int>> field, int value")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/KeyedUpdate.cshtml",
        snippets: ["SetCellValue(6000, (ResidentDirectoryGridItem x) => x.OpenTasks, 99)", 'SetText("setCellValue int called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridKeyedUpdate.cs",
        method: "setting_a_cell_value_and_row_data_by_primary_key_updates_the_visible_grid",
        snippets: ['ClickWhenStable(Page.Locator("#keyed-set-tasks"))', 'Cell("99")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SetCellValue int proof"] }
    ])) {
      return keyedUpdateMethodCoverage("SetCellValue int");
    }
  }

  if (member.kind === "method" && member.name.includes("SetCellValue(") && member.name.includes("Expression<Func<TRow, string>> field, string value")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/KeyedUpdate.cshtml",
        snippets: ['SetCellValue(6001, (ResidentDirectoryGridItem x) => x.RiskLevel, "Quarantine")', 'SetText("setCellValue string called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridKeyedUpdate.cs",
        method: "setting_a_cell_value_and_row_data_by_primary_key_updates_the_visible_grid",
        snippets: ['ClickWhenStable(Page.Locator("#keyed-set-risk"))', 'Cell("Quarantine")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SetCellValue string proof"] }
    ])) {
      return keyedUpdateMethodCoverage("SetCellValue string");
    }
  }

  if (member.kind === "method" && member.name.includes("SetRowData(") && member.name.includes("int primaryKey, TRow row)")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/KeyedUpdate.cshtml",
        snippets: ["SetRowData(6002, new ResidentDirectoryGridItem", 'SetText("setRowData called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridKeyedUpdate.cs",
        method: "setting_a_cell_value_and_row_data_by_primary_key_updates_the_visible_grid",
        snippets: ['ClickWhenStable(Page.Locator("#keyed-set-row"))', 'Cell("Keyed Row")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SetRowData client proof"] }
    ])) {
      return keyedUpdateMethodCoverage("SetRowData client");
    }
  }

  if (member.kind === "method" && member.methodName === "SelectRowsByRange") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterSelection.cshtml",
        snippets: [".SelectRowsByRange(1, 3)", 'SetText("selectRowsByRange called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridSelection.cs",
        method: "selecting_a_range_clearing_and_selecting_a_single_row_updates_the_selection",
        snippets: ['ClickWhenStable(Page.Locator("#select-range"))', "ToHaveCountAsync(3"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SelectRowsByRange proof"] }
    ])) {
      return selectionMethodCoverage("SelectRowsByRange");
    }
  }

  if (member.kind === "method" && member.methodName === "ClearSelection") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterSelection.cshtml",
        snippets: [".ClearSelection()", 'SetText("clearSelection called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridSelection.cs",
        method: "selecting_a_range_clearing_and_selecting_a_single_row_updates_the_selection",
        snippets: ['ClickWhenStable(Page.Locator("#clear-selection"))', "ToHaveCountAsync(0"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["ClearSelection proof"] }
    ])) {
      return selectionMethodCoverage("ClearSelection");
    }
  }

  if (member.kind === "method" && member.methodName === "SelectRow") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterSelection.cshtml",
        snippets: [".SelectRow(0)", 'SetText("selectRow called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridSelection.cs",
        method: "selecting_a_range_clearing_and_selecting_a_single_row_updates_the_selection",
        snippets: ['ClickWhenStable(Page.Locator("#select-first"))', "ToHaveCountAsync(1"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["SelectRow proof"] }
    ])) {
      return selectionMethodCoverage("SelectRow");
    }
  }

  if (member.kind === "method" && member.name.includes("AddRecord(") && member.name.includes("TRow row, int? index")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterCrud.cshtml",
        snippets: ["AddRecord(new ResidentDirectoryGridItem", 'SetText("addRecord called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridRosterCrud.cs",
        method: "adding_updating_and_deleting_a_resident_row_changes_the_visible_roster",
        snippets: ['ClickWhenStable(Page.Locator("#crud-add"))', 'GridCell("Zara Inline")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["AddRecord proof"] }
    ])) {
      return rosterCrudMethodCoverage("AddRecord [client]");
    }
  }

  if (member.kind === "method" && member.name.includes("UpdateRow(") && member.name.includes("int rowIndex, TRow row)")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterCrud.cshtml",
        snippets: ["UpdateRow(0, new ResidentDirectoryGridItem", 'SetText("updateRow called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridRosterCrud.cs",
        method: "adding_updating_and_deleting_a_resident_row_changes_the_visible_roster",
        snippets: ['ClickWhenStable(Page.Locator("#crud-update"))', 'GridCell("Amina Updated")']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["UpdateRow proof"] }
    ])) {
      return rosterCrudMethodCoverage("UpdateRow [client]");
    }
  }

  if (member.kind === "method" && member.methodName === "DeleteSelectedRecord") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RosterCrud.cshtml",
        snippets: [".DeleteSelectedRecord()", 'SetText("deleteRecord called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridRosterCrud.cs",
        method: "adding_updating_and_deleting_a_resident_row_changes_the_visible_roster",
        snippets: ['ClickWhenStable(Page.Locator("#crud-delete"))', '"deleteRecord called"']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["DeleteSelectedRecord proof"] }
    ])) {
      return rosterCrudMethodCoverage("DeleteSelectedRecord");
    }
  }

  if (member.kind === "method" && member.methodName === "StartEdit") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/InlineEdit.cshtml",
        snippets: [".StartEdit()", 'SetText("startEdit called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridInlineEdit.cs",
        method: "starting_canceling_and_committing_an_inline_edit_toggles_the_row_editor",
        snippets: ['ClickWhenStable(Page.Locator("#inline-start-edit"))', '"startEdit called"']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["StartEdit proof"] }
    ])) {
      return inlineEditMethodCoverage("StartEdit");
    }
  }

  if (member.kind === "method" && member.methodName === "CloseEdit") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/InlineEdit.cshtml",
        snippets: [".CloseEdit()", 'SetText("closeEdit called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridInlineEdit.cs",
        method: "starting_canceling_and_committing_an_inline_edit_toggles_the_row_editor",
        snippets: ['ClickWhenStable(Page.Locator("#inline-close-edit"))', "ToHaveCountAsync(0"]
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["CloseEdit proof"] }
    ])) {
      return inlineEditMethodCoverage("CloseEdit");
    }
  }

  if (member.kind === "method" && member.methodName === "EndEdit") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/InlineEdit.cshtml",
        snippets: [".EndEdit()", 'SetText("endEdit called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridInlineEdit.cs",
        method: "starting_canceling_and_committing_an_inline_edit_toggles_the_row_editor",
        snippets: ['ClickWhenStable(Page.Locator("#inline-end-edit"))', '"endEdit called"']
      },
      { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["EndEdit proof"] }
    ])) {
      return inlineEditMethodCoverage("EndEdit");
    }
  }

  if (member.kind === "method" && member.methodName === "EditCell") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchTaskUpdate.cshtml",
        snippets: ["EditCell(0, (ResidentDirectoryGridItem x) => x.OpenTasks)", 'SetText("editCell called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchEdit.cs",
        method: "editing_updating_and_saving_a_task_cell_commits_the_new_value",
        snippets: ['ClickWhenStable(Page.Locator("#batch-edit-cell"))', ".e-gridcontent input"]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ["EditCell proof"]
      }
    ])) {
      return batchEditMethodCoverage("EditCell");
    }
  }

  if (member.kind === "method" && member.methodName === "SaveCell") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchTaskUpdate.cshtml",
        snippets: [".SaveCell()", 'SetText("saveCell called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchEdit.cs",
        method: "editing_updating_and_saving_a_task_cell_commits_the_new_value",
        snippets: ['ClickWhenStable(Page.Locator("#batch-save-cell"))', "ToHaveCountAsync(0"]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ["SaveCell proof"]
      }
    ])) {
      return batchEditMethodCoverage("SaveCell");
    }
  }

  if (member.kind === "method" && member.name.includes("UpdateCell(") && member.name.includes("Expression<Func<TRow, int>> field, int value")) {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchTaskUpdate.cshtml",
        snippets: ["UpdateCell(0, (ResidentDirectoryGridItem x) => x.OpenTasks, 6)", 'SetText("updateCell called")']
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchEdit.cs",
        method: "editing_updating_and_saving_a_task_cell_commits_the_new_value",
        snippets: ['ClickWhenStable(Page.Locator("#batch-update-cell"))', "e-updatedtd"]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ["UpdateCell proof"]
      }
    ])) {
      return batchEditMethodCoverage("UpdateCell [int]");
    }
  }

  if (member.kind === "method" && member.methodName === "HideColumn") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareStaffColumns.cshtml",
        snippets: [
          "HideColumn((ResidentDirectoryGridItem x) => x.PrimaryNurse)",
          "HideColumn((ResidentDirectoryGridItem x) => x.NextReviewDate)",
          'SetText("care columns hidden")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridColumns.cs",
        method: "hiding_care_columns_removes_their_headers",
        snippets: [
          'ClickWhenStable(Page.Locator("#hide-care-columns"))',
          '"Resident", "Risk", "Wing", "Care Level"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ["HideColumn proof", 'grid.hideColumns(field, "field")']
      }
    ])) {
      return columnMethodCoverage("HideColumn");
    }
  }

  if (member.kind === "method" && member.methodName === "ShowColumn") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareStaffColumns.cshtml",
        snippets: [
          "ShowColumn((ResidentDirectoryGridItem x) => x.PrimaryNurse)",
          "ShowColumn((ResidentDirectoryGridItem x) => x.NextReviewDate)",
          'SetText("care columns shown")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridColumns.cs",
        method: "showing_care_columns_restores_their_headers",
        snippets: [
          'ClickWhenStable(Page.Locator("#show-care-columns"))',
          '"Resident", "Risk", "Primary Nurse", "Next Review", "Wing", "Care Level"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ["ShowColumn proof", 'grid.showColumns(field, "field")']
      }
    ])) {
      return columnMethodCoverage("ShowColumn");
    }
  }

  if (member.kind === "method" && member.methodName === "ReorderColumnBefore") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareStaffColumns.cshtml",
        snippets: [
          ".ReorderColumnBefore(",
          "(ResidentDirectoryGridItem x) => x.RiskLevel,",
          "(ResidentDirectoryGridItem x) => x.ResidentName)",
          'SetText("risk moved before resident")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridColumns.cs",
        method: "reordering_moves_risk_before_resident",
        snippets: [
          'ClickWhenStable(Page.Locator("#reorder-risk-first"))',
          '"Risk", "Resident", "Primary Nurse", "Next Review", "Wing", "Care Level"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: ["ReorderColumnBefore proof", ".AllowReordering(true)"]
      }
    ])) {
      return columnMethodCoverage("ReorderColumnBefore");
    }
  }

  if (member.kind === "method" && member.methodName === "SortBy") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, FusionGridSortDirection.Descending)",
          'p.Element("method-status").SetText("sortColumn called")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "sort_by_method_sends_typed_sorted_payload_and_refreshes_grid",
        snippets: [
          'ClickWhenStable(Page.Locator("#grid-sort-risk"))',
          'root.GetProperty("sorted")',
          'sorted[0].GetProperty("name").GetString(), Is.EqualTo("riskLevel")',
          'sorted[0].GetProperty("direction").GetString(), Is.EqualTo("descending")',
          'root.TryGetProperty("where", out _), Is.False',
          'root.TryGetProperty("search", out _), Is.False',
          'root.TryGetProperty("group", out _), Is.False',
          'Page.Locator("#method-status")).ToHaveTextAsync("sortColumn called"',
          'Page.Locator("#grid-action")).ToHaveTextAsync("sorting"',
          'Page.Locator("#grid-column")).ToHaveTextAsync("riskLevel"',
          'ToContainTextAsync("Grace Bennett"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "SortBy method proof",
          "typed `SortBy((ResidentDirectoryGridItem x) => x.RiskLevel, Descending)`",
          "`sorted[0]` contains `name=riskLevel` and `direction=descending`"
        ]
      }
    ])) {
      return {
        rawTrace: "[sorting](../traces/raw-ej2-data-state-change-sorting.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[SortBy proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "ClearSorting") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".ClearSorting()",
          'p.Element("method-status").SetText("sorting cleared")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_sorting_method_clears_active_sort_and_refreshes_grid",
        snippets: [
          'ClickWhenStable(Page.Locator("#grid-sort-risk"))',
          'ClickWhenStable(Page.Locator("#grid-clear-sorting"))',
          'root.TryGetProperty("sorted", out _), Is.False',
          'Page.Locator("#method-status")).ToHaveTextAsync("sorting cleared"',
          'Page.Locator("#grid-action")).ToHaveTextAsync("sorting"',
          'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
          'ToContainTextAsync("Amina Patel"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "ClearSorting method proof",
          "typed `ClearSorting()` starts from an active `SortBy` state",
          "`sorted` is absent from the clear request body, not an empty array"
        ]
      }
    ])) {
      return {
        rawTrace: "[clear sorting](../traces/raw-ej2-data-state-change-clear-sorting-method.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[clear-sorting proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "Search") {
    return {
      rawTrace: "[searching](../traces/raw-ej2-data-state-change-searching-method.trace.json)",
      primitiveMap: "[primitive map](../mapping/primitive-map.md)",
      verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
      playwrightProof: "[searching proof](playwright-proof.md)",
      status: "row-proven"
    };
  }

  if (member.kind === "method" && member.methodName === "ClearSearch") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".ClearSearch()",
          'p.Element("method-status").SetText("search cleared")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_search_method_clears_active_search_and_refreshes_grid",
        snippets: [
          'ClickWhenStable(Page.Locator("#grid-search-memory"))',
          'ClickWhenStable(Page.Locator("#grid-clear-search"))',
          'root.TryGetProperty("search", out _), Is.False',
          'Page.Locator("#method-status")).ToHaveTextAsync("search cleared"',
          'Page.Locator("#directory-summary")).ToHaveTextAsync("240 residents matched"',
          'ToContainTextAsync("Amina Patel"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "ClearSearch method proof",
          "typed `ClearSearch()` starts from an active `Search(\"Memory\")` state",
          "`search` is absent from the clear request body, not an empty array"
        ]
      }
    ])) {
      return {
        rawTrace: "[clear search](../traces/raw-ej2-data-state-change-clear-search-method.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[clear-search proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "GroupBy") {
    return {
      rawTrace: "[grouping](../traces/raw-ej2-data-state-change-grouping-method.trace.json)",
      primitiveMap: "[primitive map](../mapping/primitive-map.md)",
      verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
      playwrightProof: "[grouping proof](playwright-proof.md)",
      status: "row-proven"
    };
  }

  if (member.kind === "method" && member.methodName === "UngroupBy") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".UngroupBy((ResidentDirectoryGridItem x) => x.CareLevel)",
          'p.Element("method-status").SetText("ungroupColumn called")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "ungrouping_method_sends_typed_action_payload_and_refreshes_grid",
        snippets: [
          'ClickWhenStable(Page.Locator("#grid-ungroup-care"))',
          'Page.Locator("#method-status")).ToHaveTextAsync("ungroupColumn called"',
          'Page.Locator("#grid-action")).ToHaveTextAsync("ungrouping"',
          'Page.Locator("#resident-directory-grid .e-groupcaption"))',
          'ToHaveCountAsync(0',
          'ToContainTextAsync("Amina Patel"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "Ungrouping method proof",
          "typed `UngroupBy` triggers `dataStateChange`",
          "group caption count is `0`"
        ]
      }
    ])) {
      return {
        rawTrace: "[ungrouping](../traces/raw-ej2-data-state-change-ungrouping-method.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[ungrouping proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.methodName === "ClearGrouping") {
    if (filesContainRequiredEvidence([
      {
        path: "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/Directory.cshtml",
        snippets: [
          ".ClearGrouping()",
          'p.Element("method-status").SetText("grouping cleared")'
        ]
      },
      {
        path: "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridDirectory.cs",
        method: "clear_grouping_method_clears_all_active_groups_and_refreshes_grid",
        snippets: [
          'ClickWhenStable(Page.Locator("#grid-group-care"))',
          'ClickWhenStable(Page.Locator("#grid-group-wing"))',
          'ClickWhenStable(Page.Locator("#grid-clear-grouping"))',
          'root.TryGetProperty("group", out _), Is.False',
          'root.TryGetProperty("sorted", out _), Is.False',
          'Page.Locator("#method-status")).ToHaveTextAsync("grouping cleared"',
          'Page.Locator("#grid-action")).ToHaveTextAsync("ungrouping"',
          'Page.Locator("#grid-column")).ToHaveTextAsync("wing"',
          'Page.Locator("#resident-directory-grid .e-groupcaption"))',
          'ToHaveCountAsync(0',
          'ToContainTextAsync("Amina Patel"'
        ]
      },
      {
        path: join(artifactRoot, "proof/playwright-proof.md"),
        snippets: [
          "ClearGrouping method proof",
          "typed `ClearGrouping()` starts from active `GroupBy` calls for care level and wing",
          "`group` is absent from the clear request body, not an empty array"
        ]
      }
    ])) {
      return {
        rawTrace: "[clear grouping](../traces/raw-ej2-data-state-change-clear-grouping-method.trace.json)",
        primitiveMap: "[primitive map](../mapping/primitive-map.md)",
        verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
        playwrightProof: "[clear-grouping proof](playwright-proof.md)",
        status: "row-proven"
      };
    }
  }

  if (member.kind === "method" && member.name === "SetDataSource [whole response body]") {
    return remoteWholeResponseCoverage();
  }

  if (member.kind === "method" && [
    "SetDataSource [typed array source]",
    "Data [component dataSource read]",
    "Refresh [component refresh method]"
  ].includes(member.name)) {
    return {
      rawTrace: "[data-source read/rebind/refresh](../traces/raw-ej2-data-source-read-refresh.trace.json)",
      primitiveMap: "[primitive map](../mapping/primitive-map.md)",
      verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
      playwrightProof: "[data-source typed-array proof](playwright-proof.md)",
      status: "row-proven"
    };
  }

  // Typed edit-template builders (column EditTemplate + DialogForm fields) are all
  // proven by the focused resident-edit-form slice rendering each editor.
  if (member.kind === "method" && isEditTemplateMethod(member.name)) {
    if (editTemplatesProven()) return editTemplatesCoverage();
    return variantPendingCoverage("edit template builder requires a focused edit-form render proof");
  }

  // FusionGridValidation From/Field: validator metadata reaches the in-cell editor.
  if (member.kind === "method" && isGridValidationMethod(member.name)) {
    if (gridValidationProven()) return gridValidationCoverage();
    return variantPendingCoverage("grid validation requires a focused in-cell validation proof");
  }

  // Server-backed roster: SetDataSource [response path] + server AddRecord/UpdateRow/SetRowData
  // (response-body overloads) + the response-path refresh lane.
  // The response-path refresh lane is a supplemental row resolved in dataSourceLaneCoverage.
  const isServerRosterRow =
    member.name === "SetDataSource [response path]" ||
    (member.kind === "method" && member.name.includes("ResponseBody<TResponse> source") &&
      (member.name.includes("AddRecord(") || member.name.includes("UpdateRow(") || member.name.includes("SetRowData(")));
  if (isServerRosterRow) {
    if (serverRosterProven()) return serverRosterCoverage();
    return variantPendingCoverage("server-backed roster requires a focused response-body CRUD proof");
  }

  // SetDataSource [event payload path]: bind a grid from an event payload array.
  if (member.name === "SetDataSource [event payload path]") {
    if (eventPayloadPathProven()) return eventPayloadPathCoverage();
    return variantPendingCoverage("event-payload data source requires a focused payload-bind proof");
  }

  // Print: grid.Print() opens the browser print view populated with the grid rows.
  if (member.kind === "method" && member.name.includes("Print(") && member.name.includes("ComponentRef<FusionGrid, TModel> self)")) {
    if (printableRosterProven()) return printableRosterCoverage();
    return variantPendingCoverage("print requires a focused print-view proof");
  }

  return pendingCoverage();
}

function pendingCoverage() {
  return {
    rawTrace: "pending",
    primitiveMap: "pending",
    verticalSlice: "pending",
    playwrightProof: "pending",
    status: "unproven"
  };
}

// P021: read-only aggregate payload rows resolve only when every trigger variant
// that emits them is proven. Index the variant rows by member and by owning class.
function buildVariantCoverageIndex(supplementalRows) {
  const byMember = new Map();
  const byClass = new Map();
  for (const row of supplementalRows) {
    if (row.kind !== "event-payload-variant" && row.kind !== "event-payload-variant-exclusion") continue;
    const sep = row.name.lastIndexOf(": ");
    if (sep < 0) continue;
    const member = row.name.slice(sep + 2).trim();
    const proven = Boolean(row.coverage && row.coverage.status === "row-proven");
    bumpVariantCoverage(byMember, member, proven);
    // A read-only payload member proven at multiple payload positions carries a
    // ` [position]` suffix (e.g. `IsComplex [where]`, `IsComplex [where.predicates]`).
    // Roll those occurrences up under the bare member name so the aggregate row
    // resolves through P021 only when every position+trigger variant is proven.
    const bareMember = member.replace(/\s*\[[^\]]*\]\s*$/, "").trim();
    if (bareMember !== member) bumpVariantCoverage(byMember, bareMember, proven);
    const cls = bareMember.includes(".") ? bareMember.slice(0, bareMember.indexOf(".")) : bareMember;
    bumpVariantCoverage(byClass, cls, proven);
  }
  return { byMember, byClass };
}

function bumpVariantCoverage(map, key, proven) {
  const entry = map.get(key) || { count: 0, proven: 0 };
  entry.count += 1;
  if (proven) entry.proven += 1;
  map.set(key, entry);
}

function allVariantsProvenForMember(variantIndex, memberName) {
  const entry = variantIndex && variantIndex.byMember.get(memberName);
  return Boolean(entry && entry.count > 0 && entry.proven === entry.count);
}

function allVariantsProvenForClass(variantIndex, className) {
  const entry = variantIndex && variantIndex.byClass.get(className);
  return Boolean(entry && entry.count > 0 && entry.proven === entry.count);
}

function variantCoveredCoverage() {
  return {
    rawTrace: "[covered-by-variant, P021](../../_skill/pattern-map.md)",
    primitiveMap: "[variant primitive rows](../mapping/primitive-map.md)",
    verticalSlice: "[per-variant slices](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[per-variant proofs](playwright-proof.md)",
    status: "row-proven"
  };
}

function cancelMutationCoverage(name) {
  return {
    rawTrace: "[beginEdit normal edit](../traces/raw-ej2-begin-edit-normal.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

// P022: beginEdit normal-mode and dialog-mode traces are byte-identical across the
// typed surface, proven equivalent by one focused slice driving both editors.
function beginEditEquivalenceProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BeginEditNormal.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBeginEditNormal.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "Mode = EditMode.Normal",
        "Mode = EditMode.Dialog",
        'p.Element("begin-edit-dialog-resident").SetText(args, x => x.RowData.ResidentName)',
        'p.Element("begin-edit-dialog-row").SetText(args, x => x.RowIndex)',
        'p.Element("begin-edit-dialog-type").SetText(args, x => x.Type)'
      ]
    },
    {
      path: test,
      method: "begin_edit_normal_reads_row_data_and_can_cancel_edit",
      snippets: ['"Amina Patel"', 'ToHaveTextAsync("0"', 'ToHaveTextAsync("edit"']
    },
    {
      path: test,
      method: "begin_edit_dialog_reads_match_the_normal_edit_variant",
      snippets: ['#grid-begin-edit-dialog_dialogEdit_wrapper', '"Amina Patel"', 'ToHaveTextAsync("edit"']
    },
    {
      path: join(artifactRoot, "proof/playwright-proof.md"),
      snippets: ["BeginEdit equivalence proof", "raw-ej2-begin-edit-dialog"]
    }
  ]);
}

function beginEditEquivalenceCoverage() {
  return {
    rawTrace: "[normal](../traces/raw-ej2-begin-edit-normal.trace.json) + [dialog](../traces/raw-ej2-begin-edit-dialog.trace.json) byte-identical typed surface",
    primitiveMap: "[event payload read](../mapping/primitive-map.md)",
    verticalSlice: "[BeginEdit equivalence slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[BeginEdit equivalence proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// P018: the writable Cancel flag is proven by the mutation changing behavior
// (locked resident editor blocked), never by read equivalence.
function beginEditCancelFlagProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BeginEditNormal.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBeginEditNormal.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: ["args.Cancel(t)", 't.Element("begin-edit-cancelled").SetText("edit cancelled")']
    },
    {
      path: test,
      method: "begin_edit_normal_reads_row_data_and_can_cancel_edit",
      snippets: ['"edit cancelled"', "e-editedrow"]
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["BeginEdit Cancel proof"] }
  ]);
}

// cellSave/cellSaved fire only in batch edit mode, so one focused batch-cell slice
// is the complete trigger context. Reads + the cellSave Cancel mutation (block 99).
function batchCellSaveProven() {
  const batchCellView = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchCellEdit.cshtml";
  const batchCellTest = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchCellEdit.cs";
  return filesContainRequiredEvidence([
    {
      path: batchCellView,
      snippets: [
        "evt.CellSave<ResidentDirectoryGridItem, int>()",
        'p.Element("batch-cell-save-column").SetText(args, x => x.ColumnName)',
        'p.Element("batch-cell-save-value").SetText(args, x => x.Value)',
        'p.Element("batch-cell-save-previous").SetText(args, x => x.PreviousValue)',
        'p.Element("batch-cell-save-resident").SetText(args, x => x.RowData.ResidentName)',
        'p.Element("batch-cell-save-cancel").SetText(args, x => x.Cancel)',
        "args.Cancel(t)",
        'SetText("blocked 99")'
      ]
    },
    {
      path: batchCellTest,
      method: "batch_cell_save_reads_typed_cell_fields_and_blocks_an_impossible_value",
      snippets: ['"openTasks"', 'ToHaveTextAsync("4"', '"Amina Patel"', 'ToHaveTextAsync("blocked 99"', 'Not.ToContainTextAsync("99"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Batch cell save proof"] }
  ]);
}

function batchCellSavedProven() {
  const batchCellView = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchCellEdit.cshtml";
  const batchCellTest = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchCellEdit.cs";
  return filesContainRequiredEvidence([
    {
      path: batchCellView,
      snippets: [
        "evt.CellSaved<ResidentDirectoryGridItem, int>()",
        'p.Element("batch-cell-saved-column").SetText(args, x => x.ColumnName)',
        'p.Element("batch-cell-saved-value").SetText(args, x => x.Value)',
        'p.Element("batch-cell-saved-previous").SetText(args, x => x.PreviousValue)',
        'p.Element("batch-cell-saved-resident").SetText(args, x => x.RowData.ResidentName)'
      ]
    },
    {
      path: batchCellTest,
      method: "batch_cell_save_reads_typed_cell_fields_and_blocks_an_impossible_value",
      snippets: ["batch-cell-saved-column", "batch-cell-saved-value", "batch-cell-saved-resident"]
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Batch cell save proof"] }
  ]);
}

// beforeBatchSave reads ChangedRecords and the Cancel mutation blocks an oversized
// batch (8). AddedRecords/DeletedRecords stay open (need a batch admit/discharge slice).
function beforeBatchSaveChangedProven() {
  const batchCellView = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchCellEdit.cshtml";
  const batchCellTest = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchCellEdit.cs";
  return filesContainRequiredEvidence([
    {
      path: batchCellView,
      snippets: [
        "evt.BeforeBatchSave<ResidentDirectoryGridItem>()",
        "x.BatchChanges.ChangedRecords[0].ResidentName",
        "x.BatchChanges.ChangedRecords[0].OpenTasks",
        "args.Cancel(t)",
        'SetText("blocked batch 8")'
      ]
    },
    {
      path: batchCellTest,
      method: "before_batch_save_reads_batch_changes_and_blocks_an_oversized_batch",
      snippets: ['"Amina Patel"', 'ToHaveTextAsync("6"', 'ToHaveTextAsync("blocked batch 8"', "waiting after cancelled batch"]
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Before batch save proof"] }
  ]);
}

function batchEventCoverage(label, traceName) {
  return {
    rawTrace: `[${label}](../traces/${traceName}.trace.json)`,
    primitiveMap: "[event payload read](../mapping/primitive-map.md)",
    verticalSlice: "[batch cell edit slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${label} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

// Batch roster change: a focused admit/discharge slice makes AddedRecords and
// DeletedRecords non-empty so beforeBatchSave can read them by indexer.
function batchRosterChangeProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchRosterChange.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchRosterChange.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "grid.DeleteSelectedRecord()",
        ".AddRecord(new ResidentDirectoryGridItem",
        "x.BatchChanges.AddedRecords[0].ResidentName",
        "x.BatchChanges.DeletedRecords[0].ResidentName"
      ]
    },
    {
      path: test,
      method: "before_batch_save_reads_added_and_deleted_records_of_a_roster_change",
      snippets: ['"Zara Admitted"', '"Amina Patel"', "roster-added-resident", "roster-deleted-resident"]
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Batch roster change proof"] }
  ]);
}

function batchRosterCoverage() {
  return {
    rawTrace: "[batch add/delete](../traces/raw-ej2-before-batch-save-add-delete.trace.json)",
    primitiveMap: "[event payload read](../mapping/primitive-map.md)",
    verticalSlice: "[batch roster change slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Batch roster change proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// FusionGridBatchChanges is fully proven only when all three list members are:
// ChangedRecords (batch task-update slice) plus AddedRecords/DeletedRecords (roster slice).
function batchChangesFullyProven() {
  return beforeBatchSaveChangedProven() && batchRosterChangeProven();
}

// One focused edit-form slice proves all eight typed edit-template builders: the
// inline grid renders the column EditTemplate editors (Select x2, DateInput) and
// the dialog grid renders DialogForm plus its Text/Number/Date/Select fields.
function editTemplatesProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ResidentEditForm.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridResidentEditForm.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "FusionGridEditTemplates.Select((ResidentDirectoryGridItem r) => r.CareLevel, careLevelOptions)",
        "FusionGridEditTemplates.Select((ResidentDirectoryGridItem r) => r.PrimaryNurse, nursePairs, n => n.Name, n => n.Code)",
        "FusionGridEditTemplates.DateInput((ResidentDirectoryGridItem r) => r.NextReviewDate)",
        "FusionGridEditTemplates.DialogForm<ResidentDirectoryGridItem>",
        ".Text(m => m.ResidentName, \"Resident\")",
        ".Number(m => m.OpenTasks, \"Open Tasks\")",
        ".Date(m => m.NextReviewDate, \"Next Review\")",
        ".Select(m => m.RiskLevel, \"Risk\", riskOptions)"
      ]
    },
    {
      path: test,
      method: "inline_cell_editors_render_typed_select_and_date_templates",
      snippets: ["select[name='careLevel']", "select[name='primaryNurse']", "input[type='date'][name='nextReviewDate']", '"Memory Care"', '"Nora Ellis"']
    },
    {
      path: test,
      method: "dialog_admission_form_renders_text_number_date_and_select_templates",
      snippets: ["input[name='residentName']", "input[type='number'][name='openTasks']", "select[name='riskLevel']", '"Moderate"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Resident edit form templates proof"] }
  ]);
}

function editTemplatesCoverage() {
  return {
    rawTrace: "[dialog edit render](../traces/raw-ej2-begin-edit-dialog.trace.json)",
    primitiveMap: "[edit template builders](../mapping/primitive-map.md)",
    verticalSlice: "[resident edit form slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Resident edit form templates proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// FusionGridValidation.From/Field are proven by the care-ops board: the openTasks
// column rule (from validator metadata) blocks an out-of-range edit in the cell.
function gridValidationProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/CareOps.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridCareOps.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "FusionGridValidation.From<ResidentCareItemValidator, ResidentCareItem>(ClientRules)",
        "ValidationRules = careValidation.Field(r => r.OpenTasks)"
      ]
    },
    {
      path: test,
      method: "an_out_of_range_open_tasks_edit_is_blocked_by_the_generated_care_rule",
      snippets: ['FillAsync("99")', "Open tasks must be between 0 and 7."]
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Grid edit validation proof"] }
  ]);
}

function gridValidationCoverage() {
  return {
    rawTrace: "[care board validation](playwright-proof.md)",
    primitiveMap: "[validator metadata to EJ2 column rule](../mapping/primitive-map.md)",
    verticalSlice: "[care ops slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Grid edit validation proof](playwright-proof.md)",
    status: "row-proven"
  };
}

function isGridValidationMethod(name) {
  return name === "Field(Expression<Func<TRow, TField>> field)" ||
    name === "From(IClientValidationRuleSource source)";
}

// The save-edit edit-action variant: focused actionComplete slice reads every typed
// member, including the variant-sensitive RowIndex/SelectedRow/PreviousData.
function editActionSaveEditProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ActionCompleteSaveEdit.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridActionCompleteSaveEdit.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "SetText(args, x => x.RequestType)",
        "SetText(args, x => x.RowIndex)",
        "SetText(args, x => x.SelectedRow)",
        "SetText(args, x => x.Data.ResidentName)",
        "SetText(args, x => x.PreviousData.ResidentName)"
      ]
    },
    {
      path: test,
      method: "action_complete_save_edit_reads_typed_current_previous_and_action_fields",
      snippets: ['ToHaveTextAsync("save"', 'ToHaveTextAsync("edit"', 'ToHaveTextAsync("-1"', '"Amina Patel"']
    }
  ]);
}

// The add and delete edit-action variants: focused admission-audit slice reads the
// variants the save-edit slice does not cover, plus the edit-action Cancel mutation.
function editActionAddDeleteProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ResidentAdmissionAudit.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridResidentAdmissionAudit.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "evt.ActionBegin<ResidentDirectoryGridItem>()",
        "evt.ActionComplete<ResidentDirectoryGridItem>()",
        ".AddRecord(new ResidentDirectoryGridItem",
        "grid.DeleteSelectedRecord()",
        "SetText(args, x => x.RequestType)",
        "SetText(args, x => x.Action)",
        "SetText(args, x => x.Data.ResidentName)",
        "args.Cancel(t)"
      ]
    },
    {
      path: test,
      method: "admitting_a_resident_reads_the_add_edit_action_payload",
      snippets: ['ToHaveTextAsync("save"', 'ToHaveTextAsync("add"', '"Zara Added"']
    },
    {
      path: test,
      method: "discharging_a_resident_reads_the_delete_edit_action_payload",
      snippets: ['ToHaveTextAsync("delete"', 'ToHaveTextAsync("false"']
    },
    {
      path: test,
      method: "blocking_an_admission_cancels_the_add_edit_action",
      snippets: ['"admission blocked"', 'Not.ToContainTextAsync("Blocked Admission"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Resident admission audit proof"] }
  ]);
}

function editActionProven() {
  return editActionSaveEditProven() && editActionAddDeleteProven();
}

function editActionCoverage() {
  return {
    rawTrace: "[save-edit](../traces/raw-ej2-action-begin-save-edit.trace.json) + [add/delete](../traces/raw-ej2-action-add-delete.trace.json)",
    primitiveMap: "[edit-action payload read](../mapping/primitive-map.md)",
    verticalSlice: "[admission audit + action-complete slices](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Resident admission audit proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// Server-backed roster: SetDataSource [response path] + server AddRecord/UpdateRow/SetRowData.
function serverRosterProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ServerRoster.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridServerRoster.cs";
  return filesContainRequiredEvidence([
    {
      path: view,
      snippets: [
        "SetDataSource(json, x => x.Result)",
        "AddRecord(json, x => x.Row, 0)",
        "UpdateRow(0, json, x => x.Row)",
        "SetRowData(6005, json, x => x.Row)"
      ]
    },
    {
      // The [response path] load is asserted in the shared NavigateRoster helper.
      path: test,
      snippets: ['"loaded via response path"', "#server-keyed-grid"]
    },
    {
      path: test,
      method: "admitting_a_server_resident_reads_the_row_from_the_response",
      snippets: ['"Sofia Server"']
    },
    {
      path: test,
      method: "updating_row_zero_reads_the_row_from_the_response",
      snippets: ['"Amina Server Updated"']
    },
    {
      path: test,
      method: "patching_a_keyed_resident_reads_the_row_from_the_response",
      snippets: ['"Lena Server Patch"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Server-backed roster proof"] }
  ]);
}

function serverRosterCoverage() {
  return {
    rawTrace: "[server roster {result,count}](../discovery/public-api-surface.json)",
    primitiveMap: "[response-body read into data source](../mapping/primitive-map.md)",
    verticalSlice: "[server roster slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Server-backed roster proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// Initial builder-owned data source: the grid binds b.DataSource(roster) at render.
function builderRosterProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BuilderRoster.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBuilderRoster.cs";
  return filesContainRequiredEvidence([
    { path: view, snippets: [".DataSource(roster)"] },
    {
      path: test,
      method: "builder_owned_data_source_renders_the_roster_without_a_fetch",
      snippets: ['"Amina Patel"', '"Grace Bennett"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Builder-owned roster proof"] }
  ]);
}

function builderRosterCoverage() {
  return {
    rawTrace: "[builder-owned data source](playwright-proof.md)",
    primitiveMap: "[render-time builder data source](../mapping/primitive-map.md)",
    verticalSlice: "[builder roster slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Builder-owned roster proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// DataManager + UrlAdaptor remote binding: the grid fetches {result, count} itself on load.
function remoteAdaptorProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/RemoteAdaptorRoster.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridRemoteAdaptorRoster.cs";
  return filesContainRequiredEvidence([
    { path: view, snippets: ["new DataManager", 'Adaptor = "UrlAdaptor"', ".DataSource(remote)"] },
    {
      path: test,
      method: "data_manager_adaptor_fetches_the_remote_roster_on_load",
      snippets: ['"Memory Care"', ".e-pager"]
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Remote DataManager adaptor proof"] }
  ]);
}

function remoteAdaptorCoverage() {
  return {
    rawTrace: "[remote DataManager adaptor](playwright-proof.md)",
    primitiveMap: "[builder remote data manager](../mapping/primitive-map.md)",
    verticalSlice: "[remote adaptor roster slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Remote DataManager adaptor proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// Print: grid.Print() opens the browser print view populated with the grid rows.
function printableRosterProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/PrintableRoster.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridPrintableRoster.cs";
  return filesContainRequiredEvidence([
    { path: view, snippets: [".Print()", 'SetText("print issued")'] },
    {
      path: test,
      method: "printing_the_roster_opens_the_print_view_with_the_rows",
      snippets: ["RunAndWaitForPopupAsync", '"Amina Patel"', '"print issued"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Printable roster proof"] }
  ]);
}

function printableRosterCoverage() {
  return {
    rawTrace: "[print view popup](playwright-proof.md)",
    primitiveMap: "[grid print method](../mapping/primitive-map.md)",
    verticalSlice: "[printable roster slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Printable roster proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// Event-payload data source: SetDataSource(args, x => x.BatchChanges.ChangedRecords) binds a
// grid from an event payload array (the [event payload path] overload + payload-driven refresh).
function eventPayloadPathProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/BatchChangeReview.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridBatchChangeReview.cs";
  return filesContainRequiredEvidence([
    { path: view, snippets: ["SetDataSource(args, x => x.BatchChanges.ChangedRecords)"] },
    {
      path: test,
      method: "committing_a_batch_binds_the_review_grid_from_the_event_payload",
      snippets: ['"review bound from event"', '"Amina Patel"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["Event-payload data source proof"] }
  ]);
}

function eventPayloadPathCoverage() {
  return {
    rawTrace: "[event payload data source](playwright-proof.md)",
    primitiveMap: "[event payload read into data source](../mapping/primitive-map.md)",
    verticalSlice: "[batch change review slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: "[Event-payload data source proof](playwright-proof.md)",
    status: "row-proven"
  };
}

// Nested data-source property path: SetDataSource(json, x => x.Page.Result) reads a nested array.
function nestedPathProven() {
  const view = "Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Components/Fusion/Grid/ServerRoster.cshtml";
  const test = "tests/Alis.Reactive.PlaywrightTests/Components/Fusion/Grid/WhenUsingFusionGridServerRoster.cs";
  return filesContainRequiredEvidence([
    { path: view, snippets: ["SetDataSource(json, x => x.Page.Result)"] },
    {
      path: test,
      method: "loading_a_nested_page_binds_from_the_nested_data_source_path",
      snippets: ['"loaded nested path"', '"Amina Patel"', '"Henry Liu"']
    },
    { path: join(artifactRoot, "proof/playwright-proof.md"), snippets: ["nested data-source property path"] }
  ]);
}

function isEditTemplateMethod(name) {
  return new Set([
    "Date(Expression<Func<TRow, TField>> field, string label)",
    "DateInput(Expression<Func<TRow, TField>> field)",
    "DialogForm(Action<FusionGridDialogFormBuilder<TRow>> build)",
    "Number(Expression<Func<TRow, TField>> field, string label)",
    "Select(Expression<Func<TRow, TField>> field, IEnumerable<string> options)",
    "Select(Expression<Func<TRow, TField>> field, IEnumerable<TItem> items, Func<TItem, string> text, Func<TItem, string> value)",
    "Select(Expression<Func<TRow, TField>> field, string label, IEnumerable<string> options)",
    "Text(Expression<Func<TRow, TField>> field, string label)"
  ]).has(name);
}

function batchRiskMethodCoverage(name) {
  return {
    rawTrace: "[cellSave batch edit](../traces/raw-ej2-cell-save-batch-edit.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function autoFitMethodCoverage(name) {
  return {
    rawTrace: "[autofit width proven in Playwright](playwright-proof.md)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[autofit overload fix](../discovery/runtime-row-column-autofit.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function exportMethodCoverage(name) {
  return {
    rawTrace: "[export download proven in Playwright](playwright-proof.md)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function reviewMethodCoverage(name) {
  return {
    rawTrace: "[rowSelected click](../traces/raw-ej2-row-selected-click.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function keyedUpdateMethodCoverage(name) {
  return {
    rawTrace: "[beginEdit normal edit](../traces/raw-ej2-begin-edit-normal.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function selectionMethodCoverage(name) {
  return {
    rawTrace: "[rowSelected click](../traces/raw-ej2-row-selected-click.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function rosterCrudMethodCoverage(name) {
  return {
    rawTrace: "[beginEdit normal edit](../traces/raw-ej2-begin-edit-normal.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function inlineEditMethodCoverage(name) {
  return {
    rawTrace: "[beginEdit normal edit](../traces/raw-ej2-begin-edit-normal.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function batchEditMethodCoverage(name) {
  return {
    rawTrace: "[cellSave batch edit](../traces/raw-ej2-cell-save-batch-edit.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function columnMethodCoverage(name) {
  return {
    rawTrace: "[column visibility](../traces/raw-ej2-column-visibility.trace.json)",
    primitiveMap: "[primitive map](../mapping/primitive-map.md)",
    verticalSlice: "[vertical slice](../mapping/vertical-slice-plan.md)",
    playwrightProof: `[${name} proof](playwright-proof.md)`,
    status: "row-proven"
  };
}

function variantPendingCoverage(note) {
  return {
    rawTrace: note,
    primitiveMap: "pending variant matrix",
    verticalSlice: "pending variant matrix",
    playwrightProof: "pending variant matrix",
    status: "unproven"
  };
}

function isPayloadProperty(member, accepted) {
  if (member.kind !== "event-payload-property") return false;
  return accepted[member.owner]?.includes(member.propertyName) === true;
}

function methodDisplayName(name, parameters) {
  const lane = methodLane(name, parameters);
  return lane ? `${name} [${lane}]` : `${name}(${compactParameters(parameters)})`;
}

function methodLane(name, parameters) {
  if (name === "SetDataSource" && parameters.includes("ResponseBody<TResponse> source") && parameters.includes("Expression<Func<TResponse, TValue>> path")) {
    return "response path";
  }
  if (name === "SetDataSource" && parameters.includes("ResponseBody<TResponse> source")) {
    return "whole response body";
  }
  if (name === "SetDataSource" && parameters.includes("TSource source") && parameters.includes("Expression<Func<TSource, TValue>> path")) {
    return "event payload path";
  }
  if (name === "SetDataSource" && parameters.includes("TypedSource<TElement[]> source")) {
    return "typed array source";
  }
  if (name === "Data") return "component dataSource read";
  if (name === "Refresh") return "component refresh method";
  return "";
}

function compactParameters(parameters) {
  return parameters
    .split(",")
    .map(parameter => parameter.trim().replace(/\s+/g, " "))
    .filter(Boolean)
    .join(", ");
}

function extensionReceiverOwner(parameters) {
  const match = parameters.match(/\bthis\s+([A-Z][A-Za-z0-9_]*)/);
  return match?.[1] ?? "";
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

function readBalancedBody(text, open) {
  let depth = 0;
  for (let i = open; i < text.length; i++) {
    const char = text[i];
    if (char === "{") depth++;
    if (char === "}") depth--;
    if (depth === 0) return text.slice(open + 1, i);
  }
  return "";
}

function stripComments(text) {
  return text
    .replace(/\/\*[\s\S]*?\*\//g, "")
    .replace(/\/\/.*$/gm, "");
}

function compact(value) {
  return value.replace(/\s+/g, " ").trim();
}

function stripBackticks(value) {
  return value.replace(/`/g, "").trim();
}

function relativePath(path) {
  return path.startsWith(process.cwd()) ? path.slice(process.cwd().length + 1) : path;
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

function fail(message) {
  console.error(message);
  process.exit(1);
}

function pascal(value) {
  return value
    .split(/[-_\s]+/g)
    .filter(Boolean)
    .map(part => part.slice(0, 1).toUpperCase() + part.slice(1))
    .join("");
}
