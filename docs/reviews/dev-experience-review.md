# API Surface Developer Experience Review

**Reviewer perspective:** .NET MVC developer who has never used Alis.Reactive,
evaluating the API by reading XML documentation (IntelliSense) only.

**Date:** 2026-03-28

---

## File-by-File Analysis

### 1. HtmlExtensions.cs -- The Entry Point

**File:** `Alis.Reactive.Native/Extensions/HtmlExtensions.cs`

**Can I understand what this does from the XML docs alone?** Yes. The summary
("Razor view extensions for adding reactive behavior to a plan") is clear. The
`On<TModel>` method doc explicitly tells me it "adds reactive behavior to plan by
configuring browser triggers and the commands that run when each trigger fires."

**Do the parameter names help?** Mostly yes. `plan` is obvious. `trigger` is named
well since the doc explains it gives you a `TriggerBuilder`. The `html` parameter
is the standard Razor extension pattern.

**Confusing or unclear docs?** The remarks are excellent -- they list all four trigger
types (DomReady, CustomEvent, ServerPush, SignalR) and warn against duplicates. The
chaining example in the `<param name="trigger">` tag
(`t.DomReady(...).CustomEvent(...).SignalR(...).ServerPush(...)`) immediately shows me
the fluent pattern. One minor gap: there is no example showing a complete minimal usage
from `@{ var plan = ... }` through `Html.On(plan, ...)` to `@Html.RenderPlan(plan)`.
A newcomer sees how to call `On` but not how to create or render the plan.

**Could I build a feature using just IntelliSense?** I could write the `Html.On(plan, ...)`
call confidently. I would need to discover `ReactivePlan<TModel>` and `RenderPlan()`
separately since they are not documented here.

**Rating: 8/10** -- Strong entry point docs. Missing the "complete hello world" example.

---

### 2. TriggerBuilder.cs -- What Html.On Gives You

**File:** `Alis.Reactive/Builders/TriggerBuilder.cs`

**Can I understand what this does?** Yes. The class summary says "wires browser triggers
to reactions that execute in the browser" and the remarks show the exact access pattern
(`Html.On(plan, t => t.DomReady(...).CustomEvent(...))`). Each method summary is clear.

**Do the parameter names help?** Excellent. `eventName`, `url`, `eventType`, `hubUrl`,
`methodName` -- all self-documenting. The `pipeline` parameter is consistent across all
overloads, which builds muscle memory.

**Confusing or unclear docs?** Two items stood out:

1. **The `TPayload` phantom pattern is explained, but subtly.** The remark "The
   TPayload instance is used only for compile-time type inference; its property values
   are never read" is technically correct but might confuse someone who expects to pass
   real data. A one-line example showing `t.CustomEvent<OrderPayload>("submitted", (payload, p) => ...)`
   would clarify that `payload` is a phantom for IntelliSense, not a data carrier.

2. **ServerPush and SignalR with typed payloads** mirror the CustomEvent pattern, which
   is good for consistency, but none of the SSE/SignalR methods have usage examples in
   their remarks. DomReady has a remark about dispatch safety -- the others are bare
   summaries.

**Could I build a feature?** Yes. The return type (`TriggerBuilder<TModel>`) signals
chaining. The `Action<PipelineBuilder<TModel>>` parameter tells me exactly what I get
next.

**Rating: 8/10** -- Consistent, clear, chainable. SSE/SignalR methods would benefit
from one example each.

---

### 3. PipelineBuilder.cs -- What DomReady Gives You

**File:** `Alis.Reactive/Builders/PipelineBuilder.cs` (+ `.Http.cs` + `.Conditions.cs`)

**Can I understand what this does?** The class summary is excellent: "Builds the
sequence of commands that execute when a trigger fires: element mutations, event
dispatches, HTTP calls, component interactions, and conditional logic." The remarks
show the exact lambda variable name (`p`) and a concrete example.

**Do the parameter names help?** Yes. `elementId`, `eventName`, `payload`, `refId`,
`formId` -- all speak for themselves. The `expr` parameter on `Component<T>()` matches
the standard MVC expression pattern (`m => m.Address.City`).

**Confusing or unclear docs?**

