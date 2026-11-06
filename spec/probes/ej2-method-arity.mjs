import { createRequire } from "node:module";
import { readdirSync, readFileSync, statSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const ts = require("typescript");
const scope = dirname(dirname(require.resolve("@syncfusion/ej2/package.json")));
const meta = JSON.parse(readFileSync(join(scope, "ej2", "package.json"), "utf8"));

function declarations(folder) {
  const found = [];
  for (const name of readdirSync(folder)) {
    const path = join(folder, name);
    if (statSync(path).isDirectory()) found.push(...declarations(path));
    else if (name.endsWith(".d.ts")) found.push(path);
  }
  return found;
}

function hidden(node) {
  return ts.getJSDocTags(node).some(tag => tag.tagName.text === "private" || tag.tagName.text === "hidden");
}

function restricted(node) {
  return (ts.getCombinedModifierFlags(node) & (ts.ModifierFlags.Private | ts.ModifierFlags.Protected)) !== 0;
}

let largest = { arity: -1, where: "" };
let methods = 0;
let pastFour = 0;
for (const name of readdirSync(scope)) {
  const source = join(scope, name, "src");
  if (!name.startsWith("ej2") || !statSync(join(scope, name)).isDirectory()) continue;
  let files;
  try { files = declarations(source); } catch { continue; }
  for (const file of files) {
    const tree = ts.createSourceFile(file, readFileSync(file, "utf8"), ts.ScriptTarget.Latest, true);
    tree.forEachChild(function visit(node) {
      if (ts.isClassDeclaration(node) && node.name) {
        for (const member of node.members) {
          if (!ts.isMethodDeclaration(member) || restricted(member) || hidden(member)) continue;
          methods += 1;
          const arity = member.parameters.length;
          if (arity > 4) pastFour += 1;
          if (arity > largest.arity) largest = { arity, where: `${name} ${node.name.text}.${member.name.getText(tree)}` };
        }
      }
      ts.forEachChild(node, visit);
    });
  }
}

console.log(`ej2 meta-package: ${meta.version}`);
console.log(`ej2 documented-public methods: ${methods}`);
console.log(`ej2-max-public-method-arity: ${largest.arity}`);
console.log(`ej2 method at the largest arity: ${largest.where}`);
console.log(`ej2-methods-past-four-arguments: ${pastFour}`);
