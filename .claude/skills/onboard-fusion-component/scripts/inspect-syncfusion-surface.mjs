#!/usr/bin/env node

import { readFileSync } from "node:fs";

const args = parseArgs(process.argv.slice(2));
const className = requireArg(args, "class");
const dtsPath = requireArg(args, "dts");
const xmlPath = args.xml;
const builderName = args.builder ?? `${className}Builder`;

const classBody = extractClassBody(readFileSync(dtsPath, "utf8"), className);
const builderMembers = xmlPath
  ? extractBuilderMembers(readFileSync(xmlPath, "utf8"), builderName)
  : new Set();

const rows = extractMembers(classBody)
  .filter(member => !ignoredMember(member))
  .map(member => toRow(member, builderMembers));

printMarkdown(className, builderName, rows);

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

function extractClassBody(text, className) {
  const classStart = text.indexOf(`class ${className} `);
  if (classStart < 0) {
    throw new Error(`Could not find class ${className}`);
  }

  const open = text.indexOf("{", classStart);
  if (open < 0) {
    throw new Error(`Could not find class body for ${className}`);
  }

  let depth = 0;
  for (let i = open; i < text.length; i++) {
    const char = text[i];
    if (char === "{") depth++;
    if (char === "}") depth--;
    if (depth === 0) return text.slice(open + 1, i);
  }

  throw new Error(`Unclosed class body for ${className}`);
}

function extractBuilderMembers(xml, builderName) {
  const members = new Set();
  const expression = new RegExp(`M:Syncfusion\\.EJ2\\.[^"]+\\.${escapeRegExp(builderName)}\\.([A-Za-z_][A-Za-z0-9_]*)\\(`, "g");
  let match;
  while ((match = expression.exec(xml)) !== null) {
    members.add(match[1]);
  }
  return members;
}

function extractMembers(body) {
  const members = [];
  for (const rawLine of body.split(/\r?\n/)) {
    const line = rawLine.trim();
    if (!line || line.startsWith("*") || line.startsWith("/")) continue;
    if (line.startsWith("private ") || line.startsWith("protected ")) continue;

    const property = line.match(/^([A-Za-z_][A-Za-z0-9_]*)\??:\s*([^;]+);$/);
    if (property) {
      const [, name, type] = property;
      members.push({
        name,
        kind: type.startsWith("EmitType<") ? "event" : "property",
        type: type.replace(/^EmitType<(.+)>$/, "$1")
      });
      continue;
    }

    const method = line.match(/^([A-Za-z_][A-Za-z0-9_]*)\(([^)]*)\):\s*([^;]+);$/);
    if (method) {
      const [, name, parameters, returns] = method;
      members.push({ name, kind: "method", type: `${parameters}) => ${returns}` });
    }
  }
  return members;
}

function ignoredMember(member) {
  return [
    "constructor",
    "preRender",
    "getDirective",
    "getModuleName",
    "getPersistData",
    "render",
    "onPropertyChanged"
  ].includes(member.name);
}

function toRow(member, builderMembers) {
  const builderMember = pascal(member.name);
  const covered = builderMembers.has(builderMember);
  return {
    ...member,
    builder: covered ? builderMember : "",
    decision: decisionFor(member, covered)
  };
}

function decisionFor(member, builderCovered) {
  if (member.kind === "method") {
    if (member.name === "destroy") return "skip: lifecycle cleanup, not plan behavior";
    return "candidate: post-render behavior";
  }

  if (member.kind === "event") {
    return "candidate: typed event if plan needs payload/mutation";
  }

  return builderCovered
    ? "skip unless runtime read/write is needed"
    : "candidate: runtime state if proven";
}

function printMarkdown(className, builderName, rows) {
  console.log(`# ${className} Surface`);
  console.log("");
  console.log(`Builder coverage: ${builderName}`);
  console.log("");
  console.log("| JS member | Kind | Type | Builder member | Decision |");
  console.log("|---|---|---|---|---|");
  for (const row of rows) {
    console.log(`| \`${row.name}\` | ${row.kind} | \`${row.type}\` | ${row.builder || "-"} | ${row.decision} |`);
  }
}

function pascal(value) {
  return value.slice(0, 1).toUpperCase() + value.slice(1);
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
