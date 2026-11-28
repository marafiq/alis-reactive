import { createRequire } from "node:module";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { JSDOM } from "jsdom";

const require = createRequire(import.meta.url);
const { window } = new JSDOM("");
globalThis.window = window;
globalThis.document = window.document;
const root = dirname(require.resolve("@syncfusion/ej2-data/package.json"));
const meta = JSON.parse(readFileSync(join(root, "package.json"), "utf8"));
const { DataManager } = require(join(root, "dist", "ej2-data.umd.min.js"));

const rows = [{ id: 1, name: "Ann" }];
const manager = new DataManager(rows);
manager.insert({ id: 2, name: "Bob" });
manager.update("id", { id: 1, name: "Ana" });
console.log("ej2-data-manager-edits-source: ej2-data " + meta.version + " leaves the array it was given holding " + rows.length + " rows, the first named " + rows[0].name);

const sent = { items: [{ id: 1, name: "Ann" }] };
const copy = structuredClone(sent);
copy.items.push({ id: 2, name: "Bob" });
copy.items[0].name = "Ana";
console.log("js-structured-clone-separate: after the copy is edited, the original holds " + sent.items.length + " row named " + sent.items[0].name);
