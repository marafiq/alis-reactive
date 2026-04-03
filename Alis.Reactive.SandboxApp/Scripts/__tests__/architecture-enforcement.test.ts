import { describe, expect, it } from "vitest";
import * as fs from "fs";
import * as path from "path";
import { fileURLToPath } from "url";

const scriptsDir = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");

function readSource(relPath: string): string {
  return fs.readFileSync(path.join(scriptsDir, relPath), "utf-8");
}

function productionSourceFiles(dir = scriptsDir, prefix = ""): string[] {
  const results: string[] = [];

  for (const dirent of fs.readdirSync(dir, { withFileTypes: true })) {
    if (dirent.name.startsWith("__")) continue;
    if (dirent.name === "tsconfig.json") continue;

    const rel = prefix ? `${prefix}/${dirent.name}` : dirent.name;
    if (dirent.isDirectory()) {
      results.push(...productionSourceFiles(path.join(dir, dirent.name), rel));
      continue;
    }

    if (dirent.name.endsWith(".ts") && rel !== "components/lab/test-widget.ts") {
      results.push(rel);
    }
  }

  return results;
}

describe("architecture enforcement", () => {
  it("runtime module layout matches the approved V2 surface", () => {
    const approved = [
      "components/fusion/confirm.ts",
      "components/native/drawer.ts",
      "components/native/loader.ts",
      "components/native/native-action-link.ts",
      "conditions/conditions.ts",
      "core/assert-never.ts",
      "core/coerce.ts",
      "core/trace.ts",
      "core/walk.ts",
      "execution/execute.ts",
      "execution/http.ts",
      "execution/inject.ts",
      "execution/retry-indicator.ts",
      "execution/server-push.ts",
      "execution/signalr.ts",
      "execution/trigger.ts",
      "lifecycle/boot.ts",
      "lifecycle/contract-map.ts",
      "lifecycle/merge-plan.ts",
      "lifecycle/object-map.ts",
      "resolution/contracts.ts",
      "resolution/values.ts",
      "root.ts",
      "types/context.ts",
      "types/index.ts",
      "types/plan.ts",
      "validation/error-display.ts",
      "validation/index.ts",
      "validation/live-clear.ts",
      "validation/orchestrator.ts",
    ];

    expect(productionSourceFiles().sort()).toEqual(approved.sort());
  });

  it("no ej2_instances outside contracts.ts", () => {
    const violations: string[] = [];

    for (const file of productionSourceFiles()) {
      if (file === "resolution/contracts.ts") continue;

      const hasCodeRef = readSource(file).split("\n").some(line => {
        const trimmed = line.trimStart();
        return !trimmed.startsWith("//") && !trimmed.startsWith("*") && trimmed.includes("ej2_instances");
      });

      if (hasCodeRef) violations.push(file);
    }

    expect(violations).toEqual([]);
  });

  it("no window.alis writes outside confirm.ts", () => {
    const writePattern = /\(window\s+as\s+any\)\.alis\s*=/;
    const violations: string[] = [];

    for (const file of productionSourceFiles()) {
      if (file === "components/fusion/confirm.ts") continue;
      if (writePattern.test(readSource(file))) violations.push(file);
    }

    expect(violations).toEqual([]);
  });
});
