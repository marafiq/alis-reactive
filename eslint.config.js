import eslint from "@eslint/js";
import tseslint from "typescript-eslint";

export default tseslint.config(
  // Global ignores
  {
    ignores: [
      "node_modules/",
      "**/wwwroot/",
      "**/bin/",
      "**/obj/",
      "**/__experiments__/",
    ],
  },

  // Base: eslint recommended
  eslint.configs.recommended,

  // TypeScript: recommended (type-aware off for now — keep it fast)
  ...tseslint.configs.recommended,

  // Project-specific overrides for source files
  {
    files: ["Alis.Reactive.SandboxApp/Scripts/**/*.ts"],
    rules: {
      // -- Bug catchers (errors) --
      "no-fallthrough": "error",
      "no-var": "error",
      "eqeqeq": ["error", "always", { null: "ignore" }],

      // -- TypeScript bug catchers --
      "@typescript-eslint/no-unused-vars": [
        "error",
        { argsIgnorePattern: "^_", varsIgnorePattern: "^_" },
      ],

      // -- Warnings (tighten later per-module) --
      "@typescript-eslint/no-explicit-any": "warn",
    },
  },

  // Test files: relax some rules
  {
    files: ["Alis.Reactive.SandboxApp/Scripts/__tests__/**/*.ts"],
    rules: {
      "@typescript-eslint/no-explicit-any": "off",
    },
  },
);
