# Domain Model Cohesion Recommendations

The Alis.Reactive C# plan domain is already in strong shape on the two fronts CLAUDE.md names as
weaknesses: the serialization-null debt is essentially absent (exactly two `[JsonIgnore(WhenWritingNull)]`
remain, both individually justified per Rule 6), and the polymorphic discriminated-union pattern
(`Source`, `RequestInput`, `InputBinding`, `ParallelCompletion`, ...) is applied consistently with
sealed `Empty`/`None` sentinels instead of null. So this is not a rescue. The surgical opportunities
that remain are about closing small asymmetries — a determinism gap where half the reaction domain
fails fast and half does not, byte-stable plan output that holds today by `Dictionary` luck rather than
by stated invariant, a hand-written TS contract that calls itself "generated" with no guard linking it
to the C# discriminators, and a handful of folder/naming moves that make each DSL area's types live in
one place a contributor can open. Every recommendation below serves cohesion, determinism, or
one-concept-one-name without touching the public DSL surface or the plan JSON wire shape (the two
flagged exceptions carry the standard regenerate-TS gate). All file:line claims were verified against
the working tree before being written here.

---

> ### Guardrails — OUT OF BOUNDS (per root CLAUDE.md)
> No recommendation in this document proposes, and no future reader should propose, any of the following:
> - **No JSON schema revival.** Schema is retired as the plan contract. The C# plan domain plus the
>   generated TypeScript types are the contract. Do not add `AssertSchemaValid`, schema drift gates, or a schema-first process.
> - **No fallbacks / registries / generated-plan validators / claims / rejects / speculative recovery**
>   inside the generated plan graph. The runtime is a dumb executor; invalid behavior is made
>   unrepresentable in the C# PlanModel. Runtime checks are for true external boundaries only.
> - **No shared base classes for *behavior* across vertical slices.** Duplication between slices is
>   intentional (Rule 4). Shared *stateless serialization mechanics* are a different category and are allowed.
> - **No wrappers or types that only carry parameters or hide a branch.** If a type maps to no DSL graph
>   node, requires explanation before its value is obvious, or duplicates a discriminant, delete or inline it.
> - **No public-API surface changes** without explicit user approval and a downstream audit. All plan
>   model constructors stay `internal`; properties stay `internal set`. One item below (IdGenerator) is a
>   public-API touch and is flagged as approval-gated, not autonomous.
> - **Sentinel objects over null** is the direction of travel. Do not regress a justified sentinel into a
>   nullable field, and do not mechanically "tidy" a justified `[JsonIgnore(WhenWritingNull)]` into a sentinel.

---

## Theme 1 — Domain model & value objects

### 1.1 Close the determinism gap in the ReactionGraph leaf-node constructors

- **Current.** Every `ReactionGraph` leaf assigns constructor args directly with no guard:
  `SetReaction` (`ReactionGraph.cs:289-294` — `On = on; ... Value = value;`), `CallReaction`
  (`:312-317`), `RequestReaction` (`:336`), `InjectReaction` (`:475-479`), `BranchCase` (`:166-170`),
  `ParallelReaction` (`:88-92`), `SettledParallelCompletion` (`:124-127`). By contrast every sibling
  family null-guards rigorously — `ValueExpression` subclasses (`ValueExpression.cs:282-286, 502-506,
  523-527`), `ComparisonOperands`/`ValueRead`, and the aggregate `Behavior` itself
  (`Behavior.cs:11-12`, `?? throw new ArgumentNullException`). One half of the reaction domain fails
  fast; the other half does not.
- **Proposed.** Add the same `?? throw new ArgumentNullException(nameof(x))` guard already used by the
  value side to the seven reaction constructors. Before: `On = on;` After: `On = on ?? throw new
  ArgumentNullException(nameof(on));`. Keep the existing `ParallelCompletion.None` sentinel so "no
  completion" stays a real object, never null. No new types, no API change, no JSON change — these
  types are constructed only through internal factory methods.
- **Why.** Today an internal builder mistake that passes null serializes a malformed plan that only
  explodes in the browser — the layer that is meant to be a dumb executor. Guarding makes invalid plan
  state unrepresentable at the C# producer boundary, and removes the read-asymmetry that makes the
  reaction domain harder to scan than the value domain.
