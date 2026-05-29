#!/usr/bin/env node

import { existsSync, readFileSync, readdirSync, statSync } from "node:fs";
import { join } from "node:path";

const args = parseArgs(process.argv.slice(2));
const typeName = requireArg(args, "type");
const root = args.root ?? "node_modules/@syncfusion";
const explicitFile = args.dts;

const files = explicitFile ? [explicitFile] : walk(root).filter(file => file.endsWith(".d.ts"));
const matches = [];

for (const file of files) {
  if (!existsSync(file)) continue;
  const text = readFileSync(file, "utf8");
  const body = extractNamedBody(text, typeName);
  if (!body) continue;
  matches.push({ file, body });
}

if (matches.length === 0) {
  console.error(`Could not find event payload type ${typeName}`);
  process.exit(1);
}

for (const match of matches) {
  print(typeName, match.file, extractMembers(match.body));
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

function extractNamedBody(text, name) {
  const declaration = new RegExp(`(?:export\\s+declare\\s+|export\\s+|declare\\s+)?(?:interface|class)\\s+${escapeRegExp(name)}\\b[^\\{]*\\{`, "g");
  const match = declaration.exec(text);
  if (!match) return null;
  const open = text.indexOf("{", match.index);
  return readBalancedBody(text, open);
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

function extractMembers(body) {
  const members = [];
  for (const declaration of splitDeclarations(stripComments(body))) {
    const line = declaration.trim().replace(/\s+/g, " ");
    if (!line) continue;
    if (line.startsWith("private ") || line.startsWith("protected ")) continue;

    const property = line.match(/^([A-Za-z_][A-Za-z0-9_]*)\??:\s*([^;]+);$/);
    if (property) {
      members.push({ kind: "property", name: property[1], type: property[2] });
      continue;
    }

    const method = line.match(/^([A-Za-z_][A-Za-z0-9_]*)\(([^)]*)\):\s*([^;]+);$/);
    if (method) {
      members.push({ kind: "method", name: method[1], type: `${method[2]}) => ${method[3]}` });
      continue;
    }
  }
  return members;
}

function stripComments(text) {
  return text
    .replace(/\/\*[\s\S]*?\*\//g, "")
    .replace(/\/\/.*$/gm, "");
}

function splitDeclarations(body) {
  const declarations = [];
  let current = "";
  let parenDepth = 0;
  let braceDepth = 0;
  let bracketDepth = 0;

  for (const char of body) {
    current += char;
    if (char === "(") parenDepth++;
    if (char === ")") parenDepth--;
    if (char === "{") braceDepth++;
    if (char === "}") braceDepth--;
    if (char === "[") bracketDepth++;
    if (char === "]") bracketDepth--;

    if (char === ";" && parenDepth === 0 && braceDepth === 0 && bracketDepth === 0) {
      declarations.push(current);
      current = "";
    }
  }

  if (current.trim().length > 0) declarations.push(current);
  return declarations;
}

function print(type, file, members) {
  console.log(`# ${type} Payload`);
  console.log("");
  console.log(`Source: \`${file}\``);
  console.log("");
  console.log("| Member | Kind | Type | C# candidate | Proof needed |");
  console.log("|---|---|---|---|---|");
  for (const member of members) {
    const candidate = member.kind === "method"
      ? `event arg extension -> PayloadSource.Event().${member.name}(...)`
      : `event arg property -> ${pascal(member.name)}`;
    const proof = member.kind === "method"
      ? "call in raw event handler and verify visible/runtime effect"
      : "read from raw event handler payload and verify value during real interaction";
    console.log(`| \`${member.name}\` | ${member.kind} | \`${member.type}\` | ${candidate} | ${proof} |`);
  }
  console.log("");
}

function pascal(value) {
  return value.slice(0, 1).toUpperCase() + value.slice(1);
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}
