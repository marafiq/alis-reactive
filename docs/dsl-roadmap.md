# DSL Roadmap — ShowRetryIn, ShowLoadingIn, StreamInto

Design record from the RC1 retry/affordance sessions (2026-06-10), written to be revisited:
every decision here was argued against real product behavior (the Alis tab-count and
dashboard pages) and against the live-connection retry indicator that shipped with RC1.
Status: designed, not built. All three verbs are additive 1.x surface — 1.0's hardened DSL
never changes meaning because of them.

## Shared principles (settled; apply to every affordance)

- Good defaults, customizable: the default needs zero DSL; control is authored.
- The framework flips state; developers own every pixel. No framework-invented UI — the
  original retry badge (runtime-created button, hardcoded English tooltip) is the
  anti-pattern this family replaced.
- The DOM is the only registry. No parallel JS state to drift out of sync; closures ride
  DOM nodes; destroyed DOM cleans itself.
- Only a genuine outcome clears state (reconnect, settle, successful inject). A failed
  attempt never reads as success — the optimistic hide-on-click choreography shipped once,
  read as "the button does nothing," and was removed. Tests for these features must include
  the unhappy criterion ("if it's still down it keeps telling me it's down") with
  assertions the defect itself cannot satisfy.
- Affordances live where their condition lives: connection loss is page-level → app shell;
  a failed inject is element-level → the hole itself; loading is operation-level → the
  thing being updated. Never full-page overlays; the Alis product never shows one.
- Element/attribute presence is configuration. Safety affordance missing its element =
  loud traced error, behavior not invoked, no fallback. Cosmetic affordance unstyled or
  absent = silently declined feature.

## Frozen in 1.0 (shipped with RC1 — context for the family)

`<div id="alis-realtime-connection-retry-container" hidden>` in the layout. Visible while
any live connection (SSE/SignalR) is down; one marker child (`data-alis-retry="<key>"`) per
dropped connection; one click retries everything down; only an actually restored connection
removes its marker; hidden when none remain. Missing container at drop time: loud
`container.missing` trace. The anchor problem (where does a page-level condition show?) was
settled by the real product: app-level state belongs in the shell, like the "Punched Out"
chip — per-region badges turned a dead hub into badge spam across a tab strip.

---

## ShowRetryIn — retry for failed injections

**Use case:** `Into`-loaded content whose trigger the user cannot repeat — `DomReady`
autoload, push-triggered injection. A failed save button is deliberately NOT covered: the
button is its own retry affordance. Retry exists for holes the user cannot refill.

**Syntax (authored, error scope — failure behavior lives where failure is authored):**

```csharp
p.Get(url)
 .Response(r => r
     .OnSuccess(s => s.Into("panel-container"))
     .OnError(e =>
     {
         e.Element("panel-status").SetText("Resident panel couldn't load.");
         e.RetryIn("panel-container");   // final name: ShowRetryIn
     }));
```

**Anchor:** the `Into` target IS the anchor — the plan names the hole; no global slot
(element-level condition), no derivation walk.

