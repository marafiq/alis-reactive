# spec

This folder is the design of the Alis.Reactive rewrite: the core design, its TLA+ specs, the modules mapped
onto it, and the units the rewrite runs in. `spec/BRIEF.md` is the session brief and the authority on what
goes here; `spec/NOTES.md` holds the decisions, facts and owner answers, append-only.

## Standing decisions

- The owner, 2026-11-05: this is a complete rewrite, not a rebuild, and it has to be right.
- The owner, 2026-11-05: no module and no feature on either line is lost, and the session improves every
  aspect. A door whose form is refused names the doors that carry its capability; `spec/tools/check.py`
  prints the features lost, and done is zero.
- The owner, 2026-11-05: Alis.Reactive.NativeTagHelpers and Alis.Reactive.DesignSystem are out of scope on
  both lines (BRIEF.md section 4). Their inventory rows stay, marked out, and no check counts them.
- The owner, 2026-11-05: neither line's public surface is the contract. Main opened the core's internals to
  its own packages, and the redesign repeats the same mistakes; both lines are evidence of features, not of
  the design. A door is public only when a view author, a validator author or a component author needs it.
  A door that is an implementation detail is refused with that reason and the doors that carry its feature.
- The owner, 2026-11-05: any component can be onboarded on the core's public surface. Fusion and Native get
  nothing an app does not.
- The owner, 2026-11-05: the core takes as few dependencies as it can, and each one is the owner's decision.
  Dependency injection is one. Every dependency is pinned to one version.
- The owner, 2026-11-05: the HTTP module is requests and responses. What comes back from the server belongs
  to it: the status, the body, how the browser decodes it, and the error body each host sends.
- The owner, 2026-11-05: work survives compaction. A correction is written here and in `spec/NOTES.md` the
  turn it is given, and the compact hook prints this section.
- The owner, 2026-11-06: the request lifecycle keeps every feature and has a condensed default shape that
  covers most requests; a developer overrides any part. Razor made parameters and conditions in views
  complicated, and a more condensed typed syntax that loses no feature is welcome (step 7, gate 2).
- The owner, 2026-11-06: a request runs in any event or callback; conditions work uniformly in every block
  of a request.
- The owner, 2026-11-06: conditions have had two syntaxes in views; there is one uniform syntax, a branch
  holds several statements, and a branch is a simple lexical scope, as a C# `{}` block is.
- The owner, 2026-11-06: dates and time zones were not done right or to the standards; the spec states each
  date form by the standard it follows and pins each host's reading and writing of it by a probe.
- The owner, 2026-11-06: validation that agrees on the server and in the browser is a must, and much time was
  lost on it. The module uses no reflection, has one uniform syntax, and supports conditional and complex
  validation on both sides.
- The owner, 2026-11-06: culture is the server's, never the browser's: the server is the truth, and every
  culture-dependent read or write of a number or a date in the browser follows the culture the view rendered
  under.
- The owner, 2026-11-06: Syncfusion is one consumer of the framework, its dependency inverted. Syncfusion
  works with camel-case JSON and with other naming, so the framework supports the JSON naming an app
  configures, not camel case alone.
- The owner, 2026-11-06: MVC partial semantics get special attention. No weakly typed passing (ViewData)
  carries plan state. ResolvePlan and the plan primitives were designed to stop poisoning the HTTP request;
  a design that stores plan state on the request is the easy route, and it is refused.
- The owner, 2026-11-06: plugins are an integral part of the design, where customization is needed and a
  better developer experience can be offered; the request lifecycle calls into a plugin in a typed way,
  as every block does.
- The owner, 2026-11-06: "follows the server" means the framework allows the server to decide. Example to
  think through, never a framework concept: a company has many communities, each with its own configured
  time zone. The framework offers the time zone a view renders under as the app's choice, as culture is.
- The owner, 2026-11-06: any JavaScript API can return a promise, so sync and async are decided by the
  declaration, never by what the browser finds.
- For this folder, the root rule against other gates, coverage maps, checklists and proof artifacts does
  not apply, and the matrices are an upfront grid by the owner's decision.
