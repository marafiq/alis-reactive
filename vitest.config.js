import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "node",
    include: ["Alis.Reactive.Assets/runtime/**/*.test.ts"],
  },
});
