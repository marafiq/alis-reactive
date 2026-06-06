#!/usr/bin/env node

import { existsSync, mkdirSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";

const args = parseArgs(process.argv.slice(2));

const component = requireArg(args, "component");
const namespace = requireArg(args, "namespace");
const className = requireArg(args, "class");
const id = args.id ?? component;
const apiSet = args["api-set"] ?? "core";
const artifactComponent = args["artifact-component"] ?? component;

const file = resolve(
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/${artifactComponent}/probes/raw-ej2-${apiSet}.html`,
);
const traceFile = resolve(
  `tools/FusionOnboarding/wwwroot/onboarding/fusion/${artifactComponent}/traces/raw-ej2-${apiSet}.trace.json`,
);

if (existsSync(file)) {
  console.error(`Probe already exists: ${file}`);
  process.exit(1);
}

mkdirSync(dirname(file), { recursive: true });
mkdirSync(dirname(traceFile), { recursive: true });
writeFileSync(file, template({ component, namespace, className, id, apiSet, traceFile }), "utf8");
console.log(file);
console.log(`Trace output target: ${traceFile}`);

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

function requireArg(args, name) {
  const value = args[name];
  if (typeof value === "string" && value.trim().length > 0) return value.trim();
  console.error(`Missing --${name}`);
  process.exit(1);
}

function template({ component, namespace, className, id, apiSet, traceFile }) {
  return `<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <title>Fusion ${className} Raw EJ2 API Probe</title>
    <link rel="stylesheet" href="/vendor/syncfusion/material.css" />
    <script src="/vendor/syncfusion/dist/ej2.min.js"></script>
    <style>
        body { font-family: system-ui, sans-serif; margin: 20px; }
        #host { border: 1px solid #d0d7de; padding: 16px; margin-bottom: 16px; }
        table { border-collapse: collapse; margin: 16px 0; width: 100%; }
        th, td { border: 1px solid #d0d7de; padding: 8px; text-align: left; vertical-align: top; }
        th { background: #f6f8fa; }
        pre { background: #111827; color: #e5e7eb; padding: 12px; overflow: auto; }
        button { margin-right: 8px; }
    </style>
</head>
<body>
    <h1>Fusion ${className} Raw EJ2 API Probe</h1>
    <p>Use this page to discover the exact Syncfusion EJ2 JS object API before onboarding typed Fusion C# members.</p>
    <div id="host">
        <div id="${id}"></div>
    </div>

    <h2>Candidate Matrix</h2>
    <p>Every row must be proven here before it becomes typed Fusion API.</p>
    <table>
        <thead>
            <tr>
                <th>Candidate</th>
                <th>Kind</th>
                <th>Builder</th>
                <th>Args</th>
                <th>Return/Payload</th>
                <th>Proof</th>
                <th>Proposed C#</th>
                <th>Outcome</th>
            </tr>
        </thead>
        <tbody id="proposal-body"></tbody>
    </table>

    <button id="dump">Dump Keys</button>
    <button id="clear">Clear Trace</button>
    <pre id="trace"></pre>

    <script>
        const trace = [];
        const proposalRows = [];
        const target = document.getElementById("${id}");

        function safeJson(key, value) {
            if (value instanceof Element) return "[Element#" + (value.id || value.tagName) + "]";
            if (typeof value === "function") return "[Function]";
            return value;
        }

        function clean(value, seen, depth) {
            if (value === null || typeof value !== "object") return value;
            if (value instanceof Element) return "[Element#" + (value.id || value.tagName) + "]";
            if (depth > 4) return "[MaxDepth]";
            if (seen.has(value)) return "[Circular]";
            seen.add(value);
            if (Array.isArray(value)) return value.slice(0, 20).map(item => clean(item, seen, depth + 1));
            const output = {};
            Object.keys(value).slice(0, 80).forEach(key => {
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
                    if (name !== "constructor" && typeof value[name] === "function") {
                        names.add(name);
                    }
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
                    sample: clean(item, new WeakSet(), 0)
                };
            });
            return {
                ownKeys: own,
                functions: functionNames(args),
                properties
            };
        }

        function record(label, value) {
            trace.push({
                at: new Date().toISOString(),
                label,
                value: clean(value, new WeakSet(), 0)
            });
            renderTrace();
        }

        function renderTrace() {
            document.getElementById("trace").textContent =
                JSON.stringify(trace, safeJson, 2);
        }

        function renderProposal() {
            const body = document.getElementById("proposal-body");
            body.innerHTML = "";
            proposalRows.forEach(row => {
                const tr = document.createElement("tr");
                [
                    row.candidate,
                    row.kind,
                    row.builder,
                    row.args,
                    row.returnOrPayload,
                    row.proof,
                    row.csharp,
                    row.outcome
                ].forEach(value => {
                    const td = document.createElement("td");
                    td.textContent = value || "";
                    tr.appendChild(td);
                });
                body.appendChild(tr);
            });
        }

        function proposal(row) {
            proposalRows.push(row);
            renderProposal();
            return row;
        }

        const probe = {
            target,
            trace,
            proposalRows,
            proposal,
            record,
            member: (label, read) => record(label, read()),
            call: (label, invoke) => record(label, invoke()),
            event: (label, args) => record(label, describePayload(args)),
            eventCall: (label, args, method, call) => {
                const before = describePayload(args);
                const result = call();
                record(label, {
                    method,
                    before,
                    result: clean(result, new WeakSet(), 0),
                    after: describePayload(args)
                });
            }
        };

        window.__fusionProbe = probe;

        const ej2 = new ej.${namespace}.${className}({
            // Add real component options and event handlers here while tracing.
            // Example:
            // filtering: args => probe.event("filtering", args)
        });
        ej2.appendTo(target);
        probe.ej2 = ej2;

        document.getElementById("dump").addEventListener("click", () => {
            record("own keys", Object.keys(ej2).sort());
            const proto = Object.getPrototypeOf(ej2);
            record("prototype methods", Object.getOwnPropertyNames(proto).sort());
        });

        document.getElementById("clear").addEventListener("click", () => {
            trace.length = 0;
            renderTrace();
        });

        record("ready", {
            component: "${component}",
            apiSet: "${apiSet}",
            namespace: "ej.${namespace}",
            className: "${className}",
            id: "${id}",
            traceFile: "${traceFile}"
        });
        proposal({
            candidate: "replace this row",
            kind: "method | prop-read | prop-write | event | payload-read | bridge",
            builder: "covered | not covered | static-only",
            args: "exact args and order",
            returnOrPayload: "shape proven by trace",
            proof: "visible behavior or trace label",
            csharp: "typed Fusion API",
            outcome: "implement | bridge-needed | exclude | needs-proof"
        });
    </script>
</body>
</html>
`;
}
