import { copyFileSync, existsSync, mkdirSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(scriptDir, "..");

const copies = [
  {
    source: resolve(repoRoot, "Alis.Reactive.SandboxApp/wwwroot/js/alis-reactive.js"),
    destination: resolve(repoRoot, "Alis.Reactive/assets/js/alis-reactive.js")
  },
  {
    source: resolve(repoRoot, "Alis.Reactive.SandboxApp/wwwroot/css/design-system.css"),
    destination: resolve(repoRoot, "Alis.Reactive/assets/css/design-system.css")
  }
];

for (const { source, destination } of copies) {
  if (!existsSync(source)) {
    throw new Error(`Cannot sync packaged asset because source file is missing: ${source}`);
  }

  mkdirSync(dirname(destination), { recursive: true });
  copyFileSync(source, destination);
  console.log(`synced ${destination}`);
}
