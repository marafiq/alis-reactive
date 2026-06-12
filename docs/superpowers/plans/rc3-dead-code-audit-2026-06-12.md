# RC3 Dead-Code & Indirection Audit — 2026-06-12

Goal: ranked list of dead code / unnecessary indirection at >=99% confidence,
each claim adversarially prosecuted before it enters the list. DSL is frozen:
**public surface = feature even with zero callers**; only internal/private/
unreachable code qualifies as dead. Features are established by reading actual
code, never inferred from tests, docs, or memory. Fusion AND Native vendor
paths checked before any vendor-adjacent claim.

Settled earlier (not re-litigated): request scope = KEEP (deterministic-retry
landing zone, user-ruled). Set/call-on-event-payload = ALIVE (PreventOpen
family, updateData doors). alis-prefix family = deferred by design.

## Phase 1 — Docket A/B re-verified first-hand (this session)

### A. Payload scope `dispatch`
- `PayloadSource.Dispatch()` — Alis.Reactive/PlanModel/Values/Source.cs:73,
  **internal**, zero callers repo-wide (grep over all *.cs incl. Fusion,
  Native, NativeTagHelpers, tests, sandbox: only PlanTerms.cs + Source.cs hit).
- No string door: `PayloadSource(string)` ctor called only by the private
  PayloadScope path inside Source.cs; `PayloadScope.From` called only from
  Source.cs:56. No deserialization of plan JSON in C#.
- Runtime: execution-context.ts:59 aliases `case "dispatch"` to `values.event`.
  The listening side of a dispatched CustomEvent already reads scope `event`.
- native-action-link.ts:91 and execute.ts:69 `"dispatch"` hits are the Dispatch
  REACTION kind (live feature), not the payload scope — verified by reading.
- Contract: PlanContractGenerator.cs:287 emits the PayloadScope union from
  `PayloadScope.Values` — deleting Known entries narrows the union; assertNever
  in execution-context.ts forces case removal at typecheck. Compiler-checked
  end to end.

### B. Payload scope `local`
- `PayloadSource.Local()` — Source.cs:75, **internal**, zero callers repo-wide.
- Runtime: `values.local` is only TYPED (types/context.ts:11) and READ
  (execution-context.ts:66-67). No writer exists: ExecutionContext has
  event/withRequest/withResponse/withElement — no withLocal. Verified by grep
  over all non-test runtime TS.
- Set/call-on-payload cannot create `local`: requireMutablePayload
  (execute.ts:239) resolves the EXISTING payload and throws on missing —
  it mutates members of a payload that must already be in context.
- Test cost: execute.test.ts uses set-on-local as its execution-order probe
  (markLocal, lines 68-71, 408, 578-648; `local:` arranged directly as raw
  context at 578/634). Deleting local = re-probe those tests through a
  component or plugin. Tests are not a feature door (Rule 1/10).
- No authoring surface exists for "pipeline-local state": no builder method,
  no public path. By the "established by reading actual code" standard, local
  is not a feature in the code.

## Phase 2 — broad sweep (4 reader agents, every lead re-verified first-hand)

Scopes: C# plan domain; C# builder layer + FluentValidator; TS runtime;
Fusion + Native + NativeTagHelpers + DesignSystem + Analyzers.

