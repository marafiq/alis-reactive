#!/usr/bin/env node
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { cpus, release, type } from "node:os";

const usage = `usage: node scripts/playwright-result.mjs --selection <file> --trx <file> --budget <file> [--servers-left <n>]
       node scripts/playwright-result.mjs --measure --selection <file> --budget <file> --weights <file> <file.trx>...
       node scripts/playwright-result.mjs --measure-parallelism --selection <file> --budget <file> --at 1 <file.trx>... --at 2 <file.trx>... --at 4 <file.trx>... --at 8 <file.trx>...
       node scripts/playwright-result.mjs --at-once --budget <file>
       node scripts/playwright-result.mjs --proof --full <selection file> --budget <file> <run folder>...

Reads what one run of the browser tests wrote and prints its result line:
  [playwright:result] <selection> <i>/<k>: N selected, N ran, N passed, 0 failed, 0 over budget, slowest <s>s <test>
It exits 1 when the run wrote no TRX, selected no test, ran other than it selected, failed a test, ran a test over
the budget, or left a server alive, each said on a line of its own before the result line.

--measure reads the TRX of each of 20 green runs of one selection, and beside each TRX the steps the harness wrote
(<name>.steps.tsv: a test, the holder and the method of one call of Playwright, its milliseconds), and writes the
budget, four times the slowest test, and the action wait and the navigation wait, each four times the slowest step
of its kind: a navigation is goto, reload, goBack or goForward, an action every other call on a frame. It writes
to --weights, for each shard unit the selection names, the seconds its tests took in one run, the mean of the 20.

--measure-parallelism reads 20 runs at each of 1, 2, 4 and 8 tests at once, each run's wall time from its TRX, and
writes the count a run takes: the quickest of 2, 4 and 8 whose runs are green and took, together, no more than the
runs at 1 divided by 1.6, else 1. --at-once prints the count the budget file holds.

--proof reads many runs against the full list, the selection --full names: each run folder holds its selection.json
and its run.trx. It exits 1 when a run is no green run, when a test of the full list ran in no run or in more than
one, or when a run ran a test the full list lacks, and prints
  [playwright:proof] <k> runs: N of the full list, N ran, each once`;

const measuredRuns = 20;
const timesTheSlowest = 4;
const navigations = ["goto", "reload", "goBack", "goForward"];
const holderOfAnAction = "Frame";
const waits = ["action", "navigation"];
const countsAtOnce = [1, 2, 4, 8];
const timesQuicker = 1.6;

function refuse(message) {
  console.error(`[playwright:result] ${message}`);
  process.exit(2);
}

function parse(argv) {
  const options = { "servers-left": "0", measure: false, parallelism: false, atOnce: false, proof: false, trxFiles: [], runsAt: new Map() };
  let files = options.trxFiles;
  for (let index = 0; index < argv.length; index += 1) {
    const word = argv[index];
    if (word === "--measure") options.measure = true;
    else if (word === "--measure-parallelism") options.parallelism = true;
    else if (word === "--at-once") options.atOnce = true;
    else if (word === "--proof") options.proof = true;
    else if (word === "--at") {
      if (index + 1 >= argv.length) refuse(`${word} takes a value.\n${usage}`);
      files = [];
      options.runsAt.set(argv[(index += 1)], files);
    } else if (["--selection", "--trx", "--budget", "--weights", "--full", "--servers-left"].includes(word)) {
      if (index + 1 >= argv.length) refuse(`${word} takes a value.\n${usage}`);
      options[word.slice(2)] = argv[(index += 1)];
    } else if (word.startsWith("-")) refuse(`${word} is no option.\n${usage}`);
    else files.push(word);
  }
  const needed = options.atOnce ? ["budget"]
    : options.proof ? ["full", "budget"]
    : options.measure ? ["selection", "budget", "weights"]
    : options.parallelism ? ["selection", "budget"]
    : ["selection", "trx", "budget"];
  for (const name of needed) {
    if (options[name] === undefined) refuse(`--${name} is missing.\n${usage}`);
  }
  if (!/^[0-9]+$/.test(options["servers-left"])) refuse(`--servers-left takes a count, and '${options["servers-left"]}' is not one.`);
  return options;
}

