import { defineConfig } from "vitest/config";

export default defineConfig({
  test: {
    environment: "node",
    env: { TZ: "America/New_York" },
    include: ["Alis.Reactive.Assets/runtime/**/*.test.ts", "Alis.Reactive.OpenTelemetry/**/*.test.ts"],
  },
});
