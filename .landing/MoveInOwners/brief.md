# Move-in: the waitlist, the assessment objects and their owners, and the room handoff callback

Read `AGENTS.md` (a link to root `CLAUDE.md`) and `.claude/agents/builder.md` first, then each file under "Rules".

## Scope
The owner, 2026-11-18, of the app this unit builds a part of: "Do this app as a vertical slice. It must have its
domain model with the fake in-memory data. It must have real endpoints with typed payload ... use all the features in
a complex way where you use different combinations, not just a simple super happy path." And of what he reads on his
return: "I need that complex app behavior asserted and proved."
Base: d13a4ce7f, the commit this worktree, .worktrees/movein-owners, is detached at; print it with
`git rev-parse --short=9 HEAD` and stop if it differs. Built by an agent, in this worktree alone.

The app is resident move-in at a senior living community, inside the conformance app. Two files are its authority,
and you read both whole before you write:
- `spec/Reference/move-in.md`, THE APP'S ONLY SPEC: part 1 the domain and its seed, 2 the endpoints, 3 the screens
  and partials, 4 the features MI.1 to MI.31 with their interactions, 5 the validators, 6 five situations a feature
  (a success with what the model holds after and what the next read shows, a refusal by the browser, a refusal by
  the server alone, a transport failure, two features meeting), 7 coverage, 8 gaps, and 9 WHERE THE BUILD CORRECTED
  IT: each landed unit's decisions, the places the source won, its gaps and what it left waiting. Part 9 is as
  binding as the parts before it: a landed unit's decision stands, and what it left waiting on your features is
  yours to build now.
- "foundation-reply.md" beside this brief, the reply of the unit that landed the foundation and MI.1. Its part "What
  a second and a third builder must know" is a rule of this unit: the files by area, the hooks, the sections of the
  main view, the addresses, the enum's named start, the private index for the gate.

This unit builds MI.18 (bed conflict and waitlist), MI.28 (assessment objects and their owners) and MI.29 (room handoff callback): each by its interactions in part 4, on the endpoints part 2 gives it, the screens part 3
gives it and the rules part 5 gives it. Its tests are part 6's five situations for each of its features, fifteen
tests where none waits, each with the literals the spec gives: each observes the page, what the model holds after,
and what the next read of the model shows (the page opened again renders the saved draft). A situation that needs
a feature no landed unit has built is named as waiting in reply.md, its id left unused, and nothing stands in. Where the spec is wrong against
`Alis.Reactive/PublicAPI/net10.0/PublicAPI.Unshipped.txt`, `Alis.Reactive.Native/PublicAPI/net10.0/PublicAPI.Unshipped.txt`
or a TLA+ spec, the source wins: say what you changed and why.

