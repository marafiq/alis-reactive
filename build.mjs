import { build } from "esbuild";

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
