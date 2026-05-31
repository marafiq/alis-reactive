#!/usr/bin/env node

import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { homedir } from "node:os";
import { join } from "node:path";

const args = parseArgs(process.argv.slice(2));
const packageName = requireArg(args, "package");
const version = requireArg(args, "version");
const component = requireArg(args, "component");
const decompiledPath = args.decompiled;
const explicitPackageRoot = args["package-root"];

const packageRoot = explicitPackageRoot ||
  join(homedir(), ".nuget", "packages", packageName.toLowerCase(), version);
if (!existsSync(packageRoot)) {
  console.error(`Package not found in NuGet cache: ${packageRoot}`);
  console.error(`Install it with: dotnet add package ${packageName} --version ${version}`);
  console.error("Or pass --package-root /path/to/extracted/package");
  process.exit(1);
}

const xmlPath = findFirst(packageRoot, file => file.endsWith(".xml") && file.includes(packageName));
const dllPath = findFirst(packageRoot, file => file.endsWith(".dll") && file.includes(packageName));
if (!xmlPath) {
  console.error(`Could not find XML docs under ${packageRoot}`);
  process.exit(1);
}

const xml = readFileSync(xmlPath, "utf8");
const sfType = `Sf${component}`;
const methodRows = extractSfMethods(xml, packageName, sfType);
const eventArgRows = extractEventArgs(xml, packageName);
const bridgeRows = decompiledPath && existsSync(decompiledPath)
  ? extractBridgeCalls(readFileSync(decompiledPath, "utf8"))
  : [];

printMarkdown({
  packageName,
  version,
  component,
  packageRoot,
  xmlPath,
  dllPath,
  sfType,
  methodRows,
  eventArgRows,
  bridgeRows,
  decompiledPath
});

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
  if (typeof value === "string" && value.trim()) return value.trim();
  console.error(`Missing --${name}`);
  process.exit(1);
}

function findFirst(root, predicate) {
  const stack = [root];
  while (stack.length > 0) {
    const current = stack.pop();
    for (const name of readdirSync(current)) {
      const full = join(current, name);
      const stats = statSync(full);
      if (stats.isDirectory()) {
        stack.push(full);
      } else if (predicate(full)) {
        return full;
      }
    }
  }
  return "";
}

function extractSfMethods(xmlText, pkg, sfType) {
  const rows = [];
  const expression = new RegExp(`name="M:${escapeRegExp(pkg)}\\.[^"]*${escapeRegExp(sfType)}(?:\`\\d+)?\\.([^"(]+)(?:\\(([^"]*)\\))?"`, "g");
  let match;
  while ((match = expression.exec(xmlText)) !== null) {
    rows.push({
      name: match[1],
      signature: match[2] || ""
    });
  }
  return uniqueRows(rows, row => `${row.name}(${row.signature})`).sort(byName);
}

function extractEventArgs(xmlText, pkg) {
  const groups = new Map();
  const expression = new RegExp(`name="P:${escapeRegExp(pkg)}\\.([^"]*EventArgs(?:\`\\d+)?)\\.([^"]+)"`, "g");
  let match;
  while ((match = expression.exec(xmlText)) !== null) {
    const type = match[1].replace(/`1$/, "<T>");
    const property = match[2];
    const values = groups.get(type) || [];
    values.push(property);
    groups.set(type, values);
  }

  return Array.from(groups.entries())
    .map(([type, properties]) => ({
      type,
      properties: Array.from(new Set(properties)).sort()
    }))
    .sort((left, right) => left.type.localeCompare(right.type));
}

function extractBridgeCalls(source) {
  const calls = new Set();
  const expression = /sfBlazor\.[A-Za-z0-9_.]+/g;
  let match;
  while ((match = expression.exec(source)) !== null) {
    calls.add(match[0]);
  }
  return Array.from(calls).sort();
}

function uniqueRows(rows, keySelector) {
  const seen = new Set();
  return rows.filter(row => {
    const key = keySelector(row);
    if (seen.has(key)) return false;
    seen.add(key);
    return true;
  });
}

function byName(left, right) {
  return left.name.localeCompare(right.name);
}

function printMarkdown(report) {
  console.log(`# Syncfusion Blazor ${report.component} Metadata`);
  console.log("");
  console.log(`Package: \`${report.packageName}\` ${report.version}`);
  console.log(`Package root: \`${report.packageRoot}\``);
  console.log(`XML: \`${report.xmlPath}\``);
  console.log(`DLL: \`${report.dllPath || "not found"}\``);
  if (report.decompiledPath) console.log(`Decompiled source: \`${report.decompiledPath}\``);
  console.log("");
  console.log("## Public Sf Methods");
  console.log("");
  console.log("| Method | Parameters | Alis Decision |");
  console.log("|---|---|---|");
  for (const row of report.methodRows) {
    console.log(`| \`${row.name}\` | \`${row.signature || "-"}\` | candidate only; verify direct EJ2 source + HTML trace |`);
  }
  console.log("");
  console.log("## Event Args");
  console.log("");
  console.log("| Type | Properties | Alis Decision |");
  console.log("|---|---|---|");
  for (const row of report.eventArgRows) {
    console.log(`| \`${row.type}\` | \`${row.properties.join(", ")}\` | candidate only; expose direct EJ2 overlap |`);
  }
  console.log("");
  console.log("## Blazor JS Bridge Calls");
  console.log("");
  if (report.bridgeRows.length === 0) {
    console.log("No decompiled bridge calls supplied or found.");
  } else {
    for (const call of report.bridgeRows) {
      console.log(`- \`${call}\` - bridge clue, not direct Fusion API by itself`);
    }
  }
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
