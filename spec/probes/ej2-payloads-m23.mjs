import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");
const theme = require.resolve("@syncfusion/ej2/material.css");

const describe = () => {
  window.rendered = element => element === null ? "absent" : `bold:${element.querySelector("b, i") !== null} text:${element.textContent}`;
  window.asText = text => {
    const span = document.createElement("span");
    span.textContent = text;
    return span;
  };
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => !child.id && child.tagName === "DIV" && !child.classList.contains("e-dlg-container")).forEach(child => child.remove()));

const toastShows = async (page, host, set) => {
  await page.evaluate(({ host, set }) => {
    const toast = new ej.notifications.Toast({ timeOut: 0, animation: { show: { duration: 0 }, hide: { duration: 0 } } });
    toast.appendTo("#" + host);
    window[set](toast);
    toast.dataBind();
    toast.show();
  }, { host, set });
  await page.waitForFunction(host => document.querySelector(`#${host} .e-toast`) !== null, host, { timeout: 5000 });
};

const toast = async page => {
  await page.evaluate(() => {
    window.markupContent = toast => { toast.content = "<b>Ada</b> saved"; };
    window.selectorContent = toast => { toast.content = "#vitals"; };
    window.selectorTemplateContent = toast => { toast.content = "b[title='${window.location.protocol}']"; };
    window.markupTitle = toast => { toast.title = "<b>Saved</b>"; toast.content = "Care plan"; };
  });
  await toastShows(page, "toast-markup", "markupContent");
  console.log(`ej2-toast-content-markup: ${await page.evaluate(() => rendered(document.querySelector("#toast-markup .e-toast-content")))}`);
  await toastShows(page, "toast-selector", "selectorContent");
  console.log(`ej2-toast-content-selector: vitals ${await page.evaluate(() => document.querySelector("#vitals").closest(".e-toast") === null ? "stays on the page" : "moved into the toast")}`);
  const compiled = await page.evaluate(() => {
    const toast = new ej.notifications.Toast({ timeOut: 0, animation: { show: { duration: 0 }, hide: { duration: 0 } } });
    toast.appendTo("#toast-selector-template");
    selectorTemplateContent(toast);
    toast.dataBind();
    try {
      toast.show();
      return "shown " + rendered(document.querySelector("#toast-selector-template .e-toast-content"));
    } catch (error) {
      return `show throws ${error.name}: ${error.message}; toast ${document.querySelector("#toast-selector-template .e-toast") === null ? "absent" : "present"}`;
    }
  });
  console.log(`ej2-toast-content-selector-template: ${compiled}`);
  await toastShows(page, "toast-title", "markupTitle");
  console.log(`ej2-toast-title-markup: ${await page.evaluate(() => rendered(document.querySelector("#toast-title .e-toast-title")))}`);
};

