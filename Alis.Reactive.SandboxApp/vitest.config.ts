import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "jsdom",
    include: ["Scripts/__tests__/**/*.test.ts"],
  },
});
