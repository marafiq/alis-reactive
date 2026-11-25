# FormDataBoundary: a member sent outside the body is checked where MVC's binder reads it (choice C)

## Scope
The owner, 2026-11-25, verbatim, to design.md's question of D16.5 (this folder): "C (Recommended)"; and to its section 8,
a query member carried in the body too: "if a input is part of body and validator on server already works great. and if
dev also sent in query, it dev business logic how it will be validated. There is no solution around this problem is
there. so i lean toward no", then "yes lets go with no. and note down the quirk". design-read.md is the lead's read of
the design; design-brief.md and design-reply.md are the designer's.

The task: build design.md's sections 6 and 7 whole, in one commit.
- D16.5 takes section 6's text. A member the request sends outside its body is read where MVC's binder reads it: on
  MVC 5 the body, then the path, then the query; on net10 a body that binds members is read alone; a text, a file or no
  body binds no member, so the check reads the path's and the query's members as form data and every other at its start.
- The quirk, written in D16.5 and in the remarks of the XML doc of the `Query` doors of `BodyParts<TRequest>` and
  `BodiedParts<TRequest>`, in the developer's words of .claude/rules/docs.md: on .NET 10, a query member sent beside a
  body that binds members is not read by the browser's check, which reads it at its start; an endpoint that binds its
  model `[FromForm]` or `[FromBody]` binds it the same way, while one that binds a plain model binds the query's value,
  so there the endpoint's own check decides it and its message shows after the request. A model whose starting value
  breaks its own rule is refused in the browser even where the query sends a value the rule passes.
- ContainerOnly lands D16.5's sentence that a hidden input no plan binds is sent in a form body and read by no check
  (the owner, 2026-11-25: "container should take care of hidden fields in TS runtime"); section 6's text keeps it
  beside the query member, the same family, and spec.py's D16.5 anchors are read at the moved head before they run.
- Section 7's probes ran before this unit was built, each as the design states (the lead, 2026-11-25), their source
  and lines in .landing/FormDataBoundary/scratch/probes: net10.txt, a plain model binding the route's Ward and the
  query's Shift with no body, a text body and a file body, a `[FromForm]` model binding neither, "Shift=" binding null,
  a multipart body to a plain model binding the query's Shift and to a `[FromForm]` model binding none (the quirk), the
  body's Shift winning over the query's; net48.txt, MVC 5 at source. Each D line and test cites the line it rests on.
- Section 7's promises: X16.46's three rewritten on net10 and net48 (the bed notes page, its endpoints binding a plain
  model); a bodiless DELETE with Shift in the query, both hosts; on net48 a multipart form with Shift in the query beside
  it; on net10 that form to an endpoint that binds `[FromForm]`, which is the quirk's promise; a GET under a check with
  its member in the path, refused and sent. Each written failing first, its own fixture where its situation is its own
  (.claude/rules/tests.md, rule 1), each side failing on its planted defect.
- Section 6's TLA+: spec/Validation/Validation.tla takes the copy scratch/design/Validation.tla; its configs as section
  6's table names them: the invariant SentOutsideAgrees with its seven mutants, the five witnesses, and the base's 14
  other configs, each with its row in spec/plants/tlc.json; UnboundBodyChecksTheStarts' config and its witness leave.
  The config holds.QueryCarriedInTheBody and the witness ReachEveryEndpointOnACarriedMember belong to section 8 and do
  not land.
- Section 7's carriers: D16.5, X16.46 and the checker's line; spec/Validation/features.json, spec/Reference/app.json and
  page-doors.json; RequestPartsWriter's query entries where the body binds none, and beside a body that binds members on
  net48; forms.ts's arms for none, text and file, which read the entries over the start as form data; the bed notes
  page, its actions and a ".Net48.cs" controller; X16.46's fixtures. No grammar node and no contract field changes.
- The owner's words of 2026-11-25 above go to spec/NOTES.md, and the decision line to spec/CLAUDE.md, by spec.py.

