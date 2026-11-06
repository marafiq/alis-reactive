import { createRequire } from "node:module";
const require = createRequire(new URL("../../node_modules/", import.meta.url));
const { JSDOM } = require("jsdom");
const { document } = new JSDOM("<input id=line><textarea id=lines></textarea>").window;
const line = document.getElementById("line");
const lines = document.getElementById("lines");
line.value = "12\n3\r";
lines.value = "12\n3\r";
console.log("html-input-line-breaks: a text input holds " + JSON.stringify(line.value) + "; a textarea holds " + JSON.stringify(lines.value));