function attribute(tag, name) {
  const read = new RegExp(`\\s${name}="([^"]*)"`).exec(tag);
  return read === null ? undefined : read[1].replace(/&quot;/g, '"').replace(/&apos;/g, "'").replace(/&lt;/g, "<").replace(/&gt;/g, ">").replace(/&amp;/g, "&");
}

function seconds(duration) {
  if (duration === undefined) return 0;
  const [hours, minutes, rest] = duration.split(":");
  return Number(hours) * 3600 + Number(minutes) * 60 + Number(rest);
}

function resultsOf(trxPath) {
  const xml = readFileSync(trxPath, "utf8");
  const idOf = new Map();
  for (const test of xml.matchAll(/<UnitTest\s[^>]*>[\s\S]*?<\/UnitTest>/g)) {
    const method = /<TestMethod\s[^>]*>/.exec(test[0]);
    const testId = attribute(/<UnitTest\s[^>]*>/.exec(test[0])[0], "id");
    if (method !== null) idOf.set(testId, `${attribute(method[0], "className")}.${attribute(method[0], "name")}`);
  }
  return [...xml.matchAll(/<UnitTestResult\s[^>]*>/g)].map(result => ({
    id: idOf.get(attribute(result[0], "testId")) ?? attribute(result[0], "testName"),
    outcome: attribute(result[0], "outcome"),
    seconds: seconds(attribute(result[0], "duration")),
  }));
}

function hundredths(seconds) {
  return Math.round(seconds * 100);
}

function budgetOf(path) {
  if (!existsSync(path)) return { problem: `no budget is measured at ${path}; scripts/playwright.sh --measure writes it` };
  const measured = JSON.parse(readFileSync(path, "utf8"));
  if (measured.runs < measuredRuns) return { problem: `the budget at ${path} is measured over ${measured.runs} runs; a budget is measured over ${measuredRuns} green runs` };
  if (hundredths(measured.budgetSeconds) !== timesTheSlowest * hundredths(measured.slowestSeconds)) {
    return { problem: `the budget at ${path} is ${measured.budgetSeconds}s and its slowest test ${measured.slowestSeconds}s; a budget is four times the slowest test measured, never chosen` };
  }
  for (const kind of waits) {
    const wait = measured[`${kind}WaitMilliseconds`];
    const slowest = measured[`${slowestOf(kind)}Milliseconds`];
    if (wait === undefined || slowest === undefined) return { problem: `no ${kind} wait is measured at ${path}; scripts/playwright.sh --measure writes it` };
    if (wait !== timesTheSlowest * slowest) {
      return { problem: `the ${kind} wait at ${path} is ${wait}ms and its slowest step ${slowest}ms; a wait is four times the slowest step measured, never chosen` };
    }
  }
  if (measured.testsAtOnce === undefined || measured.wallSeconds === undefined) {
    return { problem: `no count of tests at once is measured at ${path}; scripts/playwright.sh --measure-parallelism writes it` };
  }
  if (measured.testsAtOnce !== keptOf(measured.wallSeconds)) {
    return { problem: `the count of tests at once at ${path} is ${measured.testsAtOnce} and its wall times keep ${keptOf(measured.wallSeconds)}; a count is measured, never chosen` };
  }
  if (measured.budgetAtOnce !== measured.testsAtOnce) {
    return { problem: `the budget at ${path} is measured at ${measured.budgetAtOnce} at once and a run takes ${measured.testsAtOnce}; scripts/playwright.sh --measure measures it again` };
  }
  return { seconds: measured.budgetSeconds };
}

function keptOf(wallSeconds) {
  const quickEnough = countsAtOnce.slice(1).filter(count => wallSeconds[count] !== undefined && wallSeconds[count] * timesQuicker <= wallSeconds[1]);
  return quickEnough.reduce((kept, count) => (wallSeconds[count] < wallSeconds[kept] ? count : kept), quickEnough[0] ?? 1);
}

