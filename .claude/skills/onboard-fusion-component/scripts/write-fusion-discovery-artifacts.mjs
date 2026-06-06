#!/usr/bin/env node

import { existsSync, mkdirSync, readFileSync, readdirSync, statSync, writeFileSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join, relative, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));
const component = requireArg(args, "component");
const fusionType = args["fusion-type"] ?? `Fusion${pascal(component)}`;
const className = requireArg(args, "class");
const namespace = requireArg(args, "namespace");
const dtsPath = requireArg(args, "dts");
const jsPath = args.js ?? "";
const xmlPath = requireArg(args, "xml");
const apiSet = args["api-set"] ?? "core";
const artifactRoot = resolve(`tools/FusionOnboarding/wwwroot/onboarding/fusion/${component}`);
const dtsRoot = args["dts-root"] ?? "node_modules/@syncfusion";
const blazorPackage = args["blazor-package"] ?? "";
const blazorVersion = args["blazor-version"] ?? "";
const write = args.write === true || args.write === "true";

if (!existsSync(dtsPath)) fail(`d.ts not found: ${dtsPath}`);
if (jsPath && !existsSync(jsPath)) fail(`JS source not found: ${jsPath}`);
if (!existsSync(xmlPath)) fail(`MVC XML not found: ${xmlPath}`);
if (!existsSync(dtsRoot)) fail(`d.ts root not found: ${dtsRoot}`);

const xml = readFileSync(xmlPath, "utf8");
const classSource = readFileSync(dtsPath, "utf8");
const classBody = extractNamedBody(classSource, "class", className);
if (!classBody) fail(`Could not find class ${className} in ${dtsPath}`);

const builderName = `${className}Builder`;
const builderMethods = extractBuilderMethods(xml, builderName);
const publicMembers = extractMembers(classBody)
  .filter(member => !ignoredMember(member))
  .map(member => describePublicMember(member, builderMethods, dtsPath))
  .sort(byMember);
const events = publicMembers.filter(member => member.kind === "event");
const eventPayloads = discoverEventPayloads(events, dtsRoot);
const blazorReport = inspectBlazor({ blazorPackage, blazorVersion, className });

const files = new Map([
  ["discovery/public-api-surface.json", `${stableJson({
    status: "static-discovery",
    component,
    fusionType,
    syncfusion: {
      className,
      namespace: `ej.${namespace}`,
      dtsPath,
      jsPath,
      xmlPath,
      builderName
    },
    counts: {
      members: publicMembers.length,
      properties: publicMembers.filter(member => member.kind === "property").length,
      methods: publicMembers.filter(member => member.kind === "method").length,
      events: events.length,
      builderMethods: builderMethods.length
    },
    members: publicMembers
  })}\n`],
  ["discovery/event-payload-surface.json", `${stableJson({
    status: "static-discovery",
    component,
    fusionType,
    syncfusion: {
      className,
      namespace: `ej.${namespace}`,
      dtsRoot
    },
    counts: {
      events: eventPayloads.length,
      payloadTypes: unique(eventPayloads.flatMap(event => event.payloadTypes.map(type => type.name))).length
    },
    events: eventPayloads
  })}\n`],
  ["discovery/mvc-builder-coverage.md", mvcBuilderCoverageMarkdown({
    component,
    fusionType,
    className,
    builderName,
    xmlPath,
    builderMethods,
    publicMembers
  })],
  ["discovery/blazor-candidates.md", blazorCandidatesMarkdown({
    component,
    fusionType,
    className,
    report: blazorReport
  })],
  [`probes/raw-ej2-${apiSet}.html`, probeHtml({
    component,
    fusionType,
    className,
    namespace,
    apiSet,
    publicMembers,
    events
  })],
  ["master-usecases-index.md", masterIndexMarkdown({
    component,
    fusionType,
    className,
    namespace,
    apiSet,
    publicMembers,
    events,
    eventPayloads
  })]
]);

