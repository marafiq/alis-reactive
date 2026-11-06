---
paths:
  - "**/*.md"
  - "**/*.cs"
---

# Docs and XML docs

## Dev-Facing Voice

Write from the developer's perspective. The developer reads docs to understand what to do, not how the framework works internally.

- No "runtime" in dev-facing docs. Say "execute in the browser" not "the runtime merges/discovers/executes".
- No internal type names in user docs: "descriptors", "entries", "CallReaction", "SetReaction", "SequenceReaction". Use "intent", "fluent builders", "reactions". Reserve internals for contributor docs.
- No implementation details in summary/remarks: script tags, data attributes, hidden divs, THelper closure, HtmlEncoder stay out of XML docs.
- "C# Fluent Builders" not "C# Modules" or "C# DSL". Framework users write fluent C# to express intent.
- "boot" not "auto-boot". Do not leak implementation module names.
- Avoid jargon; prefer the concrete ASP.NET terms developers already know: "view" and "partial view", not "parent/child". Say "owning view" if you must distinguish.
- Use plain English. "open and close" not "bookend". Lead with common words.

## Question-Driven Structure

Each section opens with a challenge or question the reader naturally has.
"How do you react when a checkbox changes?" followed by code. Never dump information unprompted.

## Progressive Disclosure

Reveal concepts in order of need. Start with the simplest (events), build to complex (HTTP orchestration, conditions). Never introduce all concepts at once.

Show small code (3-5 lines) that answers one question. Never dump big JSON blobs.

## Code Examples

All code must be verified in a working sandbox page before appearing in docs. Create the actual view/controller/model in SandboxApp, build, run, test in browser, then copy to docs.

Cascade examples use real Gather pattern (`Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Patterns/BedAssignment/Index.cshtml`):
`.Gather<WingRoomsQuery>(g => g.Form(m => m.Wing))` -- never string concatenation for URLs.

"If you get anything wrong in terms of syntax or explanation, users will not use it. It will never come back."

## XML docs

An XML doc sits only on the public surface of a shipped package. Every word in a `<summary>`, `<remarks>`
or `<param>` earns its place: concise, accurate, and written for the developer reading IntelliSense.

1. `<summary>` = dev language always. `<remarks>` = can go deeper but frame as "what this means for you".
2. Anti-patterns in remarks where relevant: "Omitting either call produces no reactive behavior."
3. Do not document `this` param on single-param extensions. When other params exist, include `html` minimally to avoid CS1573.
4. An XML doc is true of the member it sits on: read the behavior before writing it.
5. No em-dashes in XML docs. Rider grammar linter flags them as redundant punctuation. Use colons or restructure.
6. "Gets" voice for a property; framework class names, never a "Syncfusion" prefix (e.g., "FusionAutoComplete").
7. A doc agent's brief names `.claude/rules/docs.md` first, before any setup.

## Rule files

A CLAUDE.md, a file under `.claude/rules` or `.claude/agents`, or a skill's `SKILL.md` under `.claude/skills`, names the code in backticks, and
`tests/Alis.Reactive.ArchitectureTests/WhenARuleNamesTheCode.cs` fails once what it names moves or is renamed. A path
is written from the root or from the rule's folder, never with a line. Text the code writes goes in quotes (`"yyyy-MM-dd"`). A C# block quotes declarations as the code
declares them, or is marked `csharp illustration`. What a rule cites from outside the repo goes on that test's list.

## Workflow

1. Read the actual source code for the feature.
2. Create a working sandbox example (view, controller, model).
3. Build, run, verify in browser.
4. Copy verified code to docs.
5. Frame with question followed by answer. Progressive disclosure.
6. Run Rider diagnostics. Fix warnings before committing.
