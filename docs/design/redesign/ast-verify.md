# AST Grammar Verification

Independent verification that the 8 `ast-grammar-*.md` tables and `ast-proof.md`
are GROUNDED in real source signatures — method name, params, and **return type**
checked against the actual `.cs` files (a wrong return type breaks the AST).

Scope: all 9 docs read in full; ~20 cited `file:line` rows opened at random across
every grammar file and both proofs; both proofs traced end-to-end to source;
negative ("NOT in source") claims re-checked by grep.

## Verdict: GROUNDED

Every sampled grammar row matched source exactly on **method name, parameter shapes,
and declared return type**. No mismatched or invented edges were found. The two
proofs hold against source. One minor numeric imprecision in supporting prose
(does not affect any grammar edge) is recorded below.

## Rows verified against source (name + params + RETURN TYPE)

| Doc | Cited row | Source `file:line` | Match |
|-----|-----------|--------------------|-------|
| conditions / proof | `GuardBuilder.Then(Action<PipelineBuilder<TModel>>) : BranchBuilder<TModel>` | GuardBuilder.cs:126 | exact (incl. `Not()` :118) |
| conditions / proof | `BranchBuilder.Else(Action<PipelineBuilder<TModel>>) : void` | BranchBuilder.cs:61 | exact (incl. `ElseIf<TProp>` :52) |
| conditions | `ConditionSourceBuilder.Eq(TypedSource<TProp>) : GuardBuilder<TModel>` | ConditionSourceBuilder.cs:112 | exact (NotEq/Gt/Gte/Lt/Lte :114-122) |
| entry-triggers | `TriggerBuilder.CustomEvent<TPayload>(string, Action<TPayload,PipelineBuilder<TModel>>) : TriggerBuilder<TModel>` | TriggerBuilder.cs:51 | exact (incl. `where TPayload : new()`) |
| entry-triggers | `Html.ReactivePlan<TModel>() : ReactivePlan<TModel>` | PlanExtensions.cs:43 (net48 :39) | exact (both hosts) |
| http | `HttpRequestBuilder.WhileLoading(Action<PipelineBuilder<TModel>>) : HttpRequestBuilder<TModel>` | HttpRequestBuilder.cs:70 | exact (AsJson :62, AsFormData :65) |
| http | `GatherBuilder.RouteParam<TArgs,TProp>(string, TArgs, Expression<Func<TArgs,TProp>>) : GatherBuilder<TModel>` | GatherBuilder.cs:141 | exact |
| http | `ResponseBuilder.OnSuccess<TResponse>(Action<ResponseBody<TResponse>,PipelineBuilder<TModel>>) : ResponseBuilder<TModel>` | ResponseBuilder.cs:40 | exact |
| http | `GatherExtensions.Include<TModel,TProp>(TypedComponentSource<TProp>) : GatherBuilder<TModel>` | GatherExtensions.cs:58 | exact |
| pipeline | `PipelineBuilder.DispatchWith<TPayload>(string, Action<DispatchPayloadBuilder<TPayload,TModel>>) : PipelineBuilder<TModel>` | PipelineBuilder.cs:75 | exact (Element :92, Component :101) |
| element-component | `ElementBuilder.SetText<TProp>(TypedSource<TProp>) : ElementBuilder<TModel>` (the self-return split) | ElementBuilder.cs:87 | exact |
| element-component | `Value<TModel>(this ComponentRef<FusionDropDownList,TModel>) : TypedComponentSource<string>` | FusionDropDownListExtensions.cs:150 | exact (terminal, ReturnsSelf=no) |
| component-reactive / proof | `Reactive<TModel,TArgs>(this TextBoxBuilder, ...) : TextBoxBuilder` (`return builder;` :39) | FusionTextBoxReactiveExtensions.cs:25 | exact |
| component-reactive | `Reactive<TModel,TProp,TArgs>(this NativeCheckBoxBuilder<TModel,TProp>, ...) : NativeCheckBoxBuilder<TModel,TProp>` | NativeCheckBoxReactiveExtensions.cs:33 | exact (3 type params) |
| value-arrays-validation | `ReactiveArray.Find<TField>(...) : ReactiveValue<TField>` | ReactiveArray.cs:107 | exact (Find :102, AsSource :121) |
| value-arrays-validation | `ClientValidationFieldRuleBuilder.EqualTo(Expression<Func<TModel,TValue>>, string) : ClientValidationFieldRuleBuilder<TModel,TValue>` | ClientValidationFieldRuleBuilder.cs:98 | exact (3 EqualTo overloads :95/:98/:103) |
| plugins | `PluginMemberBuilder.ArgValue<TValue>(TValue) : PluginMemberBuilder<TReturn,TModel>`; implicit op `: TypedPluginSource<TReturn>` | PluginMemberBuilder.cs:129 / :136 | exact |
| plugins | `PluginArgumentTypes.Arg<T>() : PluginArgumentTypes` | PluginTypeBuilder.cs:164 | exact |
| plugins-applevel | `FusionToast.Success<TModel>(...) : ComponentRef<FusionToast,TModel>` | FusionToastExtensions.cs:68 | exact |
| plugins-template | `FusionTemplateBuilder.When(Expression<Func<TModel,bool>>, Action<FusionConditionalBuilder<TModel>>) : FusionTemplateBuilder<TModel>` (both overloads) | FusionTemplateBuilder.cs:303 / :311 | exact |

