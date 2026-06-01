---
name: vision-alignment
enabled: false
event: file
action: warn
conditions:
  - field: file_path
    operator: regex_match
    pattern: \.(cshtml|cs|ts)$
---

**Pause — end-to-end thinking check.**

Before editing this file, confirm you have answered these questions:

1. **Which layer is this file in?** (C# builders → Schema → TS types → TS runtime → Browser → Docs)
2. **Have you read the code path end-to-end** from the C# DSL through schema to runtime execution?
3. **Is this a root cause fix or a patch?** Patches create more patches. Fix the origin.
4. **Does the plan carry all information the runtime needs?** If adding logic to TS, the plan is probably missing information — fix the C# plan model class instead.
5. **Are you using the right primitive?** `.Reactive()` for component events. `Html.On` for page-level triggers. `p.When()` for conditions. `p.Get/Post` for HTTP. Do not reuse one primitive where another fits.

Load the applicable skill (`reactive-dsl`, `http-pipeline`, `conditions-dsl`, `validation-rules`) BEFORE writing code. The skill has verified canonical patterns from real sandbox views.
