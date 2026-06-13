#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const repoRoot = resolve(args.root ?? ".");
const write = args.write === true || args.write === "true";
const fusionRoot = join(repoRoot, "Alis.Reactive.Fusion/Components");
const syncfusionRoot = resolve(args["syncfusion-root"] ?? "node_modules/@syncfusion");
const xmlPath = resolve(args.xml ?? latestSyncfusionXml());
const outputJson = join(repoRoot, "tools/FusionOnboarding/wwwroot/onboarding/fusion/_inventory/discovery-inputs.json");
const outputMarkdown = join(repoRoot, "tools/FusionOnboarding/wwwroot/onboarding/fusion/_inventory/discovery-inputs.md");

if (!existsSync(fusionRoot)) fail(`Fusion component root not found: ${fusionRoot}`);
if (!existsSync(syncfusionRoot)) fail(`Syncfusion root not found: ${syncfusionRoot}`);
if (!existsSync(xmlPath)) fail(`Syncfusion MVC XML not found: ${xmlPath}`);

const declarations = findSyncfusionClassDeclarations(syncfusionRoot);
const xml = readFileSync(xmlPath, "utf8");
const components = readdirSync(fusionRoot)
  .filter(name => name.startsWith("Fusion"))
  .filter(name => statSync(join(fusionRoot, name)).isDirectory())
  .sort()
  .map(componentType => discoveryInput(componentType, declarations, xml));
const summary = summarize(components);
const report = {
  status: "fusion-discovery-inputs",
  generatedBy: ".claude/skills/onboard-fusion-component/scripts/report-fusion-discovery-inputs.mjs",
  syncfusionRoot: toRepoPath(syncfusionRoot),
  xmlPath: toRepoPath(xmlPath),
  summary,
  components
};

if (write) {
  writeFile(outputJson, `${JSON.stringify(report, null, 2)}\n`);
  writeFile(outputMarkdown, markdown(report));
}

print(report, write);

function discoveryInput(componentType, declarations, xml) {
  const componentName = componentType.replace(/^Fusion/, "");
  const artifactName = kebab(componentName);
  const candidateNames = classNameCandidates(componentName);
  const matches = declarations
    .filter(item => candidateNames.includes(item.className))
    .map(item => withMvcEvidence(item, xml));
  const exactMatches = matches.filter(item => item.className === componentName);
  const preferred = choosePreferred(exactMatches.length > 0 ? exactMatches : matches);
  const status = inputStatus(matches, preferred);

  return {
    componentType,
    componentName,
    artifactName,
    candidateNames,
    status,
    matches,
    preferred,
    command: preferred && status === "ready"
      ? discoveryCommand(artifactName, preferred)
      : ""
  };
}

function classNameCandidates(componentName) {
  const values = new Set([componentName]);
  const aliases = {
    AIAssistView: ["AIAssistView"],
    AutoComplete: ["AutoComplete"],
    BulletChart: ["BulletChart"],
    CheckBox: ["CheckBox"],
    ChipList: ["ChipList"],
    ColorPicker: ["ColorPicker"],
    ComboBox: ["ComboBox"],
    DatePicker: ["DatePicker"],
    DateRangePicker: ["DateRangePicker"],
    DateTimePicker: ["DateTimePicker"],
    DropDownButton: ["DropDownButton"],
    DropDownList: ["DropDownList"],
    DropDownTree: ["DropDownTree"],
    FileUpload: ["Uploader"],
    InPlaceEditor: ["InPlaceEditor"],
    InputMask: ["MaskedTextBox"],
    MultiColumnComboBox: ["MultiColumnComboBox"],
    MultiSelect: ["MultiSelect"],
    NumericTextBox: ["NumericTextBox"],
    OtpInput: ["OtpInput"],
    PivotView: ["PivotView"],
    SmartTextArea: ["SmartTextArea"],
    SmartPasteButton: ["SmartPasteButton"],
    TextArea: ["TextArea"],
    TextBox: ["TextBox"],
    TimePicker: ["TimePicker"]
  };
  for (const alias of aliases[componentName] ?? []) values.add(alias);
  return [...values];
}

function inputStatus(matches, preferred) {
  if (matches.length === 0) return "no-class-match";
  if (!preferred) return "ambiguous";
  if (!preferred.mvcBuilder) return "missing-mvc-builder";
  if (!preferred.namespace) return "missing-namespace";
  if (!preferred.dtsPath) return "missing-dts";
  return "ready";
}

function choosePreferred(matches) {
  if (matches.length === 0) return null;
  const withBuilder = matches.filter(item => item.mvcBuilder);
  const candidates = withBuilder.length > 0 ? withBuilder : matches;
  if (candidates.length === 1) return candidates[0];
  const componentExtends = candidates.filter(item => item.extendsComponent);
  if (componentExtends.length === 1) return componentExtends[0];
  return null;
}

function withMvcEvidence(item, xml) {
  const builder = findMvcBuilder(xml, item.className);
  return {
    ...item,
    mvcBuilder: builder?.builder ?? "",
    mvcNamespace: builder?.namespace ?? "",
    namespace: builder?.namespace ? lowerFirst(builder.namespace.split(".").pop() ?? "") : "",
    xmlPath: toRepoPath(xmlPath)
  };
}

function findMvcBuilder(xml, className) {
  const expression = new RegExp(`name="T:(Syncfusion\\.EJ2\\.([A-Za-z0-9_.]+)\\.${escapeRegExp(className)}Builder)"`);
  const match = expression.exec(xml);
  if (!match) return null;
  return {
    builder: `${match[1]}`,
    namespace: `ej.${lowerFirst(match[2].split(".")[0] ?? "")}`
  };
}