1. **`Component<TComponent>()` has four overloads.** The docs differentiate them
   ("by model expression", "from a different model", "by string ID", "app-level by
   default ID") which is helpful. However, a newcomer seeing `Component<NativeTextBox>(m => m.Name)`
   would not know what `NativeTextBox` is or where it comes from. There is no `<see cref>`
   pointing to a list of available component types. This is the biggest discoverability
   gap: the pipeline tells you *how* to target a component but not *which* components
   exist.

2. **`Into(string elementId)`** -- the doc says "injects the HTTP response body as inner
   HTML" but gives the example in the HTTP context (`p.Get("/url").Response(r => r.OnSuccess(s => s.Into(...)))`).
   Seeing `Into` as a top-level pipeline method might confuse someone who does not realize
   it is only meaningful after an HTTP response. The XML doc does not say "must be used
   inside a response handler." (Though looking at the code, it seems like it *can* be
   called at the top level, which is even more confusing.)

3. **`ValidationErrors(string formId)`** -- the summary "displays server-side validation
   errors returned in the 400 response body" is clear but the relationship to
   `HttpRequestBuilder.Validate()` is not cross-referenced. A newcomer would wonder:
   "Do I need both?"

**HTTP partial (`PipelineBuilder.Http.cs`):** The HTTP methods (`Get`, `Post`, `Put`,
`Delete`, `Parallel`) have minimal summaries ("Starts a GET request to the given URL").
These are fine for IntelliSense but lack examples. The `Post` overload that takes a
`gather` parameter is convenient but the doc does not explain what "gather" means.
Someone unfamiliar with the term would not know this collects form field values.

**Conditions partial (`PipelineBuilder.Conditions.cs`):** This is the best-documented
section I have seen. The class-level remark shows a complete When/Then/ElseIf/Else
chain. The `When` method doc explains that multiple `When` calls produce independent
blocks, with a code example. The `Confirm` method has a complete usage example and the
warning "Never use in DomReady." Outstanding.

**Could I build a feature?** For simple features (show/hide an element, dispatch an
event, make an HTTP call) -- yes. For component interactions, I would need to go
searching for the component type names and their available methods.

**Rating: 7/10** -- Excellent for core commands and conditions. Weak on component
discoverability and the "gather" concept.

---

### 4. Component HTML Extensions

#### FusionDropDownListHtmlExtensions.cs

**Can I understand what this does?** The class summary says "Creates a FusionDropDownList
inside a field wrapper, bound to a model property." The remarks show the chain:
`Html.InputField(plan, m => m.Country).FusionDropDownList(b => ...)`. This is clear.

**Parameter names:** `text`, `value`, `groupBy` on the `Fields<TItem>` method -- all
excellent. The `build` callback parameter name is generic but acceptable.

**Confusing docs?** The `FusionDropDownList<TModel, TProp>` method doc says the
`build` callback "configures the FusionDropDownList (data source, fields, etc.)" but
does not list what methods are available on `DropDownListBuilder`. Since
`DropDownListBuilder` is a Syncfusion type, this is partly out of scope, but a
`<see cref>` to the typed `Fields<TItem>` extension would help close the loop.

**Could I build a feature?** Yes, the chain `Html.InputField(...).FusionDropDownList(b => { b.Fields<Item>(...); })`
is discoverable from IntelliSense alone.

**Rating: 8/10**

#### NativeTextBoxHtmlExtensions.cs

**Can I understand what this does?** Yes. The remarks show a complete chain from
`Html.InputField` through `.NativeTextBox(b => b.Placeholder("Enter name"))`. This
is the gold standard for component docs -- I can see the full path.

**Parameter names:** `setup` and `build` are clear enough. The `build` callback
receives a `NativeTextBoxBuilder<TModel, TProp>` which tells me what to expect.

**Confusing docs?** The `build` param doc says "Configures the text box (type,
placeholder, CSS, reactive events)." This is a helpful hint about what the builder
can do. No issues.

**Could I build a feature?** Yes, trivially.

**Rating: 9/10** -- The full code example in remarks is exactly what a newcomer needs.

---

### 5. Component Reactive Extensions

#### FusionDropDownListReactiveExtensions.cs

**Can I understand what this does?** Yes. The remarks show a complete example:
`.Reactive(plan, evt => evt.Changed, (args, p) => { ... })`. The summary "wires
browser events from a FusionDropDownList into the reactive plan" is clear.

**Parameter names:** `plan`, `eventSelector`, `pipeline` -- all strong. The
`eventSelector` parameter is the cleverest piece of the API: `evt => evt.Changed`
gives IntelliSense-driven event discovery. The doc explains this with the example.

**Confusing docs?** The remark ".Reactive() is always the last call inside the build
callback" is a critical ordering constraint. This is documented well. However, the
`<param name="builder">` is documented as "The Fusion builder" which is generic.
Since this is an extension method, the user never passes this explicitly, so it is
a minor issue.

**Could I build a feature?** Yes. The event selector pattern (`evt => evt.Changed`)
means I can discover available events through IntelliSense. This is excellent API
design.

