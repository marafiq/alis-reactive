import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const ts = require("typescript");
const probeFile = "dom-iterables-probe.ts";
const probeText = "declare const element: Element;\nexport const children = element.children;\nexport const classList = element.classList;\n";
const options = { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext, lib: ["lib.es2023.d.ts", "lib.dom.d.ts", "lib.dom.iterable.d.ts"], strict: true, noEmit: true };
const host = ts.createCompilerHost(options);
const readLibrary = host.getSourceFile.bind(host);
host.getSourceFile = (name, language) => (name === probeFile ? ts.createSourceFile(name, probeText, language) : readLibrary(name, language));
const program = ts.createProgram({ rootNames: [probeFile], options, host });
const checker = program.getTypeChecker();
const source = program.getSourceFile(probeFile);

function exported(name) {
  const declaration = source.statements.flatMap((statement) => (ts.isVariableStatement(statement) ? [...statement.declarationList.declarations] : [])).find((each) => each.name.getText(source) === name);
  return checker.getTypeAtLocation(declaration.name);
}

function iterated(type) {
  const iterator = type.getProperties().find((property) => property.escapedName.startsWith("__@iterator"));
  if (iterator === undefined) return "nothing";
  const returned = checker.getTypeOfSymbol(iterator).getCallSignatures()[0].getReturnType();
  return checker.typeToString(checker.getTypeArguments(returned)[0]);
}

function describe(member) {
  const type = exported(member);
  const absent = ["filter", "map", "some", "find"].filter((name) => type.getProperty(name) === undefined);
  return "Element's " + member + " is " + checker.typeToString(type) + ", an array " + checker.isArrayType(type) + ", lacking " + absent.join(", ") + ", and its [Symbol.iterator] yields " + iterated(type);
}

console.log("dom-children-iterable: " + describe("children"));
console.log("dom-classlist-iterable: " + describe("classList"));
const text = "a\u{1F600}";
const paged = { result: [], count: 0 };
console.log("js-array-from-text: Array.from of a text with one astral character holds " + Array.from(text).length + " items where its length is " + text.length + "; Symbol.iterator in Object(text) is " + (Symbol.iterator in Object(text)) + ", and in a paged result is " + (Symbol.iterator in paged));
console.log("typescript-version: " + ts.version);
