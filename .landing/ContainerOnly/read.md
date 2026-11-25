# ContainerOnly: read

Read by the lead, 2026-11-25, against f2986ea66, in this order.

1. The reply's rule lines at their source: CLAUDE.md:79 and :94, tests.md:23, :38, :44, views.md:12 and :29, docs.md:57,
   csharp.md:20, Assets CLAUDE.md:12, the app's CLAUDE.md:23, spec/CLAUDE.md:613; each states what the reply quotes.
2. The change against its base: PublicAPI text (BeginRequestForm, RequestForm, the Form doors and the form overloads of
   UrlEncoded and Multipart leave, net10 and net48 alike), Container.kind and ContainerKind leave the grammar, model.ts
   loses Submission and submissionOf, page.ts its form reader, request.ts appendEntry; Reception.cshtml,
   ReceptionDocuments.cshtml and IntakeAdmit.cshtml read at the source. The change holds the approved design.
3. Done against the reply's lines: TLC, vitest 867, ModuleTests 1467, the spec check 40 of 40, the full run "838
   selected, 838 ran, 836 passed, 2 failed" (M9.X252, M9.X254) read in scratch/final-pw-all.log.

What stops the land:

A. The owner's decision, 2026-11-25, verbatim: "yeah container should take care of hidden fields in TS runtime since we
   remove form semantics in chstml and for good reasons, then TS runtime still will use form but in TS" and "to build
   the payload not as replacement of container". It closes gap 1. The lead's design inside it, which replaces the
   lead's earlier message where they differ:
   1. A hidden input (input type "hidden") inside the container's element, with a name and not disabled, that no plan
      binds, is sent under its own name and value in a URL-encoded or a multipart body. A JSON body and a GET's query
      carry none: a JSON member is named by the endpoint's contract and a hidden text has no declared type, and an
      antiforgery token never rides in a URL.
   2. They are read when the container is captured, as SubmissionSetAtTheRead read the form at the base, and appended
      before appendFields and appendInputs, so setInput's delete by name lets a plan input of the same name win
      (CheckBoxFor's hidden "false", a Fusion component's hidden input) and the framework's token, set by name, stays
      one field.
   3. The read is a query inside an element found by its id. It lives in one named file, the seam the base had
      (page.ts's second function into PageInputs, restored and narrowed to the hidden name and value pairs); the lint's
      allowance in eslint.config.js names that file beside plan-scripts.ts, with a refused and an accepted case in
      tests/Alis.Reactive.ArchitectureTests/LintCases and WhenTheLintIsConfigured, and the Assets CLAUDE.md sentence
      "One file finds a root's plan scripts ..." names it. No tree walk around the selector.
   4. D16.5: the check reads no member from such an input; a member a hidden input alone sends reads as one the body
      lacks. It is FormDataBoundary's family, a value sent outside the plan's inputs that the browser check does not
      read; the sentence says so and names NativeHiddenField as the door for a member a rule reads. D9.15 and D14.8
      state point 1, schema.md's capture words if they change, NOTES.md the owner's words, spec/CLAUDE.md the decision
      line, all by spec.py.
   5. ContainerRead.tla: the hidden control's action, an invariant that a hidden input in the container reaches the
      endpoint in a form body and none in a JSON body or a query, a mutant that leaves it unsent and fails it;
      HandWrittenControlSent stays for every other control. TLC lines in the reply.
   6. Tests: M9.X252 and M9.X254 green, X254 to its last line; a promise for the hidden input, both sides, each failing
      on its own plant: sent inside the container's URL-encoded body; not sent in the same container's GET query.
      X9.15's hand-written "Code" is a text input and stays out, read at IntakeAdmit.cshtml.
B. Gone tests (gap 2). WhenARichTextEditorIsSentInAForm keeps its fixture and its name: "the body holds the content once
   under the member's binding name" still holds of the browser's post; rename the new file back. WhenANativeFormIsSent's
   promise is the owner's reversal, so it is retired, never renamed: scripts/regressions.py reads
   ".landing/<Name>/retired.txt", a line each "<class.method> | <promise constant> | <class.method that now carries it>",
   prints "retired: <class.method>, its promise carried by <class.method>" for a listed test that is gone and whose
   carrier passed in the run after and was absent before, and fails a listed test that is not gone or whose carrier did
   not pass; scripts/land.sh hands it the file; process.md's table of a unit's files names it; tests.md:23 names it as
   the one door. Its refused and accepted case run as WhenARuleNamesTheCode's siblings do, or by a run of the script on
   two trx files kept under tests/Alis.Reactive.ArchitectureTests.
C. The gate. The lead staged plan.ts and plan.schema.json in this worktree, so scripts/check.sh --no-spec runs past
   step 1: run it to "[check] every check passed.", which writes spec/Reference/page-doors.json; that file goes in
   paths.txt.
D. The net48 twin and WhenAConsumerAppIsBuilt need the packs: build them by the script the repo names for them, then the
   twin's Release build and ArchitectureTests, each line in the reply.
E. Plants not run: M9.X254 past :103, X9.27, X16.18 and the bench cell http.beside.hidden, each read at its test's line.
F. Then the full run again, and the reply rewritten whole with every line of the brief's Done.

Mistakes: the reply counts 12 (written +1, rule or list +5, shell +6), held against the brief; the lead's: the message
of 14:43 put a hidden input in a GET's query without the cost read (a design written ahead of its family), counted
written +1 by the lead in this worktree's
spec/MISTAKES.md.

Round one sent it back with A to F above.


## Round two, read by the lead, 2026-11-25

1. The reply's rule lines at their source: tests.md:23-26 (the retired door now stated), :41, :49, :55; views.md:29;
   Assets CLAUDE.md:16-18 (page.ts named beside plan-scripts.ts); process.md:107 (retired.txt in a unit's files).
2. The change against its base: page.ts reads input[type="hidden"][name]:not([name=""]):not(:disabled) inside the
   element found by id; model.ts's sends() carries { members, hidden } and captured() keeps the plain body the check
   reads; request.ts's appendHidden runs in the URL-encoded and multipart bodies only, before the request's own names,
   skipping a name the request sends; eslint.config.js's allowance names page.ts; land-unit.sh gathers retired.txt,
   land.sh hands it to regressions.py, which retires a gone test only where its carrier passes now and was absent
   before. retired.txt's key matches the last lane run's trx at 20261125-020357-25220 (className with
   ("antiforgeryField"), one variant). spec.py's D9.15, D14.8, D16.5, schema and decision lines read; the lead
   changed two attributions in spec.py: the JSON and query exclusion is the lead's reading within the owner's
   decision, not the owner's words.
3. Done against the reply: TLC 17 of 17 with HiddenLeftUnsent and HiddenSentOutsideAFormBody failing
   AHiddenInputReachesAFormBodyAlone; X9.47 red then green; plants caught for X9.47 (four), X254, X9.27, X16.18,
   http.beside.hidden, X1038, X9.15; the full run "839 selected, 839 ran, 839 passed, 0 failed, 0 over budget" in
   scratch/pw-all-3.log; vitest 870; ModuleTests 1467; the twin's Release build on the packs, 0 errors.
4. The lead ran scripts/check.sh --no-spec in this worktree after staging page-doors.json, plan.ts and
   plan.schema.json: "[check] every check passed." (scratch/lead-check.log).
5. Mistakes: the reply counts six this round (written +1, rule +3, shell +2), held against read.md; none of the lead's.
   Gaps 1 and 2 of the reply are untested members the reply names with their use; neither is a promise of this unit.
   Not run here: MVC 5 and the net48 browser run (CI's Windows job), WhenAConsumerAppIsBuilt, regressions.py on real
   runs with the retired line, which the land's own run exercises.

verdict: land
