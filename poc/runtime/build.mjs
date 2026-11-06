import { build } from "esbuild";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

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
