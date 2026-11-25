# ContainerOnly: one concept on the developer's surface, the request container

## Scope
The owner, 2026-11-25, verbatim, approving ValidationForms: "i approve it but, we agreed to use container instead of
form, lets settle this here forever. u are welcome to use form in TS to build the payload for http but"; and, to the
two lines of design.md, section 12, in this folder: "yes". The owner, 2026-11-08 (spec/NOTES.md, the entry on the form
gap): "Its bascially RequestContainer ... but u choose one" and "form should be kept in TS runtime as a way to stay
native to send body". The two yeses: a request sends what its plan declares and nothing else, so a control the plan does
not know is no longer sent; and `Container.kind` leaves the plan, a change of the grammar that is the owner's by
spec/BRIEF.md:117 and given.

The task: build design.md in this folder whole, as it decides, in one commit. design-read.md is the lead's read of it;
design-brief.md and design-reply.md are the designer's.
- Section 3: the nine lines leave both PublicAPI texts (net10.0 and net48), each replaced by the line the table names;
  no public symbol is added. `RequestContainer<TModel>` is then derived by nothing and is declared sealed.
- Section 4's verdicts, each by the landed door it names.
- Section 5: the container's block renders a fieldset.
- Section 6: `Container.kind` and `ContainerKind` leave the grammar, the generated contract and plan.schema.json, and the
  contract is regenerated in the same commit; `alis.form`, `FormCheck` and `FormPlace` stay.
- Section 7: the browser by file and line: page.ts reads no form element, `submits` and `Submission` leave; request.ts
  keeps `new FormData()` to build the payload.
- Section 8: every D, N, R and X line and every rule-file line at its new full text, written by spec.py for spec/.
- Section 9: the 79 views and 80 block lines; the eight views of 9.1's table, each as its row says; the ten Fusion views
  by 9.1's verdicts, CarePlanUpload's test run first on the container, and if EJ2 changes its outcome the view writes its
  own form around the container, as 9.1 says. X9.15 and M21.X1038 are restated with a fixture of their own each, the old
  fixtures leaving with the old text (9.2); X16.18 keeps its fixture, the lead's judgment against .claude/rules/tests.md
  rule 1: its gesture, outcome, opposite and defect keep their words and only how Code is sent changes. MI.30 takes 8.2's
  text and the move-in's tests follow it. spec/api's stub, sentences and refused cases change as 9.3's table says; X9.14
  stays its own refused case, not folded into X10.23; each refused case's code is read from the compiler once, and
  PageFormGone takes the code the compiler prints.
- Section 10: ContainerRead.tla gains `handWritten` with the mutant "HandWrittenControlSent" and the witness
  ReachAHandWrittenControlLeftUnsent; Request.tla gains `NativeSubmit` with the mutant "ContainerSubmitsNatively"; each
  with its violates config and its row in spec/plants/tlc.json. The designer's copies under scratch/ are their text.
- Kept as the designer decided them: the fieldset; `AntiforgeryIn.FormField`; `alis.form`, `FormCheck`, `FormPlace`;
  the 15 fixture names that say a form.

Probes first, each before the code it rests on: whether `File`'s member lambda takes a list position's member,
`Documents[0].File` (compile it); what a declared NativeCheckBox sends while unchecked, read at its source and in a run,
which decides the literal at WhenReceptionSignsInAVisitor.cs:103. A probe that prints otherwise than the design assumes
is a gap: leave that piece, name the gap with its use, and reply; invent no member, syntax or runtime behavior.

Left alone: FormDataBoundary's read of a member sent outside the body (D16.5, Validation.tla) and ValidationForms'
doors and meanings, each its own unit after this one; FormDataBoundary's section 8; F7.

Base f2986ea66. Worktree ../container-only, detached at the base, node_modules installed at the root. Built by a
builder agent, .claude/agents/builder.md, on Opus at xhigh. Its spec/MISTAKES.md holds the base's counts; the builder adds
its own, and its spec.py edits no row of that file.
After: none

## Rules
.claude/agents/builder.md
CLAUDE.md
.claude/rules/process.md
.claude/rules/csharp.md
.claude/rules/views.md
.claude/rules/tests.md
.claude/rules/docs.md
Alis.Reactive/CLAUDE.md
Alis.Reactive.Assets/CLAUDE.md
tests/Alis.Reactive.Conformance.App/CLAUDE.md
tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md
tests/Alis.Reactive.Conformance.World/CLAUDE.md
tests/Alis.Reactive.ModuleTests/CLAUDE.md
spec/CLAUDE.md
spec/MISTAKES.md
spec/NOTES.md
spec/BRIEF.md
spec/REWRITE.md
spec/Core/DESIGN.md
spec/Core/schema.md
spec/Reference/move-in.md
spec/InputsAndIds/ContainerRead.tla
spec/RequestsAndResponses/Request.tla
.landing/ContainerOnly/design.md

