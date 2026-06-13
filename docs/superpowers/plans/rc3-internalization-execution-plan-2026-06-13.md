# RC3 Wire-Layer Internalization — Execution Playbook (2026-06-13)

Companion to `rc3-dead-code-audit-2026-06-12.md` (verification + evidence).
This file is the **mechanical execution record**: decisions, order, proof loop,
commit boundaries, checklist. Pick it up and run.

## DECISION LOG (confirmed)

- **D1 — Builders are the public surface; wire is internal.** (owner-confirmed
  2026-06-13: "builders and what gets exposed in builders is public and meant
  to be public.") The permanent public floor: `ReactivePlan<TModel>`, all
  PlanAuthoring builders (`PipelineBuilder`, `ElementBuilder`, `TriggerBuilder`,
  `GatherBuilder`, `ResponseBuilder`, condition builders), the `Typed*Source<T>`
  family, `Html` entry points, `ComponentRef`, public interfaces. NONE name a
  wire type. Never touch these.
- **D2 — The entire PlanModel wire layer goes `internal`.** Verified: zero
  permanently-public member names any wire type; `Typed*Source.ToValueExpression()`
  is internal; vendors expose only `Typed*` wrappers; the one cross-assembly
  consumer (`NativeActionLinkPayload`) is internal; `PlanDocument` is already
  internal and serializes green at 1219. Evidence in the audit doc.
- **D3 — Serialization is unaffected.** STJ reflects PUBLIC-MODIFIER properties
  regardless of the declaring class's accessibility. Wire classes go internal;
  their public PROPERTIES stay public (they are the wire). Proven by PayloadSource
  + PlanDocument.
- **D4 — `plan.ts` stays byte-identical.** PlanContractGenerator emits TS
  interface/union names from STRING LITERALS. Always `git diff --stat` it to confirm.
- **D5 — Proof loop is fast-legs-per-family + ONE final full gate.** Rationale:
  a visibility change can fail ONLY at compile time (accessibility); serialization
  is reflection-based and visibility-blind. So per family: solution build (both
  TFMs) is the real judge; one full Playwright gate at the END is the serialization
  proof for the whole internalization. (Owner may request per-family Playwright at
  ~50 min each; default is batched.)
- **D6 — Commit boundary: one commit per family**, leaf→base within each, message
  names the family + carries the fast-leg evidence. Bases go in a FINAL wave commit.

## PROOF LOOP (run per family)

```
1. Edit: public (sealed|abstract) class X  ->  internal (sealed|abstract) class X
2. dotnet build Alis.Reactive.slnx            # both TFMs; 0/0 = accessibility OK
3. npm run typecheck                          # regenerates plan.ts
4. git diff --stat .../runtime/types/plan.ts  # MUST be empty (byte-identical)
5. npm run test -w Alis.Reactive.Assets       # vitest 200/200
6. commit (fast-leg evidence in message)
```
Final wave (all bases) + then ONCE: `scripts/test.sh` (full gate incl. Playwright 1219).
Note: after any TS/contract regen, Playwright must go through the full gate /
`scripts/build.sh` first — the wrapper rejects a stale esbuild bundle (`--no-build`
fails on a fresh contract). Do NOT call `scripts/playwright.sh` directly after regen.

## ORDERING (build-enforced; leaves before their bases)

A base cannot go internal while any PUBLIC class declares a member of that base
type. So:
- **Wave 1 — leaves + non-exposed intermediates** (each family commit compiles
  because the union BASES stay public, exactly like PayloadSource shipped while
  `Source` stayed public).
- **Wave 2 — the union bases**, once every leaf is internal.

## EXECUTION CHECKLIST

### Already done
- [x] `PayloadSource` -> internal (commit a71ba35a, full gate 1219 green)
- [~] 7-class batch -> internal: ObjectExpression, ArrayExpression,
  ArrayOperationExpression, CompareCondition, LiteralExpression, ReadExpression,
  Shape — fast legs green; **full gate RUNNING**; commit on green.

### Wave 1 — leaves (one commit per family)
- [ ] **Source family** — `Values/Source.cs`: ComponentSource, PluginSource,
  UrlSource, DomSource (leaves) + RuntimeObjectSource (intermediate base; goes
  internal here — not publicly exposed). LEAVE `Source` base public (Wave 2).
- [ ] **Reactions leaves** — `Reactions/ReactionGraph.cs`: SetReaction,
  CallReaction, DispatchReaction, InjectReaction, BranchReaction, ParallelReaction,
  RequestReaction, SequenceReaction, ShowValidationErrorsReaction, BranchCase,
  NoParallelCompletion, SettledParallelCompletion. `Reactions/StartsWhen.cs`:
  ServerPushEventFilter is a base (Wave 2 if it has leaves; else here). LEAVE
  `ReactionGraph`, `BranchGuard`, `ParallelCompletion` bases public (Wave 2).
  (NOTE: this unblocks `Source` base for Wave 2.)
- [ ] **Conditions leaves** — `Conditions/ConditionGraph.cs`: AllCondition,
  AnyCondition, NotCondition, ConfirmCondition (CompareCondition already in
  7-batch). LEAVE `ConditionGraph` base public (Wave 2).
- [ ] **Values misc** — `Values/ValueExpression.cs`: PropertyValueReadAccess,
  MethodValueReadAccess (LEAVE `ValueReadAccess` base + `ValueExpression` base for
  Wave 2). `Values/Path.cs`: Path, PathSegment (no base/leaf split — flip both).
- [ ] **Requests leaves** — `Requests/RequestPlan.cs`: RequestPlan, ResponseRoute,
  FollowUpRequestChain, TerminalRequestChain, ContainerRequestValidationTarget,
  NoRequestValidationTarget, AnyResponseStatusMatch, ExactResponseStatusMatch.
  `Requests/RequestInput.cs`: NoRequestInput. `Requests/GatherRequestInput.cs`:
  RequestHeaderTarget, RequestPayloadTarget, RequestRouteParameterTarget. LEAVE
  bases RequestChain, RequestValidationTarget, ResponseStatusMatch, RequestInput,
  RequestInputTarget for Wave 2.
- [ ] **WireTerms** — `WireTerms/PlanTerms.cs`: PartialPlanScope, RootPlanScope
  (LEAVE `PlanScope` base for Wave 2).
- [ ] **BrowserObjects** — `BrowserObjects/BrowserObject.cs`: ComponentRole (leaf).
  `InputBinding` is a base -> Wave 2 if it has public leaves; else here.
- [ ] **Validation** — `Validation/ValidationGraph.cs`: ValidationContainerBinding
  is a base -> Wave 2 if leaves exist; else here.

### Wave 2 — union bases (one commit, after all leaves internal)
- [ ] `ValueExpression`, `ValueReadAccess`, `Source`, `ConditionGraph`,
  `ReactionGraph`, `BranchGuard`, `ParallelCompletion`, `RequestChain`,
  `RequestInput`, `RequestInputTarget`, `RequestValidationTarget`,
  `ResponseStatusMatch`, `PlanScope`, `InputBinding`, `ServerPushEventFilter`,
  `ValidationContainerBinding` — flip together; the build proves no public class
  declares any of them. Then `Shape`-style: confirm `plan.ts` empty diff.

### Final
- [ ] ONE full `scripts/test.sh` over the whole internalization — Playwright 1219
  = serialization proof for the layer. Commit nothing after without re-gating.

## HONEST BOUNDARY
Exposure is verified at source (the structural blocker). Per-family ordering and
interface-impl / generic-constraint edge cases are caught by each family's
solution build — never shipped, but NOT pre-proven here. "internal-ready" is an
analysis verdict; "done" is claimed only with the build + gate evidence per family.

## STATUS POINTER
Branch `tiny-safe-but-important-refactorings`, 31+ ahead of origin (unpushed; push
on owner word). Dead-code deletion theme fully resolved (see audit doc). Streaming
deferred [[project-streaming-deferred]].
