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
    ignores: [
      "Alis.Reactive.SandboxApp/Scripts/core/coerce.ts",         // coerce.ts IS the implementation
      "Alis.Reactive.SandboxApp/Scripts/components/lab/**",       // lab test components are exempt
      "Alis.Reactive.SandboxApp/Scripts/__tests__/**",            // test files are exempt
    ],
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

      // -- Coerce module enforcement --
      // Ban raw String() on value paths. Use toString() from core/coerce instead.
      // Allowed: String(err) for error logging (matched by the err/error variable name).
      // coerce.ts itself is excluded via ignores above.
      "no-restricted-syntax": [
        "error",
        {
          selector: "CallExpression[callee.name='String'][arguments.length=1]:not([arguments.0.name=/^err/]):not([arguments.0.property.name=/^err/])",
          message: "Use toString() from core/coerce instead of raw String(). String(err) for error logging is allowed.",
        },
      ],
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
