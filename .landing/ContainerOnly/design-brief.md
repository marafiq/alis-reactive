# ContainerOnly: one concept on the developer's surface, the request container

## Scope
The owner, 2026-11-25, verbatim, approving ValidationForms' five lines: "i approve it but, we agreed to use container
instead of form, lets settle this here forever. u are welcome to use form in TS to build the payload for http but". The
owner, 2026-11-08 (spec/NOTES.md, the entry on the form gap): "Its bascially RequestContainer because we are dealing
with diverse cases here... but u choose one...definately not pageform" and "form should be kept in TS runtime as a way
to stay native to send body.. but here in framework core design, you can definatly gain great features without loosing
anything major". The lead read both as: every C# door, block, handle and example a developer writes names a request
container, never a form; inside the TS runtime a FormData (page.ts, request.ts) may build an HTTP payload. The lead kept
two handles instead (D14.8, N14.3, spec/CLAUDE.md's entry of 2026-11-12), and that is the mistake this unit closes.

The task: the complete design of that change, to the bar of ValidationForms' design (validation-forms-design.md in this
folder). It answers, each read at the source at the base:
- Every public door that names a form, in both PublicAPI.Unshipped.txt texts (net10.0 and net48): `BeginRequestForm`,
  `RequestFormBlock<TModel>` and its `Form`, `RequestForm<TModel>`, `CondensedRequests.Form` on `BodyParts` and on
  `QueryParts`, `BodyParts.Multipart(RequestForm, ..)` and `BodyParts.UrlEncoded(RequestForm, ..)`, and any other a
  search finds (name the search and its count). For each, the door that replaces it, since a line leaves a PublicAPI
  text only when the commit names its replacement, or "none" with the reason.
- What `Form(form)` gives today that `Inputs(container)` does not (D9.15): a native form's submission set, hand-written
  controls, a control the `form` attribute joins, file inputs, a GET's query. For each: kept by a container (how, stated
  by C# and carried by the plan), served by another landed door (which, cited), or lost, named plainly with its use for
  the owner. "Never lose a feature" (root CLAUDE.md) holds: a loss is the owner's, one line.
- The element a container's block renders, and why: a fieldset nests where a form cannot, a form element has native
  behaviors (implicit submission on Enter, the form attribute); decide by quality and name it decided.
- What the plan carries: the `Container` entry's kind and every node or field that names a form (schema.md), each kept
  or gone; a change of the grammar is the owner's, one line.
- The browser: each place the runtime treats a container as a native form, by file and line, and what it becomes; FormData
  stays wherever it builds a payload.
- Every D, N, R and X line, and every rule file line, that names the form doors (D9.15, D14.8, N14.3, N18.3, X10.23 and
  whatever a search finds), each with its new full text.
- The app and the tests: every view and test that writes a form door, counted by a command whose line is quoted; each
  test or promise whose meaning rests on a native submission set (the bench cell http.beside.hidden among them) and its
  verdict; spec/api's compiled sentences and refused cases that name a form.
- The TLA+ specs each change meets (ContainerRead.tla and Request.tla first), each claim of a spec left unchanged held by
  a TLC run on a copy, as designer.md states.
- Appendix: validation-forms-design.md's examples and lines (sections 3, 4 and 7) restated in container terms, each
  example whole, so the owner reads one grammar. ValidationForms' five decisions are approved and stand: change only the
  doors its examples use.

Base f2986ea66. Worktree ../container-only, detached at the base, node_modules installed at the root and in
Alis.Reactive.Assets. Built by a designer agent, .claude/agents/designer.md, on the latest Fable at xhigh. It is
read-only: it writes design.md and reply.md in this landing folder, .landing/ContainerOnly/, and scratch files under
.landing/ContainerOnly/scratch/, and changes no tracked file.
After: none

## Rules
.claude/agents/designer.md
CLAUDE.md
.claude/rules/docs.md
.claude/rules/process.md
.claude/rules/views.md
.claude/rules/csharp.md
.claude/rules/tests.md
Alis.Reactive/CLAUDE.md
Alis.Reactive.Assets/CLAUDE.md
tests/Alis.Reactive.Conformance.App/CLAUDE.md
spec/CLAUDE.md
spec/MISTAKES.md
spec/NOTES.md
spec/BRIEF.md
spec/REWRITE.md
spec/Core/DESIGN.md
spec/Core/schema.md
spec/InputsAndIds/ContainerRead.tla
spec/RequestsAndResponses/Request.tla

## Outcome
design.md in this landing folder, in the shape .claude/rules/docs.md, "A design put to the owner", states the change
whole: the doors that leave and the door that replaces each, as PublicAPI lines; what a container keeps and what is
lost; the element; the plan; the browser by file; every line's new text; the app's and the tests' counts with their
commands; the TLA+ verdicts with their runs; the appendix; the two strongest objections with answers; and one line for
the owner for each decision on a principle, a closed rule, the grammar or a lost feature, in root CLAUDE.md's form,
"Spec-first", step 3. Every claim names its file and line at f2986ea66, and every count is the line its command printed.

## Done
At the base: python3 spec/tools/check.py at 3efe316b8, whose tree f2986ea66 holds but for spec/STATE.json and
HANDOFF.md, printed "spec check at 3efe316b8: 40 of 40 checks pass" (.work/logs/spec-check-3efe316b8.log in the lane).
At the base, the TLC command of Commands on a copy of spec/InputsAndIds/ContainerRead.tla printed "Model checking
completed. No error has been found." (ValidationForms' run of the same file and command, at 830817f4b, whose
ContainerRead.tla f2986ea66 holds unchanged).
design.md written; reply.md written, opening with the rule lines applied.

## Commands
git -C ../container-only rev-parse --short=9 HEAD
git show f2986ea66:<path>; git grep <pattern> f2986ea66 -- <paths>; git log; git ls-tree; git status
ls and mkdir under .landing/ContainerOnly/; wc -l and grep over scratch files and over git show's output
TLC on a copy of a spec, under .landing/ContainerOnly/scratch/ only:
( cd .landing/ContainerOnly/scratch/<copy> && /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config <name>.cfg <name>.tla < /dev/null > tlc.log 2>&1 )

## Reply
reply.md, at most 60 lines: the rule lines applied, by file and line at f2986ea66; each line of Done as printed; each
TLC run with its last line; what did not run. design.md is the answer, as long as completeness takes and no longer.
