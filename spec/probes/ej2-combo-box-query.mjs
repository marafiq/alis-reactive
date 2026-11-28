import assert from "node:assert/strict";
import { createServer } from "node:http";
import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const stock = [
  { readyForCollection: false, dispatch: { instruction: "Chair W-09 is being cleaned" } },
  { readyForCollection: true, dispatch: { instruction: "Collect chair W-14 from North stores" } },
];
let request;
const server = createServer(async (req, res) => {
  if (req.url === "/stock") {
    let body = "";
    for await (const piece of req) body += piece;
    request = { method: req.method, data: JSON.parse(body) };
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(stock));
    return;
  }
  res.writeHead(200, { "Content-Type": "text/html" });
  res.end("<!doctype html><input id='aid' aria-label='Transport aid'>");
});
await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  const pageErrors = [];
  page.on("pageerror", error => pageErrors.push(error.message));
  await page.goto("http://127.0.0.1:" + server.address().port);
  await page.addScriptTag({ path: bundle });
  const observed = await page.evaluate(async () => {
    const { Query, DataManager, UrlAdaptor } = window.ej.data;
    const box = new window.ej.dropdowns.ComboBox({ dataSource: ["Wheelchair", "Walking frame"], value: "Wheelchair" });
    box.appendTo("#aid");
    const defaultQuery = box.query;
    const manager = new DataManager({ url: "/stock", adaptor: new UrlAdaptor() });
    const query = new Query().using(manager).addParams("aid", () => box.value);
    box.setProperties({ query }, true);
    const promise = box.query.execute();
    const settled = await promise;
    return {
      defaultQuery,
      queryIsConfiguredInstance: box.query === query && box.query instanceof Query,
      nativePromise: promise instanceof Promise,
      resultPresent: Object.hasOwn(settled, "result"),
      resultIsArray: Array.isArray(settled.result),
      result: settled.result
    };
  });
  assert.equal(observed.defaultQuery, null);
  assert.equal(observed.queryIsConfiguredInstance, true);
  assert.equal(observed.nativePromise, true);
  assert.equal(observed.resultPresent, true);
  assert.equal(observed.resultIsArray, true);
  assert.deepEqual(observed.result, stock);
  assert.equal(request.method, "POST");
  assert.equal(request.data.aid, "Wheelchair");
  assert.deepEqual(pageErrors, []);
  console.log(JSON.stringify({ dropdowns: require("@syncfusion/ej2-dropdowns/package.json").version, data: require("@syncfusion/ej2-data/package.json").version, ...observed, requestedAid: request.data.aid }, null, 2));
} finally {
  await browser.close();
  await new Promise(resolve => server.close(resolve));
}
