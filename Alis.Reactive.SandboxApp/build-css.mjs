import { build } from "vite";

try {
  // MSBuild Exec can leave Vite/Rolldown file handles open after a successful build.
  await build({ configFile: "vite.config.ts" });
  process.exit(0);
} catch (error) {
  console.error(error);
  process.exit(1);
}
