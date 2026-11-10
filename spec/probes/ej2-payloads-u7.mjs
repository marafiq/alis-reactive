import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const theme = require.resolve("@syncfusion/ej2/material.css");

const describe = () => {
  const kind = value => {
    if (value === null) return "null";
    if (value === undefined) return "undefined";
    if (value instanceof Date) return "Date";
    if (Array.isArray(value)) return "Array";
    if (typeof value === "function") return "function";
    if (typeof value.then === "function") return "Promise";
    if (value instanceof Element) return "Element";
    if (value instanceof File) return "File";
    if (value instanceof Blob) return "Blob";
    return typeof value;
  };
  const shape = args => Object.keys(args).sort().map(key => key + ":" + kind(args[key])).join(",");
  const items = value => Array.isArray(value) ? kind(value) + "(" + value.map(kind).join(",") + ")" : kind(value);
  window.kind = kind;
  window.shape = shape;
  window.items = items;
  window.heard = {};
  window.listen = (prefix, component, events) => events.forEach(event => component.addEventListener(event, args => {
    window.heard[prefix + event] = window.heard[prefix + event] || shape(args);
  }));
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => !child.id && child.tagName === "DIV").forEach(child => child.remove()));

const click = async (page, selector) => {
  await withoutTrialBanner(page);
  await page.click(selector);
};

const heard = (page, name) => page.evaluate(key => window.heard[key] ?? "not raised", name);

const buttons = async page => {
  await page.evaluate(() => {
    const consent = new ej.buttons.CheckBox({ label: "Consent to photos" });
    consent.appendTo("#consent");
    listen("checkbox-", consent, ["change"]);
    const alerts = new ej.buttons.Switch({});
    alerts.appendTo("#alerts");
    listen("switch-", alerts, ["change"]);
    new ej.buttons.RadioButton({ label: "Morning", name: "slot", value: "Morning", checked: true }).appendTo("#morning");
    const evening = new ej.buttons.RadioButton({ label: "Evening", name: "slot", value: "Evening" });
    evening.appendTo("#evening");
    listen("radiobutton-", evening, ["change"]);
  });
  await click(page, "label[for=consent]");
  await click(page, "#probe .e-switch-wrapper");
  await click(page, "label[for=evening]");
  for (const event of ["checkbox-change", "switch-change", "radiobutton-change"])
    console.log(`ej2-payload-${event}: ${await heard(page, event)}`);
};

const dateRangePicker = async page => {
  await page.evaluate(() => {
    const stay = new ej.calendars.DateRangePicker({});
    stay.appendTo("#stay");
    listen("", stay, ["change"]);
    window.stay = stay;
  });
  await click(page, "#stay");
  await page.keyboard.type("11/10/2026 - 11/14/2026");
  await page.keyboard.press("Tab");
  console.log(`ej2-payload-daterangepicker-change: ${await heard(page, "change")}`);
  console.log(`ej2-daterangepicker-value-after-typing: ${await page.evaluate(() => items(window.stay.value))}`);
  console.log(`ej2-daterangepicker-range-after-typing: ${await page.evaluate(() => [window.stay.startDate.toDateString(), window.stay.endDate.toDateString(), window.stay.getSelectedRange().daySpan].join(","))}`);
};

const uploader = async page => {
  await page.evaluate(() => {
    const documents = new ej.inputs.Uploader({ autoUpload: false });
    documents.appendTo("#documents");
    listen("", documents, ["selected"]);
    window.documents = documents;
    const preloaded = new ej.inputs.Uploader({ autoUpload: false, files: [{ name: "intake-form", size: 2048, type: ".pdf" }] });
    preloaded.appendTo("#preloaded");
    window.preloaded = preloaded;
  });
  await withoutTrialBanner(page);
  await page.setInputFiles("#documents", { name: "care-plan.pdf", mimeType: "application/pdf", buffer: Buffer.from("%PDF-1.4") });
  await page.waitForFunction(() => window.documents.getFilesData().length === 1, null, { timeout: 5000 });
  console.log(`ej2-payload-uploader-selected: ${await heard(page, "selected")}`);
  console.log(`ej2-uploader-getfilesdata-selected: ${await page.evaluate(() => shape(window.documents.getFilesData()[0]))}`);
  console.log(`ej2-uploader-getfilesdata-preloaded: ${await page.evaluate(() => shape(window.preloaded.getFilesData()[0]))}`);
};

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles", locale: "en-US" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({
    contentType: "text/html",
    body: '<!doctype html><main id="probe"><input id="consent" type="checkbox"><input id="alerts" type="checkbox"><input id="morning" type="radio"><input id="evening" type="radio"><input id="stay"><input id="documents" type="file"><input id="preloaded" type="file"><button id="after">After</button></main>',
  }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.addStyleTag({ path: theme });
  await page.evaluate(describe);
  await buttons(page);
  await page.evaluate(() => { window.heard = {}; });
  await dateRangePicker(page);
  await page.evaluate(() => { window.heard = {}; });
  await uploader(page);
  await browser.close();
})();
