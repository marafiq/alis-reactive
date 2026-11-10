import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);

const page = '<!doctype html><main id="probe">'
  + '<form id="visit"><input id="name" name="ResidentName" value="">'
  + '<input type="hidden" id="bed" name="Bed" value="A-1">'
  + '<div id="days"><input type="hidden" name="VisitDays" value="">'
  + '<label><input type="checkbox" id="days_c0" name="VisitDays" value="Mon">Mon</label>'
  + '<label><input type="checkbox" id="days_c1" name="VisitDays" value="Tue">Tue</label>'
  + '<label><input type="checkbox" id="days_c2" name="VisitDays" value="Wed">Wed</label></div>'
  + '</form><button id="after">After</button></main>';

const changeListener = async browserPage => {
  await browserPage.evaluate(() => {
    const name = document.getElementById("name");
    name.addEventListener("change", (...args) => {
      const first = args[0];
      window.heardChange = {
        count: args.length,
        kind: first instanceof Event ? first.constructor.name : typeof first,
        type: first.type,
        isTrusted: first.isTrusted,
        timeStamp: typeof first.timeStamp,
        value: typeof first.value,
        during: first.currentTarget === name ? "input" : String(first.currentTarget),
        first,
      };
    });
  });
  await browserPage.click("#name");
  await browserPage.keyboard.type("Ada Lovelace");
  await browserPage.click("#after");
  const heard = await browserPage.evaluate(() => {
    const change = window.heardChange;
    return change ? `${change.count} argument,${change.kind},type:${change.type},isTrusted:${change.isTrusted},timeStamp:${change.timeStamp},value:${change.value},currentTarget:${change.during},after:${String(change.first.currentTarget)}` : "not raised";
  });
  console.log(`dom-change-listener-event: ${heard}`);
};

const clickListener = async browserPage => {
  await browserPage.evaluate(() => {
    document.getElementById("after").addEventListener("click", first => {
      window.heardClick = `${first.constructor.name},type:${first.type},isTrusted:${first.isTrusted},pointerType:${first.pointerType},detail:${first.detail},clientX:${typeof first.clientX},altKey:${first.altKey}`;
    });
  });
  await browserPage.click("#after");
  console.log(`dom-click-listener-event: ${await browserPage.evaluate(() => window.heardClick ?? "not raised")}`);
};

const hiddenWrite = async browserPage => {
  const raised = await browserPage.evaluate(() => {
    const bed = document.getElementById("bed");
    let count = 0;
    bed.addEventListener("change", () => { count += 1; });
    bed.addEventListener("input", () => { count += 1; });
    bed.value = "B-12";
    return `${count} events, value ${bed.value}`;
  });
  console.log(`dom-hidden-write-no-change: ${raised}`);
};

const checkListFormData = async browserPage => {
  await browserPage.evaluate(() => {
    const list = document.getElementById("days");
    const posted = list.querySelector("input[type=hidden]");
    const boxes = [...list.querySelectorAll("input[type=checkbox]")];
    list.addEventListener("change", () => { posted.value = boxes.filter(box => box.checked).map(box => box.value).join(","); });
  });
  await browserPage.click("#days_c0");
  await browserPage.click("#days_c2");
  const sent = await browserPage.evaluate(() => JSON.stringify(new FormData(document.getElementById("visit")).getAll("VisitDays")));
  console.log(`dom-check-list-form-data: ${sent}`);
};

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext();
  const browserPage = await context.newPage();
  await browserPage.route("http://probe.test/", route => route.fulfill({ contentType: "text/html", body: page }));
  await browserPage.goto("http://probe.test/");
  await changeListener(browserPage);
  await clickListener(browserPage);
  await hiddenWrite(browserPage);
  await checkListFormData(browserPage);
  await browser.close();
})();
