# ADR-0004: The core is an inverted dependency tree of concept slices

**Status:** accepted
**Date:** 2026-10-29
**Domain:** structure | encapsulation | public namespaces

## Context

The owner's accountability metric is the framework's structure: encapsulation first, then what changes together
living together, an inverted dependency tree and one consistent API. Measured at `9b7c36db9` from the compiled
assemblies (a slice is a source folder; references read from IL, generic arguments included): 24 slices, 21 of
them in one dependency cycle, 136 slice-to-slice dependencies, 386 internal members used from another slice, 74
files whose namespace is not their folder, two concepts split across `PlanAuthoring` and `PlanModel`. A day of
behavior work had left these flat or worse. Fifteen dependencies point up the intended order, among them
`Shape` reflecting over `JsObject`, `FormWriter` reaching into Razor, `RequestStep` depending on the gather
builders, and Validation and the condition model depending on each other.

Two independent designs were made: minimal moves (keeps today's folders and ten public namespaces, leaves an
eight-slice cycle in the authoring layer) and concept-first slices. This ADR takes the second: it is the one that
reaches no cycle at all, and the one a new engineer reads as obviously right.

## Decision

**Layers, bottom up.** A slice uses only slices in lower rows; slices in one row never use each other.

| Row | Slices |
|---|---|
| 0 | `Grammar` (wire nodes, `G`, the serializer and its discriminator), `WireTerms` (plan terms and `Path`) |
| 1 | `Members` (C# member reading: `Expressions`, `ExpressionPathHelper`, `JsonMemberName`, `CSharpTypeName`, `EnumMember`, `Field`, and `ClrShape`, the CLR-type to shape mapping taken out of `Shape`) |
| 2 | `Vendors` (`ComponentVendor`, `VendorEventPayload`, `TypedDslExemptionAttribute`) |
| 3 | `Inputs` (`IdGenerator`, `HtmlFieldPrefix`, `InputIdentity`, `RenderedInput(s)`, `FieldReference`, `MemberInputs`) |
| 4 | `PlanModel/Writing` |
| 5 | `PlanModel/Values` |
| 6 | `PlanModel/Conditions` |
| 7 | `PlanModel/Reactions`, `PlanModel/Validation` (rules, browser fields, checked fields, the form writer, the form summary) |
| 8 | `PlanModel/Requests` (the request step, routes, the parallel step, the gather model) |
| 9 | `PlanModel/Document` (`PlanBuildContext`) |
| 10 | `Components` (declarations, `TypedSource`, `MemberRead`, events, `Plugin`) |
| 11 | `PlanAuthoring/Events`, `PlanAuthoring/Arrays` (with its own value nodes) |
| 12 | `PlanAuthoring/Elements` |
| 13 | `PlanAuthoring/Pipelines` |
| 14 | `PlanAuthoring/Conditions`, `PlanAuthoring/Requests` |
| 15 | `PlanAuthoring` (`ReactivePlan`, `RenderedComponent`) |
| 16 | `InputField` |
| 17 | `Razor` (`Razor/Extensions` merged into it: one MVC boundary) |
| 18 | `Alis.Reactive.FluentValidator` |

**Placement rule.** A type lives in its concept's folder, in the lowest slice that uses it; a caller passes a
value instead of a low slice reaching up. A concept gets a model folder (`PlanModel/<Concept>`) beside its builder
folder (`PlanAuthoring/<Concept>`) only when a slice below its builders uses one of its grammar nodes; then all its
nodes live in the model folder and the model folder names no builder.

**Features extend the pipeline.** The pipeline keeps as members every feature that never calls back into a
pipeline. A feature whose builders take a pipeline callback is reached through an extension class in its own
folder (`p.When`, `p.Confirm` in `ConditionExtensions`; `p.Get`, `p.Post`, `p.Put`, `p.Delete`, `p.Parallel` in
`RequestExtensions`), so the pipeline knows nothing of the features and the features depend on it. Call sites do
not change.

**Public namespaces.** A file that declares a public type has namespace `Alis.Reactive` (or
`Alis.Reactive.FluentValidator`): a page and a component import one namespace. Every other file's namespace is
its folder (`Alis.Reactive.PlanModel.Conditions`). The eight public namespaces `Builders`, `Builders.Conditions`,
`Builders.Requests`, `Builders.Arrays`, `PlanModel`, `Validation`, `InputField` and `Components` are retired.

**The lock.** `tests/Alis.Reactive.ArchitectureTests/CoreLayerTests.cs` reads the type-to-folder map from source
(both `NET48` and `NET10_0`) and the references from IL (generic arguments, attributes, locals and operands
included) of both targets, and fails naming every reference that points up the table, every folder with no row,
every row that names no folder, and every file whose namespace breaks the rule above.

## Consequences

- Every public type moves to `Alis.Reactive`; apps, the sandbox, the examples, Fusion and Native drop the retired
  `using` lines; the analyzers' metadata names follow. Packages ship in lockstep (ADR-0001), so the break is one
  mechanical edit, made before 1.0.
- Seven pipeline methods become extension methods; the analyzer that finds several requests in one pipeline
  compares against the extension's containing type.
- Predicted: 0 slices in a cycle, 0 upward edges, about 360 internal members used across slices. Encapsulation
  is not solved by ordering alone: Grammar's node constructors and `G` (about 179 uses) remain the widest internal
  surface and are the next structural target.
- Each unit is proven by the full gate, `npm run typecheck` with `plan.ts` unchanged, and the structure tool at
  its base and head.

## Sequence

After ADR-0003 waves 2 and 3 land (they rewrite and delete many of the files this moves):
U1 the bottom rows (Grammar, WireTerms, Members, Vendors, Inputs); U2 the plan model (Values, Conditions,
Reactions, Validation, Requests, Document); U3 components and authoring (Components, Events, Arrays, Elements,
Pipelines, the extension classes, Razor); U4 namespaces and the lock. Each lands as pure-rename commits where it
moves files, so history follows them.
