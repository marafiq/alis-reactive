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

A code example in the docs is copied whole from a view a Playwright test runs, never written for the page: the view,
its controller and its model exist in the conformance app, and the test that opens it passes; or it is copied whole
from a file of a fresh app (`tests/Alis.Reactive.Consumer.Net10`, `tests/Alis.Reactive.Consumer.Net48`), which
README's steps wire and M19.X5's test opens. `WhenADocShowsAnExample` refuses an example of README.md found whole in
neither, and a view example of a skill, a cshtml block of its file, the same way.

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

A CLAUDE.md, a file under `.claude/rules` or `.claude/agents`, or a skill's `.claude/skills/<name>/SKILL.md`, names the code in backticks, and
`tests/Alis.Reactive.ArchitectureTests/WhenARuleNamesTheCode.cs` fails once what it names moves or is renamed. A path
is written from the root or from the rule's folder, never with a line. Text the code writes goes in quotes (`"yyyy-MM-dd"`). A C# block quotes declarations as the code
declares them, or is marked `csharp illustration`. What a rule cites from outside the repo goes on that test's list.
A command a rule quotes with an option goes in a bash block, where the test reads each option against its script, and
a tool under node_modules is cited by its npx command or its script, never by its path (the lead's mistakes,
2026-11-11). The test reads a command as a shell does: what stands inside quotes, single or double, is text it does
not read, and outside them a word that holds a slash is read as a path. So the text of a planted defect and a value
that is no path, a shard's fraction among them, are quoted, and a path or an option is written outside the quotes (the
lead's mistakes, 2026-11-11).

A section, a heading or the owner's words a rule file cites is read at its source before the line is written: a
section by its heading at HEAD, the owner's words in `spec/NOTES.md`. A line that leaves a rule file is traced to its
home first, a search finding its meaning there, and the commit names that home (the lead's mistakes, 2026-11-23).

## A design put to the owner

A design the owner decides on, a landing folder's "design.md", names for each defect the spec line it breaks and
whether a page of the app reaches it, each read at the source; an absence names the search and the line it printed; a
claim that a TLA+ spec stands names its TLC run; a cause is read, never inferred (the lead's mistakes, 2026-11-24: a
design called a defect before the app's actions were read, and stated a search's reach, a host's bind and a failure's
cause no run had shown). The meaning it gives a case is quoted from the D line that states it, read before the case is
written; a search over the app names every door a request can be written with, the files it read and those it did not,
and each count is the line the command printed (the lead's mistakes, 2026-11-24: a check's meaning set before D16.5 was
read, and "no page reaches it" from a search that missed the condensed doors and every lambda not named "request").

## Workflow

1. Read the actual source code for the feature.
2. Find the conformance app's screen that uses it, or the fresh app's file that holds it, or write one with its test.
3. Run its Playwright test and read the result line.
4. Copy the view's code to the docs.
5. Frame with question followed by answer. Progressive disclosure.
6. Run Rider diagnostics. Fix warnings before committing.