Yours alone: the areas Waitlist, Allocation owner, Payer owner, Capacity gauge and Room handoff, each in files of its own as the foundation lays them (a partial file an area of
`MoveInDesk`, `MoveInDeskController`, `MoveInEndpoints`, `MoveInReads`, `MoveInRules`, `MoveInValidators`, of the world
contract's `CareEndpoints`, `MoveInChange` and `MoveInChanges`; a view a screen), and in
`tests/Alis.Reactive.Conformance.App/Care/MoveIn/MoveIn.cshtml` the lines inside these elements and no other line:
"move-in-waitlist" and "move-in-allocation-owner", and an element beside the container that part 3 gives a screen of your features alone, which you name in reply.md. You edit no other landed file, with two exceptions: a file of `Care/MoveIn/Model/` gains a member
part 1 or part 2 names and it lacks, added at the end of its class; and the conformance component's start classes
gain a member a page of yours must start the component with, as `tests/Alis.Reactive.Conformance.App/CLAUDE.md`
states. Each such edit is named in reply.md. The conformance component's declaration gains no member.

Beside you, in a worktree of its own, another builder builds MI.5, MI.6 and MI.7, the areas Contacts and Contact authority and the element "move-in-contacts". You touch none of its areas. An interaction of
yours that reads a section that unit builds is built and proven only where the page holds that section at your base;
otherwise it is named in reply.md as waiting on that unit, with its situation, and nothing is invented to stand in
for it.

Each situation is one defect and one promise of the feature M9.F2 of `spec/PartialsAndSlots/features.json`, where the
foundation's five stand, placed on the screen "moveIn" of `spec/Reference/app.json`; its id is of your block alone,
M9.X65 to M9.X89, and its test is in `tests/Alis.Reactive.Conformance.PlaywrightTests/PartialsAndSlots/MoveIn/`, a file a feature.
A promise on an ordering names the TLA+ action it holds, and its test holds the machine at that action through the
harness, never by timing. A planted defect is the defect its promise names: one that escapes means the defect is
not the promise's or the test cannot see it, and you change the one or the other until it is caught.

Two gaps of the framework stand, named by the foundation, and no framework code is written for either (the owner:
"The framework is not getting new code except the current module in progress"):
- A date a person enters. Native holds no date input, and a Native text box over a date renders the server's
  culture text, which the browser's code does not read as a date. Read
  `tests/Alis.Reactive.Conformance.App/Components/ConformanceComponent/ConformanceComponentHtml.cs` and the
  component's code: where the conformance component bound to a date member lets a person set its day, a date field
  of yours is that component; where it does not, write the feature's other interactions, leave the date's rule to
  the server's check, and name the gap in reply.md with the interaction's number.
- An enum an input holds needs a named start (the foundation's decision 9).
A door an interaction needs and the framework lacks is a gap you name in reply.md, with its feature and interaction
number and its use; the rest of the feature is written without it.

You leave alone: every shipped project and the runtime; every landed page and test outside `Care/MoveIn/`;
`spec/tools/check.py`; `AppHost.cs`.

## Rules
- `.claude/rules/views.md`, `.claude/rules/tests.md`, `.claude/rules/csharp.md`.
- `tests/Alis.Reactive.Conformance.App/CLAUDE.md`, `tests/Alis.Reactive.Conformance.PlaywrightTests/CLAUDE.md`,
  `tests/Alis.Reactive.Conformance.World/CLAUDE.md`.
- `.claude/skills/build-a-screen/SKILL.md`, the map from a view author's question to its door, with landed views;
  `.claude/skills/prove-behavior/SKILL.md`.
- The exemplars, whole: `tests/Alis.Reactive.Conformance.App/Care/MoveIn/MoveIn.cshtml`,
  `tests/Alis.Reactive.Conformance.App/Care/MoveIn/Location.cshtml`,
  `tests/Alis.Reactive.Conformance.App/Care/MoveIn/MoveInDesk.Location.cs`,
  `tests/Alis.Reactive.Conformance.App/Care/MoveIn/MoveInDeskController.Location.cs`,
  `tests/Alis.Reactive.Conformance.App/Care/MoveIn/MoveInRules.Location.cs`,
  `tests/Alis.Reactive.Conformance.PlaywrightTests/PartialsAndSlots/MoveIn/WhenACoordinatorChoosesWhereAResidentWillLive.cs`,
  and `tests/Alis.Reactive.Conformance.App/Care/Admission/ArrivalDesk.cshtml` for a chained and a parallel request
  and a condition on a reply.
- `spec/Execution/Lanes.tla`, `spec/Execution/Run.tla`, `spec/InputsAndIds/ContainerRead.tla`,
  `spec/PartialsAndSlots/Partials.tla` and `spec/OwnershipAndState/Ownership.tla`, and each other TLA+ spec an
  interaction of yours names, found under `spec/` by its name.
- `spec/Reference/app.json` (the screen "moveIn" and its parts) and `spec/MISTAKES.md` for the kinds.

## Outcome
MI.18 (bed conflict and waitlist), MI.28 (assessment objects and their owners) and MI.29 (room handoff callback) work on the move-in page in a real browser beside the foundation's, and each of their situations is a test
that passes, fails on its planted defect, and reads what the model holds after and at the next read.

## Done
Each line quoted in "reply.md" as its command printed it:
- For each situation: its test written before its page does it and run red, "0 passed, 1 failed"; then green,
  "1 passed, 0 failed, 0 over budget"; then "[plant] caught:" on a defect planted in the framework's runtime or
  builders, never in the page; and, quoted, the assertion that reads what the model holds after and the one that
  reads the next read.
- `scripts/playwright.sh --all`, once: every test passes, its count 651 and your tests.
- `python3 spec/tools/check.py --generate`, then `--check promises`, `--check app`, `--check bindings`,
  `--check hedges`, `--check defects` and `--check carriers`: each "pass": true.
- `scripts/check.sh --no-spec`, once, last, under the private index "foundation-reply.md" describes in its first
  line: "[check] every check passed."
At the base: the landing of the commit under this base printed "651 selected, 651 ran, 651 passed, 0 failed, 0 over budget" and
"[check] every check passed."; in this worktree `npm ci` has run. Your first run, before any edit, is
`scripts/playwright.sh --test M9_F2.M9_X10_0`, which prints "1 selected, 1 ran, 1 passed, 0 failed, 0 over budget";
stop and say so if it prints anything else. The harness's waits were measured on 2026-11-18 with the move-in page
in place (a navigation 704 ms, a step 740 ms, a test 7.44 s): a test of yours that fails alone on a timeout at its
first navigation, or over its budget, is no defect of your page to bend around. Stop and say so in a message to the
lead: the remedy is a measurement the lead runs.

## Commands
`. scripts/toolchain-env.sh` first, in every shell. ONE BUILD AT A TIME on this Mac is the owner's rule, and three
workers share it: every command that builds or runs a browser, `scripts/playwright.sh`, `scripts/plant-defect.sh`,
`scripts/check.sh`, `scripts/run.sh` and `dotnet build`, is started through the lock and through nothing else, as
`/usr/bin/lockf -k "$HOME/.local/share/alis-toolchain/build.lock" <the command>`: it waits for its turn and then runs.
Start it in the background where it could outlast a foreground call, and act on its notification; never a sleep and
never a poll. `scripts/run.sh` holds the lock while its server stands, so a look by hand is short and its server is
stopped after it; `curl` against that server on localhost, and a node script of yours that opens the page in the
test project's own headless Chromium, are the look's clients. The session's scratchpad folder is shared by every
agent: your scratch files sit in a folder of your own inside it, named for your unit. Then only: `git` inside this worktree (rev-parse, status, diff, show, add -N, grep with options
before the pattern and no \b, \s, \d or \w, and the private index's one `git add`);
`scripts/playwright.sh --test <promise constant>`, `scripts/playwright.sh <a fixture's name>` and
`scripts/playwright.sh --all`, never by a filter;
`scripts/plant-defect.sh --test <constant> --in <file> --replace <text> --with <text>`; `python3 spec/tools/check.py`
with the options above; `python3` on your own "spec.py" and on a script of yours that edits your files. No TLC, no
commit, no push, no `git stash`, no agent of your own. An edit of more than one line is a Python script that asserts
each match's count before it writes. A command held in a variable, and a loop over a list held in one, is run by a
bash script; in zsh no variable is named path.

## Reply
Write beside this brief: "paths.txt" (every path outside `spec/` you changed or added, and the line
"spec/MISTAKES.md"; never `Promises.g.cs` or `Gates.g.cs`), "spec.py" (the defects, promises, placements, parts and
page-doors entries, each edit asserting its match's count, taking the repo's root as its argument, and at its end
your unit's part appended to part 9 of `spec/Reference/move-in.md`, as the unit before you wrote its own: where the
source won, what you decided, each gap, what waits), "message.txt" (what changed, each situation with its lines, what did not run) and "reply.md", with
no limit of lines. reply.md opens with any fault you find in this brief; then the rule lines you applied, by file and
line; for each feature its interactions with the view file each stands in, then its five situations, each with its
test's name, its red, green and planted lines and its two assertions; each place the source won over the spec;
each gap; what waits on the other unit; what is verified by a run, what is assumed, what is not checked. Reply when
your features stand. If one is blocked by a gap or by the other unit's section, reply with those that stand and name
the blocked one: a part that stands lands at once.
