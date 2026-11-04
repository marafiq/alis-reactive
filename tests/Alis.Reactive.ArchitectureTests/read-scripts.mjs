import { readFileSync } from "node:fs";
import ts from "typescript";

const files = readFileSync(0, "utf8").split("\n").filter((file) => file.length > 0);
const printer = ts.createPrinter({ removeComments: true });
const identifiers = new Set();
const code = [];

for (const file of files) {
  const kind = file.endsWith(".ts") ? ts.ScriptKind.TS : ts.ScriptKind.JS;
  const source = ts.createSourceFile(file, readFileSync(file, "utf8"), ts.ScriptTarget.Latest, true, kind);
  const collect = (node) => {
    if (ts.isIdentifier(node) || ts.isPrivateIdentifier(node)) identifiers.add(node.text);
    ts.forEachChild(node, collect);
  };
  collect(source);
  code.push(printer.printFile(source));
}

process.stdout.write(JSON.stringify({ identifiers: [...identifiers], code }));
