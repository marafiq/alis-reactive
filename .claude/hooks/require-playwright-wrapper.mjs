#!/usr/bin/env node
// PreToolUse gate for Bash: Playwright runs only through scripts/playwright.sh.
// Decides by the test TARGET, not by words in the command: `dotnet test` / `dotnet vstest` must
// name a non-Playwright test project. The solution, a bare `dotnet test` (which resolves to the
// solution or the project in the current directory), and the Playwright project are redirected.
const OPTIONS_WITH_VALUE = new Set([
  "--filter", "--configuration", "-c", "--framework", "-f", "--logger", "-l", "--settings", "-s",
  "--output", "-o", "--results-directory", "--diag", "-d", "--verbosity", "-v", "--runtime", "-r",
  "--arch", "-a", "--os", "--environment", "-e", "--test-adapter-path", "--collect",
  "--blame-crash-dump-type", "--blame-hang-dump-type", "--blame-hang-timeout",
]);

function testTarget(argText) {
  const tokens = argText.match(/"[^"]*"|'[^']*'|\S+/g) ?? [];
  for (let i = 0; i < tokens.length; i++) {
    const token = tokens[i];
    if (token === "--") return undefined;
    if (!token.startsWith("-")) return token.replace(/^["']|["']$/g, "");
    const takesValue = OPTIONS_WITH_VALUE.has(token) && !token.includes("=");
    if (takesValue) i++;
  }
  return undefined;
}

function bypassesWrapper(segment) {
  const invocation = segment.match(/\bdotnet\s+(?:test|vstest)\b(.*)$/s);
  if (!invocation) return false;
  const target = testTarget(invocation[1]);
  const runsWholeSolution = target === undefined || /\.slnx?$/i.test(target);
  const runsPlaywrightProject = target !== undefined && /PlaywrightTests/i.test(target);
  return runsWholeSolution || runsPlaywrightProject;
}

let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const command = input.tool_input?.command ?? "";
  const segments = command.split(/&&|\|\||[;|\n]/);
  if (segments.some(bypassesWrapper)) {
    console.log(
      JSON.stringify({
        hookSpecificOutput: {
          hookEventName: "PreToolUse",
          permissionDecision: "deny",
          permissionDecisionReason:
            "Playwright runs only through scripts/playwright.sh (use --filter to focus); it " +
            "rejects stale browser assets and writes progress, log and TRX artifacts that raw " +
            "dotnet test skips. For non-browser tests name the project " +
            "(dotnet test tests/Alis.Reactive.ArchitectureTests) or run scripts/test.sh --no-e2e.",
        },
      }),
    );
  }
  process.exit(0);
});