Left alone: section 8; ValidationForms, the unit after this one; every request door's surface but the XML doc above.

Base: the lane's head once ContainerOnly lands. Before the builder starts, the lead moves ../form-data-boundary there
in one command: `git checkout HEAD -- spec/MISTAKES.md`, `git checkout --detach <head>`, `npm ci` at the root, then
re-applies the lead's counts design-read.md names (a rule not applied +3, a number not read +2, a shell command +3, written from memory +1: the lead's first net10 probe sent a field named as the action's parameter, which MVC reads as the model's prefix; the third shell count a grep --include the traps hook refused),
and runs each command of "At the base:" there. Built by a builder agent, .claude/agents/builder.md, on Opus at xhigh.
After: ContainerOnly

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
spec/Validation/Validation.tla
.landing/FormDataBoundary/design.md

## Outcome
A request is checked in the browser on what the endpoint binds wherever MVC's binder decides it: on MVC 5 its body, path
and query; on net10 its body alone when the body binds members, and its path and query as form data when it binds none.
The quirk stands written where a developer reads it. Every promise of section 7 passes on its host, each side failing on
its plant; Validation.tla is green with each mutant failing; every written test passes at the tree.

## Done
At the base: in the lane at 48839f502, whose tree is 2f84ee35b's but for spec/STATE.json and HANDOFF.md, "spec check at
2f84ee35b: 40 of 40 checks pass" (.work/logs/spec-check-2f84ee35b.log); the land of 3efe316b8, "838 selected, 838 ran,
838 passed, 0 failed, 0 over budget" (.work/logs/pw-all-unit-f2dd7543e-23899.log); TLC on a copy of the lane's
spec/Validation/Validation.tla under Validation.cfg, run by the lead on 2026-11-25: "Model checking completed. No error
has been found." and "2654424 states generated, 1327212 distinct states found, 0 states left on queue.". Each runs again
at ContainerOnly's head before the builder starts.
git -C ../form-data-boundary rev-parse --short=9 HEAD: the head the lead moved it to.
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc: its pass line; each mutant of SentOutsideAgrees violating it,
each witness reached, every other config of the spec as before.
Each new or rewritten test before the code, scripts/playwright.sh --test with its constant: failing, its line quoted.
Each after the code: "1 selected, 1 ran, 1 passed, 0 failed, 0 over budget", on net10; the net48 half is CI's Windows job.
scripts/plant-defect.sh on each side of each new or rewritten test: "[plant] caught", read at the test's line it prints.
dotnet test tests/Alis.Reactive.ModuleTests, dotnet test tests/Alis.Reactive.ArchitectureTests and
node node_modules/vitest/vitest.mjs run: their pass lines.
scripts/check.sh --no-spec: "[check] every check passed."
scripts/playwright.sh --all on the finished files: "N selected, N ran, N passed, 0 failed, 0 over budget".
python3 spec/tools/check.py in the worktree: "40 of 40 checks pass".
paths.txt, message.txt and spec.py in .landing/FormDataBoundary, message.txt holding each proof line above and a
"Not run:" line.

## Commands
. scripts/toolchain-env.sh
caffeinate -i around every run longer than a minute
git -C ../form-data-boundary rev-parse --short=9 HEAD; git status; git diff; git show; git grep
dotnet new console and dotnet run, under .landing/FormDataBoundary/scratch, for a probe
python3 -I -B .landing/FormDataBoundary/spec.py .
python3 spec/tools/check.py --generate
ALIS_TLC_WORKERS=2 python3 spec/tools/check.py --check tlc
TLC on a copy under .landing/FormDataBoundary/scratch/:
( cd .landing/FormDataBoundary/scratch/<copy> && /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config <name>.cfg <name>.tla < /dev/null > tlc.log 2>&1 )
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
reply.md in .landing/FormDataBoundary, at most 90 lines, opening with the rule lines applied by file and line at the
base, then each line of "Done" as its command printed it, then each form decided and each gap left with its use, then
what did not run.
