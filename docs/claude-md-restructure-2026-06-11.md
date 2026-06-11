# Root CLAUDE.md Restructure — Reasoning Record

Date: 2026-06-11. Each concept change below was approved in-session before it
was made. Nothing is committed; the working tree carries the changes.

Directives that drove the restructure: the file was too long and repeated
ideas in different words; "Operating Standard For This Repository" was
confusing; the architecture story took 200 words where 100 carry it;
architecture belongs under one heading, followed by a Key Concepts heading;
Do/Do-not becomes Must Do plus a separate Never Do heading; no decorative
adjectives; editor-level grammar and sentence construction.

## Change 1 — Architecture told once

**What changed.** The two opening paragraphs and the "Architecture — 5
Layers, 4 Boundaries" section merged into one `## Architecture` heading: a
~100-word narrative followed by the layer diagram. The narrative follows the
intent through the system — developer expresses reactive intent in the DSL,
the plan domain captures it, `Html.RenderPlan` serializes it as plan JSON
inside the view, the browser runtime's one job is to execute that plan.

**Why.** The old file explained the architecture twice before the diagram:
once as the opening paragraphs, once as the section preamble. The arrow chain
(`DSL -> Rich Plan Domain -> Generated TS Contract -> Runtime Executor`) is
the diagram's spine already; prose restating it added words without
information. The trust-boundary detail from the old second paragraph moved to
its home, Rule 6.

**Reference updated.** `.claude/rules/process-pipeline.md` pointed at
"(Architecture — 5 Layers, 4 Boundaries)"; it now points at "(Architecture)".
No other file referenced the old heading.

## Change 2 — "Operating Standard For This Repository" dissolved

**Effectiveness evaluation.** The section held seven paragraphs. Two carried
unique content: the matrix-row definition (consumed only by Process → Pass
Protocol) and the definition of "rich domain model". The other five restated
ideas that already had homes:

| Paragraph | Already lived in |
|-----------|------------------|
| "DSL source is the requirement" | Rule 1 (DSL Source Before Code) |
| "Archived docs are historical context" | Stated three times across the file |
| "Progress only from committed work" | Pass Protocol |
| "Root CLAUDE.md is authoritative" | Skills section |
| "JSON schema retired" | The Plan Contract section |

A section whose content is 5/7 restatement and whose two live concepts serve
other sections has no subject of its own — that was the confusion. Its
effectiveness was not zero (the matrix row is load-bearing), but the value
sat in the two concepts, not the section.

**What changed.** The matrix-row definition moved into Pass Protocol, which
was the only consumer ("the matrix row defined in Operating Standard above"
now resolves inline). The "rich domain model" definition moved into Key
Concepts. The five restatements were deleted; each idea keeps one home.

## Change 3 — Must Do / Never Do replace the Do/Do-not table

**What changed.** The eight-row table became a `## Must Do` list of positive
statements and a `## Never Do` heading with its own list, placed directly
after Architecture as the quick-reference contract. "Revive JSON schema as a
contract" absorbed the schema-gate prohibitions that previously lived in The
Plan Contract section.

**Why.** Directive: enforce through positive statements, give prohibitions
their own heading. A two-column table forces each row to pair one positive
with one negative even when they are not the same idea; two lists let each
statement stand alone.

**Scope of "one home per idea".** Four ideas appear in both lists: DSL
source over old tests, typed APIs over stringly APIs, committed progress
over uncommitted claims, and Playwright interactions over `page.evaluate()`.

That is the design: Must Do and Never Do are the quick-reference layer; each
idea's home stays in its rule or process section, and the layer restates
each idea in one line. "One home per idea" governs the body of the document,
not this layer.

## Change 4 — Key Concepts groups the domain, one home per idea

**What changed.** A new `## Key Concepts` heading absorbed The Plan Contract,
Plan-Driven IDs, the runtime definition paragraph, and every Core Domain
Lessons entry. The browser object model leads the section, per direction: a
JavaScript object has members — properties, methods, and events with
callbacks — and any member that returns a value can serve as a source
wherever the DSL accepts one.

**Deduplications, each with its single home:**

