# FusionAccordion — Blind Reviewer Verdict (exit e)

A blind-reviewer agent reviewed the accordion Playwright suite with ONLY
`.claude/memory/bdd-principles.md` and the test file (no context on why the tests
were written, no access to this report). Its prompt explicitly permitted a REJECT
verdict. It confirmed the live working tree (branch `tiny-safe-but-important-refactorings`,
HEAD `9ba774ee`) before reading code, opened the page in a real browser, and re-ran
the suite at the source rather than trusting any rework report.

## VERDICT: PASS

> All 7 tests are behavior-named, independently understandable, use real interactions
> only, and each has a nameable mutation it catches (Rule 3). No REJECT-level violations.

## Reviewer verdict (quoted verbatim)

```json
{"verdict":"PASS","findings":["Branch/HEAD confirmed and suite re-run at the source (not trusted from the rework report): branch tiny-safe-but-important-refactorings, HEAD 9ba774ee, build 0 warnings/0 errors. scripts/playwright.sh --filter \"...Fusion.Accordion.WhenUsingFusionAccordion\" printed \"Test Run Successful. Total tests: 7 / Passed: 7\" exit-code=0 (TRX playwright-20260614-112212.trx). The runner executed the 7 in shuffled order and all passed, which also demonstrates Rule-2 order-independence.","VERDICT: PASS. All 7 tests are behavior-named, independently understandable, use real interactions only (trusted header clicks + real button clicks via FusionAccordionLocator/raw-ID locators), and each has a nameable mutation it catches (Rule 3). No page.evaluate, no ej2_instances, no mocking, no weak asserts (uses ToHaveTextAsync / ToHaveClassAsync, not ToContainText), no skip/[Retry]/Thread.Sleep, no plan-JSON assert. The stale plan_json_contains_accordion_behaviors test was DELETED with WhenAccordionPanelExpands.cs (git status: D) — the trace .png/.zip on disk are leftovers, not live tests.","Rule 3 mutation analysis per test (all strong). (1) the_care_plan_opens_showing_its_three_sections WhenUsingFusionAccordion.cs:34 — catches builder not rendering OR Disabled=true not producing e-overlay; asserts 3 exact headers + e-overlay (cs:38-41). (2) opening_the_care_team_section... cs:50 — catches Expanded event not wired / SetText path broken; label asserted vs initial \"No section open\" (cs:57). (3) opening_a_different_section_names_that_section_not_the_first cs:66 — DECISIVE Index-payload proof: an Index-always-0 bug makes opening Charges resolve to \"My Care Team\" and fails cs:76. (4) closing_the_open_section... cs:84 — catches IsExpanded-always-true / Else branch broken (cs:94). (5) opening_my_care_plan_summary... cs:103 — catches ExpandItem(true,0) not emitted; pre-asserts hidden cs:106 then visible after button-only click cs:110. (6) confirming_the_care_agreement_unlocks... cs:119 — catches EnableItem(2,true) not unlocking; asserts overlay+hidden while locked, overlay-gone+readable after; correctly avoids the pointer-intercept trap. (7) opening_the_charges_section_loads_this_months_charges cs:144 — full-stack Index==1-gated GET proof.","HTTP proof is genuinely unsatisfiable without the fetch: grep confirms '$6,090' and 'Billing period:' exist ONLY in _MonthlyChargesPartial.cshtml (lines 3,12), which is reachable only via the Index==1-gated GET wired at Index.cshtml:76-80 to SandboxRoute('MonthlyChargesDetail','Accordion') -> AccordionController.cs:22-27. So test 7 cannot pass unless Expanded fired, IsExpanded&&Index==1 matched, the GET ran, and the response was injected into #charges-detail.","e-overlay assertion is grounded, not invented: onboarding artifacts (primitive-map.md:49 cites accordion.js:1086 add/remove e-overlay and d.ts:424-432 enableItem signature; csharp-name-decisions.md:47) confirm e-overlay is the real Syncfusion disabled-panel class and enableItem/expandItem are real EJ2 methods. ExpandItem/EnableItem builder signatures (FusionAccordionExtensions.cs:28-39) and the Expanded event payload Index/IsExpanded (FusionAccordionExpandedArgs) match the view's usage exactly.","Nested Vertical Slice contract satisfied: AccordionModel is referenced only by its own controller+view; the route /Sandbox/Components/Accordion and FusionAccordion are referenced only by this journey's view, its FusionAccordionLocator, and WhenUsingFusionAccordion.cs — no other fixture navigates to it. Own-model/own-view/own-route/own-world all hold. Fresh navigation per test via OpenCarePlan() (NUnit PageTest gives a fresh Page per [SetUp]); no shared mutable state.","View and test are free of test-rig artifacts: grep for data-testid/echo/debug/application+json/data-reactive-plan/page.evaluate/ej2_instances/EvaluateAsync across the view, partial, test, and locator returned NONE. #open-section, #agreement-status, #open-summary, #confirm-agreement are real product UI a care-plan page would carry — passes the screenshot test.","NON-BLOCKING hygiene finding (not a BDD-rule violation, does not affect any test): CareChargesResponse class at AccordionModel.cs:18-22 is declared but referenced nowhere (the charges partial returns HTML, not a typed response). It is dead code in the slice this suite owns and violates the repo 'no dead code' hygiene rule. Recommend deleting it; no test depends on it so removal is safe and does not change the PASS verdict."]}
```

## How this verdict maps to the 5 BDD rules

- **Rule 1 (behavior, not implementation).** Every one of the 7 test names describes
  what the resident sees or does (`the_care_plan_opens_showing_its_three_sections`,
  `opening_a_different_section_names_that_section_not_the_first`, and so on), never an
  internal mechanism.
- **Rule 2 (independent).** The runner executed the 7 in shuffled order and all passed;
  each test re-navigates through `OpenCarePlan()` against a fresh `Page` per `[SetUp]`,
  so there is no shared mutable state and no ordering dependency.
- **Rule 3 (fails-when-broken).** The reviewer named the specific mutation each test
  catches — including the decisive `Index`-payload test (an `Index`-always-0 bug resolves
  the wrong section name and fails) and the full-stack `Index == 1`-gated GET test (the
  `$6,090` charge text lives only in `_MonthlyChargesPartial.cshtml` and is reachable only
  through the gated fetch).
- **Rule 4 (real interactions).** Trusted header clicks and real button clicks via
  `FusionAccordionLocator` / raw-ID locators; no `page.evaluate`, no `ej2_instances`, no
  mocking, no weak asserts (`ToHaveTextAsync` / `ToHaveClassAsync`, not `ToContainText`).
- **Rule 5 (blind reviewed).** This file records that independent review; the view carries
  only real-app elements (`#open-section`, `#agreement-status`, `#open-summary`,
  `#confirm-agreement`) with no echo spans, debug divs, or plan-JSON panel.

The single finding is explicitly NON-BLOCKING (a `CareChargesResponse` dead-code hygiene
note in the sandbox slice) and does not affect any test or the PASS verdict.
