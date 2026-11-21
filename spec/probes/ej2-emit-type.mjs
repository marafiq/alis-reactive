import { createRequire } from "node:module";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const ts = require("typescript");
const scope = dirname(dirname(require.resolve("@syncfusion/ej2/package.json")));
const loaded = { "ej2-dropdowns": ["DropDownList", "MultiSelect"], "ej2-inputs": ["NumericTextBox"], "ej2-calendars": ["DatePicker"], "ej2-grids": ["Grid"] };
const program = ts.createProgram({
  rootNames: Object.keys(loaded).map((name) => join(scope, name, "index.d.ts")),
  options: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext, moduleResolution: ts.ModuleResolutionKind.Bundler, lib: ["lib.es2022.d.ts", "lib.dom.d.ts", "lib.dom.iterable.d.ts"], strict: true, skipLibCheck: true, noEmit: true },
});
const checker = program.getTypeChecker();

function hidden(node) {
  return (ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected)) !== 0 || ts.getJSDocTags(node).some((tag) => tag.tagName.text === "private");
}

function emitted(member) {
  return (ts.isPropertyDeclaration(member) || ts.isPropertySignature(member)) && member.type !== undefined && ts.isTypeReferenceNode(member.type) && member.type.typeName.getText() === "EmitType";
}

function named(node) {
  let found = false;
  node.forEachChild(function visit(child) {
    if (ts.isTypeReferenceNode(child) && child.typeName.getText() === "T") found = true;
    child.forEachChild(visit);
  });
  return found;
}

function argumentKind(node) {
  const type = checker.getTypeFromTypeNode(node);
  if ((type.flags & ts.TypeFlags.Any) !== 0) return "any";
  if (type.isUnion()) return "a union";
  if (["Object", "object", "{}"].includes(node.getText())) return "Object";
  const declaration = type.getSymbol()?.getDeclarations()?.[0];
  if (declaration === undefined) return "a name the declarations do not resolve";
  return program.isSourceFileDefaultLibrary(declaration.getSourceFile()) ? "a lib type" : "a vendor's named type";
}

const files = program.getSourceFiles().filter((file) => file.fileName.split("\\").join("/").includes("/@syncfusion/"));
const packages = new Set(files.map((file) => file.fileName.split("\\").join("/").split("/@syncfusion/")[1].split("/")[0]));
const kinds = new Map();
const declaredBy = new Map();
let members = 0;
let alias;
for (const file of files) {
  for (const statement of file.statements) {
    if (ts.isTypeAliasDeclaration(statement) && statement.name.text === "EmitType") alias = statement;
    if (!(ts.isClassDeclaration(statement) || ts.isInterfaceDeclaration(statement)) || statement.name === undefined) continue;
    for (const member of statement.members) {
      if (!emitted(member) || hidden(member)) continue;
      members += 1;
      const kind = argumentKind(member.type.typeArguments[0]);
      kinds.set(kind, (kinds.get(kind) ?? 0) + 1);
      if (ts.isClassDeclaration(statement)) declaredBy.set(statement.name.text, (declaredBy.get(statement.name.text) ?? 0) + 1);
    }
  }
}

const five = Object.values(loaded).flat();
const onFive = five.reduce((sum, name) => sum + (declaredBy.get(name) ?? 0), 0);
const dropDownList = files.flatMap((file) => file.statements).find((statement) => ts.isClassDeclaration(statement) && statement.name?.text === "DropDownList");
const change = dropDownList.members.find((member) => member.name?.getText() === "change");
const resolved = checker.getTypeAtLocation(change.name);
const listener = checker.getSignaturesOfType(resolved, ts.SignatureKind.Call)[0];
const where = alias.getSourceFile().fileName.split("\\").join("/").split("/@syncfusion/")[1] + ":" + (alias.getSourceFile().getLineAndCharacterOfPosition(alias.getStart()).line + 1);

console.log(`ej2-emit-type-phantom: EmitType<T> at ${where} names ${named(alias.type) ? "its" : "no"} T in its type`);
console.log(`ej2-emit-type-resolved: DropDownList.change, declared ${change.type.getText()}, resolves to ${checker.typeToString(resolved)}, whose listener's first parameter is ${checker.typeToString(checker.getTypeOfSymbol(listener.getParameters()[0]))}`);
console.log(`ej2-emit-type-members: ${members} public in ${packages.size} packages, ${onFive} declared by ${five.join(", ")}`);
console.log(`ej2-emit-type-arguments: ${[...kinds].sort((left, right) => right[1] - left[1]).map(([kind, count]) => `${count} ${kind}`).join(", ")}`);
