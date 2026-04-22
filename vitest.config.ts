import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "jsdom",
    include: [
      "Alis.Reactive.Assets/Scripts/__tests__/**/*.test.ts",
      "Alis.Reactive.SandboxApp/Scripts/__tests__/**/*.test.ts",
    ],
  },
});
