import { copyFileSync, mkdirSync } from "node:fs";
import { createRequire } from "node:module";
import { basename, dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const packageDir = dirname(createRequire(import.meta.url).resolve("@syncfusion/ej2/package.json"));
const target = join(dirname(fileURLToPath(import.meta.url)), "wwwroot", "vendor", "syncfusion");
mkdirSync(target, { recursive: true });
for (const file of ["dist/ej2.min.js", "tailwind3.css"]) copyFileSync(join(packageDir, file), join(target, basename(file)));