- **CLAUDE.md alignment.** Rule 6 ("Invalid behavior belongs in the C# PlanModel where it can be made
  unrepresentable") and the DDD-depth aspiration. This is a constructor invariant on the producer side,
  the sanctioned place for fail-fast — not a forbidden generated-plan validator.
- **Effort.** S. **Risk.** Low.

### 1.2 Model `RegisteredInputSelection` as a discriminated-union sentinel pair

- **Current.** `RegisteredInputSelection` (`GatherRequestInput.cs:37-53`) is the one choice type in
  scope that is a single concrete class carrying its discriminant twice: a stringly `public string Kind`
  (`"explicit"` | `"all-registered-inputs"`) *and* a redundant `internal bool SelectsRegisteredInputs`,
  two representations of one fact. Its own generated TS contract already models it correctly as a
  two-case union of empty marker interfaces (`PlanTypeScriptContract.cs:482-491`). The only consumer
  reads `RegisteredInputs.SelectsRegisteredInputs`.
- **Proposed.** Replace the single class with the pattern used by `RequestInput`/`InputBinding`/
  `ParallelCompletion`: an abstract `RegisteredInputSelection` with two sealed sentinels
  `ExplicitAssignments` and `AllRegisteredInputs`, each exposing only `Kind`. Move the branch decision
  onto the type with an abstract member (e.g. `internal abstract bool IncludesRegisteredInputs`,
  overridden `false`/`true`), deleting the standalone bool so the discriminant exists once. Run the
  standard gate: regenerate `runtime/types/plan.ts` and `npm run typecheck` — the TS union shape is
  unchanged, so it should be a no-op there, which is the point.
- **Why.** One-concept-one-name: removes a type that stores the same decision in two fields and brings
  the last outlier into the repo's uniform union vocabulary. Determinism payoff: the bool can never
  disagree with the `Kind` string because there is no longer a second field to drift.
- **CLAUDE.md alignment.** Plan Contract ("each concrete plan model class carries its own `kind`
  property which becomes the discriminator") and the rich-domain rule "if a type ... hides a branch,
  delete it or inline it" — here the bool is a hidden branch duplicating the discriminant.
- **Effort.** S. **Risk.** Low.

### 1.3 Rename `Behavior` / `BehaviorGraph` to scream the reaction-graph vocabulary

- **Current.** The aggregate pairing a trigger with its reaction is named `Behavior` (`Behavior.cs:5`)
  holding `StartsWhen StartsWhen` and `ReactionGraph Reaction`, collected by `BehaviorGraph`
  (`BehaviorGraph.cs:6`) and serialized as `PlanDocument.Behaviors`. The domain's screaming terms are
  `ReactionGraph`, `ConditionGraph`, `BranchGuard`, `ParallelCompletion` — yet `Behavior`/`BehaviorGraph`
  are generic OO words (every object has behavior) and `StartsWhen` reads as a sentence fragment, not a
  domain noun. The atlas describes this node as "trigger -> pipeline -> reaction" but the type names do
  not say "reaction".
- **Proposed.** Rename the trigger/reaction pair and its collection to nouns that match the graph
  vocabulary — e.g. `Behavior` -> `ReactionRule`, `BehaviorGraph` -> `ReactionRules`. Both are
  `internal`, so this is a pure rename with no API or JSON impact (`PlanDocument.Behaviors` is
  internal). Record the chosen noun once in `reactive-plan-domain-language.md` so it is the single name
  across docs, tests, and code.
- **Why.** A reader scanning PlanModel then sees `ReactionGraph` + `ConditionGraph` + `ReactionRule` and
  immediately knows the trigger->reaction shape, instead of decoding the generic "Behavior". Aligns the
  aggregate name with the value/condition/reaction spine the atlas calls load-bearing.
- **CLAUDE.md alignment.** DDD-depth aspiration ("screaming names ... are underused") and the Operating
  Standard requirement to update the domain-language artifact when terms change.
- **Effort.** S. **Risk.** Low. *(Tradeoff: a rename touches every internal reference site; do it as one
  mechanical commit with the doc update, not interleaved with logic changes.)*

### 1.4 Make the SSE-trigger payload-type readable through one accessor

- **Current.** The payload-typing fact for server-push is threaded across three layers:
  `StartsWhen.ServerPush(url)` / `(url,event)` / `(url,event,payloadType)` (`StartsWhen.cs:21-28`) hand
  a `PayloadContract` to `ServerPushEventFilter.AnyEvent`/`NamedEvent` (`StartsWhen.cs:124-131`), and
  `NamedServerPushEvent` (`StartsWhen.cs:141-152`) stores the event name while the contract lives on the
  base (`StartsWhen.cs:114-122`). A reader must trace the three-overload ladder to see where the
  contract lands. This is the atlas's "documenting the same source three times in three areas" shape.
- **Proposed.** Do **not** merge the trigger families — the discriminated union page-ready /
  document-event / component-event / server-push / signalr separation is correct and must stay. Instead
  make `PayloadContract` ownership single: keep it on `ServerPushEventFilter` only and have
  `ServerPushTrigger` read through `EventFilter.PayloadType` everywhere a payload type is needed; verify
  `DocumentEventTrigger`/`SignalRTrigger` expose `PayloadType` via the same accessor name so the four
  payload-carrying triggers share one read path. A small consolidation of accessors, not a new
  abstraction. No JSON change.
- **Why.** Makes "what type is this trigger payload" answerable from one place per trigger instead of
  two, and shrinks the three-overload ladder a reader must follow.
- **CLAUDE.md alignment.** The ValueExpression lesson ("one domain concept reads all values ... use the
  shared value path instead of creating a second resolver"). Explicitly avoids the forbidden over-merge:
  the trigger union stays intact; only the payload-type accessor is unified.
- **Effort.** M. **Risk.** Med. *(Tradeoff: this touches the trigger hierarchy, the highest-traffic
  authoring surface — sequence it last in its theme and prove with the existing trigger Playwright slices.)*

---

## Theme 2 — Serializer

> The single polymorphic converter (`WriteOnlyPolymorphicConverter<T>`) is exactly the simple,
> deterministic mechanism the architecture wants — it stays. The serialization debt is duplication and
> hand-written-envelope drift, not a null-contract problem.

### 2.1 Collapse the 8 byte-identical `WriteProperty<T>` helpers into one shared writer

- **Current.** The same 3-line helper is copy-pasted verbatim in 8 places — `ConditionGraph.cs:85` and
  `:202`, `ReactionGraph.cs:198, :264, :377, :452`, `ComponentObject.cs:564` and `:687`. Half are
  `private static`, half `internal static`, differing only by where the copy was needed.
- **Proposed.** Add one `internal static class PlanJsonWriter` in `Serialization/` (next to
  `WriteOnlyPolymorphicConverter.cs`) exposing
  `WriteProperty<T>(this Utf8JsonWriter writer, JsonSerializerOptions options, string name, T value)`.
  Delete all 8 copies; each call site becomes `writer.WriteProperty(options, "left", value.Left)`.
  Identical bytes emitted — no wire-format change. One ~8-line file, 8 deletions, ~20 compiler-verified
  call-site rewrites.
- **Why.** Removes ~64 lines of exact duplication; the property-writing convention (name, then
  serialize-with-options) is stated once and cannot drift between converters. A converter body shrinks
  to its real content.
- **CLAUDE.md alignment.** Rule 11 (no duplication, small single-responsibility methods) and Rule 9
  (scout rule). This is **not** a forbidden behavior base class (Rule 4): it is stateless serialization
  mechanics, the same category as the already-shared `WriteOnlyPolymorphicConverter<T>`; it carries no
  domain branch.
- **Effort.** S. **Risk.** Low.

### 2.2 Let the node class be the single source of its wire shape; keep only genuinely-custom converters

- **Current.** Several bespoke converters re-implement the identical envelope `WriteStartObject();
  WriteString("kind", value.Kind); <write each property by hand>; WriteEndObject()` — e.g.
  `DispatchReactionJsonConverter` (`ReactionGraph.cs:360-369`), `CompareConditionJsonConverter`
  (`ConditionGraph.cs`), `ShapeJsonConverter` (`Shape.cs`). For nodes whose only need is "kind first
  then list my own public properties," the converter exists only to restate properties that already
  exist as public auto-properties on the class. To know a node's emitted JSON you must read its
  converter, and adding a property silently does nothing unless you also edit the converter — a real
  drift trap.
- **Proposed.** For nodes whose only custom need is "kind first," drop the bespoke converter and rely on
  the existing per-class `public string Kind => "..."` plus `WriteOnlyPolymorphicConverter<T>` on the
  base, which already serializes the concrete type's public properties. Where kind-first ordering is
  contractually required, add it **once** to `WriteOnlyPolymorphicConverter` (or a thin
  `KindFirstPolymorphicConverter`) rather than per node. Keep **only** the genuinely custom converters —
  those that conditionally omit an absent payload (see 2.3). Net: ~4-6 converter classes deleted; the
  node class becomes the single source of truth for its wire shape.
- **Why.** The emitted JSON for a node becomes fully described by reading the node class, not by
  cross-referencing a separate `Write()`. Eliminates the add-a-property-but-forget-the-converter drift
  class entirely.
- **CLAUDE.md alignment.** Plan Contract section ("each concrete plan model class carries its own `kind`
  property which becomes the discriminator") — this makes the code do exactly that with one mechanism.
  Rule 6, Rule 11. Does not revive JSON schema.
- **Effort.** M. **Risk.** Med. *(Tradeoff: property emission order changes from explicit to
  reflection-derived for the affected nodes; pair this with recommendation 4.3's byte-stability test so
  any reordering is caught, and verify the runtime discriminated unions do not depend on field order.)*

### 2.3 Unify the "absent payload" sentinel mechanics under one `WriteBody` no-op default

- **Current.** Three sibling families each carry an "absent" concrete type whose entire job is to write
  nothing, but they spell it three ways: `AbsentComparisonRightOperand` (`ConditionGraph.cs:170`, empty
  `WritePayload`), `NoDispatchPayload` (`ReactionGraph.cs:411`, empty `WritePayload`),
  `DefaultBranchGuard` (`ReactionGraph.cs:227`, empty `WriteGuardPayload`). Two different empty-method
  names (`WritePayload` vs `WriteGuardPayload`) describe the same idea.
- **Proposed.** Keep the sentinel-object design (it is the correct null-free model — do **not** regress
  to nullable fields). Unify only the mechanics: have each base expose one virtual
  `WriteBody(Utf8JsonWriter, JsonSerializerOptions)` with a default empty implementation, so the
  absent/none/default leaf inherits the no-op instead of declaring an empty override; the
  present/conditional leaf overrides it. Deletes the 3 empty-method declarations and standardizes the
  name across the three families.
- **Why.** Preserves the deterministic, null-free wire contract (absence is a real `kind`, never a
  missing field) while removing boilerplate. One method name makes the "optional payload" pattern
  recognizable instead of three look-alikes.
- **CLAUDE.md alignment.** DDD-depth ("Value Objects ... instead of null") and Rule 6 (sentinel over
  null) — strengthens, not weakens, the sentinels. Rule 11. Not a behavior base class: the shared member
  is a serialization no-op default carrying no domain logic.
- **Effort.** S. **Risk.** Low.

### 2.4 Leave the two `ArrayOperationExpression` `[JsonIgnore(WhenWritingNull)]` attributes in place

- **Current.** `ValueExpression.cs:555` (`Predicate`) and `:563` (`Projection`) are the only two
  `[JsonIgnore(Condition = WhenWritingNull)]` in the entire PlanModel. Both carry a written per-property
  justification in their `<remarks>` ("count/map carry no predicate while filter/any/all/find do; an
  always-true sentinel would conflate no-predicate with match-all"), and the generated TS already
  declares them `.Optional(...)` at `PlanTypeScriptContract.cs:675-676`, so C# omission and the TS
  optional field already agree.
- **Proposed.** Take no action on these two. Record (in `reactive-plan-domain-design.md` serialization
  notes) that they are the audited, justified exceptions so a future serialization pass does not "tidy"
  them into a sentinel and break the count/map-vs-filter distinction.
- **Why.** Prevents a regression disguised as cleanup. The repo's own
  `feedback_null_escape_hatch_blindness.md` records a prior incident of mechanically moving null markers
  during a refactor; the inverse (mechanically removing a justified one) is the same failure. These two
  are already proven necessary and already matched at the TS boundary — the determinism work is done.
- **CLAUDE.md alignment.** Rule 6 ("null escape hatches require justification" — here the justification
  exists and is sound). The serialization aspiration scopes the problem to attributes "scattered as a
  pattern"; two individually-justified, TS-matched attributes are not that pattern.
- **Effort.** S (documentation only). **Risk.** Low.

---

## Theme 3 — Micro-modules (project + namespace structure)

> The six-assembly split is already correct by dependency direction (core does not reference
> FluentValidation; the validator is a thin inward adapter; Native/Fusion are vendor slices). Do **not**
> merge assemblies. The debt is *inside* the core assembly's flat 25-file `PlanModel/` namespace. Every
> item below is a folder/namespace move or rename — no behavior change, no API change, no new type.
> Namespace stays `Alis.Reactive.PlanModel` in each case, so there is zero `using` churn.

### 3.1 Split the Validation plan model out of `ComponentObject.cs`

- **Current.** `ComponentObject.cs` is 697 lines. Lines 9-208 are genuine Components-area types
  (`ComponentObject`, `ComponentRole`, `InputBinding`, ...). Lines 251-690 are the entire Validation
  plan model: `ValidationContainerBinding` (`:251`), `ScopedValidationContainer` (`:274`),
  `ContainerValidations` (`:316`), `ComponentValidation` (`:383`), `ValidationRule` (`:451`) +
  `ValidationRuleJsonConverter` (`:472`), `ValidationRuleExecution` (`:501`), `ValidationRuleActivation`
  (`:632`) + converter (`:669`). A file named for the component object silently owns the validation
  vocabulary.
- **Proposed.** Keep `ComponentObject.cs` as the Components plan model (lines 9-208 + `ComponentRole`);
  move the validation-binding/rule types and their two converters into `PlanModel/Validation/`
  (`ComponentValidationPlan.cs`, or split into `ValidationContainerBinding.cs` + `ValidationRulePlan.cs`).
- **Why.** The Validation DSL area's plan types stop hiding inside a Components file; the largest
  PlanModel file drops to ~210 lines and its name matches its contents; the rule converters sit next to
  the types they serialize.
- **CLAUDE.md alignment.** DDD-depth + screaming names; Rule 11 (single responsibility). Pure colocation
  — no new type, no abstraction.
- **Effort.** M. **Risk.** Low.

### 3.2 Dissolve the `PlanTerms.cs` junk-drawer into per-area value-object files

- **Current.** `PlanTerms.cs` (603 lines) holds ~30 unrelated value objects under one meaningless name:
  plan identity (`PlanId`/`PlanScope`/...), component identity (`ComponentKey`/`MemberName`/
  `ComponentVendor`/...), HTTP (`RequestUrl`/`HeaderName`/`HttpMethodName`/...), payload
  (`PayloadContract` family), and conditions (`CompareOperator`). The atlas calls the `PlanString`
  family "the quiet contract spine" — yet it is pooled in a file whose name describes nothing.
- **Proposed.** Keep the `PlanString` base + `PlanId`/`PlanIdentity`/`PlanScope` in
  `PlanModel/PlanIdentity.cs` (the genuine plan-wide spine). Move the rest beside the type that owns
  their area: component-identity terms next to `ComponentObject.cs` (or a `ComponentIdentity.cs`);
  HTTP terms next to `RequestPlan.cs`; the `PayloadContract` family next to `Source.cs`/`StartsWhen.cs`
  which already use them; `CompareOperator` next to `ConditionGraph.cs`/`CompareOp.cs`.
- **Why.** Each value object lives beside its consumer, so an HTTP-header change touches only the HTTP
  files; `PlanTerms` is replaced by names that scream their area; invariants stay adjacent to where they
  are minted.
- **CLAUDE.md alignment.** DDD-depth (screaming names, value objects with invariants) and Rule 4
  locality. No wrapper, no registry, no behavior change — existing value objects relocated. The file,
  not the type, required the explanation.
- **Effort.** M. **Risk.** Low.

### 3.3 Move `PlanTypeScriptContract.cs` out of `PlanModel/` to a contract folder

- **Current.** `PlanTypeScriptContract.cs` (1,165 lines — largest file in the plan domain) is the
  authoritative TypeScript-contract emitter and the only entry point of `tools/PlanTypeGenerator`
  (`Program.cs:23` calls `Render()`). The architecture draws an explicit Layer 2 -> boundary -> Layer 3
  line; `PlanModel/` is Layer 2, but this file *is* the Layer-3 boundary artifact, physically nested
  inside Layer 2, so the boundary is invisible in the folder tree.
- **Proposed.** Move it to `Alis.Reactive/PlanContract/PlanTypeScriptContract.cs` (folder named for the
  boundary it owns), and optionally split the `Render()` body into area partials
  (`PlanContract.Components.cs`, `PlanContract.Http.cs`, `PlanContract.Validation.cs`) mirroring the
  structure it generates. Namespace may stay `Alis.Reactive.PlanModel` to avoid churn, or become
  `Alis.Reactive.PlanContract`; `tools/PlanTypeGenerator/Program.cs:23` is the only caller to update if
  the namespace changes.
- **Why.** The C#-domain -> TS-contract boundary becomes a folder you can see; the 1,165-line monolith
  split into area partials lets a Components-contract change be found where the Components area is
  generated.
- **CLAUDE.md alignment.** Serves the core architecture rule by making the third arrow a distinct
  module. Does **not** revive JSON schema — this is the generated-TS path CLAUDE.md endorses. File move +
  optional partial split; Rule 11, Rule 9.
- **Effort.** M. **Risk.** Low.

### 3.4 Delete the empty `Descriptors/Requests/` directory

- **Current.** `Alis.Reactive/Descriptors/` contains only one empty subdirectory `Descriptors/Requests/`
  (`find ... -type f` returns nothing). Request descriptors actually live in `Builders/Requests/` and
  the request plan model in `PlanModel/RequestPlan.cs` + `GatherRequestInput.cs`.
- **Proposed.** `git rm -r` the empty `Descriptors/` tree. No code references it.
- **Why.** Removes a misleading empty namespace candidate that suggests a "Descriptors" module exists.
- **CLAUDE.md alignment.** Rule 11 ("no dead code — git has history") applies to directories too; delete
  a structure that maps to no DSL graph node. Matches task #18 (tooling cleanup).
- **Effort.** S. **Risk.** Low.

### 3.5 Gather the loose component-registration files into a `Components/` folder

- **Current.** Nine cohesive registration files sit loose in the core root next to plan-wide entry
  points: `ComponentRef.cs`, `ComponentMember.cs`, `ComponentRegistration.cs`, `IComponent.cs`,
  `RegisteredComponentIdentity.cs`, `RegisteredInputBinding.cs`, `RegisteredInputComponents.cs`,
  `InputComponentRegistrationProfile.cs`, `ClientValidationRuleBinder.cs`. All declare
  `namespace Alis.Reactive` and together form the component/input registration subsystem, but no folder
  groups them.
- **Proposed.** Move the eight registration files into `Alis.Reactive/Components/`, and put
  `ClientValidationRuleBinder.cs` (its only method is `BindQueuedJobs`) into `Validation/`. Leave true
  plan-entry files (`ReactivePlan`, `ReactivePlugin`, `ResponseBody`, `TypedEvent`, `IdGenerator`,
  `ExpressionPathHelper`) in the root. Namespace stays `Alis.Reactive`.
- **Why.** Component registration becomes a folder you can open; the root shrinks to genuine top-level
  concepts; adding a slice's registration touches one obvious folder.
- **CLAUDE.md alignment.** Rule 4 locality, Rule 5 vendor-isolation (registration is the
  vendor-agnostic join layer). File moves only.
- **Effort.** S. **Risk.** Low.

---

## Theme 4 — Determinism

> Plan build is already strongly deterministic: IDs derive from `Type.FullName` + member chain in
> InvariantCulture; literals normalize `DateTime` via round-trip `"O"`; no `Guid.NewGuid`,
> `DateTime.Now`, or culture-sensitive casing in the build path; reactions and array items are ordered
> `List<>`. The one real gap is the object-shaped surfaces.

### 4.1 Pin ordinal key order at every dictionary snapshot boundary

- **Current.** Four object-shaped surfaces serialize a plain `Dictionary<string,T>` and rely on
  undocumented insertion-order enumeration: `BrowserObjectContracts.Snapshot()` returns
  `new Dictionary<string, BrowserObjectContract>(_byTypeKey)` (`BrowserObjectContracts.cs:10-11` — note
  the return *type* is `IReadOnlyDictionary` but the backing instance is a plain `Dictionary`, so wire
  order is that `Dictionary`'s enumeration order); `ComponentObjects.Snapshot()` likewise
  (`ComponentObjects.cs:21-22`); `BrowserObjectContract` exposes plain `_properties`/`_methods`/`_events`
  (`BrowserObjectContract.cs:8-14`); `ObjectExpression.Fields` is the `Dictionary` built in
  `ValueExpression.ObjectFields` (`ValueExpression.cs:177-187, :498`). No `.Remove(` exists in the build
  path, so order is insertion-order *today* — but .NET documents that as non-contractual. The repo
  already uses `StringComparer.Ordinal` for the same kind of map in `RegisteredInputComponents.cs:10`
  and `PlanTerms.cs:285, :315`.
- **Proposed.** At each `Snapshot()`/build boundary, project into an ordinally-sorted read-only map
  instead of copying the raw `Dictionary`: order keys by `StringComparer.Ordinal` (a small `OrderedByKey`
  projection helper, or initialize the exposed maps as already-ordered). For `ObjectExpression.Fields`,
  sort in `ValueExpression.ObjectFields` before constructing the shape/fields pair. Same DSL input then
  produces identical key ordering on every machine/runtime/.NET version.
- **Why.** The generated plan becomes byte-identical for identical DSL input as a *stated invariant* —
  diffable, snapshot-testable, cacheable, ETag-friendly — and removes a latent "reorders on their .NET"
  bug class. Ordering becomes one visible decision at the boundary instead of an implicit assumption
  spread across four files.
- **CLAUDE.md alignment.** "Invalid behavior belongs in the C# PlanModel where it can be made
  unrepresentable" / "the plan carries all info" — key order becomes a plan guarantee. **Not** a runtime
  fallback/validator: the change is entirely in C# build and reuses the comparer already chosen nearby.
- **Effort.** S. **Risk.** Low.

### 4.2 State the Components key-order rule (decouple emission order from registration timing)

- **Current.** Object targets are inserted into `_components` eagerly when a behavior references them
  (`ComponentObjects.cs:48-76`), while registered input components are inserted in a deferred pass over
  `_registrations.Entries` during `Render()` (`ComponentObjects.cs:118-122`, via
  `ReactivePlan.ResolveAll`). So a component's serialized position depends on whether it was first
  touched as a behavior target or as an input registration — a timing detail, not author intent.
- **Proposed.** This collapses automatically once 4.1 sorts the snapshot by ordinal component id (the
  re-order happens at `PlanBuildContext.BuildPlan`). Capture the rule — "Components and Types are emitted
  in ordinal id order" — in the snapshot code comment and in `reactive-plan-domain-language.md`, so the
  (correct) deferred-vs-eager *resolution* split is explicitly decoupled from *emission order*.
- **Why.** Separates two tangled concerns — *when* a component is resolved (legitimately deferred for
  input enrichment) vs. *what order* it appears in output (intent-stable). Removes a subtle reordering
  trap when DSL calls are moved around.
- **CLAUDE.md alignment.** "Bookkeeping names must describe what is remembered for execution ... not
  imply the plan is suspicious" and the determinism aspiration. Rides on 4.1 — zero new types or branches.
- **Effort.** S. **Risk.** Low.

### 4.3 Add a single byte-stability proof so the invariant cannot silently regress

- **Current.** No test asserts that `plan.Render()` is byte-identical across two builds of the same DSL,
  nor that key order is stable. Determinism rests on `Dictionary` enumeration order with no guard.
- **Proposed.** Add one focused C# domain test (Layer 1) that builds a representative plan (multiple
  components, an `ObjectExpression` with several fields, several object members) and asserts (a) two
  independent `Render()` calls return the identical string, and (b) the emitted `Components`/`Types`/
  `Fields` keys are in ordinal order. This is the failing-test-first evidence for 4.1/4.2 and the
  regression guard afterward — and it also catches any reordering introduced by recommendation 2.2.
- **Why.** Converts "deterministic by luck" into "deterministic by enforced contract"; any future change
  that reintroduces hash-order or timing-dependent emission fails at Layer 1 instead of as a flaky
  production diff. The test doubles as executable documentation of the rule.
- **CLAUDE.md alignment.** Rule 10 ("Tests Are Production Code — must prove behavior") and the Layer 1
  harness. Tests a real DSL-visible property (stable output), not an internal helper — not syntax-pinning
  debt.
- **Effort.** S. **Risk.** Low.

---

## Theme 5 — Extensibility & readability

### 5.1 Add a drift test linking the hand-written TS contract to the C# discriminators

- **Current.** `PlanTypeScriptContract.cs` (1,165 lines, ~202 `Declare()` calls, hand-written
  `Literal("set")`-style kind lines) restates every plan type's shape, kind literal, and union
  membership by hand. The C# concrete types own the real discriminators independently (69
  `public string Kind => "..."` across `PlanModel/*.cs`; e.g. `ReactionGraph.cs:281` `=> "set"`,
  `:304` `=> "call"`). Nothing links them: `tests/Alis.Reactive.DriftDetection.Tests` has **0**
  git-tracked `.cs` files (`git ls-files | grep -ic DriftDetection` = 0 — only stale `bin/` output). A
  typo'd discriminator or a primitive added to `ReactionGraph` but forgotten in the hand-written union
  compiles clean and fails only in the browser.
- **Proposed.** Revive the dead DriftDetection project with **one** behavior test (not a schema). For
  each abstract plan base decorated with `WriteOnlyPolymorphicConverter`, reflect its sealed subclasses,
  read each one's `Kind`, and assert (a) every `Kind` has a matching `Literal(...)` in the rendered
  `PlanTypeScriptContract` output and (b) every subclass name appears in that base's hand-written
  `Union`. Reuses the reflection the converter already does at runtime — same mechanism, now a build-time
  guard.
- **Why.** Today the only feedback that the hand-written contract drifted is a browser failure. The test
  makes "add a primitive" deterministic: forget the union entry and `dotnet test` fails at Layer 1 with
  the missing type name, before any TS or browser work. It makes step 4 of the 10-step checklist
  verifiable instead of hopeful.
- **CLAUDE.md alignment.** Serves "Generate TS types from C# plan domain" / forbids "Hand-maintain TS
  plan contract." A Layer-1 authoring guard over two C# artifacts — **not** a forbidden generated-plan
  validator (Rule 6) and **not** a revived JSON schema; it never inspects runtime plan JSON.
- **Effort.** M. **Risk.** Low.

### 5.2 Stop the "PlanTypeGenerator" name from claiming automation that does not exist

- **Current.** `tools/PlanTypeGenerator/Program.cs:23` is a 30-line shell whose body is
  `File.WriteAllText(fullPath, PlanTypeScriptContract.Render())`. CLAUDE.md and the atlas say the TS
  contract is "generated from the C# plan domain via PlanTypeGenerator" / "projects the same C# domain"
  — but `Render()` runs ~202 hand-coded `Declare()` statements; no plan type is reflected. The name and
  the verb describe automation that does not exist.
- **Proposed (two phases, pick by appetite).** Phase 1 (S, do first): if full reflection-generation is
  out of scope now, add a one-line class-doc — "This contract is authored by hand and guarded by the
  drift test in 5.1; it is not reflected from the domain" — or rename to `PlanTypeScriptContractAuthoring`
  so the next contributor is not misled. Phase 2 (L, optional later): drive the leaf interfaces from
  reflection (emit each subclass's `kind` literal and union membership from the C# `Kind` getters,
  leaving only genuinely TS-shaped aliases hand-declared). Recommendation 5.1 de-risks Phase 2 by proving
  equivalence across the cutover.
- **Why.** A 1,165-line file calling itself a generator is the highest-friction spot to read and extend
  here: a newcomer expects regeneration and is surprised to hand-edit three spots. Phase 1 removes the
  false mental model for S effort; Phase 2 removes the duplication.
- **CLAUDE.md alignment.** Do/Do-Not "Generate TS types from C# plan domain"; Rule 11 (a name must not
  lie about what the code does); Rule 9.
- **Effort.** S (Phase 1) / L (Phase 2). **Risk.** Low (Phase 1). *(Tradeoff: Phase 2 is a large,
  optional follow-up; do not start it before 5.1 exists.)*

### 5.3 Inline per-method component-descriptor boilerplate *within* a slice file

- **Current.** A slice such as `FusionComboBoxExtensions.cs` declares 6
  `private static readonly ComponentMethod X = ComponentMethod.Named("x")` fields plus a near-identical
  `... Method<TModel>(...) => self.EmitCall(XMethod);` for each. Each no-arg method is ~5 lines of
  ceremony around one `EmitCall`. The 3-constraint extension shape repeats ~397 times across the Fusion
  slices (`grep 'this ComponentRef<'` = 397). The signal (which JS method) is buried.
- **Proposed.** *Within a single slice file only*, define the descriptor inline so each method is one
  self-documenting line:
  `public static ComponentRef<FusionComboBox,TModel> ShowPopup<TModel>(this ComponentRef<FusionComboBox,TModel> self) where TModel:class => self.EmitCall(ComponentMethod.Named("showPopup"));`
  — dropping the separate `static readonly` field block for no-arg methods. Do **not** extract a shared
  base/helper across slices; keep the duplication between slices.
- **Why.** Reading a slice no longer means cross-referencing a top-of-file field block against the
  methods below; intent is on one line; onboarding a method copies one obvious line instead of two
  coupled edits.
- **CLAUDE.md alignment.** Rule 11 ("variables close to usage / if used once, inline it"). Explicitly
  respects Rule 4 by staying inside one file and introducing no shared behavior base — the within-slice
  tidy is the allowed kind.
- **Effort.** M (mechanical, many files). **Risk.** Low. *(Tradeoff: high file count — apply
  incrementally, one slice per commit, not as a single sweeping change.)*

### 5.4 Fix the ElementBuilder return-type inconsistency and the missing `SetHtml(ResponseBody)` overload

- **Current.** In `ElementBuilder.cs` most methods return `PipelineBuilder<TModel>` — `AddClass:31`,
  `SetText(string):55`, `SetText(ResponseBody):76`, `Show:124` — but the typed-source overloads return
  `ElementBuilder<TModel>`: `SetText<TProp>(TypedSource):87` and `SetHtml<TProp>(TypedSource):116`. So
  `.SetText("a")` and `.SetText(componentSource)` chain to *different* objects depending on the overload
  picked. Separately, `SetText` has a `ResponseBody` overload (`:76`) but `SetHtml` does **not**
  (`SetHtml` has only string / `TSource` / `TypedSource` forms) — an asymmetry, even though injecting an
  HTTP body as HTML is squarely the job this surface should support.
- **Proposed.** (1) Make the two `TypedSource` overloads return `PipelineBuilder<TModel>` like every
  sibling, so the chaining target is uniform regardless of overload. (2) Add
  `SetHtml<TResponse>(ResponseBody<TResponse> source, Expression<Func<TResponse,object>> path)` mirroring
  `SetText:76` (`Set(BrowserElementMembers.Html, ValueExpression.ReadPayload(source.Scope, responsePath))`).
  Write the matrix row first to confirm the original `SetHtml` omission was not deliberate.
- **Why.** Inconsistent return types are a readability trap — a contributor learns `.SetText(...)` chains
  back to the pipeline, then hits a different object on the typed-source overload. Symmetry between
  `SetText` and `SetHtml` means knowing one means knowing the other; the API becomes guessable.
- **CLAUDE.md alignment.** Rule 11 (consistent authoring surface) and Rule 8 — this is an additive
  overload plus a return-type widening on an existing builder (no `internal`->`public` change, no
  plan-shape change). Serves the typed/guessable-DSL bar at Layer 1.
- **Effort.** S. **Risk.** Med. *(Tradeoff: changing a return type, even to a wider/sibling builder, can
  break existing chained call sites that relied on `ElementBuilder`-specific members — audit call sites
  and run the full build/test gate before committing.)*

### 5.5 Disambiguate the eight `PipelineBuilder.Plugin` overloads and centralize their guard

- **Current.** `PipelineBuilder.cs` has eight methods named `Plugin`/`PluginProperty`
  (`:164, :179, :194, :206, :215, :226, :240, :253`) returning three different builder kinds —
  `PluginReadBuilder` (read a value), `PluginCallBuilder` (fire a void command), and
  `TypedPluginPropertySource` (read a property). Whether `p.Plugin("x","y")` reads or calls is decided
  only by the chosen overload's return type, invisible at the call site. Each read/call overload repeats
  the same name/member `string.IsNullOrWhiteSpace(...) throw` guard pair.
- **Proposed.** (1) Readability: extract the repeated guard into one private
  `RequirePluginOperation(string pluginName, string? member)` so each overload body shrinks to the one
  line that differs (the `DeclarePluginMethod` call). (2) Optional ergonomics: keep the typed builders
  but make read-vs-call intent explicit in the DSL surface — e.g. a value-context entry
  `p.PluginValue<T>(name, member)` for reads while `p.Plugin(name, member)` stays the void command — so
  the verb states intent (matching how `Element` vs `Component` already read). Stringly plugin *names*
  stay (the intentional escape-hatch boundary); only the C#-side method naming/guard duplication is
  tightened.
- **Why.** Eight same-named overloads returning three builder types is the hardest entry point in
  `PipelineBuilder` to read; a contributor cannot tell read from call without IntelliSense and must
  duplicate the guard when adding the next overload. Naming the intent and centralizing the guard makes
  the surface scannable and the next overload a one-line add.
- **CLAUDE.md alignment.** Rule 11 (small methods, no dead duplication; revealing names). Respects the
  Plugins lesson: plugin name strings stay stringly at the boundary; only the typed author surface around
  them is clarified.
- **Effort.** M. **Risk.** Med. *(Tradeoff: part (2) adds a new public DSL entry point — gate it on user
  approval; part (1) is a pure internal refactor and can ship independently. Doing part (1) alone is the
  safe default.)*

---

## De-duplication note

Several areas independently flagged `PlanTypeScriptContract.cs` (1,165 lines). They are kept as three
*distinct, non-overlapping* recommendations: **3.3** moves the file to a contract folder (structure),
**5.1** adds the C#-to-TS drift guard (correctness), **5.2** corrects the "generator" naming/automation
gap (honesty). The byte-stability test in **4.3** is the single home for the determinism proof and also
serves as the regression guard for the converter change in **2.2** — they are sequenced together below
rather than duplicated. The `WriteProperty<T>` collapse (**2.1**) and the kind-first converter cleanup
(**2.2**) are separate steps because 2.1 is risk-free duplication removal while 2.2 changes emission
mechanics; 2.1 should land first so 2.2's diff is small.

---

## Sequenced plan

Each step is small, independently reviewable, and keeps build + tests green. Vertical-slice-safe:
no step changes the plan wire shape except where the regenerate-TS gate is explicitly named.

**Phase A — zero-risk hygiene (no behavior, no wire change).**
1. **3.4** delete empty `Descriptors/` tree.
2. **2.1** collapse the 8 `WriteProperty<T>` copies into `PlanJsonWriter` (compiler-verified, identical bytes).
3. **2.4** document the two justified `[JsonIgnore]` attributes (docs only — prevents a future regression).

**Phase B — invariants & determinism (C# producer only, no wire change).**
4. **1.1** add null-guards to the seven ReactionGraph leaf constructors.
5. **4.3** add the byte-stability + ordinal-key test (failing first — drives 4.1/4.2 and guards 2.2).
6. **4.1** pin ordinal key order at the four dictionary snapshot boundaries (test from step 5 goes green).
7. **4.2** record the Components emission-order rule in code comment + domain-language doc.

**Phase C — serializer cohesion (wire shape unchanged but emission mechanics change — guarded by step 5).**
8. **2.3** unify the absent-payload sentinels under one `WriteBody` no-op default.
9. **2.2** drop the kind-first bespoke converters onto the polymorphic mechanism (step 5's test catches any reordering).

**Phase D — discriminated-union outlier + TS-contract integrity.**
10. **1.2** convert `RegisteredInputSelection` to the sentinel-pair union; regenerate `plan.ts` + `npm run typecheck` (expected no-op).
11. **5.1** revive DriftDetection with the C#-to-TS discriminator guard.
12. **5.2 Phase 1** correct the PlanTypeScriptContract naming/doc (Phase 2 reflection is a separate, later initiative gated on 5.1).

**Phase E — structure moves (file/namespace only, zero `using` churn).**
13. **3.1** split Validation plan model out of `ComponentObject.cs`.
14. **3.2** dissolve `PlanTerms.cs` into per-area value-object files.
15. **3.3** move `PlanTypeScriptContract.cs` to `PlanContract/` (optional partial split).
16. **3.5** gather loose registration files into `Components/`.

**Phase F — authoring-surface ergonomics (review individually; some are approval-gated).**
17. **1.3** rename `Behavior`/`BehaviorGraph` -> `ReactionRule`/`ReactionRules` + domain-language doc.
18. **1.4** unify the SSE-trigger `PayloadType` accessor.
19. **5.3** inline within-slice component-descriptor boilerplate (one slice per commit).
20. **5.4** ElementBuilder return-type consistency + `SetHtml(ResponseBody)` overload (matrix row first; audit call sites).
21. **5.5 part 1** centralize the `Plugin` overload guard (safe). **5.5 part 2** new `PluginValue<T>` entry — approval-gated.
22. **(approval-gated)** **IdGenerator overload audit** — `IdGenerator.For<TModel>(object?)` (`IdGenerator.cs:29`) and
    `For<TModel,TProp>` (`:43`) are two public overloads differing only by a boxing `Convert` node that
    `ExpressionPathHelper.UnwrapConvert` strips; collapsing to the typed overload would give one canonical
    ID path. **IdGenerator is public API (Rule 8)** — requires explicit user approval and a downstream
    call audit before removal; listed here as a clarity opportunity, not an autonomous change.