- Facts here come from the code at a SHA, a command's result line, or an oracle's source. A pilot's or
  an agent's draft is a lead until its cites are re-read at the source.

## Modules

The owner, 2026-11-05: "The modules are close to right", with requests and responses as one module. Each
module's folder is its name in PascalCase. A core module's C# is in Alis.Reactive and its browser code in
Alis.Reactive.Assets.

| # | Module | Who sees it | What it owns | Questions |
|---|--------|-------------|--------------|-----------|
| 1 | Triggers and pipelines | view author | on load, on an event, declared app events, dispatch, the steps a pipeline runs | Q11 |
| 2 | Values and sources | view author | the one read, literals, the one conversion, every source at every value position, the page's URL | Q2, Q3, Q7 |
| 3 | Conditions | view and validator author | one condition core behind the pipeline, validator and template doors | Q8 |
| 4 | Array lambdas | view author | Where, Select, Any and the rest over items with dotted members | Q6 |
| 5 | Elements | view author | an element's text, HTML, classes and attributes, by id | Q4 |
| 6 | Requests and responses | view author | the typed request; the response, its status, its body and how the browser decodes it, the error body per host; the flow: success, error, chained, parallel, loading, finally, into | Q9, Q10 |
| 7 | Inputs and ids | view author | an input bound to a member under MVC partials, and the refusals at render | Q14 |
| 8 | Validation | validator author | a rule written once, the server's verdict and the browser's rule in agreement, registration through dependency injection | Q16 |
| 9 | Partials and slots | view author | plan composition by plan id, slot load and unload | Q12, Q14 |
| 10 | Realtime | view author | server push and hub triggers, whose messages are data | Q5, Q11 |
| 11 | Declarations | component author | the live-object and data vocabulary, and which members are required | Q5 |
| 12 | Vendors | component author | vendor data, the one driver, mount and unmount, the bridge to a component's browser code | Q17 |
| 13 | Plugins | component author | the escape hatch: a plugin by name with typed members | Q7 |
| 14 | Schema | internal | the closed node set of the plan grammar and the generated TypeScript contract | Q19 |
| 15 | Paths and wire | internal | the typed path step, a member's four names, encoding and dates per host | Q1, Q2, Q3 |
| 16 | Type knowledge | internal | members, shapes, item types and defaults, without reflection | Q18 |
| 17 | Execution | internal | the evaluator, lanes and time, boot | Q4, Q11 |
| 18 | Ownership and state | internal | the page, a plan and a slot, what each holds, dispose, where state lives | Q12, Q13 |
| 19 | Rendering and delivery | host | rendering a plan, assets by content hash, the net48 and net10 host splits | Q14 |
| 20 | Analyzers | build | the ALIS rules, and the new rules for reflection and vendor contract drift | Q18 |
| 21 | Fusion components | built on the core | the Syncfusion slices, with the reference FusionMultiSelect | Q17 |
| 22 | Fusion templates | built on the core | the Syncfusion template engine and its conditionals | Q15 |
| 23 | Fusion app-level objects | built on the core | FusionConfirm and FusionToast | Q11 |
| 24 | Native components | built on the core | the plain-DOM components, with the native reference | Q17 |
| 25 | Native app-level objects | built on the core | NativeDrawer, NativeLoader and the action link | Q11 |

## Files

| File | What it holds | Written by |
|------|---------------|------------|
| `spec/BRIEF.md` | the session brief, with the owner's corrections | the owner's words |
| `spec/NOTES.md` | one entry per decision, fact or owner answer, two lines at most, with its evidence | appended by hand |
| `spec/tools/check.py` | every check the brief's section 1 names, and a brief mode for the compact hook | hand |
| `spec/STATE.json` | the phase, the step, each check's result and counts, the next action | `spec/tools/check.py` only |

## How a step runs

Re-read the step's section of `spec/BRIEF.md`, do it, run `python3 spec/tools/check.py`, append what was
decided to `spec/NOTES.md`, then commit by path and push the spec branch the one way section 16 of the brief allows.
