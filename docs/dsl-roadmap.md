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
