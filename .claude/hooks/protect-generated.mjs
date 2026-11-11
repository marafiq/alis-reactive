#!/usr/bin/env node
let raw = "";
process.stdin.on("data", (chunk) => (raw += chunk));
process.stdin.on("end", () => {
  const input = JSON.parse(raw);
  const filePath = (input.tool_input?.file_path ?? "").replaceAll("\\", "/");
  const editsGeneratedContract = /Alis\.Reactive\.Assets\/runtime\/Schema\/plan\.(ts|schema\.json)$/.test(filePath);
  if (editsGeneratedContract) {
    console.log(
      JSON.stringify({
        hookSpecificOutput: {
          hookEventName: "PreToolUse",
          permissionDecision: "deny",
          permissionDecisionReason:
            "plan.ts and plan.schema.json are generated from the C# grammar (spec/REWRITE.md, section 2.1). " +
            "Change the grammar and regenerate; never hand-edit.",
        },
      }),
    );
  }
  process.exit(0);
});
