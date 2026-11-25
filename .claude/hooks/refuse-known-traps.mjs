#!/usr/bin/env node
let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const command = input.tool_input?.command ?? "";
  const refusal = [
    gitGrepsWithAClassTheMachineLacks,
    runsTheBrowserTestsByAFilter,
    searchesWithGrepInclude,
    startsCodexWithItsInputOpen,
    splitsAVariableIntoWordsAsBashDoes,
    readsAModifierAfterABareName,
    restoresAFileFromTheIndex,
    substitutesABacktickedText,
  ]
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

function outsideHeredocsAndSingleQuotes(command) {
  const lines = command.split("\n");
  const kept = [];
  let until = "";
  for (const line of lines) {
    if (until !== "") {
      if (line.trim() === until) until = "";
      continue;
    }
    kept.push(line.replace(/'[^']*'/g, "''"));
    const heredoc = /<<-?\s*['"]?([A-Za-z_][A-Za-z0-9_]*)['"]?/.exec(line);
    if (heredoc !== null) until = heredoc[1];
  }
  return kept.join("\n");
}

function splitsAVariableIntoWordsAsBashDoes(command) {
  const shell = outsideHeredocsAndSingleQuotes(command);
  const splits =
    /(^|[\s;&|(])set\s+--\s+\$\{?[A-Za-z_]/.test(shell) ||
    /(^|[\s;&|(])for\s+[A-Za-z_][A-Za-z0-9_]*\s+in\s+\$\{?[A-Za-z_]/.test(shell) ||
    /(^|[;&|]\s*|\n\s*)\$\{?[A-Za-z_][A-Za-z0-9_]*\}?[ \t]+[^=\s]/.test(shell);
  return splits
    ? "zsh does not split a variable into words as bash does: `set -- $name`, `for x in $name` and a command held in a variable each run as one word here. Write the words out, or run it as a bash script (CLAUDE.md, How you work)."
    : undefined;
}

function restoresAFileFromTheIndex(command) {
  const fromTheIndex = invocationsOf(outsideHeredocsAndSingleQuotes(command), "git ").some(
    (invocation) =>
      /^git(\s+-C\s+\S+)?\s+checkout(\s+-[A-Za-z-]+)*\s+--(\s|$)/.test(invocation) ||
      (/^git(\s+-C\s+\S+)?\s+restore(\s|$)/.test(invocation) && !/\s(--source|-s|--staged|-S)(\s|=|$)/.test(invocation)),
  );
  return fromTheIndex
    ? "git checkout -- <path> and git restore <path> take the file from the index, not from HEAD, so a file staged since keeps its staged text. Name the commit: git checkout HEAD -- <path> (CLAUDE.md, What is forbidden)."
    : undefined;
}

function substitutesABacktickedText(command) {
  return outsideHeredocsAndSingleQuotes(command).replace(/\\`/g, "").includes("`")
    ? "A backtick outside single quotes runs the text it holds as a command. Write $(...) for a substitution, and put Markdown with backticks in single quotes or a file (CLAUDE.md, What is forbidden)."
    : undefined;
}

function readsAModifierAfterABareName(command) {
  return /\$(?:[A-Za-z_][A-Za-z0-9_]*|[0-9]):(?:[aAehlPrstu&]|g[s&])/.test(outsideHeredocsAndSingleQuotes(command))
    ? "zsh reads a letter after a bare $name and a colon as a modifier: $c:spec is a substitution and $c:h the head. Write ${name}: (CLAUDE.md, How you work)."
    : undefined;
}