const toastAsText = async page => {
  await page.evaluate(() => {
    const entities = { "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" };
    window.textHtml = text => "<span>" + String(text).replace(/[&<>"']/g, character => entities[character]) + "</span>";
    window.textToast = (host, title, content) => {
      const toast = new ej.notifications.Toast({ timeOut: 0, animation: { show: { duration: 0 }, hide: { duration: 0 } } });
      toast.addEventListener("beforeSanitizeHtml", args => { args.cancel = true; args.helper = textHtml; });
      toast.appendTo("#" + host);
      toast.title = title;
      toast.content = content;
      toast.dataBind();
      try {
        toast.show();
      } catch (error) {
        return `show throws ${error.name}`;
      }
      const shown = document.querySelector(`#${host} .e-toast`);
      return shown === null ? "absent" : `title ${rendered(shown.querySelector(".e-toast-title"))} content ${rendered(shown.querySelector(".e-toast-content"))}`;
    };
  });
  console.log(`ej2-toast-text-markup: ${await page.evaluate(() => textToast("text-markup", "<b>Saved</b>", "<b>Ada</b> saved"))}`);
  console.log(`ej2-toast-text-selector: ${await page.evaluate(() => textToast("text-selector", "Ward", "#chart"))} chart ${await page.evaluate(() => document.querySelector("#chart").closest(".e-toast") === null ? "stays on the page" : "moved into the toast")}`);
  console.log(`ej2-toast-text-template: ${await page.evaluate(() => textToast("text-template", "Ward", "b[title='${window.location.protocol}']"))}`);
  console.log(`ej2-toast-text-null: ${await page.evaluate(() => textToast("text-null", "Ward", null))}`);
  console.log(`ej2-toast-text-title-null: ${await page.evaluate(() => textToast("text-title-null", null, "Saved"))}`);
  const unbound = await page.evaluate(() => {
    const toast = new ej.notifications.Toast({ timeOut: 0, animation: { show: { duration: 0 }, hide: { duration: 0 } } });
    toast.appendTo("#toast-unbound");
    toast.show();
    toast.content = "Dose recorded";
    toast.cssClass = "e-toast-success";
    toast.show();
    return [...document.querySelectorAll("#toast-unbound .e-toast")].map(shown => `success:${shown.classList.contains("e-toast-success")} ${rendered(shown.querySelector(".e-toast-content"))}`).join(" | ");
  });
  console.log(`ej2-toast-show-without-databind: ${unbound}`);
};

const confirmDialog = async page => {
  await page.evaluate(() => {
    const dialog = new ej.popups.Dialog({
      isModal: true,
      visible: false,
      width: "400px",
      animationSettings: { effect: "None" },
      showCloseIcon: false,
      closeOnEscape: true,
      target: document.body,
    });
    dialog.appendTo("#confirm");
    window.answers = [];
    window.ask = (header, content, ok, cancel) => {
      dialog.header = header;
      dialog.content = content;
      dialog.buttons = [
        { click: () => { dialog.close = null; dialog.hide(); answers.push(true); }, buttonModel: { content: ok, isPrimary: true, cssClass: "e-primary" } },
        { click: () => { dialog.close = null; dialog.hide(); answers.push(false); }, buttonModel: { content: cancel } },
      ];
      dialog.close = () => answers.push(false);
      dialog.dataBind();
      dialog.show();
    };
    window.dialog = dialog;
  });
  const shown = () => page.evaluate(() => `visible ${window.dialog.visible} header ${rendered(document.querySelector("#confirm .e-dlg-header"))} content ${rendered(document.querySelector("#confirm .e-dlg-content"))}`);
  await page.evaluate(() => ask("<i>Confirm</i>", "<b>Remove Ada?</b>", "<b>OK</b>", "Cancel"));
  console.log(`ej2-dialog-ask-markup: ${await shown()} buttons ${await page.evaluate(() => [...document.querySelectorAll("#confirm .e-footer-content button")].map(button => `bold:${button.querySelector("b") !== null} text:${button.textContent}`).join("|"))}`);
  await withoutTrialBanner(page);
  await page.click("#confirm .e-footer-content .e-primary");
  console.log(`ej2-dialog-ask-ok: visible ${await page.evaluate(() => window.dialog.visible)} answers ${await page.evaluate(() => answers.join(","))}`);
  await page.evaluate(() => ask(asText("<i>Confirmar</i>"), asText("<b>Remove Ada?</b>"), textHtml("<b>Sí</b>"), textHtml("Cancelar")));
  console.log(`ej2-dialog-ask-text: ${await shown()} buttons ${await page.evaluate(() => [...document.querySelectorAll("#confirm .e-footer-content button")].map(button => `bold:${button.querySelector("b") !== null} text:${button.textContent}`).join("|"))}`);
  await withoutTrialBanner(page);
  await page.click("#confirm .e-dlg-content");
  await page.keyboard.press("Escape");
  await page.waitForFunction(() => window.answers.length === 2, null, { timeout: 5000 });
  console.log(`ej2-dialog-ask-escape: visible ${await page.evaluate(() => window.dialog.visible)} answers ${await page.evaluate(() => answers.join(","))}`);
};

(async () => {
  const browser = await chromium.launch();
  const page = await (await browser.newContext()).newPage();
  await page.route("http://probe.test/", route => route.fulfill({
    contentType: "text/html",
    body: '<!doctype html><main id="probe"><section id="vitals">Vitals panel</section><div id="toast-markup"></div><div id="toast-selector"></div><div id="toast-selector-template"></div><div id="toast-title"></div><section id="chart">Chart panel</section><div id="text-markup"></div><div id="text-selector"></div><div id="text-template"></div><div id="text-null"></div><div id="text-title-null"></div><div id="toast-unbound"></div><div id="confirm"></div></main>',
  }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.addStyleTag({ path: theme });
  await page.evaluate(describe);
  await toast(page);
  await toastAsText(page);
  await confirmDialog(page);
  await browser.close();
})();
