# Extensibility Stress Test — Does the Redesign Deliver "Mechanical to Add"?

> **Question under test.** The redesign ([`00-design.md`](./00-design.md) §5)
> promises: *a new feature is mechanical to add — touch one module, no
> architectural thinking.* This document tests that honestly against five
> concrete enhancements, each traced through actual source (`Alis.Reactive/` and
> `Alis.Reactive.Assets/runtime/`), and surfaces the one place the promise breaks.
>
> **Method.** Each enhancement's per-module trace was verified line-by-line
> against current source. The verdict is the design's, re-judged against what the
> code actually does — not against the trace's self-report. A weak spot found now,
> on paper, is the whole point.

---

## Verdict Table

| Enhancement | Modules touched | Localization | Thinking | Verdict |
|---|---|---|---|---|
| **gather-name-default** — infer the gather target name from the member expression; explicit string becomes optional override | **Value** (`ExpressionPathHelper`, reuse) + **Request** (`GatherBuilder`, additive overload) | one-module | some-design | **PASS** (but not *easier* than today) |
| **localStorage value source** — `p.LocalStorage<T>("key")` usable in conditions/element-ops/gather/dispatch/plugin-args/arrays | **Value** (author seam + node + reader, 3 files) + **Kind** (zero hand edits) + **Shape** (no change) | one-module | mechanical | **PASS** (earned by two redesign decisions) |
| **third component vendor** — register a new UI library with its own event wiring | **Component** (1 new `event-{vendor}.ts` + 1 driver registration in `component-runtime.ts`) + a vendor NuGet package + build plumbing | one-module (runtime) | mechanical | **PASS** (runtime; the package/plumbing is the intended vertical-slice cost) |
| **`Focus()` / `ScrollIntoView()` element reaction** — framed as a "new primitive + Kind discriminator + handler" | **Reaction** (`ElementBuilder` + `BrowserElementMembers`, one same-file friction) | one-module | mechanical | **PASS** (reuses `call`; NO new kind/handler) |
| **ProblemDetails / ModelState auto-routing** — a 4xx `{errors:{field:[msg]}}` body auto-shows per-field errors with no authored `OnError + ValidationErrors` | **Validation** (runtime, ~no-op) + **Request** + **Reaction** (new authoring default) + **runtime routing** + **Component/Plan** (container link) | few-modules | some-design | **WEAK** (a dev would not succeed cleanly) |

**Bottom line: 4 of 5 PASS. The design is safe to build on. The one WEAK case is
not a localization failure — it is a category the matrix was never built to
cover (an *implicit default from the absence of a DSL verb*), and it should be
addressed by a deliberate boundary decision, not patched in the runtime.**

---

## Per-Enhancement Findings (source-verified)

### 1. gather-name-default — PASS, but the redesign earns no credit here

**Verified.** `GatherBuilder.FromEvent<TArgs,TProp>(args, path, string param)`
(`Alis.Reactive/Builders/Requests/GatherBuilder.cs:54`) takes a mandatory name
and derives the event path via `ExpressionPathHelper.ToEventPath(path)`. A
name-optional overload is purely additive; the 3-arg override keeps compiling.
This is `one-module` — author seam in Request, helper reuse in Value.

**The honest caveat the trace raised is correct: this is *equally* localized in
today's code.** `FromEvent` is already one focused method; `ExpressionPathHelper`
already extracts the member chain; the runtime reads `target.name` verbatim with
no inference. The node shape does not change, so **Kind does not regenerate and
no runtime/contract edit fires** — a dev following the generic 5-step recipe in
00-design §5 Step 3 would waste time hunting for a contract edit that does not
exist. The redesign's contribution is cohesion (Request + Value are neighbors),
not fewer modules.

**The one real design judgement** is verified as a genuine trap:
`ExpressionPathHelper.ToEventPath(x => x.Data.Id)` returns the **full dotted path
`data.id`** (`ToRuntimePath` joins all members, lines 57-61, 195-202), not the
leaf `id`. The default name must be the LEAF member; a dev who naively reuses
`ToEventPath` produces the wrong key (`data.id` instead of `id`). There is **no
existing `ToLeafMemberName` helper** — it would be a new (additive) static
method. Matrix row B.3 says "param = member name unless overridden" but does not
spell out leaf-vs-path. That is the legitimate `some-design` bump.

### 2. localStorage value source — PASS, mechanical, and the PASS is *earned*