**Rating: 8/10**

#### NativeTextBoxReactiveExtensions.cs

**Can I understand what this does?** Yes. Same pattern as Fusion. The remarks show
a complete chain with `.Reactive(plan, evt => evt.Changed, (args, p) => ...)`.

**Parameter names:** Same strong pattern. The `eventSelector` is
`Func<NativeTextBoxEvents, TypedEventDescriptor<TArgs>>` which is type-safe.

**Confusing docs?** The remark about "Native builders implement IHtmlContent directly,
so no separate .Render() is needed" is useful internal context but might confuse a
newcomer who has not been told about `.Render()` in the first place. Minor issue.

**Could I build a feature?** Yes.

**Rating: 8/10**

---

### 6. HTTP Pipeline Builders

#### HttpRequestBuilder.cs

**Can I understand what this does?** Partially. The class itself has **no XML summary
or remarks**. The individual methods have docs but the class-level story is missing.
A newcomer arrives at this type via `p.Get("/url")` and sees methods like `Gather`,
`WhileLoading`, `Response`, `Validate`, `AsJson`, `AsFormData` -- but there is no
class-level doc explaining the HTTP pipeline lifecycle or the typical call order.

**Parameter names:** `url` is obvious. `gather`, `pipeline`, `response` are
descriptive enough. `formId` on `Validate<TValidator>` makes sense.

**Confusing docs?**

1. **`Gather` method:** The doc says "Adds gather items for the request body or URL
   params" but the term "gather" is framework-specific jargon. A newcomer would not
   know that "gather" means "collect values from form components." The
   `GatherBuilder` methods (`IncludeAll`, `Static`, `FromEvent`) are documented but
   the `Include` method for individual components is missing from the core
   `GatherBuilder` -- it appears to live on vendor extensions. This is a discoverability
   gap.

2. **`WhileLoading` method:** The doc "Configures commands to execute while the
   request is in-flight. These commands are reverted after the response arrives" is
   outstanding. The "reverted" behavior is the kind of detail a developer needs to
   know and it is stated clearly.

3. **`Validate` has two overloads.** The first takes a `ValidationDescriptor` (how do
   I create one?), the second takes a `TValidator` type and `formId`. The second is
   more discoverable but the doc says "Rules are extracted automatically at Render()
   time via IValidationExtractor" which assumes knowledge of the validation pipeline.

4. **The four convenience verbs** (`Get`, `Post`, `Put`, `Delete`) on the builder
   itself have **no XML docs at all**. These appear to be used inside `Chained` and
   `Parallel` lambdas but a newcomer would not know that without reading the code
   comment.

**Could I build a feature?** For a simple GET with a response handler, yes. For
anything involving `Gather` with components, I would struggle to discover the
component-specific gather extensions.

**Rating: 5/10** -- Missing class-level summary, undocumented convenience verbs,
"gather" jargon unexplained.

#### ResponseBuilder.cs

**Can I understand what this does?** Partially. Again, **no class-level XML summary**.
The methods are documented: `OnSuccess`, `OnError`, `Chained`.

