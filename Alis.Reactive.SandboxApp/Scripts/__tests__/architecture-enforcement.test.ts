import { describe, it, expect } from "vitest";
import * as fs from "fs";
import * as path from "path";

describe("architecture enforcement", () => {
  const scriptsDir = path.resolve(__dirname, "..");

  function readSource(filename: string): string {
    return fs.readFileSync(path.join(scriptsDir, filename), "utf-8");
  }

  function allSourceFiles(): string[] {
    return fs.readdirSync(scriptsDir)
      .filter(f => f.endsWith(".ts") && f !== "test-widget.ts")
      .filter(f => !f.startsWith("__"));
  }

  it("no ej2_instances outside component.ts", () => {
    const violations: string[] = [];
    for (const file of allSourceFiles()) {
      if (file === "component.ts") continue;
      const content = readSource(file);
      if (content.includes("ej2_instances")) {
        violations.push(file);
      }
    }
    expect(violations).toEqual([]);
  });

  it("no ej.base outside inject.ts", () => {
    const violations: string[] = [];
    for (const file of allSourceFiles()) {
      if (file === "inject.ts") continue;
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
      if (file === "confirm.ts") continue;
      const content = readSource(file);
      if (writePattern.test(content)) {
        violations.push(file);
      }
    }
    expect(violations).toEqual([]);
  });
});
