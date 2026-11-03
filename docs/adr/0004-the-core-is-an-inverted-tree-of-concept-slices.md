# ADR-0004: The core is an inverted dependency tree of concept slices

**Status:** accepted; implemented (U1 74fcb65f6, U2 e66dc0ae3, U3 d2cbb0138, U4 0ecfbaf8c)
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
| 18 | `Validation` (`ReactiveValidator<T>`, the FluentValidation grammar, its checks and messages, `AddReactiveFluentValidation`) |

**Placement rule.** A type lives in its concept's folder, in the lowest slice that uses it; a caller passes a
value instead of a low slice reaching up. A concept gets a model folder (`PlanModel/<Concept>`) beside its builder
folder (`PlanAuthoring/<Concept>`) only when a slice below its builders uses one of its grammar nodes; then all its
nodes live in the model folder and the model folder names no builder.

**Features extend the pipeline.** The pipeline keeps as members every feature that never calls back into a
pipeline. A feature whose builders take a pipeline callback is reached through an extension class in its own
folder (`p.When`, `p.Confirm` in `ConditionExtensions`; `p.Get`, `p.Post`, `p.Put`, `p.Delete`, `p.Parallel` in
`RequestExtensions`), so the pipeline knows nothing of the features and the features depend on it. Call sites do
not change.

**Public namespaces.** A file that declares a public type has its package's namespace (`Alis.Reactive` in the
core): a page, a component and a validator import one namespace. Every other file's namespace is
its folder (`Alis.Reactive.PlanModel.Conditions`). The eight public namespaces `Builders`, `Builders.Conditions`,
`Builders.Requests`, `Builders.Arrays`, `PlanModel`, `Validation`, `InputField` and `Components` are retired.

**The packages.** Each shipped package is the same kind of tree with rows of its own, in its `SliceTree.txt`. A row
entry `X/*` makes every subfolder of `X` a slice of its own in that row, so slices under one folder never use
each other; `.` is the package's root files.

| Package | Rows, bottom up |
|---|---|
| `Alis.Reactive.Fusion` | `.` (`FusionComponent`, the vendor base, `AddReactiveFusion`); `Templates`; `AppLevel/*`, `Components/*` (one slice per component, none using another) |
| `Alis.Reactive.Native` | `.` (`NativeComponent`, the vendor base); `AppLevel/*`, `Components/*` (one slice per component, none using another) |
| `Alis.Reactive.NativeTagHelpers` | `.` (the spacing enums and the class joiner); `*` (one slice per tag helper) |

**Sealed.** A public class that no type in its package derives from is sealed; the open classes are the bases with a
subclass beside them (`ReadOnly<T>` under `Property<T>`, `InputBoundFieldBase`, `Event<TArgs>`, the vendor bases).
Features extend the pipeline through extension classes, not subclasses, and a brought component derives the core's
`Component` and `JsObject`, never a shipped declaration or payload.

**The lock.** Analyzer ALIS010-ALIS013 (`Alis.Reactive.Analyzers/SliceTree`) runs in every build of each shipped
package, for each target, and in the editor. A package's rows are its `SliceTree.txt`, bottom row first, listed as
an additional file in its csproj; the file's directory is the package root. The analyzer fails the build naming
every reference that points up the table or sideways in a row, at the reference; every file whose namespace
breaks the rule above, at the namespace; every public class that nothing in the package derives from and that is
not sealed, at the class; and every folder with no row, every entry that names no folder, every type declared in
two slices and every shipped package without a table. `tests/Alis.Reactive.AnalyzerTests/SliceTreeTests.cs` proves
each rule fires and stays silent. The IL test that preceded it (Mono.Cecil over both targets) is retired: what the
compiler's model reads that IL did not (a const read, a nameof) is a real source dependency, and what IL read that
the model does not (the members the compiler binds by pattern: foreach, await, using, deconstruction) lives on a
type the source names directly or on a slice below it, so no upward edge is lost. The counts the test printed
(files, slices, edges per package) are printed by nothing now; files and slices are countable from the tree, the
edge count is not.

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
Pipelines, the extension classes, Razor); U4 namespaces and the lock; U5 the validator as the top slice; U6 the packages take the rule one commit each,
smallest first (DesignSystem, NativeTagHelpers, Native, Fusion), each adding its rows to the lock's table; U7 the lock as the analyzer, the IL test retired. Each
lands as pure-rename commits where it moves files, so history follows them.