## Outcome
A developer writes a request container and never a form: `Html.BeginRequestContainer`, `RequestContainer<TModel>`,
`request.Inputs(container)` and the body doors over a container, on both hosts. A request sends what its plan declares
and nothing else; the browser's own post stays where a view writes its own form. The plan carries no container kind.
Every line of design.md's sections 3 to 10 stands in the code, the app, the tests, spec/api and spec/, each changed
promise proven by its test failing on its planted defect, and every written test passing at the tree.

## Done
At the base: in the lane, "spec check at 3efe316b8: 40 of 40 checks pass" (.work/logs/spec-check-3efe316b8.log), whose
tree f2986ea66 holds but for spec/STATE.json and HANDOFF.md; the land of 3efe316b8, "838 selected, 838 ran, 838 passed,
0 failed, 0 over budget" (.work/logs/pw-all-unit-f2dd7543e-23899.log); TLC on copies of the lane's files at f2986ea66,
run by the lead on 2026-11-25: ContainerRead.cfg "Model checking completed. No error has been found." and "114912
states generated, 8352 distinct states found, 0 states left on queue."; Request.cfg "Model checking completed. No error
has been found." and "18308256 states generated, 18308256 distinct states found, 0 states left on queue.".
git -C ../container-only rev-parse --short=9 HEAD: "f2986ea66".
Each probe: the line its compile or run printed, and the decision it settled.
CarePlanUpload's test on the container, before any other view changes: its scripts/playwright.sh line.
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc: its pass line; "HandWrittenControlSent" violating
AnUnsentMemberReadsWhatTheEndpointBinds, "ContainerSubmitsNatively" violating FailedCheckSendsNothing, the witness
reached, and every other config of both specs as before.
Each changed or new test before the code, scripts/playwright.sh --test with its constant: failing, its failure line quoted.
Each changed or new test after the code: "1 selected, 1 ran, 1 passed, 0 failed, 0 over budget".
scripts/plant-defect.sh on each side of each changed or new test: "[plant] caught", read at the test's line it prints.
dotnet test tests/Alis.Reactive.ModuleTests, dotnet test tests/Alis.Reactive.ArchitectureTests and
node node_modules/vitest/vitest.mjs run: their pass lines.
The search for the nine doors' names, `git grep -n -E` over the shipped projects, spec/api, the conformance app, the tests
and the rule files: its line, each hit left with its reason.
scripts/check.sh --no-spec: "[check] every check passed."
scripts/playwright.sh --all on the finished files: "N selected, N ran, N passed, 0 failed, 0 over budget".
python3 spec/tools/check.py in the worktree: "40 of 40 checks pass".
paths.txt, message.txt and spec.py in .landing/ContainerOnly, message.txt holding each proof line above and a
"Not run:" line; MI.30's owner words and the two decisions' lines in spec/CLAUDE.md and spec/NOTES.md by spec.py.

## Commands
. scripts/toolchain-env.sh
caffeinate -i around every run longer than a minute
git -C ../container-only rev-parse --short=9 HEAD; git status; git diff; git show; git grep
python3 -I -B .landing/ContainerOnly/spec.py .
python3 spec/tools/check.py --generate
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc
TLC on a copy under .landing/ContainerOnly/scratch/:
( cd .landing/ContainerOnly/scratch/<copy> && /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config <name>.cfg <name>.tla < /dev/null > tlc.log 2>&1 )
dotnet run --project tools/PlanContractGenerator -c Release --property:RestoreLockedMode=true -- Alis.Reactive.Assets/runtime/Schema
dotnet build of a project the unit changes
dotnet test tests/Alis.Reactive.ModuleTests
dotnet test tests/Alis.Reactive.ArchitectureTests
node node_modules/vitest/vitest.mjs run
scripts/playwright.sh --test; scripts/playwright.sh --all
scripts/plant-defect.sh
scripts/check.sh --no-spec
python3 spec/tools/check.py

## Reply
reply.md in .landing/ContainerOnly, at most 100 lines, opening with the rule lines applied by file and line at
f2986ea66, then each line of "Done" as its command printed it, then each form decided and each gap left with its use,
then what did not run.
