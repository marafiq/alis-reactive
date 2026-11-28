import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const ts = require("typescript");
const scope = dirname(dirname(require.resolve("@syncfusion/ej2/package.json")));
const roots = ["ej2-dropdowns", "ej2-inputs", "ej2-calendars", "ej2-grids"];
const program = ts.createProgram({
  rootNames: roots.map((name) => join(scope, name, "index.d.ts")),
  options: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext, moduleResolution: ts.ModuleResolutionKind.Bundler, lib: ["lib.es2022.d.ts", "lib.dom.d.ts", "lib.dom.iterable.d.ts"], strict: true, skipLibCheck: true, noEmit: true },
});
const checker = program.getTypeChecker();

function hidden(node) {
  return (ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected)) !== 0 || ts.getJSDocTags(node).some((tag) => tag.tagName.text === "private");
}

function defaultTag(member) {
  return ts.getJSDocTags(member).find((tag) => tag.tagName.text === "default" && ts.getTextOfJSDocComment(tag.comment)?.trim() === "null");
}

function admitsNull(node) {
  const type = checker.getTypeFromTypeNode(node);
  return (type.isUnion() ? type.types : [type]).some((part) => (part.flags & (ts.TypeFlags.Null | ts.TypeFlags.Undefined | ts.TypeFlags.Any | ts.TypeFlags.Unknown)) !== 0);
}

function packageOf(file) {
  return file.fileName.split("\\").join("/").split("/@syncfusion/")[1];
}

const files = program.getSourceFiles().filter((file) => file.fileName.split("\\").join("/").includes("/@syncfusion/"));
const packages = new Set(files.map((file) => packageOf(file).split("/")[0]));
const documented = new Map();
let underStated = 0;
for (const file of files) {
  for (const statement of file.statements) {
    if (!ts.isClassDeclaration(statement) || statement.name === undefined) continue;
    for (const member of statement.members) {
      if (!ts.isPropertyDeclaration(member) || member.type === undefined || hidden(member)) continue;
      const tag = defaultTag(member);
      if (tag === undefined || admitsNull(member.type)) continue;
      if ((ts.getCombinedModifierFlags(member) & ts.ModifierFlags.Readonly) === 0) underStated += 1;
      const line = file.getLineAndCharacterOfPosition(tag.getStart()).line + 1;
      documented.set(statement.name.text + "." + member.name.getText(), `${member.type.getText()} at @syncfusion/${packageOf(file)}:${line}`);
    }
  }
}

function version(name) {
  return JSON.parse(readFileSync(join(scope, name, "package.json"), "utf8")).version;
}

console.log(`ej2-numerictextbox-value-default-null: NumericTextBox.value documents @default null, typed ${documented.get("NumericTextBox.value")} (ej2-inputs ${version("ej2-inputs")})`);
console.log(`ej2-datepicker-value-default-null: DatePicker.value documents @default null, typed ${documented.get("DatePicker.value")} (ej2-calendars ${version("ej2-calendars")})`);
console.log(`ej2-default-null-under-stated: ${underStated} public writable class properties in ${packages.size} packages document @default null and are typed with no null`);
