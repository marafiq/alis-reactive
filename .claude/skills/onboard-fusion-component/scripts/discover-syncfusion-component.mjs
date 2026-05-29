#!/usr/bin/env node

import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { join, relative } from "node:path";

const args = parseArgs(process.argv.slice(2));
const classQuery = requireArg(args, "class");
const root = args.root ?? "node_modules/@syncfusion";
const xmlPath = args.xml ?? `${process.env.HOME}/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/netstandard2.0/Syncfusion.EJ2.xml`;

if (!existsSync(root)) {
  console.error(`Syncfusion node_modules root not found: ${root}`);
  process.exit(1);
}

const xml = existsSync(xmlPath) ? readFileSync(xmlPath, "utf8") : "";
const matches = [];

for (const dtsPath of walk(root).filter(file => file.endsWith(".d.ts"))) {
  const text = readFileSync(dtsPath, "utf8");
  const classes = extractClasses(text);
  for (const className of classes) {
    if (!className.toLowerCase().includes(classQuery.toLowerCase())) continue;
    matches.push(describeMatch(className, dtsPath, root, xml, xmlPath));
  }
}

if (matches.length === 0) {
  console.error(`No Syncfusion d.ts class found for query: ${classQuery}`);
  process.exit(1);
}

print(matches);

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

function extractClasses(text) {
  const names = [];
  const expression = /(?:export\s+declare\s+|export\s+)?class\s+([A-Za-z_][A-Za-z0-9_]*)\b/g;
  let match;
  while ((match = expression.exec(text)) !== null) {
    names.push(match[1]);
  }
  return names;
}

function describeMatch(className, dtsPath, root, xml, xmlPath) {
  const relativePath = relative(root, dtsPath);
  const packageName = relativePath.split(/[\\/]/)[0];
  const jsPath = dtsPath.replace(/\.d\.ts$/, ".js");
  const builder = findBuilder(className, xml);
  const builderNamespace = builder?.namespace ?? "";
  const jsNamespace = builderNamespace ? builderNamespace.toLowerCase() : packageToNamespace(packageName);
  return {
    className,
    packageName: `@syncfusion/${packageName}`,
    dtsPath,
    jsPath: existsSync(jsPath) ? jsPath : "",
    builderType: builder?.type ?? "",
    builderName: `${className}Builder`,
    jsNamespace,
    component: kebab(className),
    xmlPath
  };
}

function findBuilder(className, xml) {
  if (!xml) return null;
  const expression = new RegExp(`T:Syncfusion\\.EJ2\\.([A-Za-z0-9_.]+)\\.${escapeRegExp(className)}Builder`, "g");
  const match = expression.exec(xml);
  if (!match) return null;
  return {
    namespace: match[1],
    type: `Syncfusion.EJ2.${match[1]}.${className}Builder`
  };
}

function packageToNamespace(packageName) {
  return packageName.replace(/^ej2-/, "").replace(/-/g, "");
}

function kebab(value) {
  return value
    .replace(/([A-Z]+)([A-Z][a-z])/g, "$1-$2")
    .replace(/([a-z0-9])([A-Z])/g, "$1-$2")
    .toLowerCase();
}

function print(matches) {
  console.log(`# Syncfusion Component Discovery`);
  console.log("");
  console.log("| Class | Package | d.ts | JS source | MVC builder | JS global guess |");
  console.log("|---|---|---|---|---|---|");
  for (const item of matches) {
    console.log(`| ${item.className} | ${item.packageName} | \`${item.dtsPath}\` | ${item.jsPath ? `\`${item.jsPath}\`` : "-"} | ${item.builderType || "-"} | \`ej.${item.jsNamespace}.${item.className}\` |`);
  }
  console.log("");
  console.log("## Next Commands");
  for (const item of matches) {
    console.log("");
    console.log(`### ${item.className}`);
    console.log("```bash");
    console.log(`node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-surface.mjs \\`);
    console.log(`  --class ${item.className} \\`);
    console.log(`  --dts ${item.dtsPath} \\`);
    console.log(`  --xml ${item.xmlPath}`);
    console.log("");
    console.log(`node .claude/skills/onboard-fusion-component/scripts/create-fusion-probe.mjs \\`);
    console.log(`  --component ${item.component} \\`);
    console.log(`  --namespace ${item.jsNamespace} \\`);
    console.log(`  --class ${item.className} \\`);
    console.log(`  --id ${item.component}`);
    console.log("```");
  }
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
