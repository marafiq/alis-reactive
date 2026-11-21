import assert from "node:assert/strict";
import { createServer } from "node:http";
import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require(process.env.PLAYWRIGHT_PACKAGE);
const versions = Object.fromEntries(["ej2", "ej2-querybuilder", "ej2-data"].map(name =>
  [name, require("@syncfusion/" + name + "/package.json").version]));
const lock = require("../../package-lock.json").packages;
for (const [name, version] of Object.entries(versions)) {
  assert.equal(version, lock["node_modules/@syncfusion/" + name].version);
}

const server = createServer((_request, response) => {
  response.writeHead(200, { "Content-Type": "text/html" });
  response.end("<!doctype html><html lang='en'><title>Query rules</title><div id='query'></div><button>Apply</button></html>");
});
await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
let browser;
try {
  browser = await chromium.launch();
  const page = await browser.newPage();
  const errors = [];
  page.on("pageerror", error => errors.push(error.message));
  await page.goto("http://127.0.0.1:" + server.address().port);
  await page.addScriptTag({ path: require.resolve("@syncfusion/ej2/dist/ej2.min.js") });
  await page.addStyleTag({ path: require.resolve("@syncfusion/ej2/bootstrap5.3.css") });
  await page.evaluate(() => {
    const rows = [
      { name: "Ada", wing: "North", days: 7, escalated: false },
      { name: "Ben", wing: "North", days: 2, escalated: true },
      { name: "Cora", wing: "North", days: 2, escalated: false },
      { name: "Dev", wing: "South", days: 11, escalated: false },
      { name: "Eve", wing: "South", days: 2, escalated: true },
      { name: "Finn", wing: "South", days: 2, escalated: false }
    ];
    const query = new ej.querybuilder.QueryBuilder({
      allowValidation: true,
      columns: [
        { field: "name", label: "Resident", type: "string" },
        { field: "wing", label: "Wing", type: "string" },
        { field: "days", label: "Days overdue", type: "number", validation: { isRequired: true, min: 0, max: 90 } },
        { field: "escalated", label: "Escalated", type: "boolean" },
        { field: "due", label: "Due date", type: "date", format: "yyyy-MM-dd" }
      ],
      dataSource: rows,
      rule: {
        condition: "and", rules: [
          { field: "wing", label: "Wing", type: "string", operator: "equal", value: "North" },
          { condition: "or", rules: [
            { field: "days", label: "Days overdue", type: "number", operator: "greaterthanorequal", value: 7 },
            { field: "escalated", label: "Escalated", type: "boolean", operator: "equal", value: true }
          ] }
        ]
      }
    });
    query.appendTo("#query");
    window.contract = { query, rows };
  });
  const observe = async () => page.evaluate(async () => {
    const { query, rows } = window.contract;
    const raw = query.getRules();
    const valid = query.getValidRules();
    const predicate = query.getPredicate(valid);
    const filtered = query.getFilteredRecords();
    const isPromise = typeof filtered?.then === "function";
    const answer = await filtered;
    const compiled = query.getDataManagerQuery(valid);
    return {
      raw: JSON.parse(JSON.stringify(raw)), valid, editorsValid: query.validateFields(),
      predicateMatches: rows.filter(row => predicate.validate(row)).map(row => row.name),
      isPromise, resultIsArray: Array.isArray(answer.result), filtered: answer.result.map(row => row.name),
      queryOperations: compiled.queries.map(op => op.fn),
      dataManagerMatches: new ej.data.DataManager(rows).executeLocal(compiled).map(row => row.name)
    };
  });
  const initial = await observe();
  const wing = page.getByRole("textbox", { name: "Enter Value", exact: true });
  const snapshot = await page.locator("#query").ariaSnapshot();
  if (await wing.count() !== 1) throw Error("The wing value has no unique accessible text box.\n" + snapshot);
  await wing.fill("South");
  await page.getByRole("button", { name: "Apply", exact: true }).click();
  const edited = await observe();
  await wing.fill("");
  await page.getByRole("button", { name: "Apply", exact: true }).click();
  const incomplete = await observe();
  const cases = await page.evaluate(() => {
    const { query } = window.contract;
    const leaf = (field, type, operator, value) => ({ field, label: field, type, operator, value });
    const tree = { condition: "and", rules: [
      leaf("days", "number", "between", [0, 7]), leaf("wing", "string", "in", ["North", "South"]),
      leaf("days", "number", "equal", 0), leaf("escalated", "boolean", "equal", false),
      leaf("wing", "string", "isnull", "discarded"), leaf("wing", "string", "equal", ""),
      leaf("days", "number", "between", [0, null]), leaf("wing", "string", "in", [])
    ] };
    const validLeaves = query.getValidRules(tree).rules;
    const unrecognized = query.getValidRules({ condition: "and", rules: [
      leaf("unlisted", "string", "unrecognizedOperator", "retained")
    ] });
    const empty = query.getValidRules({ condition: "and", rules: [{ condition: "or", rules: [] }] });
    const complete = query.getValidRules({ condition: "and", rules: [
      leaf("wing", "string", "equal", "north")
    ] });
    const row = { wing: "North" };
    const ignoreCase = query.getPredicate(complete).validate(row);
    query.matchCase = true;
    const matchCase = query.getPredicate(complete).validate(row);
    query.setRules({ condition: "and", rules: [leaf("wing", "string", "equal", "")] });
    const enabled = query.validateFields();
    query.allowValidation = false;
    const emptyEditorWithoutValidation = query.validateFields();
    const single = rule => query.getValidRules({ condition: "and", rules: [rule] });
    const between = query.getPredicate(single(leaf("days", "number", "between", [0, 7])));
    const insideRange = [-1, 0, 7, 8].map(days => between.validate({ days }));
    const membership = query.getPredicate(single(leaf("wing", "string", "in", ["North", "South"])));
    const insideList = ["North", "South", "West"].map(wing => membership.validate({ wing }));
    const sameDate = query.getPredicate(single(leaf("due", "date", "equal", "2026-11-20")));
    const dateMatches = [new Date(2026, 10, 20, 15), new Date(2026, 10, 21, 9)].map(due => sameDate.validate({ due }));
    query.enableNotCondition = true;
    const notGroup = query.getValidRules({ condition: "and", not: true, isLocked: false,
      rules: [{ ...leaf("wing", "string", "equal", "North"), isLocked: true }] });
    const notGroupMatches = ["North", "South"].map(wing => query.getPredicate(notGroup).validate({ wing }));
    query.enableSeparateConnector = true;
    const separate = query.getValidRules({ condition: "and", rules: [
      { ...leaf("wing", "string", "equal", "North"), condition: "or" }, leaf("days", "number", "greaterthan", 7)
    ] });
    const connectorMatches = query.getPredicate(separate).validate({wing: "South", days: 9});
    return { validLeaves, unrecognized, empty, emptyPredicateType: typeof query.getPredicate(empty),
      ignoreCase, matchCase, emptyEditorWithValidation: enabled, emptyEditorWithoutValidation,
      insideRange, insideList, dateMatches, notGroup, notGroupMatches, separate, connectorMatches };
  });
  assert.deepEqual(initial.filtered, ["Ada", "Ben"]);
  assert.deepEqual(edited.filtered, ["Dev", "Eve"]);
  assert.deepEqual(incomplete.filtered, ["Ada", "Ben", "Dev", "Eve"]);
  assert.equal(incomplete.editorsValid, false);
  for (const reading of [initial, edited, incomplete]) {
    assert.deepEqual(reading.predicateMatches, reading.filtered);
    assert.deepEqual(reading.dataManagerMatches, reading.filtered);
    assert.ok(reading.isPromise && reading.resultIsArray);
  }
  assert.deepEqual(cases.validLeaves.map(rule => rule.value), [[0, 7], ["North", "South"], 0, false, null]);
  assert.equal(cases.unrecognized.rules[0].field, "unlisted");
  assert.equal(cases.unrecognized.rules[0].operator, "unrecognizedOperator");
  assert.deepEqual(cases.empty, {});
  assert.equal(cases.emptyPredicateType, "undefined");
  assert.equal(cases.ignoreCase, true);
  assert.equal(cases.matchCase, false);
  assert.equal(cases.emptyEditorWithValidation, false);
  assert.equal(cases.emptyEditorWithoutValidation, true);
  assert.deepEqual(cases.insideRange, [false, true, true, false]);
  assert.deepEqual(cases.insideList, [true, true, false]);
  assert.deepEqual(cases.dateMatches, [true, false]);
  assert.equal(cases.notGroup.not, true);
  assert.equal(cases.notGroup.isLocked, false);
  assert.equal(cases.notGroup.rules[0].isLocked, true);
  assert.deepEqual(cases.notGroupMatches, [true, false]);
  assert.equal(cases.separate.rules[0].condition, "or");
  assert.equal(cases.connectorMatches, false);
  assert.deepEqual(errors, []);
  console.log(JSON.stringify({versions, initial, edited, incomplete, cases}, null, 2));
} finally {
  await browser?.close();
  await new Promise(resolve => server.close(resolve));
}