**Replay semantics — deterministic, same payload (Adnan's explicit requirement):**
`runtime/execution/requests/http.ts` is already factored at the needed seam:
`prepareHttpRequest` materializes everything (gathers resolved, request snapshot baked into
context, final `{url, init}`) before `sendHttpRequest` → `routeExchangeOutcome`. The click
closure captures `{ request, planDocument, preparedRequest }` and re-runs send + route
verbatim. Gathers are NOT re-read at click time. Chained follow-ups come free because
continuation hangs off `routeExchangeOutcome` with the same request node.

**Nuances settled:**
- `whileLoading` re-runs on retry (the user is loading again) — one deliberate call before
  re-send, since replay skips the prepare phase.
- The validation gate (`requestCanSend`) is skipped on replay: same payload, already
  validated.
- Bodies are replayable by construction today: JSON strings trivially; `FormData` re-sends
  (File objects live in memory in the closure). Streaming bodies would break replay —
  pinned as an invariant when built.
- No false success: the armed state stays until a genuine success injects content.
- Target destroyed by parent slot unload → affordance dies with the DOM. Nothing to clean.

**Default content:** a global, developer-owned cloneable template (`<template>` defined once
in the layout, styled once) cloned into the failed target; a target that already has
authored content keeps it — that is the 5% control escape.

---

## ShowLoadingIn — loading state, and the zero-DSL default

**The product dose that shaped this:** in the real Alis app the loader is never full-page;
it appears on the thing being updated (the tab panel, the grid, the card). "Global solution"
means the *treatment* is defined once (CSS), not that placement is global. The boilerplate
to kill is the three authored reactions per request (show + hide-on-success +
hide-on-error), not the placement choice.

**The default (zero DSL):** a request flips `data-alis-loading` on every plan-named update
target at send and clears them on settle, in a `finally`. Plan-named means:
- the `Into` target, and
- the components/elements the success scope writes — a grid bound from a typed response
  (`setDataSource` from the success body) is a known target the same way an injected panel
  is (Adnan's generalization: binding is the opposite of Into, but the target is equally
  known).

Targets may be plural. This is safe where the old badge anchor-walk was not, because the
mechanism changed: an attribute on an unstyled target is inert — bad anchors cost nothing,
good anchors light up only where the consumer wrote CSS. Reading the success scope here is
reading the plan's declared effects, not heuristic anchor derivation.

**`ShowLoadingIn("target-id")`:** the authored verb for requests with no plan-named target,
or to add a target the plan doesn't name.

**Control and interplay:**
- Authoring `WhileLoading(p => …)` opts that request out of the default entirely — taking
  control means owning the whole loading story. No stacking, one-sentence mental model.
- A future `Settled(p => …)` response scope would erase the hide-twice boilerplate in the
  control case. Deliberately unbundled; defer until wanted.

**Nuances settled:**
- Anti-flicker is consumer CSS (`transition-delay: 150ms` on their own rule) — the runtime
  stays a dumb flip; no framework debounce timers.
- Validation-aborted requests never send, so never mark.
- Upgrade-safety rule: design-system default treatments for these attributes must be
  opt-in classes, never bare attribute selectors — a 1.x upgrade must never repaint a
  running app. This rule is what keeps attribute-flipping defaults additive in 1.x.
- Observed in the real product (Billing Center, 2026-06-10): the page shell renders
  instantly; the content region being fetched shows a branded loading block (logo +
  "LOADING") that fills exactly the region's rectangle and is replaced by the data when it
  arrives. Confirms the model: loading lives in the updated thing's place, treatment is
  defined once and substantial (not a thin shimmer). Implies two consumer CSS modes under
  the same attribute: placeholder-fill for first load into an empty region, overlay-on-stale
  for refreshes (refresh mode not yet observed — check a filter change when revisiting).
  The branded block is the FOURTH appearance of the cloneable-template concept (retry
  content, loading treatment, chat bubble, and now the product's own loader).

---

## StreamInto — streaming responses (v1 cuts only; full topic is its own session)

The mechanics are genuinely easy: `fetch` already streams; `response.body` through a
`TextDecoderStream`, append each chunk into the target. `prepareHttpRequest` is unchanged;
only `sendHttpRequest`'s body handling changes from await-whole to read-loop. A constrained
v1 is a normal 8-step primitive pass. What needs the dedicated session is decision density,
not difficulty.

**v1 cuts (agreed):**
- Text-append only. `StreamInto` is NOT `Into`: it appends raw text and must never boot
  plan-bearing HTML mid-stream. Same-sounding words, different operations — do not conflate.
- Success scope fires at stream end; abort/network death routes to the error scope.
- Retry means re-ask — a new request, never replay. A half-delivered stream has no
  meaningful replay, and streaming bodies break the ShowRetryIn replay semantic anyway.

**Session agenda (the five questions that earn the dedicated session):**
1. What is a chunk in the domain? A streaming request behaves like a temporary trigger
   source — the marriage of request and push. The grammar owns both halves; name the union
   once or the vocabulary forks.
2. The `StreamInto` / `Into` boundary (append-text vs inject-and-boot) as formal contract.
3. Half-a-response semantics: error scope firing while partial content is already visible.
4. Loading hand-off: `data-alis-loading` until first token, then a streaming state?
5. The chat compose loop (create bubble → stream into it → finalize): the bubble wants a
   cloned template — the THIRD appearance of the developer-owned cloneable template concept
   (retry content, loading treatment, message bubble). That convergence likely deserves one
   named framework concept before any of the three ships its own ad-hoc version.

---

## TOP PRIORITY (rc3) — component lifecycle: onboard / audit / upgrade

The canonical, loop-consumable goal lives at
`docs/superpowers/goals/onboard-or-audit-or-upgrade-sf-components-with-100-percent-behavior-coverage.md`
— modes, oracles, coverage law, operating loop, and done-criteria. This roadmap does not
duplicate it.

## Tooling and type-quality direction (Adnan, 2026-06-10)

The agent setup and TS quality need a deliberate pass; the enforcement ladder is:
**types first, lint second, bespoke tests last.** The source-scanning architecture test
should stay minimal and shrink — every allowlist entry is migration debt toward a
compile-time or lint expression of the same rule:

- DOM-query boundaries → eslint `no-restricted-syntax`/`no-restricted-globals` with
  per-directory overrides (the allowlist becomes lint config, reviewed in diffs).
- Vendor isolation → eslint `no-restricted-imports` per directory.
- Absence conventions → types, not conversions: DOM leaves speak `null`, authored domain
  speaks `undefined`; no `?? undefined` laundering (done 2026-06-10).
- Open strings → closed unions at the boundary that owns the domain (`ResponseBodyKind`:
  the framework supports exactly json/text/empty; parsing is the only tricky part and the
  classification is the type). Hunt remaining stringly surfaces the same way.
- `npm run lint` is green and belongs in CI as a blocking step (quality ledger T7).
- Behavior-coverage honesty: docs claim full-coverage discipline; reality is partial
  (Adnan: URL module, template URLs, and more lack Playwright coverage today — retry was
  the proof). Reuse the kind/module enumeration to emit a named list of kinds and runtime
  modules with zero behavior tests; coverage statements in docs defer to that list, never
  assert "full".
- Plan contract: kill the intermediators — generation will never be fully automatable
  (Adnan), so stop pretending. The emitter is itself 1,200 hand-written, unverified lines: a
  bug there ships systematically wrong contract under a "generated — do not edit" badge that
  discourages reading the output, whereas a hand-written `plan.ts` bug is local, reviewed in
  the diff of the exact artifact the runtime consumes, and caught by tsc against real usage.
  Today is triple bookkeeping: the plan model, the emitter re-describing every node property
  in strings, and a drift gate policing the two. Target: **hand-write `plan.ts`** as a first-class contract file (humans
  express the variant splits naturally — the emitter's whole size came from fighting them),
  delete the emitter and generator tool, and keep sync via a super-strict process: every C#
  plan-shape change pairs with its `plan.ts` edit in the same commit, and BEHAVIORAL equality
  is what gets verified — a golden-plan corpus (every node kind exercised once) serialized
  through the real PlanSerializer into JSON fixtures, imported in TS with
  `satisfies PlanDocument`, so tsc itself fails the build when the hand-written contract
  drifts from what the serializer actually writes. Checking is automatable where writing is
  not, and the check sits at the true boundary (serialized JSON), not at class shapes.
  The residual drift vector is OMISSION (add a node, forget its golden plan, stay green —
  how drift crept in historically): closed mechanically from both ends. A C# test
  enumerates every registered node Kind and asserts the corpus covers it
  (reflection-as-audit, not generation); on the TS side
  `Exclude<AnyNodeKind, CoveredFixtureKinds> extends never` makes tsc name any uncovered
  variant. Humans author the model, the contract, and the golden plan; the machine proves
  the triangle is closed. Exhaustive corpus authorship is NOT expected of humans or LLMs —
  neither follows unenforced convention; both follow a red build naming the specific gap.
  So: seed the corpus by one-time HARVEST (every sandbox page already serializes real plans
  into its data-reactive-plan script tags — crawl once, keep the JSON), and let the
  named-gap coverage test force each increment (new kind → red build naming it → author
  copies the five DSL lines they already wrote for the sandbox view). Side effect: the gate
  finally enforces the existing rule that every primitive has a sandbox demonstration.
  Flips CLAUDE.md's "never hand-edit plan.ts" to its opposite on the day this lands.

## Playwright harness charter (pre-1.0, Adnan 2026-06-10)

Today's suite is "ok"; to be the real harness it needs all four, together:
- **Full coverage** — named-gap lists (kinds/modules with zero behaviors), never claimed,
  always enumerated. URL module and template URLs are known holes today.
- **Behavior-driven with real use cases** — user stories against product-shaped pages,
  not component pokes; primary paths exercised the way the product uses them (T15's
  grid inline-validation gap is the canonical example).
- **Isolated** — hermetic page-scoped state as the norm: the per-render drill-world
  pattern (id baked into the page, server state scoped to it) generalizes to any test
  needing server-side arrangement. Process-global flags are banned — that class of bug
  cost a night.
- **Independent** — no order coupling, no shared-state healing in SetUp/TearDown
  (needing teardown healing is the smell that isolation was violated upstream).

## The family at a glance

| Concern | Affordance home | Opt-in | Missing/unstyled |
|---|---|---|---|
| Live connection drops | One shell container, marker per connection | Automatic (safety) | Loud error |
| `Into` load fails | The target element + cloneable template | Authored: `ShowRetryIn` | n/a — plan names it |
| Request in flight | Every plan-named update target | Automatic via plan; `ShowLoadingIn` to add | Inert attribute, silent |
| Streaming response | The `StreamInto` target | Authored: `StreamInto` | n/a — plan names it |

**1.x freeze analysis:** the three verbs are pure additions (new nodes, new switch cases,
`assertNever` keeps unions honest). The loading default retrofits observable-but-inert
attributes onto existing plans — additive in practice because visuals require consumer CSS,
and kept that way by the opt-in-class rule. Naming is a family decided once:
`ShowRetryIn` / `ShowLoadingIn` / `StreamInto`.
