---
name: API surface is frozen - no changes without evidence
description: Public API (method names, parameter names, visibility, types) must not change without full downstream analysis and explicit user approval
type: feedback
---

The public API surface is FROZEN. Do not change method signatures, parameter names, return types, or constructor visibility without explicit user approval.

**Why:** API surface changes cascade across 100+ files: views (.cshtml), tests, docs-site, examples, skills, and CLAUDE.md. The parameter rename from `configure` to domain-specific names (pipeline, build, trigger, gather, response, request, guard) took 6 commits and touched 170+ files. Even a single rename ripples everywhere.

**How to apply:**
- A hookify rule `.claude/hookify.protect-api-surface.local.md` blocks API surface edits at the tool level
- Before ANY change: grep all call sites, read every affected file, present evidence to user
- `internal` → `public` is **strictly forbidden** — internal members protect the API surface deliberately
- Parameter naming convention is locked in CLAUDE.md rule #12
- Fusion HtmlExtension methods use the `Fusion` prefix (e.g., `.FusionDropDownList()`)
- Event args properties: `{ get; private set; }` for Fusion, "Gets" voice in XML docs
- No "Syncfusion" prefix in XML docs — use framework class names (e.g., "FusionAutoComplete")

**The convention:**
| Callback type | Parameter name |
|---|---|
| `Action<PipelineBuilder>` | `pipeline` |
| `Action<XxxBuilder>` (components) | `build` |
| `Action<TriggerBuilder>` | `trigger` |
| `Action<GatherBuilder>` | `gather` |
| `Action<ResponseBuilder>` | `response` |
| `Action<HttpRequestBuilder>` | `request` |
| `Func<ConditionSourceBuilder, GuardBuilder>` | `guard` |
| `Func<ConditionStart, GuardBuilder>` | `inner` |