Surviving leads (re-verified by orchestrator at source):
- `runtime/shared/wire-format.ts` — whole module. Zero importers repo-wide
  (grep "wire-format|formatForWire" over all *.ts/*.js/*.mjs incl. __tests__,
  configs, scripts: only the file itself; all real call sites use the
  RuntimeShape METHOD directly — gather.ts:132, request-payload-writer.ts
  88/102/114/163/190/227).
- `clearContainerValidation` — runtime/validation/orchestrator.ts:241-257 +
  barrel re-export validation/index.ts:1. Zero callers in production AND
  tests. Helper `clearContainerErrors` stays alive (orchestrator.ts:124,163).
  Unload teardown routes through boot.ts → live-clear/error-display, not here.
  `revalidateField` on the same barrel line IS alive (live-clear.ts:63).

REJECTED reader leads (false alarms, killed by orchestrator re-verification):
- `Shape.Nullable()` "zero callers" — FALSE: called unqualified by
  Shape.FromClrType at Shape.cs:76 (live path for CLR Nullable<T> model
  properties). Reader grepped only the qualified form. ALIVE.
- `OrderedPlanItems.Snapshot()` (RequestPlan.cs:105-113) — not indirection:
  defensive copy enforcing plan-model immutability at build time (mutable
  builder list → Array.Empty / new List snapshot). ALIVE, justified.
- Reader 3's "dispatch has production usage (trigger.ts)" — confusion with
  the Dispatch REACTION kind; payload SCOPE evidence stands.
- Reader 3's "local: intended for future use per architecture" — inference
  from comments, banned by the audit standard; code evidence stands.

All other areas reported clean by readers (builders, FluentValidator,
Fusion/Native slices, DesignSystem, Analyzers ALIS001-ALIS008, wire terms,
ValueExpression/ConditionGraph/ReactionGraph factories, compare operators).

## Phase 3 — adversarial prosecution (4 independent refuters, one per claim)

| Claim | Verdict | Strongest refutation attempt that failed |
|---|---|---|
| dispatch scope dead | UPHELD | Listener path traced: ElementBuilder.SetText<TPayload> always mints PayloadSource.Event() — dispatched-event payloads already read via "event"; no reflection/string/deserialization door (plans are write-only, PlanNodeDiscriminator.cs:21) |
| local scope dead | UPHELD | No withLocal; no spread/index writer of context.local outside test fixtures; conditions+validation resolve through the same evaluateValue → resolvePayload path |
| wire-format.ts dead | UPHELD | No barrel, no tsconfig alias, not an esbuild entry (entry is runtime/root.ts), no architecture-test allowlist entry, no dynamic import |
| clearContainerValidation dead | UPHELD | No namespace import, no plugin-catalog/app-object string-name exposure, no C#/.cshtml reference, unload routes through wireLiveValidation |

## Outcome — owner rulings and execution (2026-06-12, same night)

- Item 4 `local`: owner gave the staged story — pipeline-local variable,
  hold a value read from a JS object for reuse within ONE flat scope
  (`var x = read(); b = x`). RULED KEEP. Story recorded as XML doc on
  `PayloadSource.Local()` and in the member-grammar coverage matrix row.
  The execute.test.ts probes stay — they exercise the staged shape.
- Items 1-3: no story offered. Deleted in ranked order, one row per commit:
  1. wire-format.ts — commit 9326e56e (typecheck clean, vitest 200/200)
  2. clearContainerValidation — commit 15baf75f (typecheck clean, vitest 200/200)
  3. dispatch scope vertical — commit 85f26ceb (contract regenerated, union
     narrowed to 6 scopes, typecheck clean both legs, vitest 200/200,
     dotnet build 0/0)
- `PayloadSource.Request()` also got its owner-confirmed story as an XML doc
  while recording local's, so the next auditor reads it at the factory.

## Full-gate verdict (2026-06-12, after the three deletions)

scripts/test.sh on the cumulative tree: Playwright "Passed: 1219", zero
Failed in captured output. Playwright is the final stage, so all earlier
stages (typecheck, build:all, vitest, dotnet build, non-Playwright tests)
completed to reach it; typecheck/vitest 200/200/dotnet build 0-0 were also
run first-hand per commit. Captured artifact holds the run's tail (the
invocation piped through tail -100); the diag/TRX artifacts live under
tests/Alis.Reactive.PlaywrightTests/TestResults/.

## Fresh-eyes re-audit of last night's deletion commits (same day, later)

Each deletion re-verified in the CURRENT tree, not from commit messages:

1. 1935b34b PayloadContract machinery — zero survivors: grep for
   PayloadContract|PayloadType|PayloadTypeName|payloadType returns NOTHING
   in C# or TS, production or tests. No orphaned wire term (PayloadTypeName
   went with it). Deleted guard (channel re-registered with different
   payload type) became unrepresentable once typing left: same-name
   registrations are now necessarily identical.
2. 1e7a0be3 Merge/channel check — provably unreachable in current tree:
   ObjectEventContract's ctor is private and its only factory
   ForComponentEvent(eventName) sets channel ≡ name (single creation site
   BehaviorGraph.cs:29), so a same-name/different-channel re-registration
   cannot be constructed. First-wins replacement is semantically identical
   to the old Merge-returns-existing.
3. 36c5c1e5 payload contracts off the wire — zero TS readers of
   payloadType/"untyped"/"named" vocabulary anywhere in runtime incl.
   tests; contract regenerated many times since, typecheck green; vitest
   200/200 + Playwright 1219 green on the post-state.
4. 2fc271f1 retry marker rename — old data-alis-retry: zero source hits;
   new data-reactive-retry confined to retry-indicator.ts + its test
   (self-stamped cleanup invariant intact).
5. 2b54be9e onclick → DSL dispatch — zero inline onclick remain in ANY
   sandbox view; behavior pinned by reset_all_button_clears_both_vendors
   and reset_then_interact_proves_components_still_reactive_after_reset
   (WhenComponentEventsFireCrossVendor.cs:72,:115), inside the green 1219.
6. 541d20d8/06eca370 literal scrubs — zero stale payload-type literals in
   vitest fixtures (same grep as #3). Docs scrubs not re-checked
   line-by-line (no behavior at stake).

Honest bounds: #3's "the runtime never read it historically" rests on last
night's root-cause doc plus the absence of any reader today plus green
suites on the post-state — not on a re-read of pre-deletion runtime
history. #5 rests on the named Playwright tests, not on eyes-in-browser
this session. #1/#2 deliberately removed a *potential* authoring-time
guard; with typing and channel divergence unrepresentable, the guard had
nothing left to fire on — that ruling was prosecuted and recorded last
night, not re-litigated here.

## Ranked list — dead at >=99%, adversary-signed (as presented for ruling)

1. **`Alis.Reactive.Assets/runtime/shared/wire-format.ts`** (whole module).
   Deletion shape: delete file. Nothing regenerates, nothing re-probes.
   esbuild never bundles it (unreachable from root.ts).
2. **`clearContainerValidation`** — orchestrator.ts:241-257 + its name in
   validation/index.ts:1. Deletion shape: remove function + barrel name.
3. **Payload scope `dispatch`** — Source.cs:73, PlanTerms.cs:370+381,
   regenerate plan.ts (PayloadScope union narrows), remove alias case
   execution-context.ts:59 (assertNever forces it at typecheck). No test
   rework anywhere.
4. **Payload scope `local`** — Source.cs:75, PlanTerms.cs:371+382,
   regenerate plan.ts, remove case execution-context.ts:66-67, remove
   `ExecContext.local` (types/context.ts:10-11). COST: execute.test.ts
   order-probes (markLocal at 68-71, 408, 578-648) must be re-probed
   through a component or plugin.

Residual <1% on items 3-4: an out-of-repo staged-feature story only the
user can hold (per the request-scope precedent). By the
"established by reading actual code" standard, neither establishes as a
feature: internal factories, zero callers, no authoring surface, no
runtime writer (local), behavior already served by event scope (dispatch).

Items 1-2 have no staged-feature story: one duplicates a live method
verbatim; the other is runtime code no plan node can invoke (the runtime
is not frozen surface — the C# DSL is).
