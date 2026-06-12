# Member-Grammar Coverage Matrix — 2026-06-12 (in progress)

Scope: 100% behavior coverage around the browser-object member concept —
payloads/components/plugins as JS objects whose members are read, written,
and called; one factory family in C#, one `values.event` slot and one
resolver in the runtime. Built by three blind mappers (read variants,
mutations, slot+positions); every verdict carries the proving assertion;
gaps re-verified before any test gets written. Vitest closes executor rows;
page-visible rows follow the codified Playwright nested vertical slices
(bdd-testing skill loads first).

## Mapper verdicts (assertion-quoted)

- READ variants R1-R12: 24 covered, 1 gap — no test names a read with
  scope `dispatch` (R6e).
- MUTATIONS M1-M9: covered include set-on-component (4 angles),
  set-on-event-payload (`args.cancel`, sync-proven), set/call-on-local,
  call-on-component/plugin, dispatch payload variants, 3 sync-lane proofs,
  6 loud-failure proofs. Gaps/partials: set/call on payload scopes
  dispatch/element-write (M3e/M3f), call-on-event-payload (M5b), call on
  success/error/request scopes (M5c), reaction-layer unregistered-plugin
  call (M9f partial — catalog-level only).
- SLOT + POSITIONS S1-S5/P1-P10: 18 covered, 2 partial — dispatch delivery
  asserts firing but not the payload value (S5), and no test proves a
  member-READ value becomes the dispatch detail (P5).

## The bigger finding — wire width without C# producers (GROUNDING OPEN)

Pass-Protocol grounding of each gap in DSL source found that several wire
vocabulary items have NO PlanAuthoring producer located yet:

| Wire item | C# producers found | Status |
|---|---|---|
| payload scope `event` | 6 authoring sites + conditions | grounded |
| payload scope `success`/`error` | ResponseBuilder:49/88/107, PipelineBuilder:288 | grounded |
| payload scope `element` | array ops (ElementExpressionCompiler:135, ValueExpression:94-109) | grounded |
| payload scope `request` | factory prepared (`Source.cs:71`), zero callers | DESIGNED LANDING ZONE (owner-confirmed 2026-06-12): the runtime keeps the request snapshot deterministically — retry bookkeeping; the C# authoring door is future surface. Vitest over the raw shape is quality-forcing scaffold ahead of the door, kept. |
| payload scope `dispatch` | none — factory deleted | DELETED 2026-06-12 (commit 85f26ceb): owner confirmed no staged story. Adversary-upheld dead: internal factory with zero callers, no string/reflection/deserialization mint, and the behavior its name suggests is already live as scope `event` on the listening side of a dispatched CustomEvent. Factory, wire term, union member, and runtime alias case removed together; contract regenerated. |
| payload scope `local` | factory prepared (`Source.cs`, `PayloadSource.Local()`), zero callers; no live runtime creator either | DESIGNED LANDING ZONE (owner-confirmed 2026-06-12): pipeline-local variable — hold a value read from a JS object so later steps in ONE flat scope reuse it (`var x = read(); b = x`). The runtime resolver case is prepared and the execute.test.ts order probes exercise the read/write shape; the C# authoring door and the context writer are future surface. Story recorded as XML doc on the factory. |
| Set with PayloadSource target | DOOR FOUND — per-slice typed-args extensions: `args.PreventOpen(p)` et al emit `ReactionGraph.Set(PayloadSource.Event(), "cancel", true)` (FusionMenuBeforeOpenArgs.cs:34, BeforeClose:28, ContextMenu open/close, InPlaceEditor begin/end/actionBegin, Stepper changing, AutoComplete preventDefaultAction, Tooltip family). Sandbox Menu + ContextMenu views author it today. | GROUNDED. Cause of the one-hour miss, named: caller-greps scoped to the core project while the architecture places vendor knowledge in slices — grep scope must follow the architecture (all projects). Next: confirm/write the Playwright slice proving cancel in a real browser (browser is truth). |
| Call with PayloadSource target | DOOR FOUND for event scope — `FusionAutoCompleteOnFiltering.cs:63` emits `ReactionGraph.Call(PayloadSource.Event(), "updateData", ...)` (EJ2 filtering pattern). No door found yet for success/error scopes (M5c remains open/staged). | GROUNDED (event scope). M5b is a real row — needs its executor test labeled as scaffold + the filtering Playwright slice as proof. |

Rule applied: a runtime test for a shape no DSL call can emit pins a
non-DSL state. Each OPEN row resolves to exactly one of: (a) a door exists
— write the row's tests (vitest + Playwright slice where page-visible);
(b) no door — record justified-unreachable AND treat the width (union
member, runtime case, pinning test) as an evidence-gated
narrowing/deletion candidate per the standing rule: evidence + adversarial
approval, never guessing.

## Sequence

1. Full gate green (running).
2. Resolve every OPEN row by reading source — no conclusions before the
   doors are found or proven absent.
3. Write tests for grounded gaps (unhappy-path criteria mandatory:
   assertions unsatisfiable by the defect).
4. Playwright vertical slices for page-visible rows (load bdd-testing
   first).
5. Full gate again.
