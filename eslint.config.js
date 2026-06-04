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

      // Ban raw String() on value paths. Use toString() from core/coerce instead.
      // Allowed: String(err) for error logging (matched by the err/error variable name).
      "no-restricted-syntax": [
        "error",
        {
          selector: "CallExpression[callee.name='String'][arguments.length=1]:not([arguments.0.name=/^err/]):not([arguments.0.property.name=/^err/])",
          message: "Use toString() from core/coerce instead of raw String(). String(err) for error logging is allowed.",
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