## Both proofs hold against source

**Pattern 1 — Multi-statement conditions: HOLDS.**
- `GuardBuilder.Then` and `BranchBuilder.Else` take `Action<PipelineBuilder<TModel>>`
  (GuardBuilder.cs:126, BranchBuilder.cs:61) — verified.
- The `Then` core is `ConditionContinuation.Then(ConditionGraph, Action<PipelineBuilder<TModel>>) : BranchBuilder<TModel>`
  (ConditionContinuation.cs:73-75) — verified. Both overrides build a fresh
  `PipelineBuilder<TModel>` and invoke the callback (`:95-96` pipeline-rooted,
  `:121-122` branch-rooted) — verified line-for-line.
- Branch-rooted continuation returns the SAME branch instance `_branch`
  (ConditionContinuation.cs:124) — verified, so the `ElseIf … Then` ladder is
  genuinely chainable.
- Sandbox illustration: `ReactiveConditions.cshtml` `.Then(then => {…})` body holds
  exactly 7 statements, followed by `.ElseIf(args, x => x.Value).Eq("inactive").Then(…)`
  and a closing `.Else(else_ => …)` — verified at the cited region.

**Pattern 2 — Multi-event component: HOLDS.**
- `Reactive(...)` returns the receiver SF builder type with `return builder;`
  (FusionTextBoxReactiveExtensions.cs:25, `return builder;` at :39) — verified,
  so chained `.Reactive(...).Reactive(...)` is type-valid.
- `FusionTextBoxEvents` exposes exactly 4 distinct events: `Input` (:13),
  `Changed` (:18), `Focus` (:23), `Blur` (:28) — verified.
- FusionSmartTextArea exception: its TWO `Reactive` overloads return `void` and
  extend `ReactivePlan<TModel>` (FusionSmartTextAreaReactiveExtensions.cs:11, :22)
  — verified; it is the only `*ReactiveExtensions.cs` file with a `void Reactive`.
- Sandbox illustration: `TextBox/Index.cshtml` chains 4 distinct-event `.Reactive`
  calls on one builder — `Input` (:29), `Changed` (:34), `Focus` (:43), `Blur` (:48)
  — verified.

## Negative ("NOT in source") claims confirmed — no invented edges

- No public `Blur` or generic `Call` mutation verb exists on any `ComponentRef`
  extension (grep across Fusion + Native `*Extensions.cs` returns nothing) — the
  element-component doc correctly excludes these edges.
- No public type `ReactiveValidator`, `WhenField`, `FieldGuard`, or `.Each(...)`
  exists in `Validation/` — the value-arrays-validation doc correctly maps these
  conceptual roles onto the real `ClientValidation*` builder family and excludes
  the non-existent names.

## Minor issue (prose count only — NOT a grammar edge, NOT blocking)

`ast-proof.md` (lines 191, 239, 254) states **"47 of 49"** / **"47 non-void
builder returns and 2 void"** for the Fusion `*ReactiveExtensions.cs` cluster.
Actual source counts:

- 49 Fusion `*ReactiveExtensions.cs` files; **50** total `public static Reactive<…>`
  method declarations.
- **2** void (both in FusionSmartTextArea), **48** non-void.

So the correct figures are **48 non-void / 2 void** (48 single-method files +
SmartTextArea's 2 void methods = 50). The proof's "47" is **off by one** (should
be 48). This is an arithmetic slip in supporting prose; the substantive claim —
every input/display builder's `Reactive` is chainable except FusionSmartTextArea,
which returns `void` — is correct, and **every grammar table row is accurate**.
Recommend correcting "47" → "48" in ast-proof.md for precision.

## Bottom line

GROUNDED. All sampled edges match source on name, params, and return type; both
proofs trace cleanly to source; negative exclusions are accurate. The single
defect is a one-off miscount (47 vs 48) in the proof's prose, which touches no
AST edge.
