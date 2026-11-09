# Array lambdas

A list's operations, `Where`, `Select`, `OrderBy`, `OrderByDescending`, `Count`, `Any`, `All`, `Sum` and `Find`,
each over a lambda on the item's dotted members: over a list of data the server wrote or a live object holds, and over
a list of live objects a declaration declares. Module 4 of `spec/CLAUDE.md`.

## Core questions

- Q1: no read carries `each` (D1.5); a projection is `map` (D6.1), and a path's index is a non-negative integer
  (D1.1), the position `At(n)` reads.
- Q2: a data lambda names each member as Q2 names it, from the expression tree (D6.8).
- Q4: a live item's member the item lacks stops its pipeline (D4.1); data an item lacks reads its start (D4.2); a
  `find` that matches nothing reads nothing (D4.9); an async method's result inside a lambda is refused (X4.12).
- Q5: an item of a live list is a live object (D5.1), with presence and nullability from its declaration (D5.3); the
  list is what `LiveObjects<T>()` or `ReturnedObjects<T>()` declares (N5.5, N5.2).
- Q6: every operation and its meaning (D6.1 to D6.16). D6.16 holds for every `Select`, a live one included: an
  optional member its item lacks is null in its list when its type holds null and stops the pipeline when it holds
  none (M4.X9).
- Q7: an operation's result is a source at every value position (D7.2), in the live form (D7.7).
- Q8: a live list's predicate is a `TypedSource<bool>` built by N8.2's comparisons, and a comparison over a value that
  reads nothing reads nothing (D8.9).
- Q18: the identity table of translated members (D6.5) and the read of a value the view holds (D6.7) are the owner's
  listed reads (D18.1).

## Doors

Module 4 adds no door of its own: every capability its doors carry is a core door.

- `p.From(source)` leaves, all five overloads: the operations are extension methods on `ITypedSource<IEnumerable<T>>`
  (N6.1, N6.4, D6.10), so they extend the source itself, any list a model declares, and no door starts them. Main's
  `From(args, e => e.Value)` is a payload read, `Read` (N5.6), followed by the operation.
- `ReactiveArray<TElement>` and `ReactiveValue<TValue>` leave: each operation returns a `TypedSource<T>` (D6.10, D7.2),
  a list as `TypedSource<IReadOnlyList<T>>`, a count, a sum, a test or a found item as the source of its type. D6.2
  keeps `ReactiveArray`'s names, as Q6's doors state, and N6.1 moves them onto the source. Main's
  `AsSource` leaves with them, since the list is a source.
- `ListPropertyExtensions.Each` leaves, renamed `Select` (N6.1, D6.2), whose field reads an item's absent member as
  its start (M4.X2).
- `Objects<TItem>` is `LiveObjects<TItem>`, N5.5's name: its `Count` property and `Each` become `Count()` and `Select`
  (N6.1), `At(n)` and `Where` stay beside them (N6.1), and `Find` reads the first item kept (D6.9). The type is module
  4's: its three doors are list operations N6.1 names, and its meanings are D6.8's and D6.9's. The door that declares
  it, `JsObject.Objects<TItem>()`, is the declaration vocabulary's and stays with `JsObject` in module 11.
- A live list's operations are the set POC 4 builds over a delegate on the item's declaration: `At(n)`, `Count()` and
  `Count(predicate)`, `Where`, `Select`, `Any()` and `Any(predicate)`, `All`, `Find(predicate)` and
  `Find(predicate, field)` (`poc/Alis.Reactive.Poc/Components/LiveObjects.cs:24-58`). A live list sums or sorts
  through `Select`, whose list is data in the live form (D7.7): `list.Lines.Select(line => line.Units).Sum(units =>
  units)`. N6.1 names `Count()`, `Select`, `At(n)` and `Where`; gate 2 confirms the rest of the set by name.
- `PureMethodAttribute`, internal and in no inventory row, leaves: N6.2's closed table keys each member by its
  definition (D6.5).

## Improvements

- A list that reads nothing reads nothing (D6.3), so a condition on its `Any()` does not hold and its `Count()` shows
  nothing. Both lines read it as a list of no item, and a text or a number as a list of one
  (`Alis.Reactive/PlanAuthoring/Arrays/ArrayOperations.cs:106-110`, `Alis.Reactive.Assets/runtime/grammar/operators.ts:86-93`
  at 65fdf118d; `Alis.Reactive.Assets/runtime/values/array-op-engine.ts:108-116` at 623dd4a27), so a missing list
  counts 0 (X6.4, X6.7).
