import { defineConfig } from "vitest/config";

// Sandbox runtime tests stay separate from the framework runtime tests in Alis.Reactive.Assets.
export default defineConfig({
  test: {
    environment: "jsdom",
    include: ["Scripts/__tests__/**/*.test.ts"],
  },
});
