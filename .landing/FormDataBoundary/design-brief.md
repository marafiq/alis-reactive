## Scope
The queue of HANDOFF.md, the framework package, item 2, verbatim: "The form-data boundary: an enum input in form data is
sent by its rows since b824783d3 (the design's defect 1, X9.36, X9.37, X9.39 and X16.47), an object stays refused until
after 1.0 (the owner's yes of 2026-11-17); a text or file body's check landed at 6da8232ec (X16.46). On MVC 5 an action's
model binds route values and the query beside the body (probe mvc5-value-provider-order), so a rule over a member a
request sends in its path or query is checked at its start in the browser and bound on that endpoint; the meaning by
host is designed whole and put to the owner before the core freeze (D16.5, D16.6)."

The decision: what a form's check reads, in the browser, for a member its request sends in its path or its query, and
for a member its body lacks that the endpoint binds from the path or the query, on each host and each body format, so
the browser's verdict equals the endpoint's (D16.5). Design it whole: the family of cases (net10 and net48; no body, a
URL-encoded body, multipart, JSON, text, one file; a member in the path, the query, the body, or two of them; and on
net10 each binding an endpoint may take, a plain model, [FromForm], [FromBody], and [FromQuery] or [FromRoute] on a
member), what C# knows where the view renders (the host, the body format, each member and where the request sends it)
and what it does not (the endpoint's binding), the alternatives with what each makes a developer write and what each
costs, the TLA+ spec that holds the read with the design's new case and its mutant run by TLC on a copy, and the
promises a yes adds, each with its opposite. The design ends with the one line for the owner of root CLAUDE.md,
"Spec-first", step 3: what already holds, the one thing a yes changes, its cost and risk, then the recommendation.

Base 31c256b0f. Worktree ../form-data-boundary, detached at the base. Built by a designer agent,
.claude/agents/designer.md, on the latest Fable at xhigh. It is read-only: it writes design.md and reply.md in this
landing folder, .landing/FormDataBoundary/, and its scratch files under .landing/FormDataBoundary/scratch/, and changes
no tracked file.
After: none

## Rules
.claude/agents/designer.md
CLAUDE.md
.claude/rules/docs.md
.claude/rules/process.md
spec/CLAUDE.md
spec/MISTAKES.md
spec/Core/DESIGN.md
spec/Core/schema.md
spec/Validation/module.md
spec/Validation/Validation.tla
spec/InputsAndIds/ContainerRead.tla
spec/probes/facts.json
spec/REWRITE.md

## Outcome
design.md in this landing folder, in the shape .claude/rules/docs.md, "A design put to the owner", states: each case of
the family with the D line that gives it its meaning today, quoted, or the absence of one, with the search that found
it; for each case where the browser's verdict and the endpoint's differ, the probe that pins the endpoint's binding and
whether a page of tests/Alis.Reactive.Conformance.App reaches it, from a search that names every door a request can be
written with, the files it read and those it did not; the alternatives; the TLC runs; the promises a yes adds; the one
line for the owner. Every claim names its file and line at 31c256b0f; every count is the line the command printed.

## Done
At the base: python3 spec/tools/check.py at 45b14dc30, whose tree 31c256b0f holds but for spec/STATE.json and
HANDOFF.md, printed "spec check at 45b14dc30: 40 of 40 checks pass" (.work/logs/spec-check-45b14dc30.log in the lane);
its tlc check reads ContainerRead.tla's and Validation.tla's recorded results green.
Each TLC run of a copy of a spec under scratch/ with the design's new case: the line TLC prints for the clean model,
and for its mutant the invariant it violates.
design.md written; reply.md written, opening with the rule lines applied.

## Commands
git -C ../form-data-boundary rev-parse --short=9 HEAD
git -C ../form-data-boundary show 31c256b0f:<path>
git -C ../form-data-boundary grep <pattern> 31c256b0f
/opt/homebrew/opt/openjdk/bin/java -cp ~/.local/share/tlaplus/tla2tools.jar tlc2.TLC, inside (cd .landing/FormDataBoundary/scratch && ...)

## Reply
reply.md at most 80 lines, opening with the rule lines applied, by file and line at 31c256b0f, then each line of
"Done" as its command printed it, then what did not run. design.md at most 220 lines.
