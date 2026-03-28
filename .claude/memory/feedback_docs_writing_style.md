---
name: feedback_docs_writing_style
description: User's technical documentation writing style — question-driven, progressive disclosure, no information dumping, curiosity-based reveals
type: feedback
---

Documentation must follow user's blog writing style (adnanrafiq.com):

**Question → then answer.** Each section opens with a challenge or question the reader naturally has. "How do you react when a checkbox changes?" → code. Never dump information unprompted.

**Progressive disclosure.** Reveal concepts in order of need. Start with the simplest (events), build to complex (HTTP orchestration, conditions). Never introduce all concepts at once.

**No internal terminology early.** Don't expose "descriptors", "entries", "CallMutation", "SetPropMutation" in architecture docs. Those are internals for advanced/contributor docs. Use "intent", "fluent builders", "reactions".

**"auto-boot" → "boot".** Don't leak implementation module names.

**"C# Fluent Builders" not "C# Modules" or "C# DSL".** Framework users write fluent C# to express intent.

**Small code, then explain.** Never dump big JSON blobs. Show 3-5 line snippets that answer one question.

**Cascade examples use real Gather pattern.** `.Gather(g => g.Include<FusionDropDownList, Model>(m => m.Country))` — never string concatenation for URLs.

**All code must be verified in sandbox.** Create the actual view/controller/model in SandboxApp, build, run, test in browser — THEN copy to docs. First experience must work.

**Why:** "if you get anything wrong in terms of syntax or explanation, users will not use it. It will never come back."

**How to apply:** Before writing any doc page, read the actual source code. Create a working sandbox example. Copy verified code. Frame with question → answer.
