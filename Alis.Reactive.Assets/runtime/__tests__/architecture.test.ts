import { describe, expect, it } from "vitest";
import { readdirSync, readFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";

const runtimeDir = resolve(process.cwd(), "runtime");

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
  const wideQueryBoundaries = new Set([
    "plans/discover.ts",
    "plugins/slot.ts",
    "realtime/retry-indicator.ts",
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
  it("the runtime holds no Syncfusion knowledge", () => {
    const violations = violationsOf(/Syncfusion|rawFile|ej2_instances|\bej\b/, () => false);
    expect(violations, "Syncfusion knowledge in the runtime: carry it in the vendor declaration").toEqual([]);
  });
});

describe("architecture: import directions", () => {
  const allowed: Record<string, readonly string[]> = {
    grammar: ["grammar", "types", "shared", "diagnostics"],
    objects: ["objects", "grammar", "types", "shared", "diagnostics"],
    plans: ["plans", "objects", "grammar", "types", "shared", "diagnostics"],
    plugins: ["plugins", "plans", "objects", "grammar", "types", "shared", "diagnostics"],
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
