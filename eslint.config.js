import { readdirSync, readFileSync } from "node:fs";
import { posix } from "node:path";
import eslint from "@eslint/js";
import { defineConfig } from "eslint/config";
import tseslint from "typescript-eslint";

const sliceTables = [
  "tests/Alis.Reactive.Conformance.App",
  "Alis.Reactive.Assets/runtime",
  "tests/Alis.Reactive.ArchitectureTests/LintCases/rows",
  "tests/Alis.Reactive.ArchitectureTests/LintCases/package",
  "Alis.Reactive.Native",
];

const beyondItsModulesIndex = "^(\\.\\./|\\./(?!index(\\.ts)?$))";

const besideTheTable = ".";

const findsARootsPlans = "Alis.Reactive.Assets/runtime/Execution/plan-scripts.ts";

const aWideQuery =
  "CallExpression[callee.type='MemberExpression'][callee.property.name=/^(querySelector|querySelectorAll|getElementsByClassName|getElementsByTagName|getElementsByName|closest|matches)$/]";

const aFile = "[^/]+";

const eachFolder = "*";

function rowsOf(table) {
  const rows = readFileSync(new URL(table + "/SliceTree.txt", import.meta.url), "utf8")
    .split("\n")
    .map((line) => line.split(/\s+/u).filter((entry) => entry !== ""))
    .filter((row) => row.length > 0);
  return rows.flat().includes(besideTheTable) ? rows : [...rows, [besideTheTable]];
}

function literal(text) {
  return text.replace(/[.*+?^${}()|[\]\\]/gu, "\\$&");
}

function foldersUnder(table, folder) {
  return readdirSync(new URL(table + "/" + folder + "/", import.meta.url), { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => entry.name);
}

function foldersOf(table, slice) {
  return slice
    .split("/")
    .reduce(
      (folders, segment) =>
        folders.flatMap((folder) =>
          segment === eachFolder ? foldersUnder(table, folder).map((under) => posix.join(folder, under)) : [posix.join(folder, segment)],
        ),
      [besideTheTable],
    );
}

function filesOf(table, folder, from) {
  const relative = posix.relative(from, folder) || besideTheTable;
  const prefix = literal(relative.startsWith(".") ? relative : "./" + relative) + "/";
  const folders = foldersUnder(table, folder).map(literal);
  return folders.length === 0 ? prefix + aFile : prefix + "(?!(?:" + folders.join("|") + ")$)" + aFile;
}

function importsOf(table, from, earlier) {
  const allowed = [from, ...earlier].map((folder) => filesOf(table, folder, from));
  return "^(?!(?:" + allowed.join("|") + ")$)\\.";
}

function restricted(files, regex, message) {
  return { files, rules: { "no-restricted-imports": ["error", { patterns: [{ regex, message }] }] } };
}

function rowsRule(table) {
  const rows = rowsOf(table).map((row) => row.flatMap((slice) => foldersOf(table, slice)));
  return [
    restricted([table + "/**/*.ts"], "^\\.", "This file's folder has no row in its SliceTree.txt, so it imports no file of the table."),
    ...rows.flatMap((row, place) =>
      row.map((folder) =>
        restricted(
          [posix.join(table, folder) + "/*.ts"],
          importsOf(table, folder, rows.slice(0, place).flat()),
          "A slice imports its own files and the rows before its own in SliceTree.txt.",
        ),
      ),
    ),
  ];
}

export default defineConfig(
  {
    files: [
      "tests/Alis.Reactive.Conformance.App/**/*.ts",
      "Alis.Reactive.Assets/runtime/**/*.ts",
      "Alis.Reactive.OpenTelemetry/**/*.ts",
      "Alis.Reactive.Native/**/*.ts",
      "tests/Alis.Reactive.ArchitectureTests/LintCases/**/*.ts",
    ],
    extends: [eslint.configs.recommended, tseslint.configs.strictTypeChecked, tseslint.configs.stylisticTypeChecked],
    linterOptions: { noInlineConfig: true },
    languageOptions: { parserOptions: { projectService: true, tsconfigRootDir: import.meta.dirname } },
    rules: {
      "@typescript-eslint/switch-exhaustiveness-check": "error",
      "@typescript-eslint/strict-boolean-expressions": [
        "error",
        {
          allowString: false,
          allowNumber: false,
          allowNullableObject: false,
          allowNullableBoolean: false,
          allowNullableString: false,
          allowNullableNumber: false,
          allowNullableEnum: false,
          allowAny: false,
        },
      ],
      eqeqeq: ["error", "always"],
    },
  },
  {
    files: ["Alis.Reactive.Assets/runtime/**/*.ts", "tests/Alis.Reactive.ArchitectureTests/LintCases/**/*.ts"],
    ignores: [findsARootsPlans],
    rules: {
      "no-restricted-syntax": ["error", { selector: aWideQuery, message: "An element is found by its id: what the page needs to know, the plan carries." }],
    },
  },
  ...sliceTables.flatMap(rowsRule),
  restricted(
    ["Alis.Reactive.Assets/runtime/**/*.test.ts", "tests/Alis.Reactive.ArchitectureTests/LintCases/**/*.test.ts"],
    beyondItsModulesIndex,
    "A module's pure test imports its own module's index and nothing else of the runtime.",
  ),
);