function wallSecondsOf(trxPath) {
  const times = /<Times\s[^>]*>/.exec(readFileSync(trxPath, "utf8"));
  if (times === null) refuse(`${trxPath} holds no Times, so the run's wall time is not read.`);
  const moment = name => Date.parse(attribute(times[0], name).replace(/(\.\d{3})\d+/, "$1"));
  return (moment("finish") - moment("start")) / 1000;
}

function heldAt(path) {
  return existsSync(path) ? JSON.parse(readFileSync(path, "utf8")) : {};
}

function slowestOf(kind) {
  return `slowest${kind[0].toUpperCase()}${kind.slice(1)}`;
}

function stepsBeside(trxPath) {
  const path = trxPath.replace(/\.trx$/, ".steps.tsv");
  if (!existsSync(path)) return { path, problem: `no steps at ${path}; the harness of a measured run writes them` };
  const steps = readFileSync(path, "utf8").split("\n").filter(line => line !== "").map(line => {
    const [test, holder, method, milliseconds] = line.split("\t");
    const kind = navigations.includes(method) ? "navigation" : holder === holderOfAnAction ? "action" : "the harness's";
    return { kind, milliseconds: Number(milliseconds), what: `${holder}.${method} in ${test}` };
  });
  return { path, steps };
}

function read(selection, trxPath, budget, serversLeft) {
  const said = [];
  if (!existsSync(trxPath)) return { said: [`no TRX at ${trxPath}; the test process wrote no result`], counts: undefined, ran: [] };

  const results = resultsOf(trxPath);
  const ran = results.filter(result => result.outcome !== "NotExecuted");
  const selected = new Set(selection.cases);
  const timesRan = new Map();
  for (const result of ran) timesRan.set(result.id, (timesRan.get(result.id) ?? 0) + 1);

  if (selected.size === 0) said.push("the run selected no test");
  for (const id of selection.cases.filter(one => !timesRan.has(one))) said.push(`selected and not run: ${id}`);
  for (const [id, times] of timesRan) {
    if (!selected.has(id)) said.push(`run and not selected: ${id}`);
    else if (times > 1) said.push(`run ${times} times: ${id}`);
  }
  const failed = ran.filter(result => result.outcome !== "Passed");
  for (const result of failed) said.push(`failed (${result.outcome}): ${result.id}`);
  const overBudget = budget.seconds === undefined ? [] : ran.filter(result => result.seconds > budget.seconds);
  for (const result of overBudget) said.push(`over budget, ${result.seconds.toFixed(2)}s of ${budget.seconds}s: ${result.id}`);
  if (budget.problem !== undefined) said.push(budget.problem);
  if (serversLeft > 0) said.push(`${serversLeft} server of the conformance app left alive after the run`);

  const slowest = ran.reduce((slow, result) => (result.seconds > slow.seconds ? result : slow), { seconds: 0, id: "no test" });
  return { said, counts: { selected: selected.size, ran: ran.length, passed: ran.length - failed.length, failed: failed.length, overBudget: overBudget.length, slowest }, ran };
}

function resultLine(label, shard, counts) {
  return `[playwright:result] ${label} ${shard}: ${counts.selected} selected, ${counts.ran} ran, ${counts.passed} passed, ${counts.failed} failed, `
    + `${counts.overBudget} over budget, slowest ${counts.slowest.seconds.toFixed(2)}s ${counts.slowest.id}`;
}

