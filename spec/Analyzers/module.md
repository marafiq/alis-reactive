# Analyzers

The build's rules: the compiler's rung of D18.6, below the type stage and above the render, the analyzer package that
ships inside the core, and the verdict of every analyzer of both lines. Module 20 of `spec/CLAUDE.md`.

## Core questions

- Q18: the framework runs no reflection of its own, and the reflection analyzer reads operations, not names, so
  `new T()` over a type parameter is an `Activator` read (D18.1, D18.4, N18.1); every door on the public surface is
  typed (D18.5, ALIS009); a rule the source states sits on its highest rung, the analyzer when no type can stage it,
  each analyzer running the check the render keeps for code it does not follow, and every analyzer of both lines has
  its verdict (D18.6).
- Q17: a declaration is checked at build against its vendor's TypeScript, the vendor contract and its type check
  (D17.5, N17.2).

## Boundaries

A rule's meaning and its defects are the module whose question states the rule: the analyzer families of D18.6 hold
defects placed with modules 2 to 13, and X18.1, X18.2 and X18.9 are M16.F1's. This module owns the analyzers' code,
their ids, the package that carries them and ALIS009's exemption. X8.2 is M3.F1's; X17.32 is the action link's, module
25's, and X20.6 is tracing's, module 26's. A compilation's own slice table, `SliceTree.txt`, is the package's; ALIS010
to ALIS013 read it.

The analyzer ships inside the core package under `analyzers/dotnet/cs` on both lines
(`Alis.Reactive/Alis.Reactive.csproj:62-64` at 623dd4a27, `Alis.Reactive.Analyzers/Alis.Reactive.Analyzers.csproj:21`
at 65fdf118d), so every compilation that references the core runs it; this module keeps that.

## Doors

Each name is gate 2's; each door is the capability it carries.

Kept from the core, each the analyzer that holds its rule:

- `N18.1` the reflection analyzer, with the owner's exception list (D18.4).
- `N18.2` the declaration analyzer, ALIS023 and ALIS024 (D18.3).
- `N18.5` the override analyzer, ALIS025 (D18.3).
- `N18.6` ALIS004, retargeted from the lambdas of Razor files
  (`Alis.Reactive.Analyzers/ControlFlow/ControlFlowInReactiveCallbackAnalyzer.cs:98` at 65fdf118d) to every block of
  `Pipeline<TModel>` in every file.
- `N18.7` ALIS030, the request analyzer, which carries ALIS007's second `Chained` as a request written twice.
- `N18.8` ALIS031, `ShowValidationErrors` in a block no route encloses.
- `N18.9` ALIS032, the block analyzer; `N18.10` ALIS033, the lambda analyzer; `N18.12` ALIS034, the declaration
  names analyzer.
- `N18.11` ALIS006, kept as an error, the validator analyzer; main reports it as a warning
  (`Alis.Reactive.Analyzers/Validation/ServerOnlyConditionAnalyzer.cs:24` at 623dd4a27).
- `N18.13` ALIS035, the row-action analyzer: a `NativeActionLink` whose block's one request at its own level is no
  GET, whose route or query value is neither a C# value nor `Row.Read(..)` of its payload, or that sends `Form`,
  `Inputs`, `Input` or a header (D17.15).
- `N17.2` the vendor contract analyzer, ALIS014 to ALIS022 and the type check ALIS037 (D17.5).
- `N16.5` ALIS038, the flags analyzer; `M1.N6` ALIS039, the app event analyzer.
- `N18.14` ALIS040, the nullable source analyzer: a source whose nullability differs from its argument's or operand's
  is an error in every app (D7.3).
- `N6.3` `ReadsNothingSuppressor`, ALIS026 and ALIS027, and `N9.5` `RequestKeysSuppressor`, ALIS036, the two
  suppressors whose meanings are modules 4's and 6's; `N9.4` ALIS029, a file-shaped member in a GET or a JSON body;
  ALIS028, `ITypedSource<T>`'s analyzer (N6.4).

- `M20.N1` kept: the redesign's `TypedDslExemptionAttribute(reason)`, public on both target frameworks, the one
  exemption from ALIS009, on a method, a constructor or a property (M20.D1). It replaces main's internal attribute of
  the same name (`Alis.Reactive/Components/TypedDslExemptionAttribute.cs:13` at 623dd4a27), which only the framework's
  own packages could carry. Fusion carries it on its column rules and the core on `ComponentVendor`'s two JavaScript
  name doors (`Alis.Reactive/Vendors/ComponentVendor.cs:77` and `:108` at 65fdf118d), and an app's brought component
  carries it as they do.

