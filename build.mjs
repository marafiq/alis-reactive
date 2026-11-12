import { build } from "esbuild";

await build({
  entryPoints: ["Alis.Reactive.Assets/runtime/index.ts"],
  outfile: "Alis.Reactive/wwwroot/js/alis-reactive.js",
  bundle: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
});