**Verified against source.** Adding a value source today touches:
`Source.cs` (a new `sealed class … : Source`, mirroring `UrlSource:121` /
`DomSource:136`), `ValueExpression` (a `Read` factory — the existing flat `Read`
variant, no new node family), and the runtime `evaluateValue` (a guard + branch +
`readFrom…`, mirroring `readFromUrl`/`readFromDom`). That is three files inside
the Value slice. **`Vendor`-style narrowness holds: the source goes into the
`Source` union only — NOT into `SetTargetSource` / `CallTargetSource`**
(`PlanTypeScriptContract.cs:269-272` keeps those unions separate and narrow), so
"is this a target or only a source?" is a single, compile-enforced decision, not
a smell.

**The two deltas that make this `mechanical` instead of `WEAK` are real and
verified, not vocabulary:**

- **Kind kernel removes the hand-mirror.** `tools/PlanTypeGenerator/Program.cs`
  today just calls `PlanTypeScriptContract.Render()` — it does **NOT** reflect.
  `PlanTypeScriptContract.cs` is a **1,165-line hand-authored mirror** (verified
  `wc -l`). Adding a source today means ~5 manual `Declare()` edits there (the
  `Source` union :269, a `LocalStorageSource` interface like :278/:281, a
  `LocalStorageReadExpression` like `UrlParameterReadExpression`:596 /
  `DomPropertyReadExpression`:628, plus the read-expression union). The redesign's
  reflecting `PlanContractGenerator` + `ContractDriftGate` makes this hand edit
  **disappear** — the single largest correctness win for this enhancement.
- **The closed gather-source hole** means the new source reaches gather through
  the same `TypedSource<T>` and does NOT require new `g.LocalStorage(...)`
  overloads on `GatherBuilder`. Today a URL source needed both `PipelineBuilder.FromUrl`
  AND four `GatherBuilder.FromUrl` overloads (`GatherBuilder.cs:157-202`, verified).
  Matrix B.3 codifies "gather reads through `TypedSource` like everything else."

This is the canonical Value-slice row; the design delivers exactly the case it
was built for. **Without those two redesign decisions this same enhancement would
be WEAK** (rippling into the hand-mirror + GatherBuilder) — so the PASS is earned.

### 3. Third component vendor — PASS for the runtime; the rest is intended cost

**Verified — the runtime is genuinely vendor-isolated.** The ONLY vendor `===`
comparison in the entire runtime is `lifecycle/component-merge.ts:69`
(`existing.vendor === incoming.vendor`), which is the same-vendor *merge
invariant*, not a behavioral branch. `runtime-plan.ts:166` reads
`this.definition.vendor` as a **data-driven driver lookup**, not a branch. The
vendor seam is `ComponentRuntimeDriver { resolveRoot; wireEvent }`
(`domain/component-runtime.ts:9`); read/set/call in `runtime-object.ts` are
contract-path-driven and untouched. **`plan.ts:80` declares `Vendor = string`
(open), and C# `ComponentVendor.From` accepts any `^[a-zA-Z][a-zA-Z0-9_-]*$`
token** (`PlanTerms.cs:273`) — `Native`/`Fusion` are convenience constants, not a
closed enum. So neither the shared TS type nor the shared C# core needs an edit.

**Two honest qualifications the trace surfaced, both verified:**

- The driver registration is **EDITED, not append-only.** The two
  `registerComponentRuntime("native"/"fusion")` calls plus their driver literals
  sit inline at the bottom of `component-runtime.ts:94-110`. A third vendor adds a
  driver literal + import + register call *inside* that file. 00-design §5's "one
  `ComponentDriver` registration" phrasing implies dropping a file; it is a seam
  edit. Minor.
- The slogan understates the full job. A real vendor is also a whole NuGet package
  (an `XComponent : IComponent` base + N vertical-slice components) plus build
  plumbing (`build:{vendor}`, a vite config, a `.csproj`, targets wiring,
  `Program.cs`/layout script loading). That is **by design** (package-per-vendor,
  vertical slices), not coupling — but a reader of §5's last paragraph would
  under-estimate it.

