import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const bundle = require.resolve("@syncfusion/ej2/dist/ej2.min.js");

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
  window.heard = {};
  window.listen = (component, events) => events.forEach(event => component.addEventListener(event, args => {
    window.heard[event] = window.heard[event] || shape(args) + (args.action && typeof args.action === "object" ? " | action " + shape(args.action) : "");
  }));
  const detail = (value, depth = 0) => {
    const each = kind(value);
    if (each === "Array") return "Array<" + [...new Set(value.map(item => detail(item, depth + 1)))].join("|") + ">";
    if (each === "object" && depth < 3) return "{" + Object.keys(value).sort().map(key => key + ":" + detail(value[key], depth + 1)).join(",") + "}";
    return each;
  };
  window.detail = detail;
  window.listenAs = (component, prefix, events, members = []) => events.forEach(event => component.addEventListener(event, args => {
    const key = prefix + event;
    window.heard[key] = window.heard[key] || shape(args) + members.filter(member => member in args).map(member => " | " + member + " " + detail(args[member])).join("");
  }));
};


const press = async (page, selector, options) => {
  await withoutTrialBanner(page);
  await page.click(selector, options);
};

const withoutTrialBanner = page => page.evaluate(() => [...document.body.children].filter(child => child.tagName === "DIV" && (child.style.zIndex === "999999999" || child.attributes.length === 0 && child.firstElementChild?.style.zIndex === "99999")).forEach(child => child.remove()));

const click = async (page, selector) => {
  await withoutTrialBanner(page);
  await page.click(selector);
};

const grid = async page => {
  await page.evaluate(() => {
    ej.grids.Grid.Inject(ej.grids.Sort, ej.grids.Page, ej.grids.Toolbar, ej.grids.Selection);
    const residents = [{ id: 1, name: "Ada", ward: "North" }, { id: 2, name: "Rose", ward: "East" }];
    const shown = new ej.grids.Grid({
      dataSource: residents, allowSorting: true, allowPaging: true, pageSettings: { pageSize: 1 },
      toolbar: [{ text: "Discharge", id: "discharge" }],
      columns: [{ field: "id", headerText: "Id", isPrimaryKey: true }, { field: "name", headerText: "Name" }, { field: "ward", headerText: "Ward" }],
    });
    shown.appendTo("#residents");
    listen(shown, ["rowSelected", "recordClick", "toolbarClick", "actionBegin", "actionComplete"]);
    const bound = new ej.grids.Grid({
      dataSource: { result: residents, count: 2 }, allowSorting: true,
      columns: [{ field: "id", headerText: "Id" }, { field: "name", headerText: "Name" }],
    });
    bound.appendTo("#bound");
    listen(bound, ["dataStateChange"]);
  });
  await click(page, "#residents .e-row td >> nth=1");
  await click(page, "#residents .e-toolbar-item >> text=Discharge");
  await click(page, "#residents .e-headercell >> text=Name");
  await click(page, "#bound .e-headercell >> text=Name");
  const heard = await page.evaluate(() => window.heard);
  for (const event of ["rowSelected", "recordClick", "toolbarClick", "actionBegin", "actionComplete", "dataStateChange"])
    console.log(`ej2-payload-grid-${event}: ${heard[event] ?? "not raised"}`);
};

const schedule = async page => {
  await page.evaluate(() => {
    ej.schedule.Schedule.Inject(ej.schedule.Week);
    const shifts = new ej.schedule.Schedule({
      currentView: "Week", selectedDate: new Date(2026, 10, 9), views: ["Week"], height: "400px",
      eventSettings: { dataSource: [{ Id: 1, Subject: "Round", StartTime: new Date(2026, 10, 9, 9), EndTime: new Date(2026, 10, 9, 10) }] },
    });
    shifts.appendTo("#shifts");
    listen(shifts, ["navigating"]);
    window.shifts = shifts;
  });
  await click(page, "#shifts .e-toolbar-item.e-next");
  const heard = await page.evaluate(() => window.heard.navigating);
  console.log(`ej2-payload-schedule-navigating: ${heard ?? "not raised"}`);
  const saved = await page.evaluate(() => {
    const subject = () => window.shifts.eventSettings.dataSource[0].Subject;
    window.shifts.saveEvent({ Id: 1, Subject: "Rounds by text", StartTime: new Date(2026, 10, 9, 9), EndTime: new Date(2026, 10, 9, 10) }, 1);
    const byNumber = subject();
    window.shifts.saveEvent({ Id: 1, Subject: "Rounds by text", StartTime: new Date(2026, 10, 9, 9), EndTime: new Date(2026, 10, 9, 10) }, "Save");
    return `with 1 the subject is ${byNumber}; with 'Save' it is ${subject()}`;
  });
  console.log(`ej2-schedule-save-event-action: saveEvent ${saved}`);
};

