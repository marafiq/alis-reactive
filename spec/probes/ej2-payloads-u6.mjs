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
    return typeof value;
  };
  const shape = args => Object.keys(args).sort().map(key => key + ":" + kind(args[key])).join(",");
  window.kind = kind;
  window.heard = {};
  window.listen = (prefix, component, events) => events.forEach(event => component.addEventListener(event, args => {
    window.heard[prefix + event] = window.heard[prefix + event] || shape(args);
  }));
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => child.tagName === "DIV" && (child.style.zIndex === "999999999" || child.attributes.length === 0 && child.firstElementChild?.style.zIndex === "99999")).forEach(child => child.remove()));

const click = async (page, selector, options) => {
  await withoutTrialBanner(page);
  await page.click(selector, options);
};

const heard = (page, name) => page.evaluate(key => window.heard[key] ?? "not raised", name);

const otpInput = async page => {
  await page.evaluate(() => {
    const code = new ej.inputs.OtpInput({ length: 4 });
    code.appendTo("#code");
    listen("", code, ["focus", "input", "valueChanged", "blur"]);
    window.code = code;
  });
  await click(page, ".e-otp-input-field >> nth=0");
  await page.keyboard.type("4821");
  await click(page, "#after");
  for (const event of ["focus", "input", "valueChanged", "blur"])
    console.log(`ej2-payload-otpinput-${event}: ${await heard(page, event)}`);
  console.log(`ej2-otpinput-value-after-typing: ${await page.evaluate(() => kind(window.code.value))}`);
};

const slider = async page => {
  await page.evaluate(() => {
    const pain = new ej.inputs.Slider({ min: 0, max: 10, value: 3 });
    pain.appendTo("#pain");
    listen("pain-", pain, ["change", "changed"]);
    const rest = new ej.inputs.Slider({ min: 12, max: 18, type: "Range", value: [13, 15] });
    rest.appendTo("#rest");
    listen("rest-", rest, ["change", "changed"]);
  });
  await click(page, "#pain .e-handle");
  await page.keyboard.press("ArrowRight");
  await click(page, "#rest .e-handle >> nth=0");
  await page.keyboard.press("ArrowRight");
  await click(page, "#after");
  for (const event of ["pain-change", "pain-changed", "rest-change", "rest-changed"])
    console.log(`ej2-payload-slider-${event}: ${await heard(page, event)}`);
};

const numericTextBox = async page => {
  await page.evaluate(() => {
    const dose = new ej.inputs.NumericTextBox({ value: 5 });
    dose.appendTo("#dose");
    listen("", dose, ["change"]);
    window.dose = dose;
  });
  await click(page, "#dose", { clickCount: 3 });
  await page.keyboard.press("Backspace");
  await page.keyboard.press("Tab");
  console.log(`ej2-payload-numerictextbox-change-cleared: ${await heard(page, "change")}`);
  console.log(`ej2-numerictextbox-value-cleared: ${await page.evaluate(() => kind(window.dose.value))}`);
  await page.evaluate(() => { window.heard = {}; });
  await click(page, "#dose");
  await page.keyboard.type("7");
  await page.keyboard.press("Tab");
  console.log(`ej2-payload-numerictextbox-change-after-cleared: ${await heard(page, "change")}`);
};

const smartTextArea = async page => {
  await page.evaluate(() => {
    const note = new ej.inputs.SmartTextArea({
      showSuggestionOnPopup: "Disable",
      aiSuggestionHandler: () => Promise.resolve("OK:[ two glasses of water]"),
    });
    note.appendTo("#note");
    listen("", note, ["beforeSuggestionInsert", "afterSuggestionInsert"]);
  });
  await click(page, "#note");
  await page.keyboard.type("Resident drank");
  await page.waitForFunction(() => window.heard.beforeSuggestionInsert, null, { timeout: 5000 });
  await page.keyboard.press("Tab");
  for (const event of ["beforeSuggestionInsert", "afterSuggestionInsert"])
    console.log(`ej2-payload-smarttextarea-${event}: ${await heard(page, event)}`);
};

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({
    contentType: "text/html",
    body: '<!doctype html><main id="probe"><div id="code"></div><div id="pain"></div><div id="rest"></div><input id="dose"><textarea id="note"></textarea><button id="after">After</button></main>',
  }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.addStyleTag({ path: theme });
  await page.evaluate(describe);
  await otpInput(page);
  await page.evaluate(() => { window.heard = {}; });
  await slider(page);
  await page.evaluate(() => { window.heard = {}; });
  await numericTextBox(page);
  await page.evaluate(() => { window.heard = {}; });
  await smartTextArea(page);
  await browser.close();
})();