The meanings `M20.D1` to `M20.D6` and the defects `M20.X1` to `M20.X6` are in `spec/Analyzers/features.json`, on the
feature they give. `M20.D2` is the slice table's whole syntax, and `M20.D3` to `M20.D6` are the four rules that read
it, each with its messages.

One id, one rule. Each analyzer id of both lines and the rewrite:

| Ids | Rule | Verdict |
|-----|------|---------|
| ALIS001 | a `When` given no `Then` | kept, every block and file (D18.6) |
| ALIS002 | main's one request per action link | kept as ALIS008 on the action's block (D11.10, D18.6) |
| ALIS003 | a second `Reactive` for one event | refused: two handlers are D11.5's meaning (D18.6) |
| ALIS004 | control flow in a block | kept, retargeted (N18.6) |
| ALIS005 | a rule only the server runs, as information | kept on `ReactiveValidator<T>` (D18.6) |
| ALIS006 | a condition with no browser form | kept as an error (N18.11) |
| ALIS007 | a second `Chained` | the chain does not compile; ALIS030 carries a reuse (N18.7) |
| ALIS008 | two requests in one block | kept, retargeted to every block and file (D9.17, N9.9) |
| ALIS009 | an untyped public door | kept (D18.5), scoped by M20.D1, exempted by M20.N1 |
| ALIS010 | a use of a slice on a later row, or on the file's own row | kept for each project that carries a slice table (D18.6, M20.D3) |
| ALIS011 | a namespace other than the project's root namespace and the file's first folder | kept; the redesign's split by a file's public types leaves (M20.D4) |
| ALIS012 | a public class nothing derives from, left unsealed | kept (M20.D5) |
| ALIS013 | a slice table that disagrees with its project | kept; it reads the AlisSliceTree build property where the redesign names packages (M20.D6, M20.X1) |
| ALIS014 to ALIS022, ALIS037 | the vendor contract | new (N17.2) |
| ALIS023, ALIS024 | a declaration door's body and constants | new (N18.2) |
| ALIS025 | an override's JSON name and ignore | new (N18.5) |
| ALIS026, ALIS027, ALIS036 | the suppressors | new (N6.3, N9.5) |
| ALIS028, ALIS029 | a typed source; a file member | new (N6.4, N9.4) |
| ALIS030 to ALIS035 | the request, `ShowValidationErrors`, blocks, lambdas, declarations, a row's link | new (N18.7 to N18.13) |
| ALIS038, ALIS039 | the flags door; two app events of one name | new (N16.5, M1.N6) |
| ALIS040 | a source whose nullability differs from its argument's or operand's | new (N18.14) |

## Improvements

- ALIS009 names no consumer (M20.D1, M20.X1). Main checks only the namespaces `Alis.Reactive.Fusion.Components` and
  `Alis.Reactive.Native.Components` (`Alis.Reactive.Analyzers/TypedDsl/UntypedComponentApiAnalyzer.cs:87-93` at
  623dd4a27); the redesign names Fusion, Native and NativeTagHelpers by assembly name
  (`Alis.Reactive.Analyzers/AnalyzerHelpers.cs:13-19` at 65fdf118d), and so does its slice tree's missing-table check
  (`Alis.Reactive.Analyzers/SliceTree/SliceTreeAnalyzer.cs:94` at 65fdf118d). The core names its consumers, and an
  app's own component package gets no whole-surface check.
- An exemption says why (M20.X2). The redesign exempts on the attribute's name alone
  (`Alis.Reactive.Analyzers/TypedDsl/UntypedComponentApiAnalyzer.cs:124-133` at 65fdf118d), so an empty reason
  exempts as a full one does.
- An exemption is public (M20.N1). Main's is internal, and its analyzer matches it by name in the Components namespace
  (`Alis.Reactive.Analyzers/TypedDsl/UntypedComponentApiAnalyzer.cs:75-85` at 623dd4a27), so an app's component has no
  door to the exception Fusion takes.
- A block is checked in every file (N18.6, D9.17). Both lines check ALIS004 and ALIS008 in Razor files only
  (`Alis.Reactive.Analyzers/ControlFlow/ControlFlowInReactiveCallbackAnalyzer.cs:98` at 65fdf118d), so a block a view
  hands to a helper method in a `.cs` file is checked by nothing.
- One id is one rule. The POC gives ALIS025 to two analyzers, the override analyzer
  (`poc/Alis.Reactive.Poc.Analyzers/OverridesAnalyzer.cs:10`) and the type check
  (`poc/Alis.Reactive.Poc.Analyzers/Rules.cs:20`); the rewrite numbers the type check ALIS037 (D17.5).
