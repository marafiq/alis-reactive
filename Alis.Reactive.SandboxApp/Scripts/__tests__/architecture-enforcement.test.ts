import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

describe("architecture enforcement", () => {
  const scriptsDir = path.resolve(__dirname, "..");

  function readSource(relPath: string): string {
    return fs.readFileSync(path.join(scriptsDir, relPath), "utf-8");
  }

  /** Recursively collect all .ts source files (relative paths), excluding __tests__ and __experiments__. */
  function allSourceFiles(dir = scriptsDir, prefix = ""): string[] {
    const results: string[] = [];
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      if (entry.name.startsWith("__")) continue;
      const rel = prefix ? `${prefix}/${entry.name}` : entry.name;
      if (entry.isDirectory()) {
        results.push(...allSourceFiles(path.join(dir, entry.name), rel));
      } else if (entry.name.endsWith(".ts") && rel !== "components/lab/test-widget.ts") {
        results.push(rel);
      }
    }
    return results;
  }

  it("no ej2_instances outside component.ts", () => {
    const violations: string[] = [];
    for (const file of allSourceFiles()) {
      if (file === "resolution/component.ts") continue;
      const content = readSource(file);
      // Only flag non-comment lines that reference ej2_instances
      const hasCodeRef = content.split("\n").some(line => {
        const trimmed = line.trimStart();
        return !trimmed.startsWith("//") && !trimmed.startsWith("*") && trimmed.includes("ej2_instances");
      });
      if (hasCodeRef) {
        violations.push(file);
      }
    }
    expect(violations).toEqual([]);
  });

  it("no ej.base outside inject.ts", () => {
    const violations: string[] = [];
    for (const file of allSourceFiles()) {
      if (file === "lifecycle/inject.ts") continue;
      const content = readSource(file);
      if (/ej\.base/.test(content)) {
        violations.push(file);
      }
    }
    expect(violations).toEqual([]);
  });

  it("no window.alis writes outside confirm.ts", () => {
    // Only confirm.ts may ASSIGN to window.alis (set up the confirm handler).
    // conditions.ts may READ window.alis.confirm() — that's fine.
    const writePattern = /\(window\s+as\s+any\)\.alis\s*=/;
    const violations: string[] = [];
    for (const file of allSourceFiles()) {
      if (file === "components/fusion/confirm.ts") continue;
      const content = readSource(file);
      if (writePattern.test(content)) {
        violations.push(file);
      }
    }
    expect(violations).toEqual([]);
  });
});
