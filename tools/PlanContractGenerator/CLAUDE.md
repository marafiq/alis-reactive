# PlanContractGenerator

The tool that writes the plan's contract, `Alis.Reactive.Assets/runtime/Schema/plan.ts` and
`Alis.Reactive.Assets/runtime/Schema/plan.schema.json`, from the core's grammar (`Alis.Reactive/Schema`). It is not
shipped, and it is the one reader of the grammar's internals: a tool reads the built core by reflection, which the
core itself never does.

## The standard

- The contract is what System.Text.Json writes. `PlanContract.cs` reads each node from the type information of the
  options the plan is written with (`Alis.Reactive/Schema/PlanJson.cs`): a union's kinds from its polymorphism, a
  node's fields from its properties, a closed choice from each of its instances as written. It states no shape of
  its own but the path steps', which `PathSteps.cs` writes as rows and proves by writing one step of each kind.
- It walks from the plan's root, so a definition is in the contract because the plan reaches it, in the order the
  walk meets it; a field is `readonly`, and every field is required.
- A grammar the contract cannot state is refused with a `GrammarRefusedException` that names the type and the cure:
  a node that is not sealed, a nullable field, a field typed as a kind's class, a kind with no text, a kind's class
  not named for its kind and its union, a kind two classes write, a choice a case of which can be made outside it
  or is written as no text or as another case's text, two types of one name, and a type with no row.
- Each refusal has a refused case and the accepted grammar its literals in
  `tests/Alis.Reactive.ArchitectureTests/WhenThePlanContractIsGenerated.cs`.

## Commands

```bash
dotnet run --project tools/PlanContractGenerator -c Release -- Alis.Reactive.Assets/runtime/Schema
dotnet test tests/Alis.Reactive.ArchitectureTests
scripts/check.sh --no-spec
```
