---
name: docs-principles
description: Consolidated documentation and XML docs writing principles — voice, structure, style, and verification
type: reference
---

## Dev-Facing Voice

Write from the developer's perspective. The developer reads docs to understand what to do, not how the framework works internally.

- No "runtime" in dev-facing docs. Say "execute in the browser" not "the runtime merges/discovers/executes".
- No "descriptors", "entries", "CallMutation", "SetPropMutation" in user docs. Use "intent", "fluent builders", "reactions". Reserve internals for contributor docs.
- No implementation details in summary/remarks: script tags, data attributes, hidden divs, THelper closure, HtmlEncoder belong in code comments, not XML docs.
- "C# Fluent Builders" not "C# Modules" or "C# DSL". Framework users write fluent C# to express intent.
- "boot" not "auto-boot". Do not leak implementation module names.
- Avoid "parent/child" jargon. Use "view" and "partial view" (concrete ASP.NET terms). Say "owning view" if you must distinguish.
- Use plain English. "open and close" not "bookend". Lead with common words.

## Question-Driven Structure

Each section opens with a challenge or question the reader naturally has.
"How do you react when a checkbox changes?" followed by code. Never dump information unprompted.

## Progressive Disclosure

Reveal concepts in order of need. Start with the simplest (events), build to complex (HTTP orchestration, conditions). Never introduce all concepts at once.

Show small code (3-5 lines) that answers one question. Never dump big JSON blobs.

## Code Examples

All code must be verified in a working sandbox page before appearing in docs. Create the actual view/controller/model in SandboxApp, build, run, test in browser, then copy to docs.

Cascade examples use real Gather pattern:
`.Gather(g => g.Include<FusionDropDownList, Model>(m => m.Country))` -- never string concatenation for URLs.

"If you get anything wrong in terms of syntax or explanation, users will not use it. It will never come back."

## XML Docs: 12-Point Voice Rules

1. `<summary>` = dev language always. `<remarks>` = can go deeper but frame as "what this means for you".
2. Code comments (`//`) inside method bodies CAN speak internal language -- they target maintainers.
3. Anti-patterns in remarks where relevant: "Omitting either call produces no reactive behavior."
4. Do not document `this` param on single-param extensions. When other params exist, include `html` minimally to avoid CS1573.
5. Code comments must be truthful. Research actual behavior before writing.
6. No em-dashes in XML docs. Rider grammar linter flags them as redundant punctuation. Use colons or restructure.
7. Run Rider diagnostics (`mcp__jetbrains__get_file_problems`) on every file touched before committing.
8. When launching doc agents, the FIRST instruction must be: load `dotnet-xml-docs` skill. Do not bury it in a setup section.
9. No "runtime" in summaries or remarks (see Dev-Facing Voice above).
10. No implementation details in summaries (see Dev-Facing Voice above).
11. Avoid jargon; prefer concrete ASP.NET terms developers already know.
12. Use plain English; lead with common words.

## Workflow

1. Read the actual source code for the feature.
2. Create a working sandbox example (view, controller, model).
3. Build, run, verify in browser.
4. Copy verified code to docs.
5. Frame with question followed by answer. Progressive disclosure.
6. Run Rider diagnostics. Fix warnings before committing.
