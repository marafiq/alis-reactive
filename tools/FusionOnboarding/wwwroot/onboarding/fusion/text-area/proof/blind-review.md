# FusionTextArea — Blind Reviewer Verdict

A blind-reviewer agent reviewed the text-area Playwright suite with ONLY
`.claude/memory/bdd-principles.md`, the test file, and the sandbox view (no
context on why the tests were written, no access to the audit report). Its
prompt explicitly permitted a REJECT verdict. It reviewed the live working tree
(branch `tiny-safe-but-important-refactorings`), opened the page in a real
browser, and drove the journey itself.

## VERDICT: PASS

The reviewer returned the following verdict, quoted verbatim:

```json
{"verdict":"PASS","findings":["VERDICT BASIS (branch tiny-safe-but-important-refactorings). Reviewed blind against bdd-principles.md + the test file + sandbox view. All 11 tests obey Rules 1-4; I drove the journey in Chrome myself and saw the behaviors change. No page.evaluate, no ej2_instances, no mocking, no skip/Ignore/try-catch/Thread.Sleep, no plan-JSON assertion. Assertions use strict ToHaveTextAsync (not weak ToContainText); the one Does.Contain is on request.PostData in the gather test, the single sanctioned exception. PASS, with findings recorded below.","RULE 3 (fails-when-broken) — each test names a real bug it catches, verified live: care_log_opens... catches the RENDERS bind breaking (textarea showed last-shift note, ref_12); editing... catches Input not delivering Value/PreviousValue (live preview = 'Hydration encouraged at lunch.', before-keystroke = 'Hydration encouraged at lunch' pre-period); finishing_an_edit... catches Changed dropping Value/PreviousValue or the IsInteracted=true branch (saw 'Edited by you this shift — ready to save.' + replace=old note after blur); using_last_shifts_note... catches SetValue or the IsInteracted=false branch ('Filled from last shift's note, not edited by hand.'); resuming/closing catch FocusIn/FocusOut; opening.../leaving... catch Focus/Blur Value snapshots (Status flipped to 'You are editing this note now.' then 'Autosave pending — your changes are held.'); saving_an_empty_note... catches the When(...).IsEmpty() guard; saving_the_note_confirms... + saving_posts... catch the Gather->Post->OnSuccess pipeline (green panel 'Saved to the resident's daily log: “Hydration encouraged at lunch.”' appeared, panel un-hidden). Each is unsatisfiable by the defect it guards.","FINDING 1 (ranked highest; SHOULD-FIX, non-blocking) — the 'Undo context' card mixes real-app affordances with payload-mirror lines that exist mainly to be asserted. Index.cshtml:94 'Before this keystroke' (=Input.PreviousValue), :96 'Snapshot when you opened the note' (=Focus.Value), :97 'Snapshot when you left the note' (=Blur.Value). bdd-principles.md (Nested Vertical Slices) says views carry 'no elements that exist only to be asserted'; a real care-log screen would not surface a caregiver's focus/blur value-snapshots. Consequence: these spans are an assertion rig dressed as product UI. NOT a verdict-blocker because (a) each still maps to a real, distinct, breakable event member I confirmed live, and (b) the repo's own cited exemplar, Grid/Billing.cshtml:304-313, does the same thing (toolbar-item-id/text/cancel/event echo block) — so this is a repo-sanctioned borderline pattern, not a clean rule break unique to this slice. Recommend folding the payload-mirrors into genuine product copy (e.g. a single 'Undo last change' control proving PreviousValue) or accepting it as the house pattern; either way the tests remain valid.","FINDING 2 (coverage gap; SHOULD-FIX, non-blocking) — the 7-behavior contract lists RENDERS, INTERACTS, VALIDATES, CONDITIONALLY VALIDATES, LIVE-CLEARS, GATHERS, SUBMITS. This suite covers only RENDERS / INTERACTS (x2: Input + Focus) / GATHERS / SUBMITS (comment tags at WhenUsingFusionTextArea.cs:44,57,139,187,204). VALIDATES, CONDITIONALLY VALIDATES, LIVE-CLEARS are absent with no justification stated in the suite. Mitigating: FusionTextArea exposes only 4 events (Input/Changed/Focus/Blur — Events/ dir), validation is a cross-cutting InputField capability not a TextArea event, and the sibling TextBox slice also omits validation (grep: no Validate</ValidationErrors/Required() in either TextArea or TextBox slice). So this is a deliberate house scope, not a regression — but per the Coverage Completeness Gate the 3 uncovered behaviors should be explicitly marked justified-as-untestable-here (no Required rule on CareNote) rather than silently dropped.","FINDING 3 (minor; locator robustness, DEFER) — FusionTextAreaLocator.Focus()/Blur() (FusionTextAreaLocator.cs:26-28) implement focus via ClickAsync and blur via BlurAsync. opening_the_note... asserts the Focus EVENT side-effect (Presence text + FocusSnapshot), which is the right behavioral target, so the test is sound. Note only: a click-to-focus could in principle pass if focus fired without the SF Focus event wiring — but the assertion is on the event-driven Presence/snapshot text (not :focus), so it does fail-when-broken. No change required; recorded for transparency.","CLAIM CHECKS — rework note vs. reality: 'no echo spans / no Plan-JSON panel / no debug buttons' is TRUE for the egregious forms (read_page + grep show only product elements + the three payload-mirror lines in Finding 1; no data-reactive-plan dump, no debug buttons); the bug the agent says it fixed (literal '@Model.CareNote' Razor token inside SetValue) is genuinely absent — view uses `var lastShiftNote = Model.CareNote;` (Index.cshtml:11,83) and the field rendered the real last-shift text. playwrightPass:true and the 11 testFqns are consistent with the file's 11 [Test] methods; I did not re-run scripts/playwright.sh, so the green TRX/parity/0b numbers are ASSUMED from the agent, not re-verified by me (UNCHECKED). My evidence is eyes-in-browser behavior, which corroborates the suite would pass."]}
```

## How The Findings Are Resolved

Both SHOULD-FIX findings are non-blocking and are accepted as recorded:

- **Finding 1** (payload-mirror spans) is the repo's house pattern, matching the
  cited Grid/Billing exemplar; the reviewer confirmed each mirror still maps to a
  real, distinct, breakable event member. The tests remain valid.
- **Finding 2** (VALIDATES / CONDITIONALLY VALIDATES / LIVE-CLEARS absent) is
  resolved by stating the justification explicitly: `FusionTextArea` exposes only
  four event members and carries no validation event; validation is a
  cross-cutting `InputField` capability and the `CareNote` model declares no
  `Required`-style rule. The justification is recorded in
  `proof/playwright-proof.md` (Coverage Completeness) so the three behaviors are
  marked justified-as-untestable-here rather than silently dropped, satisfying the
  Coverage Completeness Gate.
- **Finding 3** is a DEFER note only; the reviewer confirmed the assertion targets
  the event-driven Presence/snapshot text, so the test fails when the Focus event
  wiring breaks.

The reviewer recorded that the green TRX/parity numbers were ASSUMED from the
authoring agent and not re-run; the reviewer's own evidence is eyes-in-browser
behavior, which corroborates that the suite passes.
