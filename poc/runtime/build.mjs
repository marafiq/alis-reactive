import { build } from "esbuild";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import { readdirSync } from "node:fs";

const here = dirname(fileURLToPath(import.meta.url));
const output = join(here, "..", "..", "tests", "Alis.Reactive.Poc.App", "wwwroot", "js", "alis-poc.js");

await build({
  entryPoints: [join(here, "src", "index.ts")],
  bundle: true,
  format: "iife",
  target: "es2022",
  outfile: output,
  sourcemap: "linked",
  logLevel: "warning",
});
console.log("alis-poc: built " + output);

const components = join(here, "..", "..", "tests", "Alis.Reactive.Poc.App", "wwwroot", "js", "poc-components.js");
await build({
  entryPoints: [join(here, "..", "..", "tests", "Alis.Reactive.Poc.App", "Components", "components.ts")],
  bundle: true,
  format: "iife",
  target: "es2022",
  outfile: components,
  sourcemap: "linked",
  logLevel: "warning",
});
console.log("alis-poc: built " + components);

const bundles = join(here, "..", "..", "tests", "Alis.Reactive.Poc.App", "Components", "Bundles");
for (const file of readdirSync(bundles).filter((name) => name.endsWith(".ts"))) {
  const bundle = join(here, "..", "..", "tests", "Alis.Reactive.Poc.App", "wwwroot", "js", "bundles", file.slice(0, -".ts".length) + ".js");
  await build({
    entryPoints: [join(bundles, file)],
    bundle: true,
    format: "iife",
    target: "es2022",
    outfile: bundle,
    sourcemap: "linked",
    logLevel: "warning",
  });
  console.log("alis-poc: built " + bundle);
}
