#!/usr/bin/env node
let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const command = input.tool_input?.command ?? "";
  const refusal = [gitGrepsWithAClassTheMachineLacks, runsTheBrowserTestsByAFilter, searchesWithGrepInclude, startsCodexWithItsInputOpen]
    .map((trap) => trap(command))
    .find((reason) => reason !== undefined);
  if (refusal !== undefined) {
    console.log(
      JSON.stringify({ hookSpecificOutput: { hookEventName: "PreToolUse", permissionDecision: "deny", permissionDecisionReason: refusal } }),
    );
  }
  process.exit(0);
});

function invocationsOf(command, program) {
  const found = [];
  let from = command.indexOf(program);
  while (from !== -1) {
    let quote = "";
    let end = from;
    for (; end < command.length; end += 1) {
      const character = command[end];
      if (quote !== "") {
        if (character === quote) quote = "";
        continue;
      }
      if (character === "'" || character === '"') quote = character;
      else if (character === "|" || character === ";" || character === "&" || character === "\n") break;
    }
    found.push(command.slice(from, end));
    from = command.indexOf(program, end);
  }
  return found;
}

function gitGrepsWithAClassTheMachineLacks(command) {
  const lacksTheClass = invocationsOf(command, "git grep")
    .concat(invocationsOf(command, "git -C").filter((invocation) => / grep /.test(invocation)))
    .some((invocation) => /\s-[A-Za-z]*E/.test(invocation) && /\\[bsdw]/.test(invocation));
  return lacksTheClass
    ? "git grep's extended expressions take no \\b, \\s, \\d or \\w on this machine: write a bracket, [[:space:]], [0-9] or ([^A-Za-z]|$) (CLAUDE.md, How you work)."
    : undefined;
}

function runsTheBrowserTestsByAFilter(command) {
  return invocationsOf(command, "scripts/playwright.sh").some((invocation) => /\s--filter(\s|=|$)/.test(invocation))
    ? "scripts/playwright.sh takes --test <constant> or --all and no --filter (CLAUDE.md, How you work)."
    : undefined;
}

function searchesWithGrepInclude(command) {
  return /(^|[\s;&|(])grep\s[^\n|;&]*--include/.test(command)
    ? "A search of the repository is git grep, its options before the pattern, never grep --include (CLAUDE.md, How you work)."
    : undefined;
}

function startsCodexWithItsInputOpen(command) {
  const starts = /(^|[;&|]\s*|\bcaffeinate\s+(-\w+\s+)*)codex exec\b/.test(command);
  return starts && !/<\s*\/dev\/null/.test(command)
    ? "codex exec reads its input and waits on it: start it with < /dev/null (CLAUDE.md, How you work)."
    : undefined;
}
