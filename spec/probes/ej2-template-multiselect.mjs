import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const theme = require.resolve("@syncfusion/ej2/material.css");

const textHelper = () => {
  const entities = { "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" };
  globalThis.alisFusionText = value => value == null ? "" : Array.from(String(value), character => entities[character] ?? character).join("");
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => child.tagName === "DIV" && (child.style.zIndex === "999999999" || child.attributes.length === 0 && child.firstElementChild?.style.zIndex === "99999")).forEach(child => child.remove()));

const renderAllergies = page => page.evaluate(() => {
  const host = document.createElement("input");
  host.id = "allergies";
  document.getElementById("probe").appendChild(host);
  new ej.dropdowns.MultiSelect({
    dataSource: [{ name: "Penicillin", severity: "severe" }, { name: "<i>Latex</i>", severity: "mild" }],
    fields: { text: "name", value: "name" },
    itemTemplate: "<span class=\"allergy\">${alisFusionText(data.name)}</span>${if(severity === \"severe\")}<b class=\"band\">Severe</b>${else}<em class=\"band\">Mild</em>${/if}",
  }).appendTo(host);
});

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles" });
  const page = await context.newPage();
  page.on("dialog", dialog => dialog.dismiss());
  await page.route("http://probe.test/", route => route.fulfill({
    contentType: "text/html",
    body: '<!doctype html><main id="probe"></main>',
  }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.addStyleTag({ path: theme });
  await page.evaluate(textHelper);
  console.log(`ej2: ${require("@syncfusion/ej2/package.json").version}`);
  await renderAllergies(page);
  await withoutTrialBanner(page);
  await page.click(".e-multi-select-wrapper");
  const items = page.locator(".e-popup .e-list-item");
  await items.nth(1).waitFor();
  const names = await items.locator(".allergy").allTextContents();
  const bands = await items.locator(".band").allTextContents();
  const injected = await items.locator("i").count();
  console.log(`ej2-multiselect-item-template-text: the list shows ${await items.count()} items, names ${names.join("|")}, ${injected} i elements`);
  console.log(`ej2-multiselect-item-template-condition: the items read ${bands.join("|")}`);
  await browser.close();
})();
