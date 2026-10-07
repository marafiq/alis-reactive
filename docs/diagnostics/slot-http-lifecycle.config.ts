import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "jsdom",
    include: ["docs/diagnostics/slot-http-lifecycle.probe.ts"],
  },
});
