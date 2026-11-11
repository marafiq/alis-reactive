# Alis.Reactive.Conformance.World

The world contract: what a browser test and the conformance app agree on, and nothing of the framework. It
references no Alis.Reactive assembly, so a test that compiles against it cannot compute what the code computes.

- `Gates.g.cs` and `Promises.g.cs` are generated from the spec and never edited by hand: a gate is an action of a
  TLA+ spec's Next, and a promise constant is one test of the spec. The command below writes both.
- `FailsForAttribute` binds a test to its promise constant, and `GestureAttribute` marks a harness call that counts
  as a test's gesture.
- A seed is written through the typed builders of `Seed.cs`; an endpoint and a page of the World area are named once,
  in `WorldEndpoints.cs` and `WorldPages.cs`.
- It compiles for `net48` and `net10.0`, since the twin serves the same world.

```bash
python3 spec/tools/check.py --generate
dotnet build tests/Alis.Reactive.Conformance.World -c Release
```