const editorEvents = ["beginEdit", "endEdit", "change", "actionBegin", "actionSuccess", "submitClick", "cancelClick"];

const editorKinds = [
  { type: "Text", value: "Ada", typed: "Ada Byron" },
  { type: "Numeric", value: 3, typed: "4" },
  { type: "Date", value: [2026, 10, 9], typed: "11/12/2026" },
  { type: "DateRange", value: [[2026, 10, 9], [2026, 10, 16]], typed: "11/10/2026 - 11/17/2026" },
  { type: "MultiSelect", value: ["Penicillin"], pick: "Latex" },
];

const editors = async page => {
  await page.evaluate(([kinds, events]) => {
    ej.inplaceeditor.InPlaceEditor.Inject(ej.inplaceeditor.MultiSelect, ej.inplaceeditor.DateRangePicker);
    const date = parts => new Date(parts[0], parts[1], parts[2]);
    kinds.forEach(each => {
      const value = each.type === "Date" ? date(each.value) : each.type === "DateRange" ? each.value.map(date) : each.value;
      const model = each.type === "MultiSelect" ? { dataSource: ["Penicillin", "Latex", "Peanuts"] } : {};
      const editor = new ej.inplaceeditor.InPlaceEditor({ type: each.type, mode: "Inline", value, model });
      listenAs(editor, each.type + ":", events, ["value", "previousValue", "data"]);
      editor.appendTo("#editor-" + each.type);
    });
  }, [editorKinds, editorEvents]);
  for (const each of editorKinds) {
    const root = "#editor-" + each.type;
    await press(page, root + " .e-editable-value-wrapper");
    if (each.typed) await page.fill(root + " .e-editable-elements input:not([type=hidden]) >> nth=0", each.typed);
    else await press(page, `#editor-${each.type}_editor_popup .e-list-item >> text=${each.pick}`);
    await press(page, root + " .e-btn-save");
  }
  await press(page, "#editor-Text .e-editable-value-wrapper");
  await press(page, "#editor-Text .e-btn-cancel");
  const heard = await page.evaluate(() => window.heard);
  for (const event of editorEvents) console.log(`ej2-payload-inplaceeditor-${event}: ${heard["Text:" + event] ?? "not raised"}`);
  for (const each of editorKinds) console.log(`ej2-inplaceeditor-change-${each.type}: ${heard[each.type + ":change"] ?? "not raised"}`);
  console.log(`ej2-inplaceeditor-action-begin-values: ${editorKinds.map(each => each.type + " " + (heard[each.type + ":actionBegin"] ?? "not raised").split(" | data ")[1]).join("; ")}`);
};

const chips = async page => {
  await page.evaluate(() => {
    const allergies = [{ text: "Penicillin", value: "pen" }, { text: "Latex", value: "lat" }, { text: "Peanuts", value: "pea" }];
    window.chipLists = {
      single: new ej.buttons.ChipList({ chips: allergies, selection: "Single" }),
      multiple: new ej.buttons.ChipList({ chips: allergies, selection: "Multiple" }),
      texts: new ej.buttons.ChipList({ chips: ["Penicillin", "Latex"], selection: "Single" }),
      multipleTexts: new ej.buttons.ChipList({ chips: ["Penicillin", "Latex", "Peanuts"], selection: "Multiple" }),
    };
    Object.entries(window.chipLists).forEach(([key, list]) => list.appendTo("#chips-" + key));
  });
  await press(page, "#chips-single .e-chip >> text=Latex");
  await press(page, "#chips-multiple .e-chip >> text=Penicillin");
  await press(page, "#chips-multiple .e-chip >> text=Peanuts");
  await press(page, "#chips-texts .e-chip >> text=Latex");
  await press(page, "#chips-multipleTexts .e-chip >> text=Penicillin");
  await press(page, "#chips-multipleTexts .e-chip >> text=Peanuts");
  const read = await page.evaluate(() => Object.entries(window.chipLists).map(([key, list]) => `ej2-chiplist-${key}-selection: selectedChips ${detail(list.selectedChips)}; getSelectedChips() ${detail(list.getSelectedChips())}`));
  read.forEach(line => console.log(line));
};

