/*
 * Alis.Reactive — NEW DESIGN graph data.
 *
 * Source of truth (do NOT infer from the old atlas):
 *   docs/design/redesign/02-micro-modules.md   — the 12 modules: responsibility, owns, depends-on, layered graph
 *   docs/design/redesign/03-naming.md          — plain-English names + key concepts per module
 *   docs/design/redesign/04-matrix-*.md         — the deterministic flow cases (input -> module path -> output)
 *
 * Shape consumed by index.html:
 *   window.NEW_DESIGN = {
 *     modules: [{ id, name, tier, responsibility, owns:[...], dependsOn:[...], concepts:[...] }],
 *     edges:   [{ from, to, label }],           // deduped; one edge per dependency pair
 *     flows:   [{ name, blurb, inOut:{in, out}, steps:[{ module, side, did }] }]
 *   }
 *
 * The 12 modules: 9 vocabulary concept-slices + 3 shared spines (Shape, Kind kernels; Plan aggregate root).
 * tier: "root" (Plan) | "slice" (the 9 concepts) | "kernel" (Shape, Kind).
 */
(function () {
  "use strict";

  // ---- Modules -----------------------------------------------------------
  // responsibility / owns / dependsOn taken verbatim-in-spirit from 02-micro-modules.md table.
  // concepts: the key names from 03-naming.md (-> C# author/plan side, => TS runtime side).
  var modules = [
    {
      id: "Plan",
      name: "Plan",
      tier: "root",
      responsibility:
        "The plan-document spine: PlanBuildContext authoring sink → immutable PlanDocument → serialize → root.ts discovery → boot. The aggregate root the concept-slices write into and the runtime reads from, with explicit plan-scoped state.",
      owns: [
        "PlanBuildContext (narrow Declare/Wire verbs)",
        "PlanDocument (v3: planId/scope/types/components/behaviors)",
        "PlanExtensions: Html.ReactivePlan / ResolvePlan / RenderPlan",
        "one id-sanitization rule",
        "⇒ root.ts discovery",
        "⇒ boot.ts wiring (active plan passed explicitly)",
        "⇒ ActivePlan (explicit, not a hidden singleton)"
      ],
      dependsOn: ["Trigger", "Reaction", "Component", "Slot", "Kind"],
      concepts: [
        "PlanBuildContext",
        "PlanDocument",
        "PlanId",
        "Html.ReactivePlan / ResolvePlan / RenderPlan",
        "root",
        "boot",
        "ActivePlan"
      ],
      replaces:
        "Hidden mutable activeRuntimePlan singleton; the 4 reset*ForTests functions shipped in production; the boot↔browser-plans callback-injection cycle."
    },
    {
      id: "Trigger",
      name: "Trigger",
      tier: "slice",
      responsibility:
        "When a behavior starts — page-ready, a document event, a component callback, a server push, SignalR — and the runtime listener wiring that feeds the originating payload into one execution context.",
      owns: [
        "Html.On + TriggerBuilder",
        "StartsWhen family (symmetric: public sealed + explicit Kind)",
        "Behavior (one trigger→reaction edge) + BehaviorGraph",
        "⇒ wireTrigger per StartsWhen kind",
        "⇒ server-push.ts / signalr.ts",
        "⇒ ONE ExecutionContext carrying the trigger payload",
        "⇒ per-trigger error boundary + AbortSignal"
      ],
      dependsOn: ["Reaction", "Component", "Kind"],
      concepts: ["Html.On / TriggerBuilder", "StartsWhen", "Behavior", "BehaviorGraph", "wireTrigger", "ExecutionContext"],
      replaces:
        "Behavior/StartsWhen internal-class-with-public-props asymmetry; raw-vs-rich ExecContext double threading."
    },
    {
      id: "Reaction",
      name: "Reaction",
      tier: "slice",
      responsibility:
        "What runs when a trigger fires — the p.Element/Component/Dispatch/Inject/ValidationErrors/Into command surface and the executable action graph, with the lane color carried in the plan. Effect edge (sync void).",
      owns: [
        "thin command sink + ElementBuilder + DispatchPayloadBuilder",
        "ReactionPipelineDraft — sequences sync/async/branch and STAMPS the lane onto each node",
        "ReactionGraph family (RequestReaction.Request renamed)",
        "ReactionLane (plan-carried sync/async fact)",
        "⇒ executeReaction = switch + assertNever on the carried lane"
      ],
      dependsOn: ["Value", "Condition", "Request", "Slot", "Component", "Kind"],
      concepts: [
        "ReactionPipelineDraft",
        "ElementBuilder / DispatchPayloadBuilder",
        "ReactionGraph",
        "ReactionLane",
        "executeReaction"
      ],
      replaces:
        "God-builder PipelineBuilder (4 partials); scattered instanceof Promise / crossedAsyncBoundary lane re-detection; RequestReaction.Request's new collision hack."
    },
    {
      id: "Condition",
      name: "Condition",
      tier: "slice",
      responsibility:
        "The if/else-if/else decision over readable values; first match wins. When/Then/ElseIf/Else authoring and the deterministic predicate graph, evaluated by ONE compare engine on both lanes.",
      owns: [
        "When / Confirm + GuardBuilder / BranchBuilder / ConditionContinuation",
        "Standalone.Then made unrepresentable (not a runtime throw)",
        "ConditionGraph (Compare/All/Any/Not/Confirm)",
        "ComparisonOperands (one shape)",
        "the 21 CompareOp tokens — single op-list source",
        "⇒ CompareEngine (ONE engine, both lanes)",
        "⇒ evaluateCondition (sync core) + confirmThenEvaluate (async wrapper)"
      ],
      dependsOn: ["Value", "Shape", "Kind"],
      concepts: [
        "When / Confirm",
        "GuardBuilder / BranchBuilder / ConditionContinuation",
        "ConditionGraph",
        "ComparisonOperands",
        "CompareOp",
        "CompareEngine",
        "evaluateCondition",
        "confirmThenEvaluate"
      ],
      replaces:
        "Dual evaluators (conditions.ts vs sync-condition.ts divergence); ValueEvaluator DI threaded through 8 fns; StandaloneConditionContinuation.Then runtime throw."
    },
    {
      id: "Value",
      name: "Value",
      tier: "slice",
      responsibility:
        "Every value the plan can read — a literal, a component member, a URL part, a payload field. One TypedSource authoring surface, lowered to one ValueExpression, read back by one evaluator. Pure-core read (no IO, no DOM mutation).",
      owns: [
        "TypedSource<T> (absorbs Component/Url/Plugin/Payload/Element source families)",
        "ValueExpression flat family: Literal / Read / ObjectValue / ArrayValue / ArrayOp",
        "WholePayload / WholeElement as real variants (not sentinels)",
        "per-op ArrayOp variants (FilterOp/MapOp/SumOp/…)",
        "⇒ evaluateValue (slim dispatcher)",
        "⇒ ArrayOpEngine (count/filter/map/sum/any/all/find/orderBy)",
        "⇒ dom-read / url-read handlers + RuntimeValue/RuntimeShape/RuntimePath"
      ],
      dependsOn: ["Shape", "Kind"],
      concepts: [
        "TypedSource<T>",
        "ValueExpression",
        "Literal / Read / ObjectValue / ArrayValue / ArrayOp",
        "WholePayload / WholeElement",
        "evaluateValue",
        "ArrayOpEngine",
        "RuntimeValue / RuntimeShape / RuntimePath"
      ],
      replaces:
        "God-facade ValueExpression.cs (590 lines) + ValueRead→ValueReadTarget→ValueReadPath 4-type indirection; god-class evaluate.ts (300 lines); responseBody/elementValue magic sentinels; the gather-source hole."
    },
    {
      id: "Request",
      name: "Request",
      tier: "slice",
      responsibility:
        "The HTTP call — the only async lane the framework opens for the network: Get/Post/Put/Delete with Gather (target ← value), Response success/error scopes, Chained, Parallel, WhileLoading/Finally. Async effect edge.",
      owns: [
        "HttpRequestBuilder + GatherBuilder/Include + ResponseBuilder + ParallelBuilder",
        "RequestPlan + GatherAssignment + ResponseRouting/Route/RequestChain",
        "PayloadScope folded to only scopes that carry data (dead local removed)",
        "⇒ http pipeline (gather → fetch → response routing → finally → chain)",
        "⇒ gather / RequestPayloadWriter / httpFetch named stages",
        "⇒ ONE writer for FormData/File"
      ],
      dependsOn: ["Value", "Condition", "Component", "Shape", "Kind"],
      concepts: [
        "HttpRequestBuilder",
        "GatherBuilder / Include",
        "ResponseBuilder / ParallelBuilder",
        "RequestPlan",
        "GatherAssignment",
        "ResponseRouting / ResponseRoute / RequestChain",
        "PayloadScope",
        "http",
        "gather / RequestPayloadWriter / httpFetch"
      ],
      replaces:
        "The 7-scope-onto-3-field fold; the dead local scope; FormData/File knowledge scattered across 3 modules."
    },
    {
      id: "Component",
      name: "Component",
      tier: "slice",
      responsibility:
        "A browser object with an id, a vendor, a type, a role, a binding, and a declared member contract — the single deterministic id threaded through render, gather, validation, slot, and getElementById. The sole vendor seam.",
      owns: [
        "IdGenerator + ModelBoundInputComponentSlot + InputBoundField + Html.InputField (the ONE id regime)",
        "BrowserObject / ComponentRole / InputBinding / BrowserObjects (repo + same-vendor invariant)",
        "BrowserObjectContract + BrowserObjectId (vendor,kind,id) value object",
        "per-vendor slice extensions (.Reactive()/mutation/read/Html)",
        "⇒ RuntimeComponents / RuntimeObject (memoized, not rebuilt per read)",
        "⇒ ComponentDriver + wireFusionEvent/wireNativeEvent (the SOLE vendor seam)"
      ],
      dependsOn: ["Value", "Shape", "Kind"],
      concepts: [
        "IdGenerator",
        "Html.InputField / InputBoundField",
        "ModelBoundInputComponentSlot",
        "BrowserObject",
        "ComponentRole",
        "InputBinding",
        "BrowserObjects",
        "BrowserObjectContract",
        "BrowserObjectId",
        "RuntimeComponents",
        "RuntimeObject",
        "ComponentDriver",
        "wireFusionEvent / wireNativeEvent"
      ],
      replaces:
        "God-file ComponentObject.cs (677 lines); RuntimePlan 4-classes-in-one + per-read rebuild; the stale resolver.ts Rule-5 claim; two-id-regime gap; TypeKey opaque-string parsing."
    },
    {
      id: "Slot",
      name: "Slot",
      tier: "slice",
      responsibility:
        "Compose plans: join by plan id on the server (SSR), load/unload partials by slot id in the browser. Recomposes the active plan from a boot snapshot plus loaded slots, aborting only slot-owned behavior.",
      owns: [
        "PlanScope (root vs partial) — decides SSR-merge vs slot-loadable",
        "⇒ injectPartial (partial injection)",
        "⇒ AppliedPlans (boot snapshots + slot loads + AbortControllers)",
        "⇒ recompose — builds a NEW PlanDocument (not in-place mutation)",
        "ONE MergePolicy (replace-vs-append) shared with the C# container merge"
      ],
      dependsOn: ["Plan", "Component"],
      concepts: ["PlanScope", "SlotId", "injectPartial", "AppliedPlans", "recompose", "MergePolicy"],
      replaces:
        "In-place resetPlanDocument mutation of a shared reference; the cross-language merge-rule divergence (C# replace vs TS append)."
    },
    {
      id: "Validation",
      name: "Validation",
      tier: "slice",
      responsibility:
        "Client-side validation metadata: explicit deterministic rules recorded through ReactiveValidator<T>/DI at render time, run inline/summary in the browser. FluentValidation stays server authority.",
      owns: [
        "ReactiveValidator<T> ClientRule/WhenField + ClientValidationFieldRuleBuilder (16 rule types)",
        "ValidationGraph — validation plan model in its own home (extracted from ComponentObject.cs)",
        "ValidationRuleNode (renamed — ends the two-ValidationRule collision)",
        "RuleName (TS union derived from C#) + RuleOperand (one operand model)",
        "CollectionItemBinding (real value object, not substring path arithmetic)",
        "⇒ validationOrchestrator / ruleEngine / errorDisplay / liveClear (reuse Condition's CompareEngine for WhenField)",
        "ErrorElementNaming — one shared {id}_error / {planId}_validation_summary constant"
      ],
      dependsOn: ["Condition", "Component", "Value", "Plan", "Kind"],
      concepts: [
        "ReactiveValidator<T>",
        "ClientRule / WhenField",
        "ClientValidationFieldRuleBuilder",
        "ValidationRuleNode",
        "RuleName",
        "RuleOperand",
        "CollectionItemBinding",
        "ValidationGraph",
        "validationOrchestrator / ruleEngine / errorDisplay / liveClear",
        "ErrorElementNaming"
      ],
      replaces:
        "The validation tower buried in ComponentObject.cs; Validation.ValidationRule vs PlanModel.ValidationRule collision; 3 independent rule-name enumerations; operands modeled twice; substring path arithmetic; ad-hoc ValidationSurface rebuild."
    },
    {
      id: "Plugin",
      name: "Plugin",
      tier: "slice",
      responsibility:
        "The intentional escape hatch: declare a plugin browser object (typed properties + operations) and read/call it through the same object-member and ValueExpression concepts. Stringly names allowed ONLY at the plugin boundary.",
      owns: [
        "ONE plugin-declaration API (Plugin)",
        "ONE args-builder-first read/call surface (PluginMemberBuilder)",
        "PluginContract → BrowserObjectContract mapping",
        "⇒ PluginCatalog (host-registered instances; resolve throws at the boundary — a real external edge)"
      ],
      dependsOn: ["Value", "Component", "Shape", "Kind"],
      concepts: ["Plugin", "PluginMemberBuilder", "PluginContract", "PluginCatalog"],
      replaces:
        "Two parallel declaration APIs (PluginTypeBuilder vs ReactivePlugin); ~95%-identical read/call builders; the arity-0..3 × member/root × function/command overload explosion (~30 methods)."
    },
    {
      id: "Shape",
      name: "Shape",
      tier: "kernel",
      responsibility:
        "The structural type tag that rides on every value, operand, gather assignment, and contract member: CLR inference at authoring, one conversion engine at runtime. The same bytes convert the same way everywhere.",
      owns: [
        "Shape + ShapeStructure + ShapeContractCompatibility value objects (merge/accept algebra)",
        "⇒ ShapeConverter — single applyShape / convertByShape engine",
        "the shape-once invariant on the gather egress path"
      ],
      dependsOn: [],
      concepts: ["Shape", "ShapeStructure", "ShapeContractCompatibility", "ShapeConverter", "shape-once rule"],
      replaces:
        "The 3 redundant re-shapings (evaluate / gather re-derive / formatForWire); the hand converter."
    },
    {
      id: "Kind",
      name: "Kind",
      tier: "kernel",
      responsibility:
        "The one discriminator that tells C# and TS apart which node this is, and generates the TS contract from it. ONE polymorphic mechanism, a reflection-driven generator, a build-time drift gate.",
      owns: [
        "PlanNodeDiscriminator — ONE polymorphic mechanism emitting kind from a compile-enforced base",
        "PlanContractGenerator — reflects node families, writes plan.ts",
        "PlanSerializer — sole JSON owner (camelCase)",
        "ContractDriftGate — build step failing on C#↔TS drift",
        "⇒ assertNever exhaustiveness guard"
      ],
      dependsOn: ["Shape"],
      concepts: [
        "Kind",
        "PlanNodeDiscriminator",
        "PlanContractGenerator",
        "PlanSerializer",
        "ContractDriftGate",
        "assertNever"
      ],
      replaces:
        "WriteOnlyPolymorphicConverter + 11 hand JsonConverters; the 1,165-line hand-authored PlanTypeScriptContract + TypeScriptContractWriter; PlanTerms op-group .Values arrays."
    }
  ];

  // ---- Edges (deduped) ---------------------------------------------------
  // One edge per (from -> to) dependency pair, straight from the layered graph in 02-micro-modules.md.
  // Acyclic: kernels (Shape, Kind) at the bottom; Plan at the top. label = WHY the dependency exists.
  var edges = [
    // Plan (root) wires the slices
    { from: "Plan", to: "Trigger", label: "wires behaviors" },
    { from: "Plan", to: "Reaction", label: "boots reactions" },
    { from: "Plan", to: "Component", label: "carries components" },
    { from: "Plan", to: "Slot", label: "composes partials" },
    { from: "Plan", to: "Kind", label: "serialize / discriminate" },

    // Trigger
    { from: "Trigger", to: "Reaction", label: "fires the reaction" },
    { from: "Trigger", to: "Component", label: "resolves event source id" },
    { from: "Trigger", to: "Kind", label: "StartsWhen discriminator" },

    // Reaction
    { from: "Reaction", to: "Value", label: "reads set/call values" },
    { from: "Reaction", to: "Condition", label: "branch guards" },
    { from: "Reaction", to: "Request", label: "opens HTTP lane" },
    { from: "Reaction", to: "Slot", label: "inject partials" },
    { from: "Reaction", to: "Component", label: "mutates browser objects" },
    { from: "Reaction", to: "Kind", label: "ReactionGraph discriminator" },

    // Request
    { from: "Request", to: "Value", label: "gather reads values" },
    { from: "Request", to: "Condition", label: "Validate guard" },
    { from: "Request", to: "Component", label: "reads input ids" },
    { from: "Request", to: "Shape", label: "shape-once egress" },
    { from: "Request", to: "Kind", label: "RequestPlan discriminator" },

    // Slot
    { from: "Slot", to: "Plan", label: "recomposes a PlanDocument" },
    { from: "Slot", to: "Component", label: "removes slot-owned objects" },

    // Validation
    { from: "Validation", to: "Condition", label: "WhenField via CompareEngine" },
    { from: "Validation", to: "Component", label: "binds rules to field ids" },
    { from: "Validation", to: "Value", label: "rule operands" },
    { from: "Validation", to: "Plan", label: "validation graph on the plan" },
    { from: "Validation", to: "Kind", label: "rule node discriminator" },

    // Plugin
    { from: "Plugin", to: "Value", label: "read/call via ValueExpression" },
    { from: "Plugin", to: "Component", label: "maps to a browser object" },
    { from: "Plugin", to: "Shape", label: "member shapes" },
    { from: "Plugin", to: "Kind", label: "plugin source discriminator" },

    // Condition
    { from: "Condition", to: "Value", label: "operands are values" },
    { from: "Condition", to: "Shape", label: "compare shape rules" },
    { from: "Condition", to: "Kind", label: "ConditionGraph discriminator" },

    // Component
    { from: "Component", to: "Value", label: "member reads" },
    { from: "Component", to: "Shape", label: "member shapes" },
    { from: "Component", to: "Kind", label: "component discriminator" },

    // Value
    { from: "Value", to: "Shape", label: "every value carries a Shape" },
    { from: "Value", to: "Kind", label: "ValueExpression discriminator" },

    // Kernel
    { from: "Kind", to: "Shape", label: "contract members carry shape" }
  ];

  // ---- Flows -------------------------------------------------------------
  // Each flow is a concrete deterministic case from 04-matrix-*.md.
  // step.side: "author" (→ C# author/plan side) | "runtime" (⇒ TS runtime side).
  // Steps name the ordered modules the data walks through and what each does to it.
  var flows = [
    {
      name: "Read a component property",
      blurb:
        "A.2 — the single Read template, component property variant. The plainest value read: pure sync, no async lane.",
      inOut: {
        in: 'p.Component<T>("resident-name").Value()',
        out: 'live value of the component member, shape-coerced to TProp'
      },
      steps: [
        { module: "Value", side: "author", did: "TypedSource lowers to ValueExpression.Read(ComponentSource.Of(id), member), access=property" },
        { module: "Shape", side: "author", did: "Shape.FromClrType(TProp) is inferred and rides on the Read node" },
        { module: "Kind", side: "author", did: 'stamps kind:"read"; PlanSerializer emits camelCase JSON' },
        { module: "Value", side: "runtime", did: 'evaluateValue case "read" dispatches an object read' },
        { module: "Component", side: "runtime", did: "RuntimeObject.read(member) via ComponentDriver — getElementById + the sole vendor seam" },
        { module: "Shape", side: "runtime", did: "usingRequestedShape(shape) coerces the live value once (shape-once)" }
      ]
    },
    {
      name: "Set text, conditionally",
      blurb:
        "Reaction band — a branch guard over a value read, first match wins, sync void effect. The lane is stamped in the plan, not re-detected.",
      inOut: {
        in: 'p.When(care, c => c.Eq("Memory")).Then(x => x.Component("billing").SetText("$2,400"))',
        out: 'billing component shows "$2,400" only when care equals "Memory"'
      },
      steps: [
        { module: "Reaction", side: "author", did: "ReactionPipelineDraft.BeginBranch sequences the branch and STAMPS lane=sync" },
        { module: "Condition", side: "author", did: "GuardBuilder → ConditionGraph.Compare(Eq), first-match BranchBuilder" },
        { module: "Value", side: "author", did: "left operand = Read(component care); right operand = Literal(\"Memory\")" },
        { module: "Kind", side: "author", did: 'kind:"branch" + kind:"compare"; serialized camelCase' },
        { module: "Reaction", side: "runtime", did: "executeReaction routes kind:branch on the carried sync lane" },
        { module: "Condition", side: "runtime", did: "evaluateCondition uses the ONE CompareEngine — Eq returns true/false" },
        { module: "Component", side: "runtime", did: "RuntimeObject set on the matched branch — billing.setText via ComponentDriver" }
      ]
    },
    {
      name: "POST with a gathered body",
      blurb:
        "B.3 + B.4 — gather a component value into the JSON body. THE async lane: Request is the only feature that opens it.",
      inOut: {
        in: 'p.Post("/api/residents", g => g.Include<Comp,Model>(m => m.Name))',
        out: 'fetch("/api/residents", { method:"POST", body:{ "name": <live value> } })'
      },
      steps: [
        { module: "Reaction", side: "author", did: "ReactionPipelineDraft.BeginHttp STAMPS lane=async, opens the Request node" },
        { module: "Request", side: "author", did: "HttpRequestBuilder.Post → RequestPlan; GatherBuilder.Include builds a GatherAssignment.Payload" },
        { module: "Component", side: "author", did: "IdGenerator.For derives the deterministic input id; declares the input on PlanBuildContext" },
        { module: "Value", side: "author", did: "assignment source = Read(ComponentSource, valueMember) via the one TypedSource path" },
        { module: "Kind", side: "author", did: 'kind:"request" + kind:"gather"; PlanSerializer emits camelCase' },
        { module: "Request", side: "runtime", did: "http pipeline: gather → httpFetch → response routing → finally → chain (all awaited)" },
        { module: "Value", side: "runtime", did: "gather.resolveGatherRequestInput calls evaluateValue(source) to read the live component value" },
        { module: "Component", side: "runtime", did: "RuntimeObject.read(member) supplies the live value via ComponentDriver" },
        { module: "Shape", side: "runtime", did: "RequestPayloadWriter formatForWire once (shape-once); \"\" → null on JSON egress" }
      ]
    },
    {
      name: "OnSuccess → chained request",
      blurb:
        "B.5 + B.7 — route the success scope, then chain a follow-up request that reads the prior response body. First-match status routing.",
      inOut: {
        in: 'r.OnSuccess<R>((json,s) => …).Chained(req => req.Get("/next/{id}").Gather(g => g.RouteParam("id", json.Read(x => x.Id))))',
        out: 'on response.ok: parse body into success scope, then fetch /next/<id> reading the prior body'
      },
      steps: [
        { module: "Request", side: "author", did: "ResponseBuilder.OnSuccess opens a success scope with PayloadContract.ForPayload(R)" },
        { module: "Value", side: "author", did: "json.Read(x=>x.Id) → ReadPayload(PayloadSource.Success, path) in the success scope" },
        { module: "Request", side: "author", did: "Chained builds a full child RequestPlan; ResponseRouting.Chain = FollowUpRequestChain(next)" },
        { module: "Kind", side: "author", did: 'kind:"request" nests a chain:{kind:"follow-up"}; emitted camelCase' },
        { module: "Request", side: "runtime", did: "routeSuccess picks the exact-status route else any; runs the success reaction" },
        { module: "Value", side: "runtime", did: "evaluateValue walks the success-scope payload path to read prior body fields" },
        { module: "Request", side: "runtime", did: "runFollowUpRequest fires the next request ONLY after success, gathering from the prior body" }
      ]
    },
    {
      name: "Filter → sum an array",
      blurb:
        "Part C — chained array ops compile to nested array-op nodes. Pure sync, no Promise: predicates are the sync condition subset only.",
      inOut: {
        in: 'p.From(p.Component<Multi>(m => m.Items).Value()).Where(x => x.Active).Sum(x => x.Amount)',
        out: 'numeric sum of Amount over Active items; binds to any value sink as a ReactiveValue'
      },
      steps: [
        { module: "Value", side: "author", did: "From(TypedSource<T[]>) seeds the op chain; ArrayFilter wraps the source, ArraySum wraps the filter" },
        { module: "Condition", side: "author", did: "the Where predicate compiles to a ConditionGraph over the element scope (sync subset)" },
        { module: "Shape", side: "author", did: "itemShape from T; out Shape.Number for sum; nested per-op variants carry only their fields" },
        { module: "Kind", side: "author", did: 'kind:"array-op" with op:"filter" feeding op:"sum"; camelCase' },
        { module: "Value", side: "runtime", did: "evaluateValue dispatches to ArrayOpEngine" },
        { module: "Condition", side: "runtime", did: "elementMatches(predicate) runs the CompareEngine per element on the sync lane" },
        { module: "Value", side: "runtime", did: "ArrayOpEngine: items.filter(...).reduce(total + toNumber(projected)) → the sum" }
      ]
    },
    {
      name: "Validate before request",
      blurb:
        "B.6 Validate + Validation module — client rules run before fetch; failure aborts the request and shows errors. WhenField reuses Condition's CompareEngine.",
      inOut: {
        in: '.Validate<V>("resident-form") on a Post; ReactiveValidator<V> records ClientRule + WhenField',
        out: 'invalid form → errors shown, request never sends; valid → fetch proceeds'
      },
      steps: [
        { module: "Request", side: "author", did: "ClientValidationBeforeRequest → RequestValidationTarget.DisplayIn(containerId)" },
        { module: "Validation", side: "author", did: "ReactiveValidator<V> records ClientRule/WhenField → ValidationGraph (its own home)" },
        { module: "Condition", side: "author", did: "WhenField lowers to a ConditionGraph — one operand model, one rule-name source" },
        { module: "Component", side: "author", did: "rules bound to deterministic field ids; ErrorElementNaming = {id}_error / {planId}_validation_summary" },
        { module: "Request", side: "runtime", did: "requestCanSend → validateContainer; invalid → abort, NO fetch" },
        { module: "Validation", side: "runtime", did: "validationOrchestrator/ruleEngine run rules; errorDisplay renders failures" },
        { module: "Condition", side: "runtime", did: "WhenField conditions evaluated by the SAME CompareEngine — no C#↔TS drift" }
      ]
    },
    {
      name: "Component event → reaction",
      blurb:
        "Trigger band — .Reactive() wires a vendor event to a reaction. SYNC so Syncfusion args.cancel is visible. The vendor seam is the only vendor-aware code.",
      inOut: {
        in: 'b.Reactive(plan, evt => evt.Changed, (args, p) => p.Component("total").SetText(args.Value))',
        out: 'the vendor "changed" event runs the reaction in the same tick, args mutations visible to SF'
      },
      steps: [
        { module: "Component", side: "author", did: "resolves the deterministic id; ComponentEventOnboarding.Wire" },
        { module: "Trigger", side: "author", did: "PlanBuildContext.WireComponentEvent → StartsWhen.ComponentEvent(id, eventName) → Behavior" },
        { module: "Kind", side: "author", did: 'kind:"component-event"; symmetric StartsWhen serialized camelCase' },
        { module: "Trigger", side: "runtime", did: "wireTrigger case component-event resolves the BrowserObject + event channel" },
        { module: "Component", side: "runtime", did: "ComponentDriver (wireFusionEvent/wireNativeEvent) attaches — the SOLE vendor seam" },
        { module: "Trigger", side: "runtime", did: "delivery builds ONE ExecutionContext.event(eventData) and runs the reaction SYNC" },
        { module: "Reaction", side: "runtime", did: "executeReaction set on the matched node — total.setText via RuntimeObject" }
      ]
    },
    {
      name: "Inject a partial into a slot",
      blurb:
        "Slot module — load partial HTML by SlotId and recompose the active plan into a NEW PlanDocument from the boot snapshot plus loaded slots.",
      inOut: {
        in: 'p.Inject("/partials/details").Into(SlotId("detail-slot"))',
        out: 'partial HTML loaded into the slot; active plan recomposed; only slot-owned behavior abortable'
      },
      steps: [
        { module: "Reaction", side: "author", did: "the inject command lowers to a ReactionGraph inject node, lane=async" },
        { module: "Slot", side: "author", did: "PlanScope marks the partial slot-loadable by SlotId" },
        { module: "Kind", side: "author", did: 'kind:"inject"; serialized camelCase' },
        { module: "Reaction", side: "runtime", did: "executeReaction routes kind:inject on the async lane → injectPartial" },
        { module: "Slot", side: "runtime", did: "injectPartial loads HTML; AppliedPlans tracks snapshot + slot + AbortController" },
        { module: "Plan", side: "runtime", did: "recompose builds a NEW PlanDocument (no in-place mutation); MergePolicy decides replace-vs-append" },
        { module: "Component", side: "runtime", did: "slot-owned browser objects mount; boot/app-level objects stay mounted" }
      ]
    },
    {
      name: "Boot a plan from the page",
      blurb:
        "Plan spine — discovery → boot. The active plan is passed explicitly down to executeReaction; no hidden singleton.",
      inOut: {
        in: '@Html.RenderPlan(plan)  (emits <script type="application/json" data-reactive-plan>)',
        out: 'each composed plan wired: triggers listening, components resolvable, reactions ready'
      },
      steps: [
        { module: "Plan", side: "author", did: "PlanBuildContext sink → immutable PlanDocument (v3) → PlanSerializer to camelCase JSON" },
        { module: "Kind", side: "author", did: "every node carries its kind discriminator; ContractDriftGate proves C#↔TS agree" },
        { module: "Plan", side: "runtime", did: "root.ts discovers [data-reactive-plan] scripts and merges partials by PlanId" },
        { module: "Plan", side: "runtime", did: "boot.ts wires each composed plan, passing ActivePlan explicitly" },
        { module: "Component", side: "runtime", did: "RuntimeComponents builds the component lookup; RuntimeObject memoized per id" },
        { module: "Trigger", side: "runtime", did: "wireTrigger attaches one listener per StartsWhen kind, AbortSignal-scoped" }
      ]
    }
  ];

  window.NEW_DESIGN = { modules: modules, edges: edges, flows: flows };
})();
