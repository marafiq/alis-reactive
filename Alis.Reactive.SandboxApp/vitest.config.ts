import { defineConfig } from "vitest/config";

// Sandbox runtime unit tests. The Alis.Reactive.Assets workspace has its own
// vitest.config.ts for the framework runtime — this one is the second half of
// the former repo-root config, kept so a sandbox test path is never silently lost.
export default defineConfig({
  test: {
    environment: "jsdom",
    include: ["Scripts/__tests__/**/*.test.ts"],
  },
});
