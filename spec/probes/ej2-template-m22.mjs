import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const theme = require.resolve("@syncfusion/ej2/material.css");

const textHelper = () => {
  const entities = { "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" };
  globalThis.alisFusionText = value => value == null ? "" : Array.from(String(value), character => entities[character] ?? character).join("");
};

const renderGrid = (page, id, rows, templates) => page.evaluate(({ id, rows, templates }) => new Promise(resolve => {
  const host = document.createElement("div");
  host.id = id;
  document.getElementById("probe").appendChild(host);
  const settle = outcome => resolve(outcome);
  const timer = setTimeout(() => settle({ error: "no dataBound" }), 4000);
  try {
    const grid = new ej.grids.Grid({
      dataSource: rows,
      columns: templates.map((template, index) => ({ headerText: "c" + index, template })),
      dataBound: () => {
        clearTimeout(timer);
        const cells = [...host.querySelectorAll(".e-row")].map(row => [...row.querySelectorAll("td")]);
        settle({ rows: cells.length, cells: cells.map(row => row.map(cell => ({ text: cell.textContent, html: cell.innerHTML, images: cell.querySelectorAll("img").length }))) });
      },
      actionFailure: failure => {
        clearTimeout(timer);
        settle({ error: (failure.error && failure.error.constructor.name) || "actionFailure" });
      },
    });
    grid.appendTo(host);
  } catch (error) {
    clearTimeout(timer);
    settle({ error: error.constructor.name });
  }
}), { id, rows, templates });

const column = (outcome, index) => outcome.error ? outcome.error : outcome.cells.map(row => row[index].text).join("|");

const reads = async page => {
  const xss = "<img src=x onerror=alert(1)>";
  const outcome = await renderGrid(page, "reads", [{ name: xss, ward: null }, { name: "Ann" }], [
    "<span>${name}</span>",
    "<span>${alisFusionText(data.name)}</span>",
    "<span>${ward}</span>",
    "<span>${alisFusionText(data.ward)}</span>",
  ]);
  const first = outcome.cells[0];
  console.log(`ej2-grid-template-unencoded: a raw read renders ${first[0].images} img element and text "${first[0].text}"`);
  console.log(`ej2-grid-template-helper-text: a helper read renders ${first[1].images} img elements and text "${first[1].text}"`);
  console.log(`ej2-grid-template-missing-raw: a raw read shows "${outcome.cells[0][2].text}" for null and "${outcome.cells[1][2].text}" for a missing member`);
  console.log(`ej2-grid-template-missing-helper: a helper read shows "${outcome.cells[0][3].text}" for null and "${outcome.cells[1][3].text}" for a missing member`);
  const bare = await renderGrid(page, "bare", [{ windowSeat: "yes" }], ["<span>${alisFusionText(windowSeat)}</span>"]);
  const prefixed = await renderGrid(page, "prefixed", [{ windowSeat: "yes" }], ["<span>${alisFusionText(data.windowSeat)}</span>"]);
  console.log(`ej2-grid-template-helper-prefix: windowSeat passed bare gives ${column(bare, 0)}, passed as data.windowSeat gives ${column(prefixed, 0)}`);
};

const holders = async page => {
  const rows = [{ name: "Ann", resident: { address: { city: "Leeds" } } }, { name: "Bo", resident: {} }, { name: "Cy" }];
  const unguarded = await renderGrid(page, "unguarded", rows, ["<span>${resident.address.city}</span>"]);
  const guarded = await renderGrid(page, "guarded", rows, ["<span>${if(resident && resident.address)}${alisFusionText(data.resident.address.city)}${/if}</span>", "<span>${alisFusionText(data.name)}</span>"]);
  console.log(`ej2-grid-template-unguarded-holder: a read past a missing holder gives ${unguarded.error ?? unguarded.rows + " rows"}`);
  console.log(`ej2-grid-template-guarded-holders: reads guarded on each holder render ${guarded.rows} rows, cells ${column(guarded, 0)}, names ${column(guarded, 1)}`);
};