**Parameter names:** `statusCode` and `pipeline` are clear. The typed `OnSuccess<TResponse>`
has a good doc explaining the `ResponseBody<T>` phantom pattern ("enables compile-time
path walking into the response body").

**Confusing docs?**

1. **The `OnSuccess<TResponse>` usage comment** shows the expression path pattern
   (`json, s) => s.Element("x").SetText(json, r => r.Data.Name)`) which is extremely
   helpful. This is one of the best docs in the codebase -- it shows input, output,
   and what gets generated at runtime.

2. **`Chained` method:** "Chains a sequential HTTP request that fires after the
   current request succeeds." Clear enough, but no example showing the pattern.

3. **`BuildHandler` (private):** Has a doc but it is internal implementation detail.
   Not relevant to IntelliSense.

**Could I build a feature?** Yes for success/error handling. The typed response
pattern is discoverable.

**Rating: 6/10** -- Missing class summary. `OnSuccess<T>` is exemplary, the rest
is bare.

---

### 7. Conditions Builders

#### GuardBuilder.cs

**Can I understand what this does?** Yes. The class summary "Composes guard conditions
with And, Or, and Not, then terminates with Then" is precise. The remarks section is
exceptional -- it shows three distinct patterns (compound And, lambda And for mixed
boolean logic, and Not) with complete code examples.

**Parameter names:** `payload`, `path`, `source`, `inner`, `pipeline` -- all clear
and consistent with the rest of the API.

**Confusing docs?**

1. **The `And<TPayload, TProp>` vs `And(Func<...>)` distinction** is documented well.
   The flat form is for simple chaining; the lambda form is for mixed And/Or. The
   remark "Chained .And().And() calls flatten into a single guard group rather than
   nesting" is an important optimization detail that would help a power user.

2. **`Then` method:** The doc is comprehensive. It shows the full
   When/Then/ElseIf/Then/Else chain, documents the return type, and notes the exception
   for orphaned lambdas. This is the highest-quality method doc I have reviewed.

3. **The `TypedSource<TProp>` overloads** (`And<TProp>(TypedSource<TProp> source)`)
   are documented clearly: "Adds an AND condition from a component's current value."
   The `source` param doc says "A typed source from a component's Value() extension"
   which tells me how to get one.

**Could I build a feature?** Yes. The examples in remarks make the full pattern clear.

**Rating: 9/10** -- Exemplary. The code examples in remarks are exactly what a
developer needs.

#### BranchBuilder.cs

**Can I understand what this does?** Yes. "Chains additional branches after an initial
When().Then() block using ElseIf and Else." The remarks show a complete 4-branch
grading example.

**Parameter names:** `payload`, `path`, `source`, `pipeline` -- consistent with
everything else.

**Confusing docs?** None. The `Else` method doc explains "Must be the last branch.
Only one Else is allowed" and throws `InvalidOperationException` with a clear message
if violated. The `ElseIf` throws the same exception if called after `Else`. This is
defensive API design with helpful error messages.

**Could I build a feature?** Yes, trivially. The grading example is a complete pattern.

**Rating: 9/10**

---

### 8. Supporting Files (Bonus)

#### ConditionSourceBuilder.cs

This is the bridge between `When()` and the operators. The class-level doc is
outstanding: it groups all operators by category (Comparison, Presence, Membership,
Range, Text, Array, Source-vs-source) and shows examples for each. Every operator
method has a one-line summary and returns `GuardBuilder<TModel>`. The parameter names
(`operand`, `substring`, `prefix`, `suffix`, `pattern`, `length`, `low`, `high`, `item`)
are all domain-appropriate.

**Rating: 10/10** -- This is the best-documented file in the codebase.

#### ElementBuilder.cs

Every mutation method has a clear summary. The overloaded `SetText` methods distinguish
between static strings, event payload sources, response body sources, `BindSource`,
and `TypedSource<TProp>`. The remarks on `SetText(BindSource)` helpfully point to the
typed overload: "Prefer the SetText<TProp>(TypedSource<TProp>) overload when a
component's Value() method is available."

**Rating: 9/10**

#### InputFieldExtensions.cs

The `InputField` method doc shows the full chain and explains that the result
receives a component extension. The `options` parameter is optional with a clear
doc.

**Rating: 8/10**

#### InputFieldOptions.cs

Minimal but complete. `Required()` and `Label()` are self-documenting. The remarks
show the exact usage pattern.

**Rating: 9/10**

#### GatherBuilder.cs

The `IncludeAll`, `Static`, and `FromEvent` methods have clear docs. However, the
class has no summary, and the most common operation -- gathering a specific named
component -- appears to live on vendor extensions that are not referenced here.

**Rating: 5/10** -- Missing class summary, missing cross-reference to vendor gather
extensions.

---

## Overall Assessment

### Discoverability Rating: 7/10

### What Works Exceptionally Well

1. **The fluent chain is self-documenting.** `Html.On(plan, t => t.DomReady(p => p.Element("x").AddClass("active")))`
   reads like English. Each step returns the right builder type, so IntelliSense
   naturally guides the developer forward.

2. **The conditions DSL is a model of API documentation.** `ConditionSourceBuilder`,
   `GuardBuilder`, and `BranchBuilder` have class-level remarks with categorized
   operator lists and multiple complete code examples. A developer could write
   complex When/Then/ElseIf/Else chains from IntelliSense alone.

3. **The event selector pattern** (`evt => evt.Changed`) on `.Reactive()` is brilliant
   for discoverability. Instead of passing string event names, you get IntelliSense
   completion on the available events for each component type.

4. **The phantom type pattern** is explained wherever it appears. "Used only for
   compile-time type inference; its property values are never read" appears on
   TriggerBuilder, ElementBuilder, and ResponseBuilder consistently.

5. **Parameter names are consistently excellent.** `elementId`, `eventName`, `className`,
   `pipeline`, `operand` -- they are domain-appropriate and self-documenting throughout
   the entire API surface.

6. **Component HTML extensions** show full chain examples in remarks. NativeTextBox
   shows `Html.InputField(plan, m => m.Name, o => o.Required().Label("Name")).NativeTextBox(b => b.Placeholder(...))`.
   This is exactly what a newcomer needs.

### Specific Gaps

1. **No "hello world" at the entry point.** `HtmlExtensions.On()` tells you how to
   add triggers but does not show plan creation (`new ReactivePlan<TModel>()`) or
   rendering (`@Html.RenderPlan(plan)`). A newcomer's first question is "what do I
   need before I can call this?" and the answer is not in the IntelliSense.

2. **HttpRequestBuilder and ResponseBuilder lack class-level summaries.** These are
   major API types that a developer navigates to from `p.Get()` / `p.Post()`. The
   methods are documented but the class-level "what am I looking at?" is missing.
   Adding a summary like "Configures an HTTP request: URL, body, loading state,
   validation, and response handlers" would orient the developer immediately.

3. **GatherBuilder lacks a class-level summary and the word "gather" is jargon.**
   The concept of collecting component values into a request body is central to the
   HTTP pipeline, but "gather" is not a standard .NET term. A summary like "Collects
   values from form components, event payloads, and static data to build the HTTP
   request body" would bridge the vocabulary gap.

4. **The four convenience verbs on HttpRequestBuilder (Get/Post/Put/Delete) have no
   XML docs.** These are used inside `Chained` and `Parallel` contexts. Without docs,
   a developer does not know they exist or when to use them vs. the pipeline-level
   `p.Get()`.

5. **Component type discoverability is missing.** `p.Component<TComponent>(...)` tells
   you the generic constraint (`IComponent, new()`) but does not list or link to the
   available component types. A developer must already know that `NativeTextBox`,
   `FusionDropDownList`, etc. exist. An `<seealso>` section listing the common
   component types would close this gap.

6. **The relationship between `Into()`, `ValidationErrors()`, and the HTTP pipeline is
   implicit.** Both methods appear as top-level pipeline commands but only make sense
   in an HTTP response context. The docs should note "Typically used inside
   `.Response(r => r.OnSuccess(s => ...))` handlers."

7. **`PipelineBuilder.Http.cs` methods are bare one-liners.** The main `Post(string url)`
   has no example and does not mention gather or response handling. Adding one remark
   showing the typical pattern `p.Post("/api/save", g => g.IncludeAll()).Response(r => r.OnSuccess(...))`
   would dramatically improve first-encounter discoverability.

### Summary Table

| File | Rating | Key Strength | Key Gap |
|------|--------|-------------|---------|
| HtmlExtensions (entry) | 8/10 | Lists all trigger types | No plan creation/render example |
| TriggerBuilder | 8/10 | Consistent chainable pattern | SSE/SignalR lack examples |
| PipelineBuilder (core) | 7/10 | Conditions docs are exemplary | Component discoverability |
| PipelineBuilder (HTTP) | 5/10 | Methods exist | No class summary, no examples |
| PipelineBuilder (Conditions) | 9/10 | Full When/Then/Else example | None |
| FusionDropDownList HTML | 8/10 | Fields<TItem> is typed | No list of builder methods |
| NativeTextBox HTML | 9/10 | Full chain example in remarks | None |
| FusionDropDownList Reactive | 8/10 | Event selector is brilliant | Minor: "Fusion builder" param |
| NativeTextBox Reactive | 8/10 | Consistent pattern | IHtmlContent remark may confuse |
| HttpRequestBuilder | 5/10 | WhileLoading is well-documented | No class summary, undocumented verbs |
| ResponseBuilder | 6/10 | OnSuccess<T> is exemplary | No class summary |
| GuardBuilder | 9/10 | Three patterns with examples | None |
| BranchBuilder | 9/10 | Complete grading example | None |
| ConditionSourceBuilder | 10/10 | Categorized operator list | None |
| ElementBuilder | 9/10 | All overloads documented | None |
| GatherBuilder | 5/10 | IncludeAll/Static are clear | No class summary, "gather" jargon |
| InputFieldExtensions | 8/10 | Full chain example | None |
| InputFieldOptions | 9/10 | Usage example in remarks | None |

### Verdict

The conditions/branching layer and the component wiring layer are documentation
exemplars. A developer could write complex conditional UI logic and wire component
events purely from IntelliSense. The HTTP pipeline layer is the weak link: missing
class summaries, undocumented convenience verbs, and the unexplained "gather"
vocabulary create a cliff where the guided experience breaks down. Fixing the 7
specific gaps above (particularly items 1, 2, 3, and 7) would push the overall
rating from 7 to 9.
