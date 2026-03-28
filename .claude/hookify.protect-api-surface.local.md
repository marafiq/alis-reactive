---
name: protect-api-surface
enabled: true
event: file
action: block
conditions:
  - field: file_path
    operator: regex_match
    pattern: (Builders/|Extensions/|Components/.*Html|Components/.*Reactive|Components/.*Extensions|ReactivePlan|InputBoundField|ElementBuilder|PipelineBuilder|TriggerBuilder|ResponseBody|TypedEventDescriptor|TypedSource|ComponentRef).*\.cs$
  - field: new_text
    operator: regex_match
    pattern: \b(public\s+(static\s+)?(void|sealed|class|interface|abstract|partial|override|virtual|new|async)?\s*\w+\s*[<(]|public\s+\w+\s+\w+\s*\{|internal\s+(static\s+)?(void|sealed|class)?\s*\w+|private\s+(static\s+)?(void|sealed|class)?\s*\w+)
---

**BLOCKED: Public API surface change detected.**

You are modifying a public API signature in a builder, extension, or core type file. This is a breaking change that affects views, tests, docs, and downstream consumers.

**HARD RULES:**
- Changing `internal` to `public` is **STRICTLY FORBIDDEN**. Internal members were made internal deliberately to protect the API surface. No exceptions.
- Changing `public` method signatures (parameter names, types, return types) requires full downstream analysis.
- Changing constructor visibility requires full downstream analysis.
- Renaming public methods or types requires full downstream analysis.

**Before ANY API surface change can proceed, you MUST provide the user with:**

1. **WHAT** is changing: exact method/class/parameter being modified
2. **WHY** it needs to change: the specific problem this solves (not "cleanup" or "consistency")
3. **DOWNSTREAM IMPACT**: which views (.cshtml), tests, docs-site pages, examples, and skills are affected
4. **EVIDENCE**: show the grep results of all call sites that will break
5. **END-TO-END ANALYSIS**: confirm you have read every affected file, not just the declaration

**Ask the user explicitly: "I need to change [X] because [Y]. Here are the [N] call sites affected: [list]. Shall I proceed?"**

Do NOT proceed without user approval. This rule exists because API surface changes cascade across 100+ files and break IntelliSense, docs, skills, and examples.
