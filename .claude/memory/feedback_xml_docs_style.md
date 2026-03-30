---
name: XML docs voice and style principles
description: Hard-won nuances from PlanExtensions docs tour — dev-facing voice, no internals, no jargon
type: feedback
---

XML documentation must be written from the developer's perspective, not the framework's internals.

**Why:** Developer reads docs to understand what to do, not how the framework works internally. Leaking internals (runtime, JSON, script tags, hidden divs) creates noise and breaks the abstraction.

**How to apply:**

1. **No "runtime" in dev-facing docs** — the developer doesn't care about the JS runtime at this level. Say "execute in the browser" not "the runtime merges/discovers/executes".
2. **No implementation details in summary/remarks** — script tags, data attributes, hidden divs, THelper closure, HtmlEncoder — these belong in code comments, not XML docs.
3. **Code comments CAN speak internal language** — inline `//` comments inside method bodies are for future maintainers, so internal details (summary div, planId, IsPartial) are appropriate there.
4. **summary = dev language, remarks = can speak internal but prefer dev language** — summary is always from the dev's mental model. Remarks can go deeper but still frame from "what this means for you" not "how the sausage is made".
5. **Avoid "parent/child" jargon** — use "view" and "partial view" (concrete ASP.NET terms devs know). Say "owning view" if you must distinguish.
6. **Use plain English** — "bookend" is obscure, "open and close" is clear. Lead with common words.
7. **Anti-patterns in remarks where relevant** — "Omitting either call produces no reactive behavior" is an anti-pattern warning framed as a fact.
8. **Don't document `this` param on single-param extensions** — skill rule. But when other params exist, include `html` minimally to avoid CS1573 compiler warning about inconsistency.
9. **Code comments must be truthful** — when writing inline comments, research actual behavior. The validation summary div comment was rewritten after discovering it's a fallback (hidden fields, unenriched fields, server errors), not the primary error display path.
10. **Always run Rider diagnostics after writing XML docs** — use `mcp__jetbrains__get_file_problems` on every file touched. Catches grammar issues (unpaired symbols, redundant punctuation, missing objects) that manual review misses. Do this before committing.
11. **When launching doc agents, front-load skill loading** — the FIRST instruction must be: "Your FIRST action MUST be: invoke the Skill tool with skill: 'dotnet-xml-docs'. Do NOT proceed until loaded." Burying it in a setup section means agents skip it.
12. **No em-dashes (—) in XML docs** — Rider grammar linter flags them as redundant punctuation. Use colons or restructure the sentence instead.
