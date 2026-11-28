# Alis.Reactive.ModuleTests

A core module's pure tests of its C# half: a sentence a view writes through the core's public doors, and the plan it
renders compared with a literal written from `spec/Core/schema.md` and read against the generated
`Alis.Reactive.Assets/runtime/Schema/plan.schema.json`. They prove that a module's half writes the schema's nodes.
They prove no behavior in a browser: a Playwright test does.

## The standard

- A test stands on the shipped projects' public surface alone: the doors a view, a validator or a component
  declaration calls. No internal is exposed to it, which `WhenAnInternalIsExposed` holds, and it names no node class.
- `Views.cs` hands a test MVC's own HTML helper for a model, built from MVC's services with no host and no stand-in
  object. A test sets the culture its view renders under with NUnit's own attribute for it.
- The expected plan is a literal in the test, written from the schema's lines: never a recorded output, and never
  built by the code under test. `PlanSchema.cs` reads a plan against the plan's JSON schema and names what fails.
- A refusal a view meets where it renders is expected as its whole message, a literal. Where an analyzer refuses
  the same defect at build, the test writes the defect in code the analyzer does not follow:
  `PartialsAndSlots/WhenAViewsPlanHasRendered.cs` runs the second render and the late door from a lambda written
  before the render, as a section runs.
- A fixture is named `When` and its situation, in the folder of the module whose half it proves, and a test is its
  promise as a sentence. `SliceTree.txt` orders the folders: the files beside it, then one row of module folders.
- A module's unit writes its tests before the half they prove and reads them failing first. A promise has both
  sides: a culture read from the view is proven under two cultures, since one would pass a culture written in.
- The fraction's number text on net48 differs from net10's (`spec/Core/DESIGN.md`, D3.11) and no test here runs on
  net48: CI's Windows job proves it when the rewrite is done.

```bash
dotnet test tests/Alis.Reactive.ModuleTests
```
