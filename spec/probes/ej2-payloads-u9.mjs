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
  const items = value => Array.isArray(value) ? kind(value) + "(" + value.map(kind).join(",") + ")" : kind(value);
  window.kind = kind;
  window.shape = shape;
  window.items = items;
  window.heard = {};
  window.held = {};
  window.listen = (prefix, component, events) => events.forEach(event => component.addEventListener(event, args => {
    window.heard[prefix + event] = window.heard[prefix + event] || shape(args);
  }));
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => child.tagName === "DIV" && (child.style.zIndex === "999999999" || child.attributes.length === 0 && child.firstElementChild?.style.zIndex === "99999")).forEach(child => child.remove()));

const heard = (page, name) => page.evaluate(key => window.heard[key] ?? "not raised", name);

const assistView = async page => {
  await page.evaluate(() => {
    const assist = new ej.interactivechat.AIAssistView({});
    assist.appendTo("#assist");
    listen("", assist, ["promptChanged", "promptRequest", "stopRespondingClick"]);
    assist.addEventListener("promptRequest", args => {
      args.cancel = true;
    });
    window.assist = assist;
  });
  await withoutTrialBanner(page);
  await page.click("#assist .e-assist-textarea");
  await page.keyboard.type("Can Mr Patel have a second dose?");
  await page.click("#assist .e-assist-send");
  const stopShown = await page.locator("#assist .e-assist-stop").count() > 0;
  await page.click("#assist .e-assist-stop");
  console.log(`ej2-payload-aiassistview-promptchanged: ${await heard(page, "promptChanged")}`);
  console.log(`ej2-payload-aiassistview-promptrequest: ${await heard(page, "promptRequest")}`);
  console.log(`ej2-payload-aiassistview-stoprespondingclick: ${await heard(page, "stopRespondingClick")}`);
  console.log(`ej2-aiassistview-promptrequest-cancelled: ${await page.evaluate(() => "prompts:" + window.assist.prompts.length + ",prompt:" + window.assist.prompts[0].prompt)},stop:${stopShown}`);
};

const assistResponses = async page => {
  await page.evaluate(() => {
    const answered = new ej.interactivechat.AIAssistView({});
    answered.appendTo("#answered");
    answered.addPromptResponse({ prompt: "Which ward is Mrs Khan in?", response: "Ward B, bed 4." });
    const told = new ej.interactivechat.AIAssistView({});
    told.appendTo("#told");
    told.addPromptResponse("Rounds start at 8:00.");
    window.answered = answered;
    window.told = told;
  });
  const shown = selector => page.locator(selector).innerText();
  const answered = await shown("#answered .e-content");
  const told = await shown("#told .e-content");
  console.log(`ej2-aiassistview-addpromptresponse-object: ${await page.evaluate(() => "prompts:" + window.answered.prompts.length + ",prompt:" + kind(window.answered.prompts[0].prompt) + ",response:" + kind(window.answered.prompts[0].response))},shown:${answered.includes("Which ward is Mrs Khan in?") && answered.includes("Ward B, bed 4.")}`);
  console.log(`ej2-aiassistview-addpromptresponse-text-unrequested: ${await page.evaluate(() => "prompts:" + window.told.prompts.length + ",prompt:" + kind(window.told.prompts[0].prompt))},shown:${told.includes("Rounds start at 8:00.")}`);
};

const bulletChart = async page => {
  await page.evaluate(() => {
    ej.charts.BulletChart.Inject(ej.charts.BulletTooltip);
    const doses = new ej.charts.BulletChart({
      dataSource: [{ given: 6, goal: 8, ward: "Ward B" }],
      valueField: "given", targetField: "goal", categoryField: "ward",
      tooltip: { enable: true }, labelFormat: "{value} doses", minimum: 0, maximum: 10, interval: 2, width: "400px", height: "120px",
    });
    doses.appendTo("#doses");
    listen("", doses, ["tooltipRender", "bulletChartMouseClick"]);
    doses.addEventListener("tooltipRender", args => {
      window.held.value = window.held.value || [kind(args.value), items(args.target), args.value, [].concat(args.target).join("|")].join(",");
    });
  });
  await withoutTrialBanner(page);
  await page.hover("#doses_svg_FeatureMeasure_0");
  await page.waitForFunction(() => window.heard.tooltipRender, null, { timeout: 5000 });
  await page.click("#doses_svg_FeatureMeasure_0");
  console.log(`ej2-payload-bulletchart-tooltiprender: ${await heard(page, "tooltipRender")}`);
  console.log(`ej2-bulletchart-tooltiprender-value-target: ${await page.evaluate(() => window.held.value)}`);
  console.log(`ej2-payload-bulletchart-bulletchartmouseclick: ${await heard(page, "bulletChartMouseClick")}`);
};

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles", locale: "en-US" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({
    contentType: "text/html",
    body: '<!doctype html><main id="probe"><div id="assist"></div><div id="answered"></div><div id="told"></div><div id="doses"></div></main>',
  }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.addStyleTag({ path: theme });
  await page.evaluate(describe);
  await assistView(page);
  await page.evaluate(() => { window.heard = {}; });
  await assistResponses(page);
  await page.evaluate(() => { window.heard = {}; });
  await bulletChart(page);
  await browser.close();
})();