function findSyncfusionClassDeclarations(root) {
  const output = [];
  for (const file of walk(root).filter(path => path.endsWith(".d.ts"))) {
    const text = readFileSync(file, "utf8");
    const expression = /export\s+declare\s+class\s+([A-Z][A-Za-z0-9_]*)\b([^{]*)\{/g;
    let match;
    while ((match = expression.exec(text)) !== null) {
      const [, className, tail] = match;
      output.push({
        className,
        packageName: packageNameFor(file),
        dtsPath: toRepoPath(file),
        jsPath: jsPathFor(file),
        extendsComponent: /\bextends\s+Component\b/.test(tail)
      });
    }
  }
  return output.sort((left, right) => left.className.localeCompare(right.className) || left.dtsPath.localeCompare(right.dtsPath));
}

function jsPathFor(dtsPath) {
  const js = dtsPath.replace(/\.d\.ts$/, ".js");
  return existsSync(js) ? toRepoPath(js) : "";
}

function packageNameFor(path) {
  const normalized = path.replace(/\\/g, "/");
  const marker = "/node_modules/@syncfusion/";
  const index = normalized.indexOf(marker);
  if (index < 0) return "";
  return normalized.slice(index + marker.length).split("/")[0] ?? "";
}

function discoveryCommand(component, input) {
  const parts = [
    "node .claude/skills/onboard-fusion-component/scripts/write-fusion-discovery-artifacts.mjs",
    `--component ${component}`,
    `--class ${input.className}`,
    `--namespace ${input.namespace.replace(/^ej\./, "")}`,
    `--dts ${input.dtsPath}`,
    `--xml ${input.xmlPath}`,
    "--write"
  ];
  if (input.jsPath) parts.splice(5, 0, `--js ${input.jsPath}`);
  return parts.join(" ");
}

function summarize(items) {
  const byStatus = {};
  for (const item of items) byStatus[item.status] = (byStatus[item.status] ?? 0) + 1;
  return {
    componentCount: items.length,
    ready: items.filter(item => item.status === "ready").length,
    blocked: items.filter(item => item.status !== "ready").length,
    byStatus
  };
}

function markdown(report) {
  return `# Fusion Discovery Inputs

Generated by:

\`\`\`bash
node .claude/skills/onboard-fusion-component/scripts/report-fusion-discovery-inputs.mjs --write
\`\`\`

Status: mechanical candidate discovery. Ready rows are command-ready inputs for
static discovery; they are not proof that a component is onboarded.

| Metric | Count |
|---|---:|
| Components | ${report.summary.componentCount} |
| Ready discovery inputs | ${report.summary.ready} |
| Blocked discovery inputs | ${report.summary.blocked} |

## Status Counts

| Status | Components |
|---|---:|
${Object.entries(report.summary.byStatus).sort(([left], [right]) => left.localeCompare(right)).map(([status, count]) => `| ${status} | ${count} |`).join("\n")}

## Components

| Component | Artifact | Status | Class | Namespace | Package | d.ts | MVC Builder |
|---|---|---|---|---|---|---|---|
${report.components.map(item => {
  const input = item.preferred ?? {};
  return `| ${item.componentType} | \`${item.artifactName}/\` | ${item.status} | ${input.className ?? ""} | ${input.namespace ?? ""} | ${input.packageName ?? ""} | ${input.dtsPath ? `\`${input.dtsPath}\`` : ""} | ${input.mvcBuilder ? `\`${input.mvcBuilder}\`` : ""} |`;
}).join("\n")}
`;
}

function print(report, wrote) {
  console.log("# Fusion Discovery Inputs");
  console.log("");
  console.log(`Components: ${report.summary.componentCount}`);
  console.log(`Ready discovery inputs: ${report.summary.ready}`);
  console.log(`Blocked discovery inputs: ${report.summary.blocked}`);
  console.log(`Artifacts ${wrote ? "written" : "not written"}: ${toRepoPath(outputMarkdown)}`);
  console.log("");
  console.log("| Status | Components |");
  console.log("|---|---:|");
  for (const [status, count] of Object.entries(report.summary.byStatus).sort(([left], [right]) => left.localeCompare(right))) {
    console.log(`| ${status} | ${count} |`);
  }
}

function latestSyncfusionXml() {
  const packageRoot = join(process.env.HOME ?? "", ".nuget/packages/syncfusion.ej2.aspnet.core");
  if (!existsSync(packageRoot)) return "";
  const versions = readdirSync(packageRoot)
    .filter(name => existsSync(join(packageRoot, name, "lib/net10.0/Syncfusion.EJ2.xml")))
    .sort(compareVersions);
  const version = versions[versions.length - 1] ?? "";
  return version ? join(packageRoot, version, "lib/net10.0/Syncfusion.EJ2.xml") : "";
}

function compareVersions(left, right) {
  const l = left.split(".").map(Number);
  const r = right.split(".").map(Number);
  for (let i = 0; i < Math.max(l.length, r.length); i++) {
    const diff = (l[i] ?? 0) - (r[i] ?? 0);
    if (diff !== 0) return diff;
  }
  return left.localeCompare(right);
}

function walk(folder) {
  const output = [];
  for (const entry of readdirSync(folder)) {
    const path = join(folder, entry);
    const stats = statSync(path);
    if (stats.isDirectory()) output.push(...walk(path));
    else output.push(path);
  }
  return output;
}

function writeFile(file, content) {
  mkdirSync(dirname(file), { recursive: true });
  writeFileSync(file, content, "utf8");
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

function lowerFirst(value) {
  return value ? value.slice(0, 1).toLowerCase() + value.slice(1) : "";
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function fail(message) {
  console.error(message);
  process.exit(1);
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
