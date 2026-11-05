import { readFileSync } from "node:fs";
import ts from "typescript";

const files = readFileSync(0, "utf8").split("\n").filter((file) => file.length > 0);
const printer = ts.createPrinter({ removeComments: true });
const identifiers = new Set();
const code = [];
const comments = [];

for (const file of files) {
  const text = readFileSync(file, "utf8");
  const kind = file.endsWith(".ts") ? ts.ScriptKind.TS : ts.ScriptKind.JS;
  const source = ts.createSourceFile(file, text, ts.ScriptTarget.Latest, true, kind);
  const commentStarts = new Set();
  const collect = (node) => {
    if (ts.isIdentifier(node) || ts.isPrivateIdentifier(node)) identifiers.add(node.text);
    for (const range of ts.getLeadingCommentRanges(text, node.getFullStart()) ?? []) commentStarts.add(range.pos);
    for (const range of ts.getTrailingCommentRanges(text, node.getEnd()) ?? []) commentStarts.add(range.pos);
    for (const child of node.getChildren(source)) collect(child);
  };
  collect(source);
  for (const start of commentStarts) comments.push(`${file}:${source.getLineAndCharacterOfPosition(start).line + 1}`);
  code.push(printer.printFile(source));
}

process.stdout.write(JSON.stringify({ identifiers: [...identifiers], code, comments }));
