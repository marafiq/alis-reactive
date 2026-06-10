#!/usr/bin/env node
// PreToolUse gate for Edit/Write: generated files are regenerated, never hand-edited.
let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const filePath = input.tool_input?.file_path ?? "";
  const guards = [
    {
      pattern: /Alis\.Reactive\.Assets\/runtime\/types\/plan\.ts$/,
      reason:
        "plan.ts is generated from the C# plan domain by PlanContractGenerator. " +
        "Change the C# plan model and regenerate (`npm run typecheck` runs generation " +
        "first, or `npm run generate:plan-types -w Alis.Reactive.Assets`) — never hand-edit.",
    },
    {
      pattern:
        /tools\/FusionOnboarding\/wwwroot\/onboarding\/fusion\/.+\/(discovery\/[^/]+\.json|traces\/[^/]+\.trace\.json)$/,
      reason:
        "Generated onboarding artifact. Regenerate it via the onboard-fusion-component " +
        "skill scripts so evidence stays derived from source — never hand-edit.",
    },
  ];
  const hit = guards.find((guard) => guard.pattern.test(filePath));
  if (hit) {
    console.log(
      JSON.stringify({
        hookSpecificOutput: {
          hookEventName: "PreToolUse",
          permissionDecision: "deny",
          permissionDecisionReason: hit.reason,
        },
      }),
    );
  }
  process.exit(0);
});
