# ValidationForms: every validation use case, its complete form

## Scope
The owner, 2026-11-25, verbatim: "write these use cases a md document, take this to fable xhigh designer, ask it to
deliver on correctness, grammer consistency, quality, typed and your document must contain small but complete use
examples. so design from fable is expected to be complete, then it gets tla+ and then it lands without loosing
quality". On bulk edit: "Bulk Edit is not about checking one row alone, it has two use cases, if action is in row, then
row validate, if its applying to all rows then all validate." And: "Its very critical to get this right with high
quality code and keep grammer and dev experience." Earlier the same day, on lead 4 (N16.3, a named rule set for a check
with no request): "1 seems more aligned, but make sure that all use cases are satified, including where bulk edit say
row gets validated. I would like to see each use case form here before you action it."

The task: use-cases.md in this folder lists every validation use case: F1 to F8 for one form, P1 and P2 for partials,
B1 to B5 with B1s, B2s and B2c for bulk edit, G1 and G2 for a grid's row edit. Each is marked built or to design. For
each case, design.md gives the complete form a developer writes, as a small complete example: the model and validator
lines it needs, the view block, the endpoint where the case turns on it, what the request sends, what the user sees
and where each message shows (D16.13).
- A built case: the form confirmed at the source at the base, and the test that proves it, cited by class and case.
  Every test use-cases.md cites is the lead's, unopened: open each and state whether its gesture and outcome prove
  the case. For P1, P2, B4, F2 and F3 it names none: find one, or name the case unproved, with the search that found
  nothing.
- A to-design case: decided by four tests, in this order.
  1. Correctness. The browser's verdict equals the endpoint's on the model the endpoint binds, on net10 and net48
     (D16.5, under the owner's choice C). D16.5 at the base still holds the old text; C's D16.5 is section 6 of
     form-data-boundary-design.md in this folder, and its seven probes (its section 10) have not run, so a form that
     rests on one names it unproven.
  2. Grammar consistency. One door for one meaning; the landed doors and grammar nodes first; the same shape the
     built cases use (`Validate<TValidator>(ruleSet)`, `NoValidation()`, `Validate(container)`, `Container<T>(id)`,
     `BeginRequestContainer`, `PartialFor`, `plan.Rows<T>`).
  3. Typed. ALIS009: no string that names a member.
  4. Developer experience. The least a developer types for the common case, with D16.2's condensed default.
- Bulk edit's two cases are the owner's: an action in a row validates that row, and an action on all rows validates
  every row, each under the default rule set and a named one, to save and to check without saving.
- G1 and G2 get their form, or a verdict that they are outside 1.0, read against spec/FusionComponents/module.md, its
  features.json and the probe spec/probes/ej2-grid-batch-rows.mjs.

For every new piece, design.md names:
- the door, as its PublicAPI.Unshipped.txt line;
- the grammar node it writes, an existing one by its kind in spec/Core/schema.md; a new production is one line for
  the owner;
- what the browser runs, by file;
- each D or N line added or changed, with its full text;
- each promise in the form of features.json: page, gesture, outcome, opposite, defect.

Each binding fact a form rests on (a list whose first index is not 0, an item's prefix, a member absent from a part's
body) is cited as a fact of spec/probes/facts.json by its id, or for net48 as a line of the MVC 5 source under
spec/probes/aspnetwebstack. Where neither states it, name the probe the build runs first: its statement, its host and
the line it must print. Each ordering a form meets (a check before its request sends, a live check, a row's request in
flight while another row acts) names its TLA+ spec under spec/, the action and the invariant, and the mutant that must
fail. spec/InputsAndIds/ContainerRead.tla and spec/Validation/Validation.tla are the first to read. The TLA+ change
itself is the next step after the owner approves this design. A claim that a spec stands unchanged holds only by a TLC
run on a copy, as .claude/agents/designer.md states.

Base 830817f4b. Worktree ../validation-forms, detached at the base, node_modules installed at the root and in
Alis.Reactive.Assets. Built by a designer agent, .claude/agents/designer.md, on the latest Fable at xhigh. It is
read-only: it writes design.md and reply.md in this landing folder, .landing/ValidationForms/, and any scratch file
under .landing/ValidationForms/scratch/, and changes no tracked file.
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
spec/Validation/module.md
spec/Validation/features.json
spec/Validation/Validation.tla
spec/InputsAndIds/ContainerRead.tla
spec/FusionComponents/module.md
spec/probes/facts.json
spec/Reference/move-in.md

## Outcome
design.md in this landing folder, in the shape .claude/rules/docs.md, "A design put to the owner", states:
- one section per case of use-cases.md, in its order, each with its complete small example and the parts the Scope
  names;
- a table of every case: built or new, its door, its node, its proof;
- the new doors as PublicAPI lines, and the D and N lines with their text;
- the binding facts and probes, and the orderings with their TLA+ specs and invariants;
- the two strongest objections, each with its answer;
- for each decision on a principle, a closed rule or the scope, one line for the owner, in root CLAUDE.md's form,
  "Spec-first", step 3: what already holds, the one thing a yes changes, its cost and risk, then the recommendation.
Every claim names its file and line at 830817f4b, and every count is the line the command printed. A shape
use-cases.md marks "candidate" is weighed, never assumed.

## Done
At the base: python3 spec/tools/check.py at cad8e3581, whose tree 830817f4b holds but for spec/STATE.json and
HANDOFF.md, printed "spec check at cad8e3581: 40 of 40 checks pass" (.work/logs/spec-check-cad8e3581.log in
the lane, the full check before the push of 830817f4b).
At the base, the TLC command of Commands on a copy of spec/InputsAndIds/ContainerRead.tla and its cfg, under
scratch/probe-tlc: "Model checking completed. No error has been found." and "114912 states generated, 8352 distinct
states found, 0 states left on queue."
design.md written; reply.md written, opening with the rule lines applied.

## Commands
git -C ../validation-forms rev-parse --short=9 HEAD
git show 830817f4b:<path>; git grep <pattern> 830817f4b -- <paths>; git log; git ls-tree; git status
ls and mkdir under .landing/ValidationForms/
TLC on a copy of a spec, under .landing/ValidationForms/scratch/ only, the java check.py runs when JAVA_HOME is unset:
( cd .landing/ValidationForms/scratch/<copy> && /opt/homebrew/opt/openjdk/bin/java -cp "$HOME/.local/share/tlaplus/tla2tools.jar" tlc2.TLC -deadlock -workers 2 -metadir states -config <name>.cfg <name>.tla < /dev/null > tlc.log 2>&1 )

## Reply
reply.md, at most 60 lines: the rule lines applied, by file and line at 830817f4b; the line of Done as printed; each
TLC run with its last line; what did not run. design.md is the answer, as long as completeness takes and no longer.