function measureParallelism(options, selection) {
  for (const count of countsAtOnce) {
    const given = (options.runsAt.get(String(count)) ?? []).length;
    if (given < measuredRuns) refuse(`a count of tests at once is measured over ${measuredRuns} runs at each of ${countsAtOnce.join(", ")}, and ${given} were given at ${count}.`);
  }
  const wallSeconds = {};
  const took = [];
  for (const count of countsAtOnce) {
    let hundredthsTaken = 0;
    let red;
    for (const trxPath of options.runsAt.get(String(count))) {
      const { said } = read(selection, trxPath, {}, 0);
      if (said.length > 0 && red === undefined) red = { trxPath, said };
      hundredthsTaken += hundredths(wallSecondsOf(trxPath));
    }
    if (red === undefined) {
      wallSeconds[count] = hundredthsTaken / 100;
      took.push(`${wallSeconds[count]}s at ${count}`);
      continue;
    }
    for (const line of red.said) console.log(`[playwright:result] ${red.trxPath}: ${line}`);
    if (count === 1) {
      console.log(`[playwright:measured] no count is written: ${red.trxPath} is no green run of '${selection.label}' at 1 at once`);
      process.exit(1);
    }
    console.log(`[playwright:measured] ${count} at once is not kept: ${red.trxPath} is no green run of '${selection.label}'`);
    took.push(`a red run at ${count}`);
  }
  const kept = keptOf(wallSeconds);
  writeFileSync(options.budget, `${JSON.stringify({ ...heldAt(options.budget), testsAtOnce: kept, wallSeconds }, null, 2)}\n`);
  const why = kept === 1 ? `none took as little as the runs at 1 divided by ${timesQuicker}` : `${kept} is the quickest that took as little as the runs at 1 divided by ${timesQuicker}`;
  console.log(`[playwright:measured] ${kept} at once: ${measuredRuns} runs took ${took.slice(0, -1).join(", ")} and ${took.at(-1)}; ${why}`);
}

function measure(options, selection) {
  if (options.trxFiles.length < measuredRuns) refuse(`a budget is measured over ${measuredRuns} green runs, and ${options.trxFiles.length} TRX files were given.`);
  let slowest = { seconds: 0, id: "no test" };
  const slowestStep = Object.fromEntries(waits.map(kind => [kind, { milliseconds: 0, what: "no step" }]));
  const secondsOf = new Map();
  for (const trxPath of options.trxFiles) {
    const { said, counts, ran } = read(selection, trxPath, {}, 0);
    if (said.length > 0) {
      for (const line of said) console.log(`[playwright:result] ${trxPath}: ${line}`);
      console.log(`[playwright:measured] no budget is written: ${trxPath} is no green run of '${selection.label}'`);
      process.exit(1);
    }
    if (counts.slowest.seconds > slowest.seconds) slowest = counts.slowest;
    for (const result of ran) secondsOf.set(selection.units[result.id], (secondsOf.get(selection.units[result.id]) ?? 0) + result.seconds);
    const recorded = stepsBeside(trxPath);
    if (recorded.problem !== undefined) {
      console.log(`[playwright:measured] no budget is written: ${recorded.problem}`);
      process.exit(1);
    }
    for (const step of recorded.steps.filter(one => waits.includes(one.kind))) {
      if (step.milliseconds > slowestStep[step.kind].milliseconds) slowestStep[step.kind] = step;
    }
  }
  for (const kind of waits.filter(one => slowestStep[one].milliseconds === 0)) {
    console.log(`[playwright:measured] no budget is written: the ${options.trxFiles.length} runs hold no ${kind} step, so no ${kind} wait is measured`);
    process.exit(1);
  }
  const held = heldAt(options.budget);
  if (held.testsAtOnce === undefined) {
    console.log(`[playwright:measured] no budget is written: no count of tests at once is measured at ${options.budget}; scripts/playwright.sh --measure-parallelism writes it first`);
    process.exit(1);
  }
  const slowestHundredths = Math.ceil(slowest.seconds * 100 - 1e-6);
  const slowestSeconds = slowestHundredths / 100;
  const now = new Date();
  const measured = {
    budgetSeconds: (timesTheSlowest * slowestHundredths) / 100,
    slowestSeconds,
    slowest: slowest.id,
    ...Object.fromEntries(waits.flatMap(kind => {
      const milliseconds = Math.ceil(slowestStep[kind].milliseconds - 1e-6);
      return [
        [`${kind}WaitMilliseconds`, timesTheSlowest * milliseconds],
        [`${slowestOf(kind)}Milliseconds`, milliseconds],
        [slowestOf(kind), slowestStep[kind].what],
      ];
    })),
    runs: options.trxFiles.length,
    tests: selection.cases.length,
    selection: selection.label,
    measuredOn: `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, "0")}-${String(now.getDate()).padStart(2, "0")}`,
    machine: `${cpus()[0].model}, ${type()} ${release()}`,
    budgetAtOnce: held.testsAtOnce,
    testsAtOnce: held.testsAtOnce,
    wallSeconds: held.wallSeconds,
  };
  writeFileSync(options.budget, `${JSON.stringify(measured, null, 2)}\n`);
  const weights = { ...heldAt(options.weights), ...Object.fromEntries([...secondsOf].map(([unit, seconds]) => [unit, hundredths(seconds / options.trxFiles.length) / 100])) };
  writeFileSync(options.weights, `${JSON.stringify(Object.fromEntries(Object.entries(weights).sort(([one], [other]) => (one < other ? -1 : 1))), null, 2)}\n`);
  console.log(`[playwright:measured] budget ${measured.budgetSeconds}s, four times the slowest test of ${measured.runs} green runs of ${measured.tests} tests, ${slowestSeconds}s ${slowest.id}`);
  for (const kind of waits) {
    console.log(`[playwright:measured] ${kind} wait ${measured[`${kind}WaitMilliseconds`]}ms, four times the slowest ${kind}, ${measured[`${slowestOf(kind)}Milliseconds`]}ms ${measured[slowestOf(kind)]}`);
  }
  console.log(`[playwright:measured] a run's seconds by shard unit: ${[...secondsOf.keys()].sort().map(unit => `${unit} ${weights[unit]}s`).join(", ")}`);
}

