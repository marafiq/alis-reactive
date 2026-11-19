import assert from "node:assert/strict";
import { createServer } from "node:http";
import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const requests = [];
const suppliers = [
  { code: "northbank", name: "Northbank Medical", delivery: "Northbank delivers Monday at 09:30" },
  { code: "harbor", name: "Harbor Mobility", delivery: "Harbor delivers Tuesday at 14:00" }
];
const server = createServer(async (request, response) => {
  if (request.url === "/suppliers") {
    let body = "";
    for await (const part of request) body += part;
    const query = JSON.parse(body);
    const supplier = query.supplier;
    requests.push({ method: request.method, supplier: supplier ?? null });
    const rows = supplier === undefined ? suppliers : suppliers.filter(row => row.code === supplier);
    response.writeHead(200, { "Content-Type": "application/json" });
    response.end(JSON.stringify(query.requiresCounts ? { result: rows, count: rows.length } : rows));
    return;
  }
  response.writeHead(200, { "Content-Type": "text/html" });
  response.end("<!doctype html><input id='local'><input id='supplier'>");
});
await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  const pageErrors = [];
  page.on("pageerror", error => pageErrors.push(error.message));
  await page.goto("http://127.0.0.1:" + server.address().port);
  await page.addScriptTag({ path: require.resolve("@syncfusion/ej2/dist/ej2.min.js") });
  const facts = await page.evaluate(async () => {
    const { ComboBox } = window.ej.dropdowns;
    const { DataManager, UrlAdaptor, Query } = window.ej.data;
    const local = new ComboBox();
    local.appendTo("#local");
    const manager = new DataManager({ url: "/suppliers", adaptor: new UrlAdaptor() });
    const box = new ComboBox({ dataSource: manager, fields: { text: "name", value: "code" } });
    box.appendTo("#supplier");
    const catalogueReady = new Promise(resolve => box.addEventListener("dataBound", resolve));
    box.showPopup();
    await catalogueReady;
    box.hidePopup();
    const received = [];
    for (const code of ["northbank", "harbor"]) {
      const supplied = new Query().addParams("supplier", code);
      const task = box.dataSource.executeQuery(supplied);
      const answer = await task;
      received.push({
        queryIsActualInstance: supplied instanceof Query && answer.query === supplied,
        nativePromise: task instanceof Promise,
        resultIsArray: Object.hasOwn(answer, "result") && Array.isArray(answer.result),
        delivery: answer.result[0].delivery
      });
    }
    return {
      defaultSourceIsEmptyList: Array.isArray(local.dataSource) && local.dataSource.length === 0,
      defaultBoxQueryIsNull: local.query === null,
      registeredManagerIsSameInstance: document.querySelector("#supplier").ej2_instances[0].dataSource === manager && box.dataSource instanceof DataManager,
      received
    };
  });
  assert.equal(facts.defaultSourceIsEmptyList, true);
  assert.equal(facts.defaultBoxQueryIsNull, true);
  assert.equal(facts.registeredManagerIsSameInstance, true);
  assert.deepEqual(facts.received, suppliers.map(row => ({
    queryIsActualInstance: true, nativePromise: true, resultIsArray: true, delivery: row.delivery
  })));
  assert.ok(requests.some(request => request.supplier === null));
  assert.deepEqual(requests.filter(request => request.supplier !== null), suppliers.map(row => ({ method: "POST", supplier: row.code })));
  assert.deepEqual(pageErrors, []);
  console.log(JSON.stringify({ dropdowns: require("@syncfusion/ej2-dropdowns/package.json").version, data: require("@syncfusion/ej2-data/package.json").version, ...facts, requests }, null, 2));
  console.log("ej2-combo-box-manager: a registered DataManager received each supplied Query by identity, then its Promise returned " + facts.received.map(row => row.delivery).join(" and "));
} finally {
  await browser.close();
  await new Promise(resolve => server.close(resolve));
}
