// Architecture enforcement: fails the suite when runtime source drifts from the boundaries the
// runtime's CLAUDE.md prescribes. The rules live here as code, so prose never has to track which
// files hold an exception: the allowlists below do.
import { describe, expect, it } from "vitest";
import { readdirSync, readFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";

// Vitest runs from the workspace root (vitest.config.ts lives there).
const runtimeDir = resolve(process.cwd(), "runtime");

// The generated contract and the test suite itself are outside the rules' scope.
const outOfScope = [/^__tests__\//, /^types\//];

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

// Rationale comments may name vendors and DOM APIs; only code counts as a violation. Comments are
// blanked instead of removed so reported line numbers match the file.
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

describe("architecture: plan-driven IDs, no DOM scanning", () => {
  // The plan carries every id the runtime needs, so a wide DOM query means the plan is missing
  // information. The only justified wide queries are external boundaries: HTML the runtime did not
  // author, and cleanup by an attribute the runtime itself stamped.
  const wideQueryBoundaries = new Set([
    "plans/discover.ts", // plan scripts in the page and in loaded HTML
    "modules/slot.ts", // the scripts of loaded HTML, run the way a parsed page runs them
    "realtime/retry-indicator.ts", // self-stamped retry markers inside the developer-owned container
  ]);

  it("wide DOM queries appear only at justified external boundaries", () => {
    const violations = violationsOf(
      /querySelector|getElementsBy/,
      file => wideQueryBoundaries.has(file),
    );
    expect(violations, "wide DOM query outside a justified boundary: carry the id in the plan instead").toEqual([]);
  });
});

describe("architecture: vendor isolation", () => {
  // A vendor is data in the plan (where its object lives, how it subscribes, what a listener
  // receives), so the runtime names no vendor. Vendor code lives in components/, beside the runtime.
  it("the runtime holds no Syncfusion knowledge", () => {
    const violations = violationsOf(/Syncfusion|rawFile|ej2_instances|\bej\b/, () => false);
    expect(violations, "Syncfusion knowledge in the runtime: carry it in the vendor declaration").toEqual([]);
  });
});

describe("architecture: import directions", () => {
  // The interpreter is the core and imports nothing that knows a feature: grammar evaluates and
  // executes, objects locate, plans start owners, and the modules are the vocabulary on top.
  const allowed: Record<string, readonly string[]> = {
    grammar: ["grammar", "types", "shared", "diagnostics"],
    objects: ["objects", "grammar", "types", "shared", "diagnostics"],
    plans: ["plans", "objects", "grammar", "types", "shared", "diagnostics"],
    modules: ["modules", "plans", "objects", "grammar", "types", "shared", "diagnostics"],
    realtime: ["realtime", "objects", "types", "shared", "diagnostics"],
    diagnostics: ["diagnostics", "shared"],
    shared: ["shared"],
  };

  function layerOf(file: string): string {
    return file.split("/")[0]!;
  }

  it("each layer imports only the layers beneath it", () => {
    const violations: string[] = [];
    for (const file of runtimeSourceFiles()) {
      const layer = layerOf(file);
      const permitted = allowed[layer];
      if (permitted === undefined) continue;
      const source = codeOnly(readFileSync(join(runtimeDir, file), "utf8"));
      for (const match of source.matchAll(/from\s+"(\.[^"]+)"|import\s+"(\.[^"]+)"/g)) {
        const specifier = match[1] ?? match[2]!;
        const target = relative(runtimeDir, resolve(runtimeDir, dirname(file), specifier)).replaceAll("\\", "/");
        if (!permitted.includes(layerOf(target))) violations.push(`${file} imports ${target}`);
      }
    }
    expect(violations, "an import against the layer direction").toEqual([]);
  });

  it("nothing imports the entry point", () => {
    const violations = runtimeSourceFiles()
      .filter(file => /from\s+"\.{1,2}\/(\.\.\/)*root"/.test(codeOnly(readFileSync(join(runtimeDir, file), "utf8"))));
    expect(violations).toEqual([]);
  });
});

describe("architecture: the framework's components are built like an app's", () => {
  // components/ holds the framework's own components (Native, Fusion, the app-level objects). They
  // construct on alis:mount and queue plugins in window.__alisPlugins, the hooks an app's components
  // use, so neither side imports the other: a component the framework ships needs nothing a brought
  // one cannot have.
  const assetsDir = resolve(runtimeDir, "..");
  const componentsDir = join(assetsDir, "components");

  function importsOf(file: string): string[] {
    const source = codeOnly(readFileSync(file, "utf8"));
    return [...source.matchAll(/from\s+"(\.[^"]+)"|import\s+"(\.[^"]+)"/g)]
      .map(match => resolve(dirname(file), match[1] ?? match[2]!));
  }

  it("components import nothing from the runtime", () => {
    const violations = walk(componentsDir).flatMap(file =>
      importsOf(file)
        .filter(target => !target.startsWith(componentsDir))
        .map(target => `${relative(assetsDir, file)} imports ${relative(assetsDir, target)}`));
    expect(violations).toEqual([]);
  });

  it("the runtime imports nothing from components", () => {
    const violations = runtimeSourceFiles().flatMap(file =>
      importsOf(join(runtimeDir, file))
        .filter(target => !target.startsWith(runtimeDir))
        .map(target => `runtime/${file} imports ${relative(assetsDir, target)}`));
    expect(violations).toEqual([]);
  });
});
