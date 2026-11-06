import { createRequire } from "node:module";
import { readdirSync, readFileSync, statSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const ts = require("typescript");
const scope = dirname(dirname(require.resolve("@syncfusion/ej2/package.json")));

function declarations(folder) {
  const found = [];
  for (const name of readdirSync(folder)) {
    const path = join(folder, name);
    if (statSync(path).isDirectory()) found.push(...declarations(path));
    else if (name.endsWith(".d.ts")) found.push(path);
  }
  return found;
}

function eventArgs(file) {
  const source = ts.createSourceFile(file, readFileSync(file, "utf8"), ts.ScriptTarget.Latest, true);
  return source.statements.filter(node => ts.isInterfaceDeclaration(node) && node.name.text.endsWith("EventArgs"));
}

function fields(node) {
  return node.members.filter(member => ts.isPropertySignature(member));
}

const packages = ["ej2-grids", "ej2-dropdowns", "ej2-calendars", "ej2-popups"];
for (const name of packages) {
  let optional = 0;
  let required = 0;
  for (const file of declarations(join(scope, name, "src"))) {
    for (const node of eventArgs(file)) {
      for (const field of fields(node)) {
        if (field.questionToken) optional++;
        else required++;
      }
    }
  }
  console.log(`ej2-event-args-presence-marks: ${name} ${optional} optional ${required} required`);
}

const base = join(scope, "ej2-dropdowns", "src", "drop-down-base", "drop-down-base.d.ts");
const filtering = eventArgs(base).find(node => node.name.text === "FilteringEventArgs");
const members = filtering.members.map(member => {
  const named = member.name.getText();
  if (ts.isMethodSignature(member)) return named + "()";
  return named + (member.questionToken ? "?" : "");
});
console.log("ej2-filtering-args-members: " + members.join(", "));
