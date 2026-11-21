import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";

const require = createRequire(import.meta.url);
const root = dirname(require.resolve("@syncfusion/ej2-base/package.json"));
const meta = JSON.parse(readFileSync(join(root, "package.json"), "utf8"));
const { JSDOM } = require("jsdom");
const dom = new JSDOM("<!doctype html><html><body></body></html>", { url: "http://localhost/" });
globalThis.window = dom.window;
globalThis.document = dom.window.document;
const { compile } = require(join(root, "dist", "ej2-base.umd.min.js"));

const html = nodes => Array.from(nodes).map(node => node.outerHTML ?? node.textContent).join("");
const shown = html(compile("<b>${name}</b>")({ name: "<img src=x onerror=alert(1)>" }));
const conditional = compile("${if(count > 2)}many${else}few${/if}");
const missing = compile("<b>${resident.name}</b>");
let missingHolder;
try {
  missingHolder = html(missing({}));
} catch (error) {
  missingHolder = error.constructor.name;
}
const rowTemplate = row => [document.createTextNode(String(row.name))];
console.log(`ej2-base: ${meta.version}`);
const rendered = compile(rowTemplate)({ name: "<img src=x onerror=alert(1)>" });
const elementTemplate = () => [document.createElement("em")];
console.log(`ej2-template-function-text-node: a function returning a text node renders ${html(rendered)} as ${rendered[0] && rendered[0].nodeType === 3 ? "text" : "markup"}`);
console.log(`ej2-template-function-elements: a function returning elements renders ${html(compile(elementTemplate)({}))}`);
console.log(`ej2-template-unencoded: ${shown}`);
console.log(`ej2-template-condition: count 3 shows ${html(conditional({ count: 3 }))}; count 1 shows ${html(conditional({ count: 1 }))}`);
console.log(`ej2-template-missing-holder: a member past a missing holder gives ${missingHolder}`);
const entities = { "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" };
globalThis.alisFusionText = value => value == null ? "" : Array.from(String(value), character => entities[character] ?? character).join("");
const encoded = html(compile("<b>${alisFusionText(name)}</b>")({ name: "<img src=x onerror=alert(1)>" }));
console.log(`ej2-template-helper-encodes: a read through a global helper renders ${encoded}`);
const guarded = compile("<b>${if(resident)}${resident.name}${/if}</b>");
console.log(`ej2-template-guarded-holder: a read inside the engine's condition on its holder renders ${html(guarded({}))} for no holder and ${html(guarded({ resident: { name: "Ann" } }))} for one`);
