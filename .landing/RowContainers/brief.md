# RowContainers: a container under a field prefix sends, reads and checks its own row

## Scope
The owner, 2026-11-25, verbatim, to the five lines of section 10 of
../validation-forms/.landing/ValidationForms/design.md: "i approve it but, we agreed to use container instead of form,
lets settle this here forever. u are welcome to use form in TS to build the payload for http but". So every door,
block and example is written in a container's terms, as ../container-only/.landing/ContainerOnly/design.md, Appendix A,
restates that design's sections 3, 4 and 7: where the two differ, Appendix A's text is the one built.

This unit builds line 1 of that section, B2, B2s, B2c and B3, and the defect of section 2.4:
- Section 4.2, as Appendix A.2 restates it: a container under a field prefix is its view's model. `Container` carries
  `model`, `prefix`, `summary` and `live`, with no `kind`, and `SentInput` gains `path`, in spec/Core/schema.md and
  the generated contract (a change of the grammar the owner approved with line 1); D14.8, D9.15 (A.2's joined text),
  D13.3 and D16.15 gain section 4.2's sentences; model.ts, forms.ts and request.ts change where section 4.2 names, with
  request.ts's `setInput` deleting and appending under the sent name over the set members' fields alone. The name is
  cut by one stated rule, the prefix and its joining dot, and `InputBinding` stays as it is (line 1's recommendation).
- A row's partial sends its own row: `request.Inputs(shift.Container).Set(sent => sent.Id, Model.Id)`, as Appendix A.1
  writes B2. X16.51 to X16.56, X16.51's defect and X16.54's page as A.3 words them; X16.53 written and seen failing
  before B2c's code, the probe section 5 names; X16.55 holds the defect of section 2.4, an Input sent under another path
  losing D16.7 and D16.13, fixed in this unit.
- ContainerRead.tla gains `prefixed` and `slotOutside` as ../validation-forms/.landing/ValidationForms/scratch/tlc-prefix
  holds them: ATypedTextReachesTheEndpoint and AnInputOutsideThePrefixStopsTheSend, the mutants "PrefixKept" and
  "OutsideThePrefixSent", two witnesses, written onto the spec ContainerOnly lands, every config run again, each new
  config with its row in spec/plants/tlc.json.
- Partials.tla takes .landing/RowContainers/scratch/tlc-kin/Partials.tla, section 6's next step written and run by the
  lead on 2026-11-25: the kin's own container cuts its inputs' names at `KinPrefix`; KinContainerBindsItsAttachedMembers
  (an input of the kin's view and of an address attached under it lands in the kin's own member), the mutant
  "KinPrefixKept", and the witnesses ReachKinBoundUnderItsPrefix and ReachKinSendStopsOnNet48 (net48's partial rendered
  with a model stops inside the kin's container). TLC printed, by config: Partials.cfg "Model checking completed. No
  error has been found." "2106 states generated, 2106 distinct states found, 0 states left on queue."; the mutant
  "Invariant KinContainerBindsItsAttachedMembers is violated." and under the base's five invariants "No error has been
  found."; each witness "is violated", the state reached. Each config joins spec/plants/tlc.json.
- Section 6's consequence stands as written: each row's Save is a lane of its own, and the endpoint orders them.
- B2c's one unrun observation (spec/Reference/move-in.md, MoveInSlice's "Decided" line 1: a check in an attached
  partial's own plan left the container's summary empty, not isolated) does not reproduce at 48839f502. The lead, on
  2026-11-25, moved Review resident details from ResidentDesk.cshtml into Resident.cshtml's own plan, the partial
  MoveIn.cshtml:34 attaches by `Html.PartialFor`, built the app fresh (its dll 14:04:01 after the edit at 14:03:36) and
  ran M9.X21's test, which reads "The email addresses must match." in #move-in_summary: "1 selected, 1 ran, 1 passed, 0
  failed, 0 over budget"; then restored both views from HEAD. The code agrees: FormChecks.cs writes the summary place
  for every check with no request, model.ts `summaryOf` reads the block's entry across every live plan, and places.ts
  writes the summary by its id.
Left alone: the page's own container under no prefix (the owner's "OwnPlanStops" stays open there); the misname of a
net10 partial rendered with a model under an inherited prefix, which section 6 names and does not close; F7; the grid.

Base: the lane's head once ValidationForms lands. Before the builder starts, the lead moves ../row-containers there in
one command, `git checkout --detach <head>` and `npm ci` at the root, and runs each command of "At the base:" there.
Built by a builder agent, .claude/agents/builder.md, on Opus at xhigh.
After: ValidationForms

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
spec/InputsAndIds/ContainerRead.tla
spec/PartialsAndSlots/Partials.tla
spec/Reference/move-in.md
../validation-forms/.landing/ValidationForms/design.md
../container-only/.landing/ContainerOnly/design.md

## Outcome
A row's partial saves, approves and checks its own row with the doors F1 to F5 use; its container sends, reads and
checks its inputs under the names past its view's prefix; an input outside the prefix stops the send; X16.51 to X16.56
pass, each side failing on its plant; ContainerRead.tla and Partials.tla hold the case with each mutant failing; every
written test passes at the tree.

## Done
At the base: in the lane at 48839f502, whose tree is 2f84ee35b's but for spec/STATE.json and HANDOFF.md, "spec check at
2f84ee35b: 40 of 40 checks pass" (.work/logs/spec-check-2f84ee35b.log); the land of 3efe316b8, "838 selected, 838 ran,
838 passed, 0 failed, 0 over budget" (.work/logs/pw-all-unit-f2dd7543e-23899.log); TLC on copies of the lane's files,
run by the lead on 2026-11-25: spec/InputsAndIds/ContainerRead.tla under ContainerRead.cfg, "Model checking completed. No
error has been found." and "114912 states generated, 8352 distinct states found, 0 states left on queue."; and
spec/PartialsAndSlots/Partials.tla under Partials.cfg, "Model checking completed. No error has been found." and "2106
states generated, 2106 distinct states found, 0 states left on queue.". Each runs again at ValidationForms' head before
the builder starts.
X16.53 at the base, before B2c's code: failing, its line quoted.
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc: its pass line; each new mutant violating its invariant, each
witness reached, every other config of both specs as before.
Each new or rewritten test before the code, scripts/playwright.sh --test with its constant: failing, its line quoted.
Each after the code: "1 selected, 1 ran, 1 passed, 0 failed, 0 over budget", on net10; the net48 half is CI's Windows job.
scripts/plant-defect.sh on each side of each new or rewritten test: "[plant] caught", read at the test's line it prints.
dotnet test tests/Alis.Reactive.ModuleTests, dotnet test tests/Alis.Reactive.ArchitectureTests and
node node_modules/vitest/vitest.mjs run: their pass lines.
scripts/check.sh --no-spec: "[check] every check passed."
scripts/playwright.sh --all on the finished files: "N selected, N ran, N passed, 0 failed, 0 over budget".
python3 spec/tools/check.py in the worktree: "40 of 40 checks pass".
paths.txt, message.txt and spec.py in .landing/RowContainers, message.txt holding each proof line above and a "Not run:" line.

## Commands
. scripts/toolchain-env.sh
caffeinate -i around every run longer than a minute
git rev-parse --short=9 HEAD; git status; git diff; git show; git grep
python3 -I -B .landing/RowContainers/spec.py .
python3 spec/tools/check.py --generate
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc
TLC on a copy under .landing/RowContainers/scratch/:
( cd .landing/RowContainers/scratch/<copy> && /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config <name>.cfg <name>.tla < /dev/null > tlc.log 2>&1 )
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
reply.md in .landing/RowContainers, at most 90 lines, opening with the rule lines applied by file and line at the base, then
each line of "Done" as its command printed it, then each form decided and each gap left with its use, then what did
not run.