**The one genuine leak risk (not triggered by today's two vendors):** the
`resolveRoot(element) => root` signature and `RuntimeObject`'s synchronous
path-based read/set/call assume "one DOM element → one resolvable root with
member paths." A vendor that mounts in shadow DOM, keys a separate JS registry by
something other than the element id, or exposes members via async getters would
not fit the two-method driver. Both current vendors fit; a genuinely different
vendor architecture is the scenario that could push knowledge beyond the seam.

### 4. `Focus()` / `ScrollIntoView()` — PASS; the probe's own framing was wrong

**Verified.** The probe framed this as "new primitive + Kind discriminator + one
runtime handler." Source says otherwise: `executeCall` (`execution/execute.ts:175`)
switches only on `reaction.on.kind` (component/plugin/payload — the *source*),
never on the method name; it calls `plan.objectForSource(reaction.on).call(reaction.method, args)`
generically. `RuntimePath.call → callMember` ends in `fn.apply(member.owner, args)`
(`domain/runtime-path.ts:122-129`). So `focus` / `scrollIntoView` resolve through
the existing `call` path with **zero new switch case, no new Kind, no
`assertNever` edit, no new TS interface** (CallReaction already carries `method:
string` + `args: ValueExpression[]`). This is the `toggle-class` worked example in
00-design §5 verbatim ("or none, if reusing `call`").

**The one real friction, verified:** `ElementBuilder.Call(ComponentMethod method,
ValueExpression arg)` (`Builders/ElementBuilder.cs:146`) is hardcoded to a single
arg and wraps it as `new List<ValueExpression> { arg }` (lines 151-154). A no-arg
verb needs a `Call(ComponentMethod)` overload passing an empty list. The
underlying `ReactionGraph.Call` already takes `IReadOnlyList<ValueExpression>`, so
it is a trivial same-file addition — but a dev must notice the helper signature.
Mechanical.

> **Honest design-weakness caveat (not triggered here).** A reaction that is
> *genuinely* a new kind — e.g. a `delay`/`debounce` async opener with new lane
> semantics — *would* touch Reaction node + Kind discriminator + a real
> `executeReaction` switch case + `assertNever` + `ReactionLane` stamping. That
> stays localized to Reaction (+ Kind) per the dependency graph, but it is
> `some-design`, not mechanical. Focus/ScrollIntoView simply are not that case.

### 5. ProblemDetails auto-routing — WEAK (the design's true soft spot)

**Verified — the field-placement half is already built and needs ~nothing.**
`validation/orchestrator.ts:147 showServerErrors(plan, containerKey, data)`
already parses `{errors:{field:[msg]}}` (`serverValidationErrorsFrom`:484), maps
each `serverFieldName` to its `ComponentValidation` (:460), and places inline when
`canRenderInlineValidationMessage` (:343) else into the `{planId}_validation_summary`.
RFC-7807 needs **zero** change: `responseContentKind` already classifies
`application/problem+json` via `mediaType.endsWith("+json")` → `"json"`
(`execution/http.ts:233`), so the body parses for free, and the sibling
`type`/`title`/`status` keys are ignored (only `errors` is read).

**But that is the wrong half.** The genuinely new work is an authoring DEFAULT,
and three couplings make a clean implementation cross-cutting — all verified:

1. **The feature is the ABSENCE of a DSL verb producing a node.** Today the
   `show-validation-errors` reaction exists *only* when the dev writes
   `.OnError(...).ValidationErrors(formId)` (matrix A4; `PipelineBuilder.cs:263`).
   `routeError → routeResponseRoutes` returns silently when the error-route list
   is empty (`http.ts:269`, verified). To auto-fire, the *absence* of an `OnError`
   must synthesize a `show-validation-errors` node. **Every matrix row is
   explicit-verb→node determinism** (B.5: "match = `any` unless a status is given;
   first match wins"). "Implicit default when the dev says nothing" is a different
   axis than the matrix's entire generator spec — the determinism proof gives no
   template for it. This is the structural mismatch, not a localization miss.

2. **"Which container?" has no plan-carried answer.** `ValidationErrors(formId)`
   carries the container id explicitly *because the request does not otherwise
   know its form*. Auto-mode must invent a request→container edge or a
   page-default container — a new dependency between **Request** and
   **Component/Plan** that the acyclic graph (00-design §2) does not currently
   have, and that is genuinely ambiguous for a two-form page.

3. **The clean fix is a full new primitive; the cheap fix is a banned fallback.**
   To stay legal under "the plan carries all behavior; the runtime synthesizes
   nothing," the auto-decision must lift into C# as a real default-error-route
   node — which ripples author surface (Request) + plan node (Reaction) + generated
   TS (Kind) + runtime reader, i.e. the entire new-primitive checklist, for what
   feels like a convenience default. The tempting shortcut ("if no route matched
   and the body looks like ProblemDetails, guess the container and show errors")
   is exactly the speculative-recovery fallback the architecture forbids
   (CLAUDE.md Rule 6; 00-design §1 non-negotiables).

A dev who opens the **Validation** module (where the matrix files this feature)
finds `showServerErrors` done and would wrongly conclude it is one-module — then
hit the authoring-default wall. **A dev would not succeed cleanly without a
design decision first.**

---

## Weak Spots & Coupling Risks

### WEAK SPOT 1 (primary) — No "implicit default from absence" axis in the matrix

- **Where.** Request / Reaction authoring vs the determinism matrix
  ([`04-matrix-validation-components-slots.md`](./04-matrix-validation-components-slots.md)
  A4 + [`04-matrix-http-arrays-values.md`](./04-matrix-http-arrays-values.md) B.5).
- **Why it couples.** The matrix's strongest property — *one DSL verb lowers to
  one node, generation is mechanical* — is defined entirely over **present**
  verbs. Any future "do X automatically when the developer configures nothing"
  feature (ProblemDetails auto-routing is the first, but error toasts on any 5xx,
  default loaders, default-confirm-on-destructive-verb are the same shape) has no
  row template and forces a new author→node→contract→runtime ripple plus a new
  cross-module edge to answer "applied to *what*?". The redesign's own matrix
  self-flags an adjacent instance (validation gap #3: `WhenField`-vs-server-`When`
  is "deterministic by a throw, not by an unrepresentable state"), which is the
  same "the design relies on an authoring-time decision the type system does not
  carry" pattern.
- **Severity.** Medium. It does not threaten the 120 *present-verb* feature
  families (all genuinely PASS). It is a missing **axis**, surfaced now on paper —
  cheaper to decide than to discover mid-build.

### WEAK SPOT 2 (latent) — `resolveRoot`'s synchronous-single-root vendor assumption

- **Where.** `Component` runtime seam — `ComponentRuntimeDriver`
  (`domain/component-runtime.ts:9`) + `RuntimeObject`'s synchronous path read/set/call.
- **Why it couples.** The two-method driver assumes every vendor is "one DOM
  element → one synchronously-resolvable root addressed by member paths." Both
  shipping vendors (native = the element, Fusion = `ej2_instances[0]`) fit. A
  vendor with shadow-DOM mounts, an external id-keyed registry, or async member
  getters would not fit the `resolveRoot(element) => root` signature, leaking
  vendor knowledge past the seam into `RuntimeObject`. The "third vendor touches
  one file" slogan holds only for vendors that match the existing shape.
- **Severity.** Low / contingent. No current trigger; flag it so the *first*
  divergent vendor is recognized as a seam-redesign, not a slice.

### NON-WEAK (recorded so they are not mistaken for coupling)

- **gather-name-default leaf-vs-path** and **`ElementBuilder.Call` single-arg** are
  trivial same-file frictions, not cross-module couplings. They cost a moment of
  attention, not a rip-apart.
- **The vendor NuGet package + build plumbing** is the deliberate
  package-per-vendor vertical-slice cost, explicitly chosen — not a design defect.

---

## Bottom Line — Safe to Build On

**The redesign delivers its core promise.** For the canonical extensibility cases
— a new value source, a new element reaction verb, a third vendor, an authoring
sugar overload — the work is **one module, mechanical or near-mechanical, and a
dev would succeed**, verified against source. Two specific redesign decisions do
the load-bearing work and were proven (not asserted): the **reflecting Kind kernel
+ drift gate** (deletes the 1,165-line hand-mirror that today is the heaviest edit
for any node change) and the **closed gather-source hole** (a new source reaches
gather without re-authoring `GatherBuilder` overloads). Strip either and the
localStorage case alone regresses from PASS to WEAK — the cohesion is real, not
nominal.

**The one weak spot is honest and worth fixing before building, but it is not a
boundary defect that blocks the build.** ProblemDetails auto-routing is WEAK
because it is an *implicit default from the absence of a DSL verb* — a category
the matrix's "one present verb → one node" generator spec never covered, not a
case where a clean module boundary was drawn in the wrong place. The recommended
action is a **deliberate design decision, not a code patch**: either (a) add an
explicit author verb (e.g. `.AutoShowValidationErrors()` or a plan-level default)
that lowers to the *existing* `show-validation-errors` node and carries the
container link explicitly — keeping the plan the source of truth and reusing the
done runtime half — or (b) consciously decline auto-routing and keep
`ValidationErrors(formId)` explicit. **What must NOT happen** is the runtime
"guess the container and show errors when no route matched" shortcut — that is the
speculative-recovery fallback the architecture forbids, and choosing it would
quietly reintroduce exactly the class of debt the redesign was built to remove.

**Recommendation: build on this design. Add one matrix sub-section that names the
"implicit-default / absence-of-verb" axis and the rule that such features must
lower to a real plan node via an explicit (even if defaulted) author surface —
never via runtime inference. That single boundary note converts the one WEAK case
into a mechanical one and inoculates the next four features of the same shape.**
