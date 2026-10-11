#!/usr/bin/env node
// PreToolUse gate for Edit/Write: the generated plan contract is regenerated, never hand-edited.
let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const filePath = (input.tool_input?.file_path ?? "").replaceAll("\\", "/");
  const editsGeneratedContract = /Alis\.Reactive\.Assets\/runtime\/types\/plan\.ts$/.test(filePath);
  if (editsGeneratedContract) {
    console.log(
      JSON.stringify({
        hookSpecificOutput: {
          hookEventName: "PreToolUse",
          permissionDecision: "deny",
          permissionDecisionReason:
            "plan.ts is generated from the C# plan domain by PlanContractGenerator. " +
            "Change the C# plan model and regenerate (`npm run typecheck` runs generation " +
            "first, or `npm run generate:plan-types -w Alis.Reactive.Assets`) — never hand-edit.",
        },
      }),
    );
  }
  process.exit(0);
});
