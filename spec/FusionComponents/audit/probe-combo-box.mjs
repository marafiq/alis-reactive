import assert from "node:assert/strict";
import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const version = require("@syncfusion/ej2-dropdowns/package.json").version;
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const style = require.resolve("@syncfusion/ej2-dropdowns/styles/material.css");
const browser = await chromium.launch();

async function onPage(gesture) {
  const page = await browser.newPage();
  try {
    await page.setContent("<!doctype html><input id='choice' aria-label='Choice'>");
    await page.addScriptTag({ path: bundle });
    await page.addStyleTag({ path: style });
    await gesture(page);
  } finally {
    await page.close();
  }
}

const changes = [
  { kind: "string", first: "call", second: "letter" },
  { kind: "number", first: 30, second: 0 },
  { kind: "boolean", first: true, second: false },
  { kind: "object", first: "en", second: "pa" },
];

const filtering = [
  { control: "ComboBox", nativeEvent: "baseEventArgs" },
  { control: "AutoComplete", nativeEvent: null },
  { control: "MultiSelect", nativeEvent: "event" },
];

const observed = { version, change: [], filtering: [] };

try {
  for (const scenario of changes) {
    await onPage(async (page) => {
      await page.evaluate(({ kind, first, second }) => {
        const rows = [
          { label: "First option", id: first, support: { interpreter: false } },
          { label: "Second option", id: second, support: { interpreter: true } },
        ];
        const combo = new window.ej.dropdowns.ComboBox({
          dataSource: rows,
          fields: { text: "label", value: "id" },
          allowCustom: false,
          allowObjectBinding: kind === "object",
          value: kind === "object" ? rows[0] : first,
        });
        combo.appendTo("#choice");
        combo.addEventListener("change", args => {
          window.selection = {
            value: args.value,
            itemData: args.itemData,
            previousItemData: args.previousItemData,
            item: args.item.constructor.name,
            previousItem: args.previousItem.constructor.name,
            selectedOptionText: args.item.textContent,
            interaction: args.isInteracted,
          };
        });
      }, scenario);

      const input = page.getByRole("combobox", { name: "Choice" });
      await input.focus({ timeout: 4000 });
      await page.keyboard.press("Alt+ArrowDown");
      assert.deepEqual(
        await page.getByRole("listbox").getByRole("option").allTextContents(),
        ["First option", "Second option"],
      );
      await page.keyboard.press("ArrowDown");
      await page.keyboard.press("Enter");
      await page.waitForFunction(() => window.selection?.interaction === true, null, { timeout: 6000 });

      const selection = await page.evaluate(() => window.selection);
      assert.equal(await input.inputValue(), "Second option");
      assert.equal(selection.selectedOptionText, "Second option");
      assert.equal(selection.item, "HTMLLIElement");
      assert.equal(selection.previousItem, "HTMLLIElement");
      assert.deepEqual(selection.itemData, { label: "Second option", id: scenario.second, support: { interpreter: true } });
      assert.deepEqual(selection.previousItemData, { label: "First option", id: scenario.first, support: { interpreter: false } });
      assert.equal(typeof selection.value, scenario.kind);
      assert.deepEqual(selection.value, scenario.kind === "object" ? selection.itemData : scenario.second);
      observed.change.push({ kind: scenario.kind, ...selection });
    });
  }

  for (const { control, nativeEvent } of filtering) {
    await onPage(async (page) => {
      await page.evaluate((name) => {
        const Dropdown = window.ej.dropdowns[name];
        const dropdown = new Dropdown({ allowFiltering: true, debounceDelay: 0, dataSource: ["Latex", "Penicillin"] });
        dropdown.appendTo("#choice");
        dropdown.addEventListener("filtering", args => {
          if (!args.text) return;
          window.filtered = {
            text: args.text,
            members: Object.fromEntries(Object.keys(args).sort().map(key => {
              const value = args[key];
              const kind = value === null ? "null" : value instanceof Event ? value.constructor.name : typeof value;
              return [key, kind];
            })),
          };
        });
      }, control);

      const input = page.getByRole("combobox", { name: "Choice" });
      if (control === "MultiSelect") {
        await input.focus({ timeout: 4000 });
        await page.keyboard.press("ArrowDown");
        for (const character of "lat") await page.keyboard.press(character);
      } else {
        await input.pressSequentially("lat", { delay: 30, timeout: 4000 });
      }
      await page.waitForFunction(() => window.filtered?.text === "lat", null, { timeout: 8000 });

      const result = await page.evaluate(() => window.filtered);
      const expected = { cancel: "boolean", name: "string", preventDefaultAction: "boolean", text: "string", updateData: "function" };
      if (nativeEvent !== null) expected[nativeEvent] = "KeyboardEvent";
      assert.deepEqual(result.members, expected);
      observed.filtering.push({ control, ...result });
    });
  }

  console.log(JSON.stringify(observed, null, 2));
} finally {
  await browser.close();
}
