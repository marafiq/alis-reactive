#!/usr/bin/env node
import { existsSync, readFileSync, writeFileSync } from "node:fs";
import { cpus, release, type } from "node:os";

const usage = `usage: node scripts/playwright-result.mjs --selection <file> --trx <file> --budget <file> [--shard <i>/<k>] [--servers-left <n>]
       node scripts/playwright-result.mjs --measure --selection <file> --budget <file> <file.trx>...

Reads what one run of the browser tests wrote and prints its result line:
  [playwright:result] <selection> <i>/<k>: N selected, N ran, N passed, 0 failed, 0 over budget, slowest <s>s <test>
It exits 1 when the run wrote no TRX, selected no test, ran other than it selected, failed a test, ran a test over
the budget, or left a server alive, each said on a line of its own before the result line.

--measure reads the TRX of each of 20 green runs of one selection, and beside each TRX the steps the harness wrote
(<name>.steps.tsv: a test, the holder and the method of one call of Playwright, its milliseconds), and writes the
budget, four times the slowest test, and the action wait and the navigation wait, each four times the slowest step
of its kind: a navigation is goto, reload, goBack or goForward, an action every other call on a frame.`;

const measuredRuns = 20;
const timesTheSlowest = 4;
const navigations = ["goto", "reload", "goBack", "goForward"];
const holderOfAnAction = "Frame";
const waits = ["action", "navigation"];

function refuse(message) {
  console.error(`[playwright:result] ${message}`);
  process.exit(2);
}

function parse(argv) {
  const options = { shard: "1/1", "servers-left": "0", measure: false, trxFiles: [] };
  for (let index = 0; index < argv.length; index += 1) {
    const word = argv[index];
    if (word === "--measure") options.measure = true;
    else if (["--selection", "--trx", "--budget", "--shard", "--servers-left"].includes(word)) {
      if (index + 1 >= argv.length) refuse(`${word} takes a value.\n${usage}`);
      options[word.slice(2)] = argv[(index += 1)];
    } else if (word.startsWith("-")) refuse(`${word} is no option.\n${usage}`);
    else options.trxFiles.push(word);
  }
  const needed = options.measure ? ["selection", "budget"] : ["selection", "trx", "budget"];
  for (const name of needed) {
    if (options[name] === undefined) refuse(`--${name} is missing.\n${usage}`);
  }
  if (!/^[1-9][0-9]*\/[1-9][0-9]*$/.test(options.shard)) refuse(`--shard takes <i>/<k>, and '${options.shard}' is not that.`);
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
  return { seconds: measured.budgetSeconds };
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
  if (!existsSync(trxPath)) return { said: [`no TRX at ${trxPath}; the test process wrote no result`], counts: undefined };

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
  return { said, counts: { selected: selected.size, ran: ran.length, passed: ran.length - failed.length, failed: failed.length, overBudget: overBudget.length, slowest } };
}

function resultLine(label, shard, counts) {
  return `[playwright:result] ${label} ${shard}: ${counts.selected} selected, ${counts.ran} ran, ${counts.passed} passed, ${counts.failed} failed, `
    + `${counts.overBudget} over budget, slowest ${counts.slowest.seconds.toFixed(2)}s ${counts.slowest.id}`;
}

function measure(options, selection) {
  if (options.trxFiles.length < measuredRuns) refuse(`a budget is measured over ${measuredRuns} green runs, and ${options.trxFiles.length} TRX files were given.`);
  let slowest = { seconds: 0, id: "no test" };
  const slowestStep = Object.fromEntries(waits.map(kind => [kind, { milliseconds: 0, what: "no step" }]));
  for (const trxPath of options.trxFiles) {
    const { said, counts } = read(selection, trxPath, {}, 0);
    if (said.length > 0) {
      for (const line of said) console.log(`[playwright:result] ${trxPath}: ${line}`);
      console.log(`[playwright:measured] no budget is written: ${trxPath} is no green run of '${selection.label}'`);
      process.exit(1);
    }
    if (counts.slowest.seconds > slowest.seconds) slowest = counts.slowest;
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
  };
  writeFileSync(options.budget, `${JSON.stringify(measured, null, 2)}\n`);
  console.log(`[playwright:measured] budget ${measured.budgetSeconds}s, four times the slowest test of ${measured.runs} green runs of ${measured.tests} tests, ${slowestSeconds}s ${slowest.id}`);
  for (const kind of waits) {
    console.log(`[playwright:measured] ${kind} wait ${measured[`${kind}WaitMilliseconds`]}ms, four times the slowest ${kind}, ${measured[`${slowestOf(kind)}Milliseconds`]}ms ${measured[slowestOf(kind)]}`);
  }
}

const options = parse(process.argv.slice(2));
if (!existsSync(options.selection)) refuse(`no selection at ${options.selection}; scripts/playwright-select.mjs writes it.`);
const selection = JSON.parse(readFileSync(options.selection, "utf8"));

if (options.measure) {
  measure(options, selection);
  process.exit(0);
}

const { said, counts } = read(selection, options.trx, budgetOf(options.budget), Number(options["servers-left"]));
for (const line of said) console.log(`[playwright:result] ${line}`);
if (counts === undefined) {
  console.log(`[playwright:result] ${selection.label} ${options.shard}: no result`);
  process.exit(1);
}
console.log(resultLine(selection.label, options.shard, counts));
process.exit(said.length === 0 ? 0 : 1);
