import { readFileSync } from "node:fs";
import eslint from "@eslint/js";
import { defineConfig } from "eslint/config";
import tseslint from "typescript-eslint";

const sliceTables = ["Alis.Reactive.Assets/runtime", "tests/Alis.Reactive.ArchitectureTests/LintCases/rows"];

const beyondItsModulesIndex = "^(\\.\\./|\\./(?!index(\\.ts)?$))";

const besideTheTable = ".";

const aFile = "[^/]+";

function rowsOf(table) {
  return readFileSync(new URL(table + "/SliceTree.txt", import.meta.url), "utf8")
    .split("\n")
    .map((line) => line.split(/\s+/u).filter((entry) => entry !== ""))
    .filter((row) => row.length > 0);
}

function literal(text) {
  return text.replace(/[.*+?^${}()|[\]\\]/gu, "\\$&");
}

function toTheTable(slice) {
  return slice === besideTheTable ? "./" : "../".repeat(slice.split("/").length);
}

function filesOf(slice, from, slices) {
  if (slice !== besideTheTable) return literal(toTheTable(from) + slice + "/") + aFile;
  const folders = slices.filter((other) => other !== besideTheTable).map(literal);
  return literal(toTheTable(from)) + "(?!(?:" + folders.join("|") + ")$)" + aFile;
}

function ownFilesOf(slice, slices) {
  return slice === besideTheTable ? filesOf(slice, slice, slices) : "\\./" + aFile;
}

function importsOf(from, earlier, slices) {
  const allowed = [ownFilesOf(from, slices), ...earlier.map((slice) => filesOf(slice, from, slices))];
  return "^(?!(?:" + allowed.join("|") + ")$)\\.";
}

function restricted(files, regex, message) {
  return { files, rules: { "no-restricted-imports": ["error", { patterns: [{ regex, message }] }] } };
}

function rowsRule(table) {
  const rows = rowsOf(table);
  const slices = rows.flat();
  const folderOf = (slice) => (slice === besideTheTable ? table : table + "/" + slice);
  return [
    restricted([table + "/**/*.ts"], "^\\.", "This file's folder has no row in its SliceTree.txt, so it imports no file of the table."),
    ...rows.flatMap((row, place) =>
      row.map((slice) =>
        restricted(
          [folderOf(slice) + "/*.ts"],
          importsOf(slice, rows.slice(0, place).flat(), slices),
          "A slice imports its own files and the rows before its own in SliceTree.txt.",
        ),
      ),
    ),
  ];
}

export default defineConfig(
  {
    files: ["Alis.Reactive.Assets/runtime/**/*.ts", "tests/Alis.Reactive.ArchitectureTests/LintCases/**/*.ts"],
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
  ...sliceTables.flatMap(rowsRule),
  restricted(
    ["Alis.Reactive.Assets/runtime/**/*.test.ts", "tests/Alis.Reactive.ArchitectureTests/LintCases/**/*.test.ts"],
    beyondItsModulesIndex,
    "A module's pure test imports its own module's index and nothing else of the runtime.",
  ),
);
