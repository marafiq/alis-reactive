# ContainerOnly: reply, rounds one and two, whole

Rule lines applied, at f2986ea66:
- CLAUDE.md:79 (spec lines, TLC, tests red, code, plants), :94 (one meaning, every carrier), :96 (a playwright.sh line).
- .claude/rules/tests.md:23 (a test that passed keeps its fixture and name), :41 (a plant on the outcome's path, read at
  the test's line), :49 (each side its own plant), :55 (a request's literal read from the code at HEAD).
- .claude/rules/views.md:12 and :29 (page-doors.json); docs.md:57; csharp.md:20; Alis.Reactive/CLAUDE.md:10 (PublicAPI).
- Alis.Reactive.Assets/CLAUDE.md:12 (the contract regenerated) and :16 (one file per wide query); the app's CLAUDE.md:23.
- tests/Alis.Reactive.ArchitectureTests/CLAUDE.md:18; .claude/rules/process.md:107, :139; spec/CLAUDE.md:613.
- .claude/agents/builder.md:42-43, :53, :58, :59-61; tests/Alis.Reactive.Conformance.App.Net48/CLAUDE.md:31, :34.

Done, as each command printed it:
- At the base: the brief's lines, the lead's runs; not rerun. git -C ../container-only rev-parse --short=9 HEAD: "f2986ea66".
- Probe, File over an item index: "[playwright:result] M6_F1.X9_27_0 1/1: 1 selected, 1 ran, 1 passed, 0 failed, 0 over
  budget, slowest 2.00s". Decided: File(m => m.Documents[0].File, ..) binds; IntakeDocuments sends each file so.
- Probe, NativeCheckBox unchecked: index 0 "Consent" matched, "Values differ at index [9] Missing: <
  "__RequestVerificationToken", ... >". Decided: the literal at WhenReceptionSignsInAVisitor.cs:103 gains "Consent".
- Probe, the reception under net10: "M9_F2.M9_X252_0 1/1: 1 selected, 1 ran, 0 passed, 1 failed"; M9.X250 and M9.X261
  "1 passed". Decided by the owner's hidden-field decision (read.md A): X252 and X254 pass in both full runs.
- CarePlanUpload on the container first: "[playwright:result] M21_F22.M21_X1014_0 1/1: 1 selected, 1 ran, 1 passed, 0
  failed, 0 over budget, slowest 2.70s".
- ALIS_TLC_WORKERS=2 check.py --check tlc: {"pass": true, "detail": "every spec green, every invariant killed by its
  mutant, every witness reached", "counts": {"specs": 17, "met": 17}. ContainerRead.cfg "Model checking completed. No
  error has been found." "492480 states generated, 33408 distinct states found, 0 states left on queue.";
  HiddenLeftUnsent, HiddenSentOutsideAFormBody "violated: AHiddenInputReachesAFormBodyAlone"; "violated:
  ReachAHiddenInputInAFormBody"; HandWrittenControlSent "violated: AnUnsentMemberReadsWhatTheEndpointBinds"; "violated:
  ReachAHandWrittenControlLeftUnsent"; Request.cfg "18308256 states generated, 18308256 distinct states found, 0
  states left on queue."; ContainerSubmitsNatively "violated: FailedCheckSendsNothing"; every other config as before.
- Red before the code, each "0 passed, 1 failed": X9_15 "Locator expected to have text 'Ward East, Code none, fields 1'
  But was: 'Ward East, Code A7, fields 1'"; M21_X1038 "Locator expected to have text 'Summary' But was: '<p>Ate a full
  lunch.</p>'"; M9_X254, :103 changed, "Expected: "Consent" But was: "CoordinatorId""; X9_47 "Locator expected to have
  text 'Desk Front; Name Ada; Shift Night; Ward North' But was: 'Desk Front; Name Ada'".
- After, each "1 selected, 1 ran, 1 passed, 0 failed, 0 over budget": M6_F1.X9_15_0 slowest 1.80s, M21_F38.M21_X1038_0
  2.51s (before its file took its base name back), M6_F1.X9_47_0 1.68s, M9_F2.M9_X254_0 0.92s, to its last line.
- Plants, each "[plant] caught: <test> failed with the defect in place (1 of 1).", restored, at the line it printed:
  X9.47: URL-encoded appendHidden dropped, :21 "But was: 'Desk Front; Name Ada'"; hidden inputs in the query, :24 "But
  was: 'Name Ada; Shift Night; Ward North'"; ":not(:disabled)" dropped, :21 "Bed 4; ..."; document for the container, :21.
  M9.X254: URL-encoded appendHidden dropped, :line 103 "Missing: < "__RequestVerificationToken", ... >"; setInput's
  form.delete dropped (A.2), :line 103 "Expected: "Copies" But was: "CoordinatorId"" (Consent twice); the Set of
  Caption dropped, :line 113 "But was: 'DOC1 · arrival.txt · 17 bytes · Consent · '". X9.27: the second File read
  from the first component, :30. X16.18: Code sent as "A12", :line 21 "But was: '[]'". http.beside.hidden: multipart
  appendHidden dropped, :21 "But was: ': a-1'". M21.X1038: the name attribute dropped, :19, and null content written,
  :25 "But was: "Summary=%3Cp%3ENo+summary.%3C%2Fp%3E"". X9.15: the fields' append dropped, :23; container.contains
  dropped, :31; the token appended, not set, "fields 2". Escaped, each replaced above: X254's Set of Kind dropped (the
  filing reads Consent either way), X16.18's Code sent as "" ("Unknown code." either way).