const conditions = async page => {
  const rows = [{ status: "Active", beds: 3, age: 80, score: 3 }, { status: "Away", beds: 1, age: 95, score: 2 }];
  const text = await renderGrid(page, "text", rows, [
    "${if(status === 'Active')}<span class=\"e-badge\">Active</span>${else}<span>Away</span>${/if}",
    "${if(beds > 2 && age < 90)}yes${else}no${/if}",
  ]);
  console.log(`ej2-grid-template-condition-text: status === 'Active' shows ${column(text, 0)}`);
  console.log(`ej2-grid-template-condition-integers: beds > 2 && age < 90 shows ${column(text, 1)}`);
  const plain = await renderGrid(page, "plain-text", [{ status: "On leave_2-b.c" }, { status: "Active" }], ["${if(status === 'On leave_2-b.c')}plain${else}other${/if}"]);
  console.log(`ej2-grid-template-condition-plain-text: status === 'On leave_2-b.c' shows ${column(plain, 0)}`);
  const afterMember = await renderGrid(page, "after-member", rows, ["${if(2 < beds)}yes${else}no${/if}"]);
  await renderGrid(page, "ends-on-number", rows, ["${if(age < 90)}under${else}over${/if}"]);
  const afterNumber = await renderGrid(page, "after-number", rows, ["${if(2 < beds)}yes${else}no${/if}"]);
  await renderGrid(page, "ends-on-number-again", rows, ["${if(age < 90)}under${else}over${/if}"]);
  const memberFirst = await renderGrid(page, "member-first", rows, ["${if(beds > 2)}yes${else}no${/if}"]);
  console.log(`ej2-grid-template-member-first: beds > 2 gives ${column(memberFirst, 0)} after a template whose condition ends on a number`);
  console.log(`ej2-grid-template-constant-first: 2 < beds gives ${column(afterMember, 0)} after a template whose condition ends on text and ${column(afterNumber, 0)} after one whose condition ends on a number`);
  const pair = [{ a: 3, b: 2, isActive: true, status: "Active", beds: 3 }, { a: 1, b: 1, isActive: false, status: "Away", beds: 1 }];
  const branch = async (id, condition) => column(await renderGrid(page, id, pair, ["${if(" + condition + ")}yes${else}no${/if}"]), 0);
  const arithmetic = [];
  for (const [id, condition] of [["add", "a + b > 3"], ["subtract", "a - b > 0"], ["multiply", "a * b > 3"], ["divide", "a / b > 1"], ["multiply-constant", "a * 2 > 4"], ["add-constant", "a + 1 > 3"]])
    arithmetic.push(`${condition} gives ${await branch(id, condition)}`);
  console.log(`ej2-grid-template-arithmetic: ${arithmetic.join(", ")}`);
  const operators = [];
  for (const [id, condition] of [["boolean", "isActive === true"], ["not", "!isActive"], ["or", "beds > 2 || a > 2"], ["not-equal", "status !== 'Active'"], ["at-least", "beds >= 3"], ["at-most", "beds <= 1"]])
    operators.push(`${condition} gives ${await branch(id, condition)}`);
  console.log(`ej2-grid-template-operators: ${operators.join(", ")}`);
  const decimal = await renderGrid(page, "decimal", rows, ["${if(score > 2.5)}high${else}low${/if}"]);
  console.log(`ej2-grid-template-decimal-constant: score > 2.5 gives ${column(decimal, 0)}`);
  const choice = [{ status: "Resident's choice" }, { status: "Active" }];
  const apostrophe = await renderGrid(page, "apostrophe", choice, ["${if(status === 'Resident's choice')}chosen${else}other${/if}"]);
  console.log(`ej2-grid-template-apostrophe-constant: a constant written 'Resident's choice' gives ${column(apostrophe, 0)}`);
  const escaped = await renderGrid(page, "escaped", choice, ["${if(status === \"Resident\\u0027s choice\")}chosen${else}other${/if}"]);
  console.log(`ej2-grid-template-escaped-constant: a constant written "Resident\\u0027s choice" gives ${column(escaped, 0)}`);
  const accents = await renderGrid(page, "accents", [{ name: "Zoë Brontë" }], ["${if(name === \"Zo\\u00EB Bront\\u00EB\")}same${else}other${/if}"]);
  console.log(`ej2-grid-template-two-escapes: a constant written "Zo\\u00EB Bront\\u00EB" gives ${column(accents, 0)}`);
};

const attributes = async page => {
  const rows = [{ name: "O'Brien\" title=\"x", url: "javascript:alert(1)" }, { name: "Ann", url: "/residents?id=7&tab=care" }];
  const outcome = await renderGrid(page, "attributes", rows, [
    "<button class=\"e-btn\" data-alis-detail-name=\"${alisFusionText(data.name)}\">Review</button>",
    "<a href=\"${alisFusionText(data.url)}\">Open</a>",
  ]);
  const read = await page.evaluate(() => {
    const buttons = [...document.querySelectorAll("#attributes .e-row button")];
    const links = [...document.querySelectorAll("#attributes .e-row a")];
    return { detail: buttons.map(button => button.getAttribute("data-alis-detail-name")).join("|"), titles: buttons.filter(button => button.hasAttribute("title")).length, hrefs: links.map(link => link.getAttribute("href")).join("|") };
  });
  console.log(`ej2-grid-template-attribute: ${outcome.rows} rows, data-alis-detail-name reads ${read.detail}, ${read.titles} buttons with a title`);
  console.log(`ej2-grid-template-link-href: hrefs read ${read.hrefs}`);
};

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
  await reads(page);
  await holders(page);
  await conditions(page);
  await attributes(page);
  await browser.close();
})();
