import { createRequire } from "node:module";
import { pathToFileURL } from "node:url";

const require = createRequire(new URL("../../node_modules/", import.meta.url));
const { parse } = await import(pathToFileURL(require.resolve("parse5")).href);

const elementsNamed = (node, name) => [
  ...(node.nodeName === name ? [node] : []),
  ...(node.childNodes ?? []).flatMap(child => elementsNamed(child, name)),
];

const scriptText = document => elementsNamed(document, "script")[0].childNodes.map(child => child.value).join("");

const name = "</script><b>Ada</b>";
const raw = JSON.stringify({ name });
const escaped = raw.replace(/</g, "\\u003C").replace(/>/g, "\\u003E").replace(/&/g, "\\u0026");
const rawDocument = parse(`<script type="application/json">${raw}</script>`);
const escapedDocument = parse(`<script type="application/json">${escaped}</script>`);

console.log("html-script-text-ends-at-close-tag: a script whose text holds " + raw
  + " ends at its </script>, its text " + JSON.stringify(scriptText(rawDocument))
  + ", and the rest parses as markup with " + elementsNamed(rawDocument, "b").length + " b element"
  + "; with <, > and & written as \\u003C, \\u003E and \\u0026 the text is the whole JSON " + (scriptText(escapedDocument) === escaped)
  + ", no b element parses (" + elementsNamed(escapedDocument, "b").length + ")"
  + " and JSON.parse reads the name back " + (JSON.parse(scriptText(escapedDocument)).name === name));
