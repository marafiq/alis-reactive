import { readdirSync } from "node:fs";
import { posix, sep } from "node:path";
import { build } from "esbuild";

const app = "tests/Alis.Reactive.Conformance.App";

const screens = ["Bench", "Care", "Refusals", "World"];

function pageScriptsOf(screen) {
  return readdirSync(posix.join(app, screen), { recursive: true })
    .map((file) => posix.join(screen, file.split(sep).join(posix.sep)))
    .filter((file) => file.endsWith(".ts") && !file.endsWith(".d.ts"))
    .map((file) => ({ in: posix.join(app, file), out: file.slice(0, -".ts".length).toLowerCase() }));
}

await build({
  entryPoints: ["Alis.Reactive.Assets/runtime/index.ts"],
  outfile: "Alis.Reactive/wwwroot/js/alis-reactive.js",
  bundle: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
});

await build({
  entryPoints: ["tests/Alis.Reactive.Conformance.App/conformance.ts"],
  outfile: "tests/Alis.Reactive.Conformance.App/wwwroot/js/alis-reactive-conformance.js",
  bundle: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
});

await build({
  entryPoints: screens.flatMap(pageScriptsOf),
  outdir: app + "/wwwroot/js",
  bundle: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
});
