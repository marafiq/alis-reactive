import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "jsdom",
    include: ["runtime/__tests__/**/*.test.ts"],
  },
});
