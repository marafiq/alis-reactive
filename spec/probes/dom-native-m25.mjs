import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);

const probePage = `<!doctype html>
<style>
  #faded { opacity: 1; }
  #faded.out { opacity: 0; transition: opacity 50ms; }
  #toasts { inset: auto 0 0 auto; margin: 0; }
</style>
<main id="probe">
  <button id="behind">Save</button>
  <dialog id="ask"><p>Discharge Ada?</p><button id="ask-cancel">Cancel</button><button id="ask-ok">OK</button></dialog>
  <dialog id="ask-stopped"><p>Discharge Bea?</p><button id="stopped-cancel">Cancel</button><button id="stopped-ok">OK</button></dialog>
  <div id="toasts" popover="manual"><p>Ward B is full</p><button id="dismiss">Dismiss</button></div>
  <a id="resident" href="http://probe.test/residents/7">Ada</a>
  <aside id="plain">Drawer</aside>
  <aside id="faded">Drawer</aside>
</main>`;

const describe = () => {
  window.heard = { behind: 0, dismiss: 0, documentEscape: 0, events: {}, click: [], auxclick: 0, plain: 0, faded: 0 };
  const record = (id, type) => document.getElementById(id).addEventListener(type, () => {
    (window.heard.events[id] = window.heard.events[id] || []).push(type);
  });
  for (const id of ["ask", "ask-stopped"]) record(id, "cancel");
  document.getElementById("ask-stopped").addEventListener("keydown", event => {
    if (event.key === "Escape") event.stopPropagation();
  });
  document.addEventListener("keydown", event => {
    if (event.key === "Escape") window.heard.documentEscape += 1;
  });
  document.getElementById("behind").addEventListener("click", () => { window.heard.behind += 1; });
  document.getElementById("dismiss").addEventListener("click", () => { window.heard.dismiss += 1; });
  const link = document.getElementById("resident");
  link.addEventListener("click", event => window.heard.click.push(`button:${event.button} modifier:${event.ctrlKey || event.metaKey}`));
  link.addEventListener("auxclick", () => { window.heard.auxclick += 1; });
  document.getElementById("plain").addEventListener("transitionend", () => { window.heard.plain += 1; });
  document.getElementById("faded").addEventListener("transitionend", () => { window.heard.faded += 1; });
};

const center = async (page, selector) => {
  const box = await page.locator(selector).boundingBox();
  return { x: box.x + box.width / 2, y: box.y + box.height / 2 };
};

const read = (page, reader) => page.evaluate(reader);

const dialogEscape = async page => {
  const behind = await center(page, "#behind");
  await page.evaluate(() => document.getElementById("ask").showModal());
  await page.mouse.click(behind.x, behind.y);
  console.log(`dom-dialog-modal-click-behind: ${await read(page, () => `behind:${window.heard.behind} open:${document.getElementById("ask").open}`)}`);
  await page.keyboard.press("Escape");
  console.log(`dom-dialog-escape: ${await read(page, () => `${(window.heard.events.ask || []).join(",")} open:${document.getElementById("ask").open} document:${window.heard.documentEscape}`)}`);
  await page.evaluate(() => { window.heard.documentEscape = 0; document.getElementById("ask-stopped").showModal(); });
  await page.keyboard.press("Escape");
  console.log(`dom-dialog-escape-stopped-at-dialog: ${await read(page, () => `${(window.heard.events["ask-stopped"] || []).join(",")} open:${document.getElementById("ask-stopped").open} document:${window.heard.documentEscape}`)}`);
};

const manualPopover = async page => {
  const behind = await center(page, "#behind");
  await page.evaluate(() => document.getElementById("toasts").showPopover());
  await page.keyboard.press("Escape");
  await page.mouse.click(behind.x, behind.y);
  console.log(`dom-popover-manual-escape-and-click-elsewhere: ${await read(page, () => `open:${document.getElementById("toasts").matches(":popover-open")} behind:${window.heard.behind}`)}`);
  await page.evaluate(() => {
    document.getElementById("ask").showModal();
    const toasts = document.getElementById("toasts");
    toasts.hidePopover();
    toasts.showPopover();
  });
  const dismiss = await center(page, "#dismiss");
  await page.mouse.click(dismiss.x, dismiss.y);
  console.log(`dom-popover-reshown-over-modal-dismiss-click: ${await read(page, () => `dismiss:${window.heard.dismiss} open:${document.getElementById("ask").open}`)}`);
  await page.evaluate(() => document.getElementById("ask").close());
  await page.mouse.click(dismiss.x, dismiss.y);
  console.log(`dom-popover-dismiss-click-after-modal-closed: ${await read(page, () => `dismiss:${window.heard.dismiss}`)}`);
};

const linkClicks = async (context, page) => {
  const middle = context.waitForEvent("page");
  const link = await center(page, "#resident");
  await page.mouse.click(link.x, link.y, { button: "middle" });
  const middlePage = await middle;
  await middlePage.waitForLoadState();
  console.log(`dom-link-middle-click: ${await read(page, () => `click:${window.heard.click.length} auxclick:${window.heard.auxclick}`)} new-page:${middlePage.url()} stays:${page.url()}`);
  await middlePage.close();
  const modified = context.waitForEvent("page");
  await page.click("#resident", { modifiers: ["ControlOrMeta"] });
  const modifiedPage = await modified;
  await modifiedPage.waitForLoadState();
  console.log(`dom-link-modified-click: ${await read(page, () => window.heard.click.join(","))} new-page:${modifiedPage.url()} stays:${page.url()}`);
  await modifiedPage.close();
};

const transitionEnd = async page => {
  await page.evaluate(() => {
    document.getElementById("plain").classList.add("out");
    document.getElementById("faded").classList.add("out");
  });
  await page.waitForFunction(() => window.heard.faded > 0, null, { timeout: 5000 });
  await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
  console.log(`dom-transitionend-needs-a-transition: ${await read(page, () => `without:${window.heard.plain} with:${window.heard.faded}`)}`);
};

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext();
  await context.route("http://probe.test/**", route => route.fulfill({
    contentType: "text/html",
    body: route.request().url() === "http://probe.test/" ? probePage : "<!doctype html><main id=\"probe\">Resident 7</main>",
  }));
  const page = await context.newPage();
  await page.goto("http://probe.test/");
  await page.evaluate(describe);
  await dialogEscape(page);
  await manualPopover(page);
  await linkClicks(context, page);
  await transitionEnd(page);
  await browser.close();
})();
