#!/usr/bin/env node
import { existsSync, readFileSync, writeFileSync } from "node:fs";

const usage = `usage: node scripts/playwright-select.mjs --spec <file> --assembly <file> --root <namespace> --weights <file> --out <file> <selector> [--host <name>] [--shard <i>/<k>]

Joins the tests the spec lands (spec/tools/landings.py) with the tests an assembly holds (scripts/list-tests.cs),
writes the cases one run must run to --out and prints the VSTest filter that selects them.

Selectors, one a run:
  --all                 every test the assembly holds
  --unit U<n>           the tests the spec lands in that unit
  --through U<n>        the tests the spec lands in that unit and in every unit before it
  --feature M<n>.F<n>   the tests of that feature
  --screen <id>         the tests app.json places on that screen
  --test <constant>     one test, by its promise constant as a test writes it after Promises. (M3_F1.X8_27_2)
  --pool <name>         one pool of the tests the assembly holds: fusion, the Fusion module folders; hosts, every
                        other test under a host variant; core, every other test under net10
  <name>                a folder or a fixture of the test project, by its name

--host <name> takes that host alone. Without it a run takes the hosts the conformance app serves on this machine.
--shard <i>/<k> takes the i-th of a pool's k shards. A shard unit is a module folder, a slice under FusionComponents,
and the hosts pool whole; units go longest first, each onto the lightest shard, by the seconds --weights holds.`;

const selectorsWithAValue = ["--unit", "--through", "--feature", "--screen", "--test", "--pool"];
const pools = ["core", "hosts", "fusion"];
const fusionFolders = ["FusionComponents", "FusionTemplates", "FusionAppLevelObjects"];
const folderPartedBySlice = "FusionComponents";
const theAppsOwnHost = "net10";

function refuse(message) {
  console.error(`[playwright:select] ${message}`);
  process.exit(2);
}

function parse(argv) {
  const options = { selectors: [] };
  for (let index = 0; index < argv.length; index += 1) {
    const word = argv[index];
    const valued = ["--spec", "--assembly", "--root", "--weights", "--out", "--host", "--shard", ...selectorsWithAValue].includes(word);
    if (valued && index + 1 >= argv.length) refuse(`${word} takes a value.\n${usage}`);
    if (["--spec", "--assembly", "--root", "--weights", "--out", "--host", "--shard"].includes(word)) options[word.slice(2)] = argv[(index += 1)];
    else if (selectorsWithAValue.includes(word)) options.selectors.push({ kind: word.slice(2), value: argv[(index += 1)] });
    else if (word === "--all") options.selectors.push({ kind: "all", value: "all" });
    else if (word.startsWith("-")) refuse(`${word} is no option.\n${usage}`);
    else options.selectors.push({ kind: "name", value: word });
  }
  for (const needed of ["spec", "assembly", "root", "weights", "out"]) {
    if (options[needed] === undefined) refuse(`--${needed} is missing.\n${usage}`);
  }
  if (options.selectors.length !== 1) refuse(`a run takes one selector, and ${options.selectors.length} were given.\n${usage}`);
  return options;
}

function shardOf(options, selector) {
  if (options.shard === undefined) return { index: 1, count: 1 };
  if (selector.kind !== "pool") refuse(`--shard parts a pool, and '${selector.value}' is none; a shard is taken with --pool.`);
  const read = /^([1-9][0-9]*)\/([1-9][0-9]*)$/.exec(options.shard);
  if (read === null || Number(read[1]) > Number(read[2])) refuse(`--shard takes <i>/<k>, the i-th of k shards, and '${options.shard}' is not that.`);
  return { index: Number(read[1]), count: Number(read[2]) };
}

function unitsOnShard(units, weights, shard) {
  const shards = Array.from({ length: shard.count }, () => ({ seconds: 0, units: [] }));
  const longestFirst = [...units].sort((one, other) => (weights[other] ?? 0) - (weights[one] ?? 0) || (one < other ? -1 : 1));
  for (const unit of longestFirst) {
    const lightest = shards.reduce((light, other) => (other.seconds < light.seconds ? other : light), shards[0]);
    lightest.units.push(unit);
    lightest.seconds += weights[unit] ?? 0;
  }
  return new Set(shards[shard.index - 1].units);
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
    case "pool":
      if (!pools.includes(selector.value)) refuse(`'${selector.value}' is no pool; the pools are ${pools.join(", ")}.`);
      return new Set(written.keys());
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

const partsOf = test => test.fixture.slice(options.root.length + 1).split(".");
const poolOf = (row, test) => (fusionFolders.includes(partsOf(test)[0]) ? "fusion" : row.host === theAppsOwnHost ? "core" : "hosts");
const unitOf = (row, test) => {
  if (row.served !== "pool") return row.served;
  if (poolOf(row, test) === "hosts") return "hosts";
  const [folder, slice] = partsOf(test);
  return folder === folderPartedBySlice ? `${folder}.${slice}` : folder;
};
const shard = shardOf(options, selector);
const served = row => (options.host === undefined ? row.served === "pool" : row.host === options.host);
const ofThePool = row => selector.kind !== "pool" || poolOf(row, written.get(row.promise)) === selector.value;
const inReach = landed.filter(row => selected.has(row.promise) && served(row) && ofThePool(row));
const weights = existsSync(options.weights) ? JSON.parse(readFileSync(options.weights, "utf8")) : {};
const onShard = unitsOnShard(new Set(inReach.map(row => unitOf(row, written.get(row.promise)))), weights, shard);
const taken = inReach.filter(row => onShard.has(unitOf(row, written.get(row.promise))));
const cases = taken.map(row => {
  const test = written.get(row.promise);
  return { fixture: test.fixture, method: test.method, host: row.host, unit: unitOf(row, test), id: `${test.fixture}("${row.host}").${test.method}` };
});
if (cases.length === 0) {
  if (selector.kind === "pool") {
    refuse(inReach.length === 0
      ? `the pool '${selector.value}' holds no test of the assembly.`
      : `shard ${shard.index}/${shard.count} of the pool '${selector.value}' takes no unit; the pool holds ${new Set(inReach.map(row => unitOf(row, written.get(row.promise)))).size}.`);
  }
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
const hostsTaken = new Set(cases.map(one => one.host));
const otherHosts = [...new Set(landed.filter(row => selected.has(row.promise) && !hostsTaken.has(row.host)).map(row => row.host))].sort();
const hosts = options.host === undefined ? otherHosts.map(host => `TestCategory!=${host}`) : [`TestCategory=${options.host}`];
const tests = everyTest ? [] : [clauses.length === 1 ? clauses[0] : `(${clauses.join("|")})`];
const filter = [...tests, ...hosts].join("&");

const selection = selector.kind === "through" ? `through ${selector.value}` : selector.value;
const label = options.host === undefined ? selection : `${selection}@${options.host}`;
const units = Object.fromEntries(cases.map(one => [one.id, one.unit]).sort(([one], [other]) => (one < other ? -1 : 1)));
writeFileSync(options.out, `${JSON.stringify({ label, shard: `${shard.index}/${shard.count}`, filter, cases: cases.map(one => one.id).sort(), units }, null, 2)}\n`);
console.log(filter);
