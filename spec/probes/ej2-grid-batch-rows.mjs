import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => child.tagName === "DIV" && (child.style.zIndex === "999999999" || child.attributes.length === 0 && child.firstElementChild?.style.zIndex === "99999")).forEach(child => child.remove()));

const held = page => page.evaluate(() => {
  const kind = value => value === null ? "null" : value instanceof Date ? "Date " + value.toISOString() : Array.isArray(value) ? "Array" : typeof value + " " + JSON.stringify(value);
  const row = value => (Object.getPrototypeOf(value) === Object.prototype ? "plain" : "not plain") + " {" + Object.keys(value).sort().map(key => key + ":" + kind(value[key])).join(",") + "}";
  const changes = window.billing.getBatchChanges();
  return "added " + changes.addedRecords.length + ", deleted " + changes.deletedRecords.length + ", changed [" + changes.changedRecords.map(row).join(" ") + "]";
});

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles", locale: "en-US" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({ contentType: "text/html", body: '<!doctype html><div id="billing"></div>' }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.evaluate(() => {
    ej.grids.Grid.Inject(ej.grids.Edit);
    window.billing = new ej.grids.Grid({
      dataSource: ej.data.DataUtil.parse.isJson([{ residentId: 1, name: "Ada", dueOn: "2026-11-10T00:00:00", amount: 12.5 }, { residentId: 2, name: "Rose", dueOn: "2026-11-11T00:00:00", amount: 40 }]),
      editSettings: { allowEditing: true, mode: "Batch", showConfirmDialog: false },
      columns: [
        { field: "residentId", headerText: "Resident id", isPrimaryKey: true, visible: false },
        { field: "name", headerText: "Resident" },
        { field: "dueOn", headerText: "Due on", type: "date", format: "yMd", editType: "datepickeredit" },
        { field: "amount", headerText: "Amount", type: "number", editType: "numericedit" },
      ],
      dataBound: () => { window.bound = true; },
    });
    window.billing.appendTo("#billing");
  });
  await page.waitForFunction(() => window.bound === true, null, { timeout: 10000 });
  await withoutTrialBanner(page);
  console.log("ej2-grid-batch-row-start: the builder's isJson holds dueOn " + await page.evaluate(() => {
    const due = window.billing.dataSource[0].dueOn;
    return due instanceof Date ? "as a Date at " + due.toISOString() : "as " + typeof due;
  }));
  await page.dblclick("#billing .e-row >> nth=0 >> td >> nth=3");
  await page.waitForFunction(() => document.activeElement?.matches("#billing td.e-editedbatchcell input.e-field") && document.activeElement.value === "12.5", null, { timeout: 10000 });
  await page.fill("#billing td.e-editedbatchcell input.e-field", "");
  await page.locator("#billing td.e-editedbatchcell input.e-field").pressSequentially("20.25");
  await page.keyboard.press("Enter");
  console.log("ej2-grid-batch-row-number-edit: after Amount is typed 20.25, " + await held(page));
  await page.dblclick("#billing .e-row >> nth=1 >> td >> nth=2");
  await page.waitForFunction(() => document.activeElement?.matches("#billing td.e-editedbatchcell input.e-field") && document.activeElement.value === "11/11/2026", null, { timeout: 10000 });
  await page.fill("#billing td.e-editedbatchcell input.e-field", "");
  await page.locator("#billing td.e-editedbatchcell input.e-field").pressSequentially("11/20/2026");
  await page.keyboard.press("Enter");
  console.log("ej2-grid-batch-row-date-edit: after Rose's Due on is typed 11/20/2026, " + await held(page));
  await browser.close();
})();
