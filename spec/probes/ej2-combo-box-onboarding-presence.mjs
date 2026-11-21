import assert from "node:assert/strict";
import { createRequire } from "node:module";
import { createServer } from "node:http";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const server = createServer((_request, response) => {
  response.writeHead(200, { "content-type": "text/html" });
  response.end('<body style="padding: 160px 100px"><input id="choice" aria-label="Choice"><button id="clear">Clear choice</button><input id="amount" aria-label="Amount"><button id="done">Done</button>');
});
await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const browser = await chromium.launch();
try {
  const page = await browser.newPage();
  const pageErrors = [];
  page.on("pageerror", error => pageErrors.push(error.message));
  await page.goto("http://127.0.0.1:" + server.address().port);
  await page.addScriptTag({ path: require.resolve("@syncfusion/ej2/dist/ej2.min.js") });
  await page.addStyleTag({ path: require.resolve("@syncfusion/ej2-inputs/styles/material.css") });
  await page.addStyleTag({ path: require.resolve("@syncfusion/ej2-dropdowns/styles/material.css") });
  page.setDefaultTimeout(8000);
  await page.evaluate(() => {
    const shape = (args, names) => Object.fromEntries(names.map(key => {
      if (!Object.hasOwn(args, key)) return [key, { presence: "absent" }];
      const value = args[key];
      if (value === undefined) return [key, { presence: "own undefined" }];
      return [key, value instanceof Element ? { element: value.constructor.name, text: value.textContent } :
        value instanceof Event ? value.constructor.name : value];
    }));
    window.changes = [];
    window.amountChanges = [];
    const box = new ej.dropdowns.ComboBox({
      fields: { text: "label", value: "code" }, allowCustom: false, showClearButton: true,
      dataSource: [{ label: "First", code: "one", support: { flag: false } }, { label: "Second", code: "two", support: { flag: true } }]
    });
    box.appendTo("#choice");
    window.queryDefault = box.query;
    document.querySelector("#clear").addEventListener("click", () => { box.clear(); box.dataBind(); });
    box.addEventListener("change", args => window.changes.push(shape(args, ["value", "itemData", "previousItemData", "item", "previousItem", "cancel", "isInteracted", "element", "event", "e"])));
    const amount = new ej.inputs.NumericTextBox();
    amount.appendTo("#amount");
    window.amountDefault = amount.value;
    amount.addEventListener("change", args => window.amountChanges.push(shape(args, ["value", "previousValue", "isInteracted", "event", "name"])));
  });
  const amount = page.locator("#amount");
  await amount.click();
  await page.keyboard.type("17");
  await page.keyboard.press("Tab");
  await amount.click({ clickCount: 3 });
  await page.keyboard.press("Backspace");
  await page.keyboard.press("Tab");
  const choice = page.getByRole("combobox", { name: "Choice" });
  await choice.focus();
  await page.keyboard.press("Alt+ArrowDown");
  await page.getByRole("listbox").getByRole("option", { name: "First" }).waitFor();
  await page.keyboard.press("ArrowDown");
  await page.keyboard.press("Enter");
  const selected = await page.evaluate(() => ({ selection: document.querySelector("#choice").ej2_instances[0].value, changes: window.changes }));
  await page.getByRole("button", { name: "Clear choice" }).click();
  const queryObservation = await page.evaluate(async () => {
    const manager = new ej.data.DataManager([{ id: 7, dispatch: { note: "Carry upstairs" } }]);
    const query = new ej.data.Query().using(manager);
    const host = document.createElement("input");
    document.body.append(host);
    new ej.dropdowns.ComboBox({ dataSource: manager, query, fields: { text: "id", value: "id" } }).appendTo(host);
    const box = host.ej2_instances[0];
    const call = box.query.execute();
    const envelope = await call;
    return { sameManager: box.dataSource === manager, sameQuery: box.query === query,
      returnedPromise: call instanceof Promise, rows: envelope.result };
  });
  const seen = await page.evaluate(() => ({ changes: window.changes, queryDefault: window.queryDefault, amountDefault: window.amountDefault, amountChanges: window.amountChanges }));
  seen.query = queryObservation;
  console.log(JSON.stringify(seen, null, 2));
  assert.equal(seen.query.sameManager, true);
  assert.equal(seen.query.sameQuery, true);
  assert.equal(seen.query.returnedPromise, true);
  assert.equal(seen.query.rows[0].dispatch.note, "Carry upstairs");
  const selectedRow = seen.changes.find(event => event.itemData?.code);
  const cleared = seen.changes.find(event => event.value === null);
  assert.ok(selectedRow);
  assert.equal(cleared.itemData, null);
  assert.equal(cleared.previousItemData.code, selectedRow.itemData.code);
  assert.equal(seen.queryDefault, null);
  assert.equal(seen.amountDefault, null);
  assert.equal(seen.amountChanges[0].value, 17);
  assert.equal(seen.amountChanges[1].value, null);
  assert.deepEqual(pageErrors, []);
} finally {
  await browser.close();
  await new Promise(resolve => server.close(resolve));
}