const boards = async page => {
  await page.evaluate(() => {
    const board = (id, keys, status) => {
      const shown = new ej.kanban.Kanban({
        keyField: "status", columns: keys.map(key => ({ headerText: "Column " + key, keyField: key })),
        dataSource: [{ id: 1, status, summary: "Round" }], cardSettings: { contentField: "summary", headerField: "id" },
      });
      listenAs(shown, id + ":", ["queryCellInfo"], ["data"]);
      shown.appendTo("#" + id);
    };
    board("board-text", ["Open", "Done"], "Open");
    board("board-number", [1, 2], 1);
  });
  await page.waitForFunction(() => window.heard["board-text:queryCellInfo"] && window.heard["board-number:queryCellInfo"], null, { timeout: 10000 }).catch(() => {});
  const heard = await page.evaluate(() => window.heard);
  for (const id of ["text", "number"]) console.log(`ej2-payload-kanban-queryCellInfo-${id}: ${heard["board-" + id + ":queryCellInfo"] ?? "not raised"}`);
};

const menus = async page => {
  await page.evaluate(() => {
    const items = [{ text: "Ward", items: [{ text: "North" }, { text: "East" }] }];
    const menu = new ej.navigations.Menu({ items });
    listenAs(menu, "menu:", ["beforeOpen", "beforeClose"]);
    menu.appendTo("#menu");
    const context = new ej.navigations.ContextMenu({ target: "#context-area", items });
    listenAs(context, "contextmenu:", ["beforeOpen", "beforeClose"]);
    context.appendTo("#context");
  });
  await press(page, "#menu .e-menu-item >> text=Ward");
  await page.keyboard.press("Escape");
  await press(page, "#context-area", { button: "right" });
  await page.keyboard.press("Escape");
  const heard = await page.evaluate(() => window.heard);
  for (const id of ["menu", "contextmenu"]) for (const event of ["beforeOpen", "beforeClose"]) console.log(`ej2-payload-${id}-${event}: ${heard[id + ":" + event] ?? "not raised"}`);
};

const batch = async page => {
  await page.evaluate(() => {
    ej.grids.Grid.Inject(ej.grids.Edit);
    const shown = new ej.grids.Grid({
      dataSource: [{ id: 1, name: "Ada" }, { id: 2, name: "Rose" }],
      editSettings: { allowEditing: true, mode: "Batch", showConfirmDialog: false },
      columns: [{ field: "id", headerText: "Id", isPrimaryKey: true }, { field: "name", headerText: "Name" }],
    });
    listenAs(shown, "batch:", ["dataBound", "beforeBatchSave"], ["batchChanges"]);
    shown.appendTo("#batch");
    window.batchGrid = shown;
  });
  await page.waitForFunction(() => window.heard["batch:dataBound"], null, { timeout: 10000 });
  await withoutTrialBanner(page);
  await page.dblclick("#batch .e-row >> nth=0 >> td >> nth=1");
  await page.fill("#batch td.e-editedbatchcell input", "Ada Byron");
  await page.keyboard.press("Enter");
  console.log(`ej2-grid-get-batch-changes: ${await page.evaluate(() => detail(window.batchGrid.getBatchChanges()))}`);
  await page.evaluate(() => window.batchGrid.endEdit());
  console.log(`ej2-payload-grid-beforeBatchSave: ${(await page.evaluate(() => window.heard["batch:beforeBatchSave"])) ?? "not raised"}`);
};

const editorHosts = editorKinds.map(each => `<div id="editor-${each.type}"></div>`).join("");

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ timezoneId: "America/Los_Angeles" });
  const page = await context.newPage();
  await page.route("http://probe.test/", route => route.fulfill({ contentType: "text/html", body: '<!doctype html><div id="residents"></div><div id="bound"></div><div id="shifts"></div>' + editorHosts + '<div id="chips-single"></div><div id="chips-multiple"></div><div id="chips-texts"></div><div id="chips-multipleTexts"></div><div id="board-text"></div><div id="board-number"></div><ul id="menu"></ul><div id="context-area">Bed 4</div><ul id="context"></ul><div id="batch"></div>' }));
  await page.goto("http://probe.test/");
  await page.addScriptTag({ path: bundle });
  await page.evaluate(describe);
  await grid(page);
  await schedule(page);
  await batch(page);
  await editors(page);
  await chips(page);
  await boards(page);
  await menus(page);
  await browser.close();
})();
