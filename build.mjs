import { copyFileSync, mkdirSync, readdirSync } from "node:fs";
import { posix, sep } from "node:path";
import { build } from "esbuild";

const app = "tests/Alis.Reactive.Conformance.App";

const screens = ["Bench", "Care", "Refusals", "World"];

const syncfusion = "node_modules/@syncfusion/ej2";

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
  entryPoints: ["Alis.Reactive.OpenTelemetry/opentelemetry.ts"],
  outfile: "Alis.Reactive.OpenTelemetry/wwwroot/js/alis-reactive-opentelemetry.js",
  bundle: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
});

await build({
  entryPoints: ["Alis.Reactive.Native/native.ts"],
  outfile: "Alis.Reactive.Native/wwwroot/js/alis-reactive-native.js",
  bundle: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
});

await build({
  entryPoints: ["Alis.Reactive.Fusion/FusionTemplates/fusion-templates.ts"],
  outfile: "Alis.Reactive.Fusion/wwwroot/js/alis-reactive-fusion-templates.js",
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

mkdirSync(app + "/wwwroot/css", { recursive: true });
copyFileSync(syncfusion + "/dist/ej2.min.js", app + "/wwwroot/js/ej2.min.js");
copyFileSync(syncfusion + "/bootstrap5.3.css", app + "/wwwroot/css/bootstrap5.3.css");
