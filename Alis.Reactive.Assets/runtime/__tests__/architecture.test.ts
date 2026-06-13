// Architecture enforcement: fails the suite when runtime source drifts from the
// boundaries CLAUDE.md prescribes. The rules live here as code, so prose never has
// to track which files currently hold an exception — the allowlists below do.
import { describe, expect, it } from "vitest";
import { readdirSync, readFileSync } from "node:fs";
import { join, relative, resolve } from "node:path";

// Vitest runs from the workspace root (vitest.config.ts lives there).
const runtimeDir = resolve(process.cwd(), "runtime");

// Generated contract and the test suite itself are outside the rules' scope.
const outOfScope = [/^__tests__\//, /^types\/plan\.ts$/];

function walk(dir: string): string[] {
  const files: string[] = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const fullPath = join(dir, entry.name);
    if (entry.isDirectory()) files.push(...walk(fullPath));
    else if (entry.name.endsWith(".ts")) files.push(fullPath);
  }
  return files;
}

function runtimeSourceFiles(): string[] {
  return walk(runtimeDir)
    .map(file => relative(runtimeDir, file).replaceAll("\\", "/"))
    .filter(file => !outOfScope.some(pattern => pattern.test(file)));
}

// Rationale comments may name vendors and DOM APIs; only code counts as a violation.
// Comments are blanked instead of removed so reported line numbers match the file.
function codeOnly(source: string): string {
  const blank = (match: string): string => match.replace(/[^\n]/g, " ");
  return source
    .replace(/\/\*[\s\S]*?\*\//g, blank)
    .replace(/(^|[ \t])\/\/[^\n]*/gm, blank);
}

function violationsOf(pattern: RegExp, isAllowed: (file: string) => boolean): string[] {
  const violations: string[] = [];
  for (const file of runtimeSourceFiles()) {
    if (isAllowed(file)) continue;
    const lines = codeOnly(readFileSync(join(runtimeDir, file), "utf8")).split("\n");
    lines.forEach((line, index) => {
      const match = pattern.exec(line);
      if (match) violations.push(`${file}:${index + 1} -> ${match[0]}`);
    });
  }
  return violations;
}

describe("architecture: plan-driven IDs — no DOM scanning", () => {
  // CLAUDE.md "Plan-Driven IDs": the plan carries every ID the runtime needs, so a wide
  // DOM query means the plan is missing information. The only justified wide queries are
  // true external boundaries: discovering plan scripts in HTML the runtime did not author,
  // and cleanup by a data attribute the runtime itself stamped.
  const wideQueryBoundaries = new Set([
    "root.ts", // plan discovery at boot
    "execution/partials/inject.ts", // plan discovery in injected partial HTML
    "execution/realtime/retry-indicator.ts", // self-stamped retry markers inside the developer-owned container
  ]);

  it("wide DOM queries appear only at justified external boundaries", () => {
    const violations = violationsOf(
      /querySelector|getElementsBy/,
      file => wideQueryBoundaries.has(file),
    );
    expect(violations, "wide DOM query outside a justified boundary — carry the ID in the plan instead").toEqual([]);
  });
});

describe("architecture: vendor isolation", () => {
  // CLAUDE.md Rule 5: Syncfusion knowledge lives in the per-vendor driver, the vendor
  // event adapter, and vendor component modules. Every other module stays vendor-blind.
  const vendorBoundaries = [
    /^components\/fusion\//, // vendor component modules (app-level Syncfusion objects)
    /^events\/event-fusion\.ts$/, // vendor event adapter
    /^browser-objects\/component-driver\.ts$/, // registers per-vendor drivers (ej2_instances root)
    // Documented exceptions, tracked in .claude/rules/process-layers.md known gaps.
    // Fixing one means deleting its line here — the test then enforces the stricter rule.
    /^execution\/partials\/inject\.ts$/, // ej.base.append initializes injected Syncfusion HTML
    /^execution\/requests\/request-payload-writer\.ts$/, // unwraps Syncfusion rawFile uploads
  ];

  it("Syncfusion knowledge stays behind the vendor boundary", () => {
    const violations = violationsOf(
      /Syncfusion|rawFile|ej2_instances|\bej\b/,
      file => vendorBoundaries.some(pattern => pattern.test(file)),
    );
    expect(violations, "Syncfusion knowledge outside the vendor boundary — route it through the vendor driver or event adapter").toEqual([]);
  });
});