- `Sum` agrees with .NET for each number type (D6.11). Both lines add a term that is not a finite number as 0 and add
  decimals in binary floating point (`Alis.Reactive/PlanAuthoring/Arrays/ArrayOperations.cs:171-178`,
  `Alis.Reactive.Assets/runtime/grammar/operators.ts:99-102` at 65fdf118d;
  `Alis.Reactive.Assets/runtime/values/array-op-engine.ts:25-27`, `Alis.Reactive.Assets/runtime/values/array-op-engine.ts:118-121`
  at 623dd4a27), so 0.1 and 0.2 sum to 0.30000000000000004, a null term adds nothing where .NET throws, and an `int`
  total runs past its range (X6.9, X6.47).
- `OrderBy` orders as .NET does (D6.13). Both lines turn a key that is not a number into text and compare by code unit,
  so a null key orders among text after the numbers (`Alis.Reactive.Assets/runtime/grammar/operators.ts:104-117` at
  65fdf118d, `Alis.Reactive.Assets/runtime/values/array-op-engine.ts:123-135` at 623dd4a27); the core orders null
  first and text by the view's culture (X6.27, X6.32).
- The text members agree with .NET (D6.5). The redesign maps `ToUpperInvariant` and `ToLowerInvariant` to
  JavaScript's `toUpperCase` and `toLowerCase` (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:24-26`
  at 65fdf118d), which map ß to SS and a final Σ to ς (X6.19), and scans the item's type for `PureMethodAttribute`
  (`Alis.Reactive/PlanAuthoring/Arrays/ElementExpressionCompiler.cs:50-55`, B11).
- No read carries `each` (D1.5, `Schema.tla:ReadsTakeNoEach`). The redesign's two `Each` doors write a read path with
  the member `"[]"`, which its runtime reads as a marker (`Alis.Reactive/Components/JsMembers.cs:143`,
  `Alis.Reactive/Components/JsMembers.cs:172`, `Alis.Reactive.Assets/runtime/grammar/paths.ts:5`,
  `Alis.Reactive.Assets/runtime/grammar/paths.ts:17` at 65fdf118d), where D1.2 makes every string a member name.
- One count and one projection (B8): `Count` and `Each` on a declared list (`Alis.Reactive/Components/JsMembers.cs:135`,
  `Alis.Reactive/Components/JsMembers.cs:140` at 65fdf118d) beside `Count()` and `Select` on an array leave for N6.1's
  `Count()` and `Select` (X6.1).
- An item's absent member reads its start in a `Select` as in every data read (D4.2, M4.X2). The redesign's `Each`
  reads it as nothing (`Alis.Reactive/Components/JsMembers.cs:154-155` at 65fdf118d).
- A list is a source with no door in or out. Main's `ReactiveArray` is no source and needs `AsSource`
  (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:16`, `Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:121`
  at 623dd4a27); the redesign's is one (`Alis.Reactive/PlanAuthoring/Arrays/ReactiveArray.cs:21` at 65fdf118d), and
  both start it with `From`. The core's operations extend the source and return one (D6.10).
- A live list counts, filters, tests and finds over a delegate on its item, its members required (D6.8, D6.9): the
  redesign's declared list has only `At`, `Count` and `Each` (`Alis.Reactive/Components/JsMembers.cs:128-147` at
  65fdf118d), so a view counts or finds live items only after projecting them to data. The POC's live `Select` writes its field with no
  `nonNull` (`poc/Alis.Reactive.Poc/Components/LiveObjects.cs:35-36`), since gate 1's P6 came after it; the rewrite
  builds D6.16 for live items (M4.X9).
- A date or a time in a list's lambda compares and orders as .NET does, to the tick (D6.12, D6.13, X6.11, X6.29). The
  redesign sorts by a `Date`'s `getTime` (`Alis.Reactive.Assets/runtime/grammar/operators.ts:119-121` at 65fdf118d),
  which drops ticks below a millisecond and orders a wall time daylight saving skips as the hour after it; the core
  sorts by each item's key with `compareOrdinal`, and a lookup in a list the view holds compares keys, so one instant
  written at two offsets is found.
