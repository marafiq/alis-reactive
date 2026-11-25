# ValidationForms: the designer's reply

Head: `git -C ../validation-forms rev-parse --short=9 HEAD` printed "830817f4b". The answer is design.md, 1036 lines.

## Rule lines applied, by file and line at 830817f4b
- .claude/agents/designer.md:43-45 (no writing git command; decide only what the brief asks), :52-56 (every claim with
  its file and line; a spec stands only by a TLC run on a copy; an absence names its search; a defect names the spec
  line it breaks and the app's own use), :60-62 (a case holds a line only by a plant that ran: every plant here is named unrun).
- .claude/rules/docs.md:72-80, "A design put to the owner": each meaning quoted from its D line, each search with the
  doors and files it read and did not, each count the line its command printed.
- CLAUDE.md:47 (TLA+ governs every ordering), :52 (the schema governs the seam), :63 (a door is typed), :90 (a decision
  on a principle goes to the owner as one line, its counts read), :142 (a gap is raised, never invented), :32 (bash).
- spec/CLAUDE.md:17 (one shared operation before a new form; a concrete case and a shown difference in meaning),
  :89-91 (G13), :1443-1453 (the rule set's form, and its refusal held open).
- spec/BRIEF.md:62 (the highest rung), :117 (a unit never changes the grammar: the owner's), :119 (no guessing in the runtime).
- .claude/rules/tests.md:38, :44; .claude/rules/views.md:12; .claude/rules/csharp.md:25; Alis.Reactive/CLAUDE.md:9;
  Alis.Reactive.Assets/CLAUDE.md:15; tests/Alis.Reactive.Conformance.App/CLAUDE.md:19, :23; spec/MISTAKES.md:16, :18;
  spec/REWRITE.md:431-460 (section 2.6).

## Done, each line as printed
- At the base (the lead's run, scratch/probe-tlc/tlc.log, read): "Model checking completed. No error has been found."
  "114912 states generated, 8352 distinct states found, 0 states left on queue."
- design.md written; reply.md written, opening with the rule lines applied.
- `git status --short` printed " M spec/MISTAKES.md" alone, the change the worktree held at its start.

## TLC runs, each by the brief's command through scratch/run-tlc.sh, with its result line
Copy scratch/tlc-prefix (ContainerRead.tla with a container under a prefix):
- ContainerRead.cfg, eight invariants, no mutant: "Model checking completed. No error has been found." "344736 states generated, 25056 distinct states found, 0 states left on queue."
- ContainerRead.PrefixKept.sixOnly.cfg, the base's six under today's reading: "Model checking completed. No error has been found." So the base's spec does not hold the case.
- ContainerRead.violates.ATypedTextReachesTheEndpoint.cfg: "Error: Invariant ATypedTextReachesTheEndpoint is violated by the initial state:"
- ContainerRead.violates.AnInputOutsideThePrefixStopsTheSend.cfg: "Error: Invariant AnInputOutsideThePrefixStopsTheSend is violated."
- two witnesses: "Invariant ReachATextBoundUnderAPrefix is violated." and "Invariant ReachStopOnAnInputOutsideThePrefix is violated."
Copy scratch/tlc-ruleset (Validation.tla with a rule set on its check plane):
- Validation.cfg, fourteen invariants: "Model checking completed. No error has been found." "2655504 states generated, 1327752 distinct states found, 0 states left on queue."
- Validation.violates.TheNamedSetIsTheOneRun.cfg and Validation.violates.Agreement.CheckRunsTheDefaultSet.cfg: each "is violated by the initial state:"
- Validation.violates.ANamedSetSelectsARule.cfg: "Error: Invariant ANamedSetSelectsARule is violated by the initial state:"
- two witnesses, each violated; Validation.CheckRunsTheDefaultSet.baseOnly.cfg: "No error has been found."
One run failed first and ran again: the rule-set copy's two witnesses stood above `Agreement`, TLC printed
"Unknown operator: `Agreement'.", and they were moved below it.

## What the design decides
- Built, each confirmed at the source with the test that proves it: F1, F1e, F2, F3 (two doors, on no one request), F4, F6, F8, P1, P2, B1, B4, B5.
- New: F5's door `pipeline.Validate(container, ruleSet)`; B2, B2s, B2c and B3 by one meaning, a container under a field
  prefix is its view's model, with two fields of the plan's contract and no door; B1s by the validator's own line and
  one refusal. F7 is designed and not built. G1 and G2 take no core door.
- Found at the source: use-cases.md's F1 test proves F1e; its F8 case shows in the notice; its G search missed three
  grid views; an `Input` sent under another path loses its typed-text refusal and its inline place (read, not run).
- Five lines for the owner, design.md section 10.

## What did not run, and what was not read
- No browser test, build, pure test or plant: every new promise (X16.48 to X16.56) and the defect of 2.4 are unrun.
- TLC on no spec but the two copies: Partials.tla, which B2's meaning also meets, Request.tla, Lanes.tla and
  Ownership.tla were not run; the base's 21 other ContainerRead configs and 16 other Validation configs not run on the copies.
- Read in part, by the lines design.md section 11 lists, where the brief's Rules name each whole: spec/Core/DESIGN.md,
  spec/Core/schema.md, spec/CLAUDE.md, spec/NOTES.md, spec/BRIEF.md, spec/REWRITE.md, spec/Validation/features.json,
  spec/FusionComponents/module.md, spec/probes/facts.json and spec/Reference/move-in.md.
- Read outside the base, as a lead: ../core-leads/.landing/CoreLeads/design.md:54-70, :108-133.
- Commands beyond the brief's list: `wc -l` and `grep` over my scratch files and over `git show`'s output.
