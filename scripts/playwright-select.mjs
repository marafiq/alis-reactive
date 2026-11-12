#!/usr/bin/env node
import { readFileSync, writeFileSync } from "node:fs";

const usage = `usage: node scripts/playwright-select.mjs --spec <file> --assembly <file> --root <namespace> --out <file> <selector> [--host <name>]

Joins the tests the spec lands (spec/tools/landings.py) with the tests an assembly holds (scripts/list-tests.cs),
writes the cases one run must run to --out and prints the VSTest filter that selects them.

Selectors, one a run:
  --all                 every test the assembly holds
  --unit U<n>           the tests the spec lands in that unit
  --through U<n>        the tests the spec lands in that unit and in every unit before it
  --feature M<n>.F<n>   the tests of that feature
  --screen <id>         the tests app.json places on that screen
  --test <constant>     one test, by its promise constant as a test writes it after Promises. (M3_F1.X8_27_2)
  <name>                a folder or a fixture of the test project, by its name

--host <name> takes that host alone. Without it a run takes the hosts the conformance app serves on this machine.`;

const selectorsWithAValue = ["--unit", "--through", "--feature", "--screen", "--test"];

function refuse(message) {
  console.error(`[playwright:select] ${message}`);
  process.exit(2);
}

function parse(argv) {
  const options = { selectors: [] };
  for (let index = 0; index < argv.length; index += 1) {
    const word = argv[index];
    const valued = ["--spec", "--assembly", "--root", "--out", "--host", ...selectorsWithAValue].includes(word);
    if (valued && index + 1 >= argv.length) refuse(`${word} takes a value.\n${usage}`);
    if (["--spec", "--assembly", "--root", "--out", "--host"].includes(word)) options[word.slice(2)] = argv[(index += 1)];
    else if (selectorsWithAValue.includes(word)) options.selectors.push({ kind: word.slice(2), value: argv[(index += 1)] });
    else if (word === "--all") options.selectors.push({ kind: "all", value: "all" });
    else if (word.startsWith("-")) refuse(`${word} is no option.\n${usage}`);
    else options.selectors.push({ kind: "name", value: word });
  }
  for (const needed of ["spec", "assembly", "root", "out"]) {
    if (options[needed] === undefined) refuse(`--${needed} is missing.\n${usage}`);
  }
  if (options.selectors.length !== 1) refuse(`a run takes one selector, and ${options.selectors.length} were given.\n${usage}`);
  return options;
}

function rows(path, columns) {
  return readFileSync(path, "utf8").split("\n").filter(line => line !== "").map(line => {
    const cells = line.split("\t");
    if (cells.length !== columns.length) refuse(`${path} holds a line of ${cells.length} cells where ${columns.length} are read: ${line}`);
    return Object.fromEntries(columns.map((column, index) => [column, cells[index]]));
  });
}

function unitNumber(unit) {
  const read = /^U([1-9][0-9]*)$/.exec(unit);
  if (read === null) refuse(`'${unit}' is no unit; a unit is written U and its number.`);
  return Number(read[1]);
}

function wanted(selector, landed, written, root) {
  switch (selector.kind) {
    case "all":
      return new Set(written.keys());
    case "unit":
      unitNumber(selector.value);
      return new Set(landed.filter(row => row.unit === selector.value).map(row => row.promise));
    case "through": {
      const last = unitNumber(selector.value);
      return new Set(landed.filter(row => unitNumber(row.unit) <= last).map(row => row.promise));
    }
    case "feature":
      return new Set(landed.filter(row => row.promise.startsWith(`${selector.value} `)).map(row => row.promise));
    case "screen":
      return new Set(landed.filter(row => row.screen === selector.value).map(row => row.promise));
    case "test":
      return new Set(landed.filter(row => row.constant === selector.value).map(row => row.promise));
    default:
      return new Set([...written].filter(([, test]) => test.fixture.slice(root.length + 1).split(".").includes(selector.value)).map(([promise]) => promise));
  }
}

const options = parse(process.argv.slice(2));
const selector = options.selectors[0];
const landed = rows(options.spec, ["promise", "constant", "unit", "screen", "host", "served"]);
const written = new Map();
for (const test of rows(options.assembly, ["promise", "fixture", "method"])) {
  if (written.has(test.promise)) refuse(`two tests of the assembly carry '${test.promise}': ${written.get(test.promise).method} and ${test.method}.`);
  written.set(test.promise, test);
}

const ofTheSpec = new Set(landed.map(row => row.promise));
const unknown = [...written.keys()].filter(promise => !ofTheSpec.has(promise));
if (unknown.length > 0) refuse(`the assembly holds a test on '${unknown[0]}', no promise of the spec, ${unknown.length} such in all; build the test project again.`);

const selected = wanted(selector, landed, written, options.root);
if (selected.size === 0) refuse(`'${selector.value}' selects no test.`);
const unwritten = [...selected].filter(promise => !written.has(promise));
if (unwritten.length > 0) {
  refuse(`'${selector.value}' selects ${selected.size} tests of the spec; the assembly holds no test for ${unwritten.length} of them, the first '${unwritten[0]}'.`);
}

const taken = row => (options.host === undefined ? row.served === "pool" : row.host === options.host);
const cases = landed.filter(row => selected.has(row.promise) && taken(row)).map(row => {
  const test = written.get(row.promise);
  return { fixture: test.fixture, method: test.method, id: `${test.fixture}("${row.host}").${test.method}` };
});
if (cases.length === 0) {
  const where = options.host === undefined ? "a host the conformance app serves on this machine" : `the host '${options.host}'`;
  refuse(`'${selector.value}' selects no test under ${where}; the spec runs what it selects, ${selected.size}, under other hosts.`);
}

const methodsOf = new Map();
for (const test of written.values()) methodsOf.set(test.fixture, (methodsOf.get(test.fixture) ?? 0) + 1);
const selectedOf = new Map();
for (const test of new Map(cases.map(one => [`${one.fixture}.${one.method}`, one])).values()) {
  selectedOf.set(test.fixture, [...(selectedOf.get(test.fixture) ?? []), test.method]);
}
const clauses = [...selectedOf].sort(([one], [other]) => (one < other ? -1 : 1)).flatMap(([fixture, methods]) => {
  const namedLonger = [...methodsOf.keys()].filter(other => other !== fixture && other.includes(fixture)).sort();
  const ofTheFixture = [`FullyQualifiedName~${fixture}`, ...namedLonger.map(other => `FullyQualifiedName!~${other}`)].join("&");
  if (methods.length < methodsOf.get(fixture)) return methods.sort().map(method => `(${ofTheFixture}&Name=${method})`);
  return [namedLonger.length === 0 ? ofTheFixture : `(${ofTheFixture})`];
});
const everyTest = [...methodsOf].every(([fixture, methods]) => (selectedOf.get(fixture) ?? []).length === methods);
const otherHosts = [...new Set(landed.filter(row => selected.has(row.promise) && !taken(row)).map(row => row.host))].sort();
const hosts = options.host === undefined ? otherHosts.map(host => `TestCategory!=${host}`) : [`TestCategory=${options.host}`];
const tests = everyTest ? [] : [clauses.length === 1 ? clauses[0] : `(${clauses.join("|")})`];
const filter = [...tests, ...hosts].join("&");

const selection = selector.kind === "through" ? `through ${selector.value}` : selector.value;
const label = options.host === undefined ? selection : `${selection}@${options.host}`;
writeFileSync(options.out, `${JSON.stringify({ label, filter, cases: cases.map(one => one.id).sort() }, null, 2)}\n`);
console.log(filter);
