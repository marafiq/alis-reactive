# ValidationForms: a check names its rule set, and a set its validator runs no rule under is refused

## Scope
The owner, 2026-11-25, verbatim, to the five lines of section 10 of
../validation-forms/.landing/ValidationForms/design.md: "i approve it but, we agreed to use container instead of form,
lets settle this here forever. u are welcome to use form in TS to build the payload for http but". So every door,
block and example is written in a container's terms, as ../container-only/.landing/ContainerOnly/design.md, Appendix A,
restates that design's sections 3, 4 and 7: where the two differ, Appendix A's text is the one built.

This unit builds lines 2 and 3 of that section, records lines 4 and 5, and leaves line 1 to RowContainers, the unit
after it:
- Line 2, F5's door: `pipeline.Validate(container, ruleSet)` beside `pipeline.Validate(container)`, the PublicAPI line
  Appendix A.2 quotes, in both texts, with its XML doc; the check with no request runs the set it names, where
  FormChecks.cs fixes the default set today; N16.3's and D16.5's sentences of section 4.1; X16.48, its page as A.3 states.
- Line 3, the refusal of section 4.3: a check under a rule set its validator runs no rule under is refused where the
  view renders and where the endpoint checks, with X16.49's sentence; D16.2's sentence; X16.49 and X16.50 (B1s, "Approve
  all"), on A.1's roster. Section 10, line 3, names each of the app's 9 named checks in a view and 5 at an endpoint as
  declaring its set; the walk runs over each before the code lands, and a page it refuses is a stop.
- Validation.tla gains the rule set as scratch/tlc-ruleset/Validation.tla holds it (`asked`, `inDefault`, `inNamed`):
  TheNamedSetIsTheOneRun and ANamedSetSelectsARule beside Agreement, the mutants "CheckRunsTheDefaultSet" and
  "ANamedSetNoRuleDeclaresRenders", and two witnesses, written onto the spec FormDataBoundary lands, every config of the
  spec run again, each new config with its row in spec/plants/tlc.json.
- Lines 4 and 5: nothing is built. F7's form (section 4.5, as A.2 restates it) and the grid's need (section 8) go into
  the decision lines of spec/CLAUDE.md, with the owner's words in spec/NOTES.md, by spec.py.
- Section 5's one open fact, read at source by the lead on 2026-11-25: spec/probes/aspnetwebstack/DefaultModelBinder.cs.txt:217
  returns when no provider holds a member's key, so on MVC 5 a text member no key names keeps its start (F5); X16.48's
  twin cites that line.
Left alone: section 4.2 and X16.51 to X16.56, RowContainers' unit; F7's door; the grid.

Base: the lane's head once FormDataBoundary lands. Before the builder starts, the lead moves ../validation-forms there
in one command: `git checkout HEAD -- spec/MISTAKES.md`, `git checkout --detach <head>`, `npm ci` at the root, then
re-applies the lead's counts design-read.md names (written from memory +3, a number not read +1), and runs each
command of "At the base:" there. Built by a builder agent, .claude/agents/builder.md, on Opus at xhigh.
After: FormDataBoundary

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
spec/Validation/Validation.tla
.landing/ValidationForms/design.md
../container-only/.landing/ContainerOnly/design.md

## Outcome
A check without a request runs the rule set it names, on both sides; a check under a set its validator runs no rule
under is refused where the view renders and where the endpoint checks; X16.48 to X16.50 pass, each side failing on its
plant; Validation.tla holds the rule set with each mutant failing; every written test passes at the tree.

## Done
At the base: in the lane at 48839f502, whose tree is 2f84ee35b's but for spec/STATE.json and HANDOFF.md, "spec check at
2f84ee35b: 40 of 40 checks pass" (.work/logs/spec-check-2f84ee35b.log); the land of 3efe316b8, "838 selected, 838 ran,
838 passed, 0 failed, 0 over budget" (.work/logs/pw-all-unit-f2dd7543e-23899.log); TLC on copies of the lane's files,
run by the lead on 2026-11-25: spec/Validation/Validation.tla under Validation.cfg, "Model checking completed. No error has
been found." and "2654424 states generated, 1327212 distinct states found, 0 states left on queue.". Each runs again
at FormDataBoundary's head before the builder starts.
The walk of section 10, line 3, over the app's 14 named checks: each check and the set it names, none refused.
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc: its pass line; each new mutant violating its invariant, each
witness reached, every other config of the spec as before.
Each new or rewritten test before the code, scripts/playwright.sh --test with its constant: failing, its line quoted.
Each after the code: "1 selected, 1 ran, 1 passed, 0 failed, 0 over budget", on net10; the net48 half is CI's Windows job.
scripts/plant-defect.sh on each side of each new or rewritten test: "[plant] caught", read at the test's line it prints.
dotnet test tests/Alis.Reactive.ModuleTests, dotnet test tests/Alis.Reactive.ArchitectureTests and
node node_modules/vitest/vitest.mjs run: their pass lines.
scripts/check.sh --no-spec: "[check] every check passed."
scripts/playwright.sh --all on the finished files: "N selected, N ran, N passed, 0 failed, 0 over budget".
python3 spec/tools/check.py in the worktree: "40 of 40 checks pass".
paths.txt, message.txt and spec.py in .landing/ValidationForms, message.txt holding each proof line above and a "Not run:" line.

## Commands
. scripts/toolchain-env.sh
caffeinate -i around every run longer than a minute
git rev-parse --short=9 HEAD; git status; git diff; git show; git grep
python3 -I -B .landing/ValidationForms/spec.py .
python3 spec/tools/check.py --generate
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc
TLC on a copy under .landing/ValidationForms/scratch/:
( cd .landing/ValidationForms/scratch/<copy> && /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config <name>.cfg <name>.tla < /dev/null > tlc.log 2>&1 )
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
reply.md in .landing/ValidationForms, at most 90 lines, opening with the rule lines applied by file and line at the base, then
each line of "Done" as its command printed it, then each form decided and each gap left with its use, then what did
not run.
