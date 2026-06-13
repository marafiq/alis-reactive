#!/usr/bin/env node
// PreToolUse gate for Bash: Playwright runs only through scripts/playwright.sh.
let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const command = input.tool_input?.command ?? "";
  const runsPlaywrightDirectly =
    /dotnet\s+(test|vstest)\b/.test(command) && /playwright/i.test(command);
  if (runsPlaywrightDirectly) {
    console.log(
      JSON.stringify({
        hookSpecificOutput: {
          hookEventName: "PreToolUse",
          permissionDecision: "deny",
          permissionDecisionReason:
            "Playwright tests run only through scripts/playwright.sh (use --filter to focus). " +
            "The wrapper provides progress markers, log/TRX artifacts, and stale-asset " +
            "rejection that raw dotnet test skips.",
        },
      }),
    );
  }
  process.exit(0);
});