if (!write) {
  console.log(`# Fusion discovery artifact preview`);
  console.log("");
  console.log(`Component: ${fusionType}`);
  console.log(`Syncfusion: ej.${namespace}.${className}`);
  console.log(`Members: ${publicMembers.length}`);
  console.log(`Events: ${events.length}`);
  console.log(`Builder methods: ${builderMethods.length}`);
  console.log("");
  console.log("Files:");
  for (const path of files.keys()) console.log(`- ${join(artifactRoot, path)}`);
  process.exit(0);
}

for (const [path, content] of files) {
  const fullPath = join(artifactRoot, path);
  mkdirSync(dirname(fullPath), { recursive: true });
  writeFileSync(fullPath, content, "utf8");
}

console.log(`Wrote ${files.size} discovery artifacts for ${fusionType}.`);
console.log(`Members: ${publicMembers.length}; events: ${events.length}; builder methods: ${builderMethods.length}.`);

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
  fail(`Missing --${name}`);
}

function fail(message) {
  console.error(message);
  process.exit(1);
}

function extractNamedBody(text, kind, name) {
  const declaration = new RegExp(`(?:export\\s+declare\\s+|export\\s+|declare\\s+)?${kind}\\s+${escapeRegExp(name)}\\b[^\\{]*\\{`, "g");
  const match = declaration.exec(text);
  if (!match) return "";
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
      const [, name, rawType] = property;
      const eventType = unwrapEmitType(rawType);
      members.push({
        name,
        kind: eventType ? "event" : "property",
        type: eventType || rawType
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

function splitDeclarations(body) {
  const declarations = [];
  let current = "";
  let parenDepth = 0;
  let braceDepth = 0;
  let bracketDepth = 0;
  let angleDepth = 0;

  for (const char of body) {
    current += char;
    if (char === "(") parenDepth++;
    if (char === ")") parenDepth--;
    if (char === "{") braceDepth++;
    if (char === "}") braceDepth--;
    if (char === "[") bracketDepth++;
    if (char === "]") bracketDepth--;
    if (char === "<") angleDepth++;
    if (char === ">") angleDepth = Math.max(0, angleDepth - 1);

    if (char === ";" && parenDepth === 0 && braceDepth === 0 && bracketDepth === 0 && angleDepth === 0) {
      declarations.push(current);
      current = "";
    }
  }

  if (current.trim().length > 0) declarations.push(current);
  return declarations;
}

function stripComments(text) {
  return text
    .replace(/\/\*[\s\S]*?\*\//g, "")
    .replace(/\/\/.*$/gm, "");
}

function unwrapEmitType(rawType) {
  const type = rawType.trim();
  if (!type.startsWith("EmitType<")) return "";
  return type.slice("EmitType<".length, -1).trim();
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

function describePublicMember(member, builderMethods, sourcePath) {
  const builderName = pascal(member.name);
  const builderOverloads = builderMethods.filter(method => method.name === builderName);
  return {
    name: member.name,
    kind: member.kind,
    type: member.type,
    sourcePath,
    builder: builderOverloads.length === 0
      ? { covered: false, overloads: [] }
      : { covered: true, overloads: builderOverloads.map(method => method.signature) },
    discoveryDecision: decisionFor(member, builderOverloads.length > 0)
  };
}

function decisionFor(member, builderCovered) {
  if (member.kind === "event") return "candidate: typed event; payload and browser gesture proof required";
  if (member.kind === "method") {
    if (member.name === "destroy") return "skip: lifecycle cleanup, not Fusion plan behavior";
    return "candidate: runtime method or method return source; raw EJ2 visible effect proof required";
  }
  return builderCovered
    ? "builder-owned unless post-render read/write behavior is proven necessary"
    : "candidate: runtime property source/write only if raw EJ2 proof shows useful behavior";
}

function extractBuilderMethods(xmlText, builderName) {
  const rows = [];
  const expression = new RegExp(`name="M:Syncfusion\\.EJ2\\.[^"]+\\.${escapeRegExp(builderName)}(?:\`\\d+)?\\.([A-Za-z_][A-Za-z0-9_]*)\\(([^"]*)\\)"`, "g");
  let match;
  while ((match = expression.exec(xmlText)) !== null) {
    rows.push({ name: match[1], signature: match[2] });
  }
  return uniqueRows(rows, row => `${row.name}(${row.signature})`).sort(byName);
}

function discoverEventPayloads(events, root) {
  const files = walk(root).filter(file => file.endsWith(".d.ts"));
  const cache = new Map();
  return events.map(event => {
    const typeNames = event.type.split("|").map(type => cleanTypeName(type)).filter(Boolean);
    return {
      event: event.name,
      eventType: event.type,
      payloadTypes: typeNames.map(typeName => describeType(typeName, files, cache))
    };
  });
}

function describeType(typeName, files, cache, stack = []) {
  if (cache.has(typeName)) return cache.get(typeName);
  if (stack.includes(typeName)) {
    return { name: typeName, status: "cycle", sourcePath: "", extends: [], members: [] };
  }

  const declaration = findDeclaration(typeName, files);
  if (!declaration) {
    const missing = { name: typeName, status: "not-found", sourcePath: "", extends: [], members: [] };
    cache.set(typeName, missing);
    return missing;
  }

  const directMembers = extractMembers(declaration.body)
    .map(member => ({
      name: member.name,
      kind: member.kind,
      type: member.type,
      declaredOn: typeName,
      sourcePath: declaration.sourcePath
    }))
    .sort(byMember);
  const inherited = declaration.extends
    .map(base => describeType(base, files, cache, [...stack, typeName]))
    .flatMap(base => base.members.map(member => ({ ...member, inheritedVia: base.name })));
  const result = {
    name: typeName,
    status: "found",
    sourcePath: declaration.sourcePath,
    extends: declaration.extends,
    members: [...directMembers, ...inherited].sort(byMember)
  };
  cache.set(typeName, result);
  return result;
}

function findDeclaration(typeName, files) {
  for (const file of files) {
    const text = readFileSync(file, "utf8");
    const expression = new RegExp(`(?:export\\s+declare\\s+|export\\s+|declare\\s+)?(interface|class)\\s+${escapeRegExp(typeName)}\\b([^\\{]*)\\{`, "g");
    const match = expression.exec(text);
    if (!match) continue;
    const open = text.indexOf("{", match.index);
    return {
      sourcePath: file,
      body: readBalancedBody(text, open),
      extends: extractExtends(match[2])
    };
  }
  return null;
}

function extractExtends(header) {
  const match = header.match(/\bextends\s+([^{]+)/);
  if (!match) return [];
  return splitTopLevel(match[1], ",")
    .map(item => cleanTypeName(item))
    .filter(Boolean);
}

function cleanTypeName(type) {
  return type
    .trim()
    .replace(/\[\]$/g, "")
    .replace(/<.*>$/g, "")
    .replace(/^Array<(.+)>$/g, "$1")
    .replace(/^\(?\s*/g, "")
    .replace(/\s*\)?$/g, "")
    .split(/\s+/)[0]
    .replace(/[^A-Za-z0-9_].*$/g, "");
}

function splitTopLevel(value, separator) {
  const items = [];
  let current = "";
  let angleDepth = 0;
  let parenDepth = 0;
  for (const char of value) {
    if (char === "<") angleDepth++;
    if (char === ">") angleDepth = Math.max(0, angleDepth - 1);
    if (char === "(") parenDepth++;
    if (char === ")") parenDepth = Math.max(0, parenDepth - 1);
    if (char === separator && angleDepth === 0 && parenDepth === 0) {
      items.push(current);
      current = "";
      continue;
    }
    current += char;
  }
  if (current.trim()) items.push(current);
  return items;
}

function inspectBlazor({ blazorPackage, blazorVersion, className }) {
  if (!blazorPackage || !blazorVersion) {
    return {
      status: "not-requested",
      packageName: blazorPackage,
      version: blazorVersion,
      note: "No Syncfusion Blazor package was supplied for this discovery pass."
    };
  }

  const packageRoot = join(homedir(), ".nuget", "packages", blazorPackage.toLowerCase(), blazorVersion);
  if (!existsSync(packageRoot)) {
    return {
      status: "not-installed",
      packageName: blazorPackage,
      version: blazorVersion,
      packageRoot,
      note: `Install ${blazorPackage} ${blazorVersion} or pass decompiled metadata before making C# naming decisions.`
    };
  }

  const xml = findFirst(packageRoot, file => file.endsWith(".xml"));
  return {
    status: xml ? "metadata-present" : "xml-missing",
    packageName: blazorPackage,
    version: blazorVersion,
    packageRoot,
    sfType: `Sf${className}`,
    xmlPath: xml
  };
}

function mvcBuilderCoverageMarkdown({ component, fusionType, className, builderName, xmlPath, builderMethods, publicMembers }) {
  const covered = publicMembers.filter(member => member.builder.covered);
  return `# ${fusionType} MVC Builder Coverage

Status: static-discovery.

Syncfusion class: \`${className}\`
MVC builder: \`${builderName}\`
XML source: \`${xmlPath}\`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | ${builderMethods.length} |
| JS members with matching builder method | ${covered.length} |
| JS members without matching builder method | ${publicMembers.length - covered.length} |

## Builder Methods

| Builder Method | Parameters |
|---|---|
${builderMethods.map(method => `| \`${method.name}\` | \`${method.signature}\` |`).join("\n")}

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
${publicMembers.map(member => `| \`${member.name}\` | ${member.kind} | ${member.builder.covered ? "yes" : "no"} | ${member.discoveryDecision} |`).join("\n")}
`;
}

function blazorCandidatesMarkdown({ component, fusionType, className, report }) {
  return `# ${fusionType} Blazor Naming Candidates

Status: ${report.status}.

Syncfusion class: \`${className}\`

Blazor metadata is naming evidence only. It is never proof that a Fusion API
member exists or behaves correctly. Direct EJ2 behavior still requires raw HTML
trace proof before C# naming decisions are accepted.

## Evidence

| Field | Value |
|---|---|
| Package | \`${report.packageName || "not supplied"}\` |
| Version | \`${report.version || "not supplied"}\` |
| Package root | \`${report.packageRoot || "not supplied"}\` |
| Status | ${report.status} |
| Note | ${report.note || "Metadata exists; inspect XML/IL before C# naming decisions."} |

## Current Decision

No C# name is accepted from Blazor metadata in this artifact. Naming remains
pending until direct EJ2 rows are traced and mapped.
`;
}

function probeHtml({ component, fusionType, className, namespace, apiSet, publicMembers, events }) {
  const rows = publicMembers
    .filter(member => member.kind === "method" || member.kind === "event" || member.discoveryDecision.startsWith("candidate"))
    .slice(0, 80)
    .map(member => `<tr><td><code>${escapeHtml(member.name)}</code></td><td>${member.kind}</td><td>${escapeHtml(member.type)}</td><td>${member.builder.covered ? "yes" : "no"}</td><td>${escapeHtml(member.discoveryDecision)}</td></tr>`)
    .join("\n");
  const eventHandlers = events
    .map(event => `${event.name}: args => probe.event("${event.name}", args)`)
    .join(",\n                ");

  return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>${fusionType} Raw EJ2 ${apiSet} Probe</title>
    <link rel="stylesheet" href="/css/syncfusion.dev.css" />
    <script src="/vendor/syncfusion/dist/ej2.min.js"></script>
    <style>
        body { font-family: system-ui, sans-serif; margin: 20px; }
        #${component}-host { border: 1px solid #d0d7de; padding: 16px; margin-bottom: 16px; }
        table { border-collapse: collapse; margin: 16px 0; width: 100%; }
        th, td { border: 1px solid #d0d7de; padding: 8px; text-align: left; vertical-align: top; }
        th { background: #f6f8fa; }
        pre { background: #111827; color: #e5e7eb; padding: 12px; overflow: auto; }
        button { margin-right: 8px; }
    </style>
</head>
<body>
    <h1>${fusionType} Raw EJ2 ${apiSet} Probe</h1>
    <p>Status: generated static probe. Execute in the sandbox/browser, interact with the component, then save <code>window.__fusionProbe.trace</code> as <code>traces/raw-ej2-${apiSet}.trace.json</code>.</p>
    <div id="${component}-host">
        <div id="${component}"></div>
    </div>

    <button id="dump">Dump Keys</button>
    <button id="clear">Clear Trace</button>

    <h2>Candidate Rows</h2>
    <table>
        <thead><tr><th>JS Member</th><th>Kind</th><th>Type</th><th>Builder</th><th>Decision</th></tr></thead>
        <tbody>
${rows}
        </tbody>
    </table>

    <pre id="trace"></pre>

    <script>
        const trace = [];
        const target = document.getElementById("${component}");

        function clean(value, seen = new WeakSet(), depth = 0) {
            if (value === null || typeof value !== "object") return value;
            if (value instanceof Element) return "[Element#" + (value.id || value.tagName) + "]";
            if (depth > 4) return "[MaxDepth]";
            if (seen.has(value)) return "[Circular]";
            seen.add(value);
            if (Array.isArray(value)) return value.slice(0, 20).map(item => clean(item, seen, depth + 1));
            const output = {};
            Object.keys(value).sort().slice(0, 120).forEach(key => {
                const item = value[key];
                output[key] = typeof item === "function" ? "[Function]" : clean(item, seen, depth + 1);
            });
            return output;
        }

        function functionNames(value) {
            const names = new Set();
            let current = value;
            while (current && current !== Object.prototype) {
                Object.getOwnPropertyNames(current).forEach(name => {
                    if (name !== "constructor" && typeof value[name] === "function") names.add(name);
                });
                current = Object.getPrototypeOf(current);
            }
            return Array.from(names).sort();
        }

        function describePayload(args) {
            const own = Object.keys(args).sort();
            const properties = {};
            own.forEach(key => {
                const item = args[key];
                properties[key] = {
                    type: item === null ? "null" : Array.isArray(item) ? "array" : typeof item,
                    sample: clean(item)
                };
            });
            return { ownKeys: own, functions: functionNames(args), properties };
        }

        function record(label, value) {
            trace.push({ at: new Date().toISOString(), label, value: clean(value) });
            document.getElementById("trace").textContent = JSON.stringify(trace, null, 2);
        }

        const probe = {
            target,
            trace,
            record,
            event: (label, args) => record(label, describePayload(args)),
            member: (label, read) => record(label, read()),
            call: (label, invoke) => record(label, invoke())
        };
        window.__fusionProbe = probe;

        const ej2 = new ej.${namespace}.${className}({
            dataSource: [
                { id: 1, name: "Alpha", status: "Open" },
                { id: 2, name: "Beta", status: "Closed" },
                { id: 3, name: "Gamma", status: "Open" }
            ],
            columns: [
                { field: "id", headerText: "ID", width: 90, textAlign: "Right" },
                { field: "name", headerText: "Name", width: 160 },
                { field: "status", headerText: "Status", width: 140 }
            ],
            allowPaging: true,
            allowSorting: true,
            allowFiltering: true,
            pageSettings: { pageSize: 2 },
            ${eventHandlers}
        });

        ej2.appendTo(target);
        probe.ej2 = ej2;
        record("ready", {
            component: "${component}",
            apiSet: "${apiSet}",
            namespace: "ej.${namespace}",
            className: "${className}"
        });

        document.getElementById("dump").addEventListener("click", () => {
            record("own keys", Object.keys(ej2).sort());
            record("prototype methods", functionNames(ej2));
        });

        document.getElementById("clear").addEventListener("click", () => {
            trace.length = 0;
            document.getElementById("trace").textContent = "[]";
        });
    </script>
</body>
</html>
`;
}

function masterIndexMarkdown({ component, fusionType, className, namespace, apiSet, publicMembers, events, eventPayloads }) {
  return `# ${fusionType} Master Use Cases

Status: static-discovery. Runtime trace, primitive mapping, vertical slice
decision, implementation proof, and audit closeout are still pending.

This file is the entry point for deterministic Fusion onboarding or audit of
\`${fusionType}\`. Existing C#, sandbox, tests, docs, and memory are evidence
only after raw EJ2 discovery and primitive mapping prove them.

Syncfusion target: \`ej.${namespace}.${className}\`

No API member is accepted until the row is proven end to end:

\`\`\`text
raw EJ2 probe -> trace JSON -> candidate classification -> primitive map ->
C# name decision -> vertical slice plan -> implementation -> typed proof matrix ->
Playwright proof -> audit report
\`\`\`

## Current Counts

| Item | Count |
|---|---:|
| Static JS members | ${publicMembers.length} |
| Static event members | ${events.length} |
| Event payload entries | ${eventPayloads.length} |

## Use Case Rows

| Use Case | API Members | Event Payloads | Builder-Owned? | Primitive | C# Target | Proof Status |
|---|---|---|---|---|---|---|
| component inventory | current Fusion source, sandbox, and tests inventoried | n/a | n/a | n/a | n/a | inventory committed |
| shipped EJ2 static discovery | [public-api-surface.json](discovery/public-api-surface.json) | [event-payload-surface.json](discovery/event-payload-surface.json) | [mvc-builder-coverage.md](discovery/mvc-builder-coverage.md) | pending mapping | pending naming | static-discovery only |
| raw EJ2 ${apiSet} probe | [raw-ej2-${apiSet}.html](probes/raw-ej2-${apiSet}.html) | pending runtime gesture traces | pending runtime confirmation | pending mapping | pending naming | trace not yet executed |

## Linked Artifacts

- [Source inventory](discovery/source-inventory.md)
- [MVC builder coverage](discovery/mvc-builder-coverage.md)
- [Blazor candidates](discovery/blazor-candidates.md)
- [Public API surface](discovery/public-api-surface.json)
- [Event payload surface](discovery/event-payload-surface.json)
- [Raw EJ2 ${apiSet} probe](probes/raw-ej2-${apiSet}.html)
- \`traces/raw-ej2-${apiSet}.trace.json\` pending real browser execution
- \`mapping/primitive-map.md\` pending authoritative primitive mapping
- \`mapping/csharp-name-decisions.md\` pending Blazor candidate review and raw trace proof
- \`mapping/vertical-slice-plan.md\` pending vertical slice design
- \`proof/typed-api-coverage-matrix.md\` pending implemented public API inventory
- \`proof/playwright-proof.md\` pending behavior proof
- \`proof/audit-report.md\` pending audit closeout
`;
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

function stableJson(value) {
  return JSON.stringify(sortJson(value), null, 2);
}

function sortJson(value) {
  if (Array.isArray(value)) return value.map(sortJson);
  if (!value || typeof value !== "object") return value;
  return Object.fromEntries(Object.keys(value).sort().map(key => [key, sortJson(value[key])]));
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

function unique(values) {
  return Array.from(new Set(values)).sort();
}

function byName(left, right) {
  return left.name.localeCompare(right.name);
}

function byMember(left, right) {
  return left.name.localeCompare(right.name) || left.kind.localeCompare(right.kind);
}

function pascal(value) {
  return value
    .split(/[-_\s]+/g)
    .filter(Boolean)
    .map(part => part.slice(0, 1).toUpperCase() + part.slice(1))
    .join("");
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}
