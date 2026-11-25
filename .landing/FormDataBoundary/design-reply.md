Rule lines applied, each at 31c256b0f:
- .claude/agents/designer.md:20 (every file read at the base), :42-44 (limits), :52-53 (a spec stands only by a TLC
  run with its mutant; an absence names its search), :59 (a plant not run is named unrun).
- CLAUDE.md:32 (git grep takes no \b), :47 (TLA+ governs), :90-93 (one line for the owner, from a family of cases,
  alternatives and counts read at the source).
- .claude/rules/docs.md:72-78 (a design names each defect's spec line and the app's reach, quotes each D line, names
  every door and the files read and not read); .claude/rules/process.md:108, :111; .claude/rules/tests.md:38, :44;
  .claude/rules/views.md, whole.
- spec/CLAUDE.md:17, :22, :68, :414, :2036; spec/MISTAKES.md:16, :18, :19; spec/REWRITE.md:433-437.
- spec/Core/DESIGN.md:1785-1788, :1931-1932, :1956-1970, :3941-3951, :4386-4388.

Done, each line as its command printed it:
- At the base: not run by me, as the brief gives it: "spec check at 45b14dc30: 40 of 40 checks pass". My own base
  run, the unchanged spec/Validation/Validation.tla under its Validation.cfg, TLC 2.19: "Model checking completed.
  No error has been found.", "1327212 distinct states found"; the jar's SHA-256 opens 936a2620, as 6da8232ec names it.
- git -C ../form-data-boundary rev-parse --short=9 HEAD: "31c256b0f".
- scratch/design/Validation.tla, the recommended read, 37 runs, each run's output kept beside its config as ".out".
  Validation.cfg: "Model checking completed. No error has been found.", "5308524 distinct states found". Each
  mutant, under its own config and then under Agreement alone: OutsideReadAtItsStart, QueryBesideUnreadOnNet48,
  QueryBesideReadOnNet10, OutsideReadOnNet48Alone, OutsideEmptyStaysText, ConfinedEndpointLeavesWhatWasSent and
  ModelTakesTheQueryBesideItsBody, each "Invariant SentOutsideAgrees is violated by the initial state" and
  "Invariant Agreement is violated by the initial state".
  Five witnesses, each "Invariant Reach... is violated". Validation.holds.QueryCarriedInTheBody.cfg: "No error has
  been found.", "5750892 distinct states found"; its witness ReachEveryEndpointOnACarriedMember violated under it
  and holding under no mutant. The base's 14 other violates configs: each its own invariant violated.
- scratch/alternatives-by-body, 48 runs of Agreement, six reads by four endpoints by two kinds of body; the cells
  that differ: today 6 of 8, MVC 5 alone 4, the recommended read 3, everywhere 2, with the query carried 1.
  scratch/alternatives, 32 runs of the first matrix with no split by body, agrees with it.
- design.md written, 220 lines ("wc -l" printed 220); reply.md written.

The answer, in design.md:
- Today a GET's check reads its path and query (RequestPartsWriter.cs:304); a POST, PUT or DELETE with no body, a
  text or a file reads every member at its start (forms.ts:203-207); a `Query` member beside a body that binds
  members is not read (RequestPartsWriter.cs:234). No D line states the GET's read or the query member's.
- No page shows the two verdicts apart. Six requests, all of BedNotes.cshtml, send a path member with a body that
  binds none under a check, to six `[FromForm]` actions that answer the model's start; five send a query member
  beside a body under a check, none with a rule the browser decides on the query's value; no net48 page sends a
  member outside its body under a check.
- TLC shows no read agrees with every endpoint. Recommended, C: a member sent outside the body is read where MVC's
  binder reads it, on MVC 5 always and on net10 where the body binds no member. It reverses X16.46's three promises
  and six actions, and it needs seven probes first. B, MVC 5 alone, reverses nothing. Carrying a query member in the
  body is a second change, with its own row and cost.

Objections, the two strongest:
1. C reverses a unit landed on 2026-11-24 under the owner's yes, and its gain on net10 rests on a plain model
   binding a route or a query value with no body, which no probe pins. Answer: the landed read checks such a request
   on nothing it sends, its page's actions answer "Admissions" to a note sent to North (features.json:2351), and a
   GET is already read as sent; the probes come first and each is a stop, after which B is what stands.
2. C forks by host beside a body that binds members and still differs in three cells, where reading everywhere
   differs in two under one rule. Answer: those two cells are the app's 159 `[FromForm]` and `[FromBody]` parameters
   (parse), today's read agrees with them, and TLC shows only the carry serves both bindings there.

What did not run:
- python3 spec/tools/check.py, any build, any test, any probe: the seven probes of design.md, section 7, are unwritten.
- spec/InputsAndIds/ContainerRead.tla: no copy was run. It holds no path and no query, so I make no claim by a run
  that it stands.
- The plant of every proposed promise. Alternatives F and G are not modelled.
- Not read: tests/Alis.Reactive.Poc.App, the module tests, spec/api and every .ts file of the app, for requests.
  Grepped and not read whole: .claude/rules/csharp.md, Alis.Reactive/CLAUDE.md, Alis.Reactive.Assets/CLAUDE.md, the
  Playwright and World projects' CLAUDE.md. Of spec/CLAUDE.md's 155 owner entries I read whole those from 2026-11-15
  on, four of 2026-11-14 and twenty older ones that name validation, a body, a binder, an endpoint, a query or a
  host; the rest by their first line, and 1907 and 1922 in part.
- The request counts are a script's parse of 430 files, and the action tally a parse of 505; each is marked.

Commands outside the brief's list, each read-only on tracked files: git ls-tree, to list files; git show -s
6da8232ec, to read why X16.46 landed; git status --porcelain, which printed no tracked change; shasum of the TLC jar;
ls, wc, du, grep and mkdir under the landing folder; rm -rf of scratch/design, twice, to rebuild a copy whose base
read I had modelled otherwise, so those runs are not reported, and once of TLC's state folders, 4.0G, after each
run's output was kept.

My mistakes, for spec/MISTAKES.md: two git greps written with \b, which CLAUDE.md:32 names, one printing nothing
and redone; a count of promises written as five where four are new, a line count of the spec copy kept from an
earlier copy, and two opposite defects their own opposite passed, each read and corrected before design.md closed.