- "Passed!  - Failed:     0, Passed:  1467, Skipped:     0, Total:  1467, Duration: 19 s - Alis.Reactive.ModuleTests.dll".
- vitest: " Test Files  41 passed (41)", "      Tests  870 passed (870)".
- ArchitectureTests under a private index (its obj/private-index, git add -N): "Failed!  - Failed:     1, Passed:
  349, Skipped:     0, Total:   350": git_lists_no_file_a_build_of_the_fresh_net48_app_wrote, "MSBUILD : error MSB1001:
  Unknown switch. Switch: -tl:off", Visual Studio's MSBuild, CI's Windows job. In the gate: "Passed!  - Failed:     0,
  Passed:   338, Skipped:     0, Total:   338" (all but WhenAConsumerAppIsBuilt); AnalyzerTests 1066, TestRules 163.
- git grep -n -E 'BeginRequestForm|RequestForm|[.]Form[(]|[.]Form[)]' over the shipped projects, spec/api, tests,
  .claude, CLAUDE.md, README.md and spec/CLAUDE.md: 45 lines; 41 in Alis.Reactive read source.Form or sent.Form;
  spec/CLAUDE.md:10, the owner's decision; three endpoint constants named Form. No request door.
- scripts/check.sh --no-spec, page-doors.json staged by the lead: "[check] every check passed." (scratch/check-sh-3.log),
  before the door-map edit, the lead's REWRITE.md line and the last count; git grep -l over the gate's four test
  projects and check.sh finds no file that reads REWRITE.md, move-in.md or MISTAKES.md (one CLAUDE.md names them).
- "[playwright:result] all 1/1: 839 selected, 839 ran, 839 passed, 0 failed, 0 over budget, slowest 4.44s"
  (TestResults/20261125-160856-52228), after the RTE file's name went back and the bench cell's revert; the run before,
  after the rename and before the revert, 839 of 839 too. Since: spec text by spec.py, the skill's line, MISTAKES.md
  and the lead's builder.md line; no code, view, test or generated binding.
- python3 spec/tools/check.py, after the last spec.py edit: "spec check at f2986ea66: 40 of 40 checks pass".
- scripts/pack.sh: "Packed 6 packages to ./nupkgs (version 0.0.0-containeronly.20261125205807)."; the twin's Release
  build on them: "0 Warning(s)" "0 Error(s)".
- .landing/ContainerOnly: paths.txt (170 paths), message.txt, spec.py, retired.txt. The owner's words: spec/NOTES.md:1646
  and :1647; the decisions: spec/CLAUDE.md:9-17 and :472-475; MI.30 in spec/Reference/move-in.md; by spec.py.

Forms decided:
- sends() captures { members, hidden } (model.ts ContainerBody); captured() keeps its plain body, which Validate(container)
  reads (FormChecks.cs:48), so the check reads no member from a hidden input (D16.5).
- A hidden input goes before the request's own members; a name the request sends by an input, Set, a part or the token
  replaces it; a part whose condition failed was never gathered (D10.16) and replaces nothing (D9.15).
- page.ts reads input[type="hidden"][name]:not([name=""]):not(:disabled) inside the element found by id; the lint's
  allowance names it beside plan-scripts.ts, its refused case beside it, as plan-scripts.ts's is.
- http.beside.hidden is back to its base's hand-written hidden Batch: the one browser proof of a multipart hidden input.
- Request.tla: ContainerSubmitsNatively is the mutant branch of Check (spec/CLAUDE.md:613); spec/api's refused file is
  PageFormGone.CS1061; X9.27's file input is Care/Admission/IntakeDocumentFile.cs, linked into the twin;
  ReceptionDocuments sends Version and Kind by Set, since NativeHiddenField over DocumentsPage took the move-in card's ids.
- retired.txt: land.sh's name is the batch, so land-unit.sh gathers each folder's file into .work/logs/retired-<batch>.txt.
- Round one's carriers of what a container sends, read line by line against A.1: X9.15, spec/CLAUDE.md:473,
  REWRITE.md's cell sentence, MI.30's opening and move-in's door map (Form left the map: 141 names) by spec.py; the
  build-a-screen skill :250-252.

Gaps left, each with its use:
1. ReceptionDocuments' Set of Kind is proven by no test: the filing reads Consent without it. Use: another kind's document.
2. A nameless hidden input has a pure test only. Use: a hidden input a vendor writes with no name.
3. A Set member or a part replacing a hidden input: pure tests only; the browser proves an input and the token winning.
   Use: a view whose hidden input shares a name with a member its request sets.
4. Landing: paths.txt holds scripts/land-unit.sh, which land-unit.sh copies over itself while bash runs it; sent to the
   lead with the recommendation that the retirement door land as a unit of its own first.

Mistakes counted this round, mine: written +2 (a guarded part's meaning before sentParts was read; a run's order in the
reply); a rule +7 (mv, date and an ls of the landing folder, no brief commands; the private index first outside obj; a
reply not whole; land-unit.sh copying itself; round one's lines swept by phrase); a shell command +2 (unawaited calls).
The rule row moved 463 to 471: one count is not mine, in the file when I read 470.

Not run: MVC 5 and the net48 browser run (CI's Windows job); WhenAConsumerAppIsBuilt's fresh MVC 5 app; land-unit.sh to
land.sh to regressions.py with the retired line (WhenALandingRetiresATest runs it on written TRX files); delivery.sh's
consumerNet10 run. check-full.log, round one's, stands untracked at the worktree's root.