function prove(options) {
  if (options.trxFiles.length === 0) refuse(`a proof reads at least one run folder.\n${usage}`);
  if (!existsSync(options.full)) refuse(`no full list at ${options.full}; scripts/playwright-select.mjs --all --every-host writes it.`);
  const full = JSON.parse(readFileSync(options.full, "utf8")).cases;
  const budget = budgetOf(options.budget);
  const said = [];
  const ranIn = new Map();
  for (const folder of options.trxFiles) {
    const selectionPath = `${folder}/selection.json`;
    if (!existsSync(selectionPath)) {
      said.push(`${folder}: no selection.json; the run selected nothing`);
      continue;
    }
    const run = read(JSON.parse(readFileSync(selectionPath, "utf8")), `${folder}/run.trx`, budget, 0);
    for (const line of run.said) said.push(`${folder}: ${line}`);
    for (const result of run.ran) ranIn.set(result.id, [...(ranIn.get(result.id) ?? []), folder]);
  }
  const ofTheFullList = new Set(full);
  for (const id of full.filter(one => !ranIn.has(one))) said.push(`in no run: ${id}`);
  for (const [id, folders] of ranIn) {
    if (!ofTheFullList.has(id)) said.push(`run in ${folders[0]} and not of the full list: ${id}`);
    else if (folders.length > 1) said.push(`run in ${folders.length} runs, ${folders.join(" and ")}: ${id}`);
  }
  for (const line of said) console.log(`[playwright:proof] ${line}`);
  const ran = [...ranIn.keys()].filter(id => ofTheFullList.has(id)).length;
  console.log(`[playwright:proof] ${options.trxFiles.length} runs: ${full.length} of the full list, ${ran} ran${said.length === 0 ? ", each once" : ""}`);
  process.exit(said.length === 0 ? 0 : 1);
}

const options = parse(process.argv.slice(2));
if (options.proof) prove(options);
if (options.atOnce) {
  const { testsAtOnce } = heldAt(options.budget);
  if (testsAtOnce === undefined) process.exit(1);
  console.log(testsAtOnce);
  process.exit(0);
}
if (!existsSync(options.selection)) refuse(`no selection at ${options.selection}; scripts/playwright-select.mjs writes it.`);
const selection = JSON.parse(readFileSync(options.selection, "utf8"));

if (options.parallelism) {
  measureParallelism(options, selection);
  process.exit(0);
}
if (options.measure) {
  measure(options, selection);
  process.exit(0);
}

const { said, counts } = read(selection, options.trx, budgetOf(options.budget), Number(options["servers-left"]));
for (const line of said) console.log(`[playwright:result] ${line}`);
if (counts === undefined) {
  console.log(`[playwright:result] ${selection.label} ${selection.shard}: no result`);
  process.exit(1);
}
console.log(resultLine(selection.label, selection.shard, counts));
process.exit(said.length === 0 ? 0 : 1);