| Idea | Home | Removed from |
|------|------|--------------|
| Plan-driven IDs, wide-query boundaries | Key Concepts | Rule 7 (now four sentences pointing there) |
| Trust generated plans, boundary errors | Rule 6 | Intro paragraph 2, Plan Contract section |
| JSON schema retired | Never Do | Operating Standard, Plan Contract section |
| Archived docs are historical context | Rule 1 | Operating Standard (twice), Architecture coda |
| Progress only from committed work | Pass Protocol close (Must Do and Never Do restate it) | Operating Standard |
| `page.evaluate()` ban | Never Do (summary), Rule 12 (the exception) | Do/Do-not table |
| Sandbox URL | Build & Run | Plan Contract section |
| Tests are production code | Rule 10 | Architecture preamble |

## Change 5 — Rules content tightened (follow-up approval)

**What changed.** Every rule kept its number and its force; vague phrasing
became declarative sentences. Rule 1 absorbed "the DSL source is the
requirement, not samples/old tests/memories" from Operating Standard. Rule 6
absorbed the bookkeeping-naming sentence and the external-JSON sentence.
Rule 7 shrank to a pointer at Key Concepts. Rule 9 dropped its preamble and
states the protocol directly.

**Why numbering stayed.** Live files reference rules by number:
`.claude/memory/coding-principles.md` (Rule 11),
`.claude/memory/quality-principles.md` (Rule 13),
`.claude/rules/process-task-types.md` and
`.claude/rules/plan-contract-boundary.md` (Rule 3), and the
onboard-fusion-component skill (Rule 8). Renumbering would have broken four
live references to save nothing.

## Change 6 — Style pass (follow-up directive)

**What changed.** Decorative adjectives removed throughout; technical
modifiers kept (typed, generated, sync, deterministic, per-vendor). "Rich
domain model" and "frozen public authoring surface" stayed as named domain
terms. A line-editor review pass produced 13 grammar and construction
findings — ambiguous pronouns, fused sentences, a dangling modifier, broken
list parallelism — and all 13 were verified against the text and applied.

## Result

Measured against HEAD (`c295a56a`); the pre-restructure working tree was 525
lines.

| Measure | Before | After |
|---------|--------|-------|
| Lines | 520 | 502 |
| Words | 3,819 | 3,441 |
| Architecture explanations | 3 (intro, preamble, diagram) | 1 (narrative + diagram) |
| Opening narrative | ~200 words across two passes | ~110 words, one pass |
| Homes per repeated idea | 2–4 | 1, restated once in Must Do/Never Do |
| Sections before Build & Run | 5 | 4 (Architecture, Must Do, Never Do, Key Concepts) |

The word reduction is modest by design: every load-bearing fact stayed. The
gain is structural — one explanation per idea, one home per idea in the body,
and a quick-reference layer that restates without re-explaining.

## Addendum — review fixes 2026-06-11

A review pass after the restructure re-verified every claim above at the
source — counts (`wc`: 502 lines, 3,441 words), heading references (grep:
zero outside this record), rule-number references (Rules 3, 11, 13 and the
Architecture pointer all resolve), and the home of each deleted paragraph.

The claims held. The pass then fixed five findings:

1. `.claude/memory/quality-principles.md` (Positive Framing) still taught
   "Root yourself in pragmatic excellence" as the Right preamble while the
   working tree had replaced that language in `agent-dispatch.md` and
   `process-pipeline.md`. The example now quotes Template 3 and says to
   quote, not paraphrase.
2. "One home per idea" was overclaimed: Must Do/Never Do restate four ideas.
   Change 3 and the Result table now scope the claim to the body.
3. Rule 6 regained its example sentence — if the plan says source A is
   assigned to target B, the runtime reads A and writes B. The restructure
   had deleted it with no home; it was the rule's one concrete statement.
4. The Skills section line on subagent constraints was rewrapped to the
   file's measure.
5. Guidance edits that predate the restructure (the evidence-language pass,
   two checklist additions, and the onboarding-skill description) carried no
   record; a record now exists in
   `docs/superpowers/plans/claude-setup-experiments-rc3.md`.
