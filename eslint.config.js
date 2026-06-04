import eslint from "@eslint/js";
import tseslint from "typescript-eslint";

export default tseslint.config(
  {
    ignores: [
      "node_modules/",
      "**/wwwroot/",
      "**/bin/",
      "**/obj/",
      "**/__experiments__/",
      "Alis.Reactive.Assets/dist/",
    ],
  },

  eslint.configs.recommended,

  // TypeScript: recommended (type-aware off for now — keep it fast)
  ...tseslint.configs.recommended,

  {
    files: ["Alis.Reactive.Assets/runtime/**/*.ts"],
    ignores: [
      "Alis.Reactive.Assets/runtime/__tests__/**", // test files relaxed below
    ],
    rules: {
      "no-fallthrough": "error",
      "no-var": "error",
      "eqeqeq": ["error", "always", { null: "ignore" }],

      "@typescript-eslint/no-unused-vars": [
        "error",
        { argsIgnorePattern: "^_", varsIgnorePattern: "^_" },
      ],

      "@typescript-eslint/no-explicit-any": "warn",

      "no-restricted-syntax": [
        "error",
        {
          selector: "CallExpression[callee.name='String'][arguments.length=1]",
          message: "Use shape conversion for plan values or toJavaScriptString() at runtime boundaries.",
        },
      ],
    },
  },

  {
    files: ["Alis.Reactive.SandboxApp/Scripts/**/*.ts"],
    ignores: [
      "Alis.Reactive.SandboxApp/Scripts/__tests__/**",
    ],
    rules: {
      "no-fallthrough": "error",
      "no-var": "error",
      "eqeqeq": ["error", "always", { null: "ignore" }],
      "@typescript-eslint/no-unused-vars": [
        "error",
        { argsIgnorePattern: "^_", varsIgnorePattern: "^_" },
      ],
      "@typescript-eslint/no-explicit-any": "warn",
    },
  },

  {
    files: [
      "Alis.Reactive.Assets/runtime/__tests__/**/*.ts",
      "Alis.Reactive.SandboxApp/Scripts/__tests__/**/*.ts",
    ],
    rules: {
      "@typescript-eslint/no-explicit-any": "off",
    },
  },
);
