import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const theme = require.resolve("@syncfusion/ej2/material.css");

const render = (page, kind) => page.evaluate(kind => {
  const host = document.createElement("input");
  host.id = kind;
  document.getElementById("probe").appendChild(host);
  if (kind === "range") {
    const picker = new ej.calendars.DateRangePicker({ startDate: new Date(2026, 6, 14), endDate: new Date(2026, 6, 20), format: "yyyy-MM-dd" });
    picker.appendTo(host);
    return host.value;
  }
  if (kind === "time") {
    const picker = new ej.calendars.TimePicker({ value: new Date(2026, 10, 10, 14, 30) });
    picker.appendTo(host);
    return host.value;
  }
  if (kind === "numeric") {
    const box = new ej.inputs.NumericTextBox({ value: 12.5 });
    box.appendTo(host);
    return host.value;
  }
  const picker = new ej.inputs.ColorPicker({ value: "#ff0000ff" });
  picker.appendTo(host);
  return String(picker.value);
}, kind);

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ locale: "en-US", timezoneId: "America/New_York" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({
    contentType: "text/html",
    body: '<!doctype html><main id="probe"></main>',
  }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.addStyleTag({ path: theme });
  console.log(`ej2: ${require("@syncfusion/ej2/package.json").version}`);
  console.log(`ej2-daterangepicker-shown-range: a range of 2026-07-14 and 2026-07-20 under format yyyy-MM-dd shows "${await render(page, "range")}"`);
  console.log(`ej2-timepicker-shown-time: a value of 14:30 under the default culture shows "${await render(page, "time")}"`);
  console.log(`ej2-numerictextbox-shown-value: a value of 12.5 under the default format shows "${await render(page, "numeric")}"`);
  console.log(`ej2-colorpicker-held-value: a value of #ff0000ff reads back "${await render(page, "color")}"`);
  await browser.close();
})();
