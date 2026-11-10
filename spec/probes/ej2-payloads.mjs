import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");

const describe = () => {
  const kind = value => {
    if (value === null) return "null";
    if (value === undefined) return "undefined";
    if (value instanceof Date) return "Date";
    if (Array.isArray(value)) return "Array";
    if (typeof value === "function") return "function";
    if (typeof value.then === "function") return "Promise";
    if (value instanceof Element) return "Element";
    return typeof value;
  };
  const shape = args => Object.keys(args).sort().map(key => key + ":" + kind(args[key])).join(",");
  window.heard = {};
  window.listen = (component, events) => events.forEach(event => component.addEventListener(event, args => {
    window.heard[event] = window.heard[event] || shape(args) + (args.action && typeof args.action === "object" ? " | action " + shape(args.action) : "");
  }));
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => !child.id && child.tagName === "DIV").forEach(child => child.remove()));

const click = async (page, selector) => {
  await withoutTrialBanner(page);
  await page.click(selector);
};

const grid = async page => {
  await page.evaluate(() => {
    ej.grids.Grid.Inject(ej.grids.Sort, ej.grids.Page, ej.grids.Toolbar, ej.grids.Selection);
    const residents = [{ id: 1, name: "Ada", ward: "North" }, { id: 2, name: "Rose", ward: "East" }];
    const shown = new ej.grids.Grid({
      dataSource: residents, allowSorting: true, allowPaging: true, pageSettings: { pageSize: 1 },
      toolbar: [{ text: "Discharge", id: "discharge" }],
      columns: [{ field: "id", headerText: "Id", isPrimaryKey: true }, { field: "name", headerText: "Name" }, { field: "ward", headerText: "Ward" }],
    });
    shown.appendTo("#residents");
    listen(shown, ["rowSelected", "recordClick", "toolbarClick", "actionBegin", "actionComplete"]);
    const bound = new ej.grids.Grid({
      dataSource: { result: residents, count: 2 }, allowSorting: true,
      columns: [{ field: "id", headerText: "Id" }, { field: "name", headerText: "Name" }],
    });
    bound.appendTo("#bound");
    listen(bound, ["dataStateChange"]);
  });
  await click(page, "#residents .e-row td >> nth=1");
  await click(page, "#residents .e-toolbar-item >> text=Discharge");
  await click(page, "#residents .e-headercell >> text=Name");
  await click(page, "#bound .e-headercell >> text=Name");
  const heard = await page.evaluate(() => window.heard);
  for (const event of ["rowSelected", "recordClick", "toolbarClick", "actionBegin", "actionComplete", "dataStateChange"])
    console.log(`ej2-payload-grid-${event}: ${heard[event] ?? "not raised"}`);
};

const schedule = async page => {
  await page.evaluate(() => {
    ej.schedule.Schedule.Inject(ej.schedule.Week);
    const shifts = new ej.schedule.Schedule({
      currentView: "Week", selectedDate: new Date(2026, 10, 9), views: ["Week"], height: "400px",
      eventSettings: { dataSource: [{ Id: 1, Subject: "Round", StartTime: new Date(2026, 10, 9, 9), EndTime: new Date(2026, 10, 9, 10) }] },
    });
    shifts.appendTo("#shifts");
    listen(shifts, ["navigating"]);
    window.shifts = shifts;
  });
  await click(page, "#shifts .e-toolbar-item.e-next");
  const heard = await page.evaluate(() => window.heard.navigating);
  console.log(`ej2-payload-schedule-navigating: ${heard ?? "not raised"}`);
  const saved = await page.evaluate(() => {
    const subject = () => window.shifts.eventSettings.dataSource[0].Subject;
    window.shifts.saveEvent({ Id: 1, Subject: "Rounds by text", StartTime: new Date(2026, 10, 9, 9), EndTime: new Date(2026, 10, 9, 10) }, 1);
    const byNumber = subject();
    window.shifts.saveEvent({ Id: 1, Subject: "Rounds by text", StartTime: new Date(2026, 10, 9, 9), EndTime: new Date(2026, 10, 9, 10) }, "Save");
    return `with 1 the subject is ${byNumber}; with 'Save' it is ${subject()}`;
  });
  console.log(`ej2-schedule-save-event-action: saveEvent ${saved}`);
};

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({ contentType: "text/html", body: '<!doctype html><div id="residents"></div><div id="bound"></div><div id="shifts"></div>' }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.evaluate(describe);
  await grid(page);
  await schedule(page);
  await browser.close();
})();
