# Alis.Reactive.Conformance.World

The world contract: what a browser test and the conformance app agree on, and nothing of the framework. It
references no Alis.Reactive assembly, so a test that compiles against it cannot compute what the code computes.

- `Gates.g.cs` and `Promises.g.cs` are generated from the spec and never edited by hand: a gate is an action of a
  TLA+ spec's Next, and a promise constant is one test of the spec, carrying `HoldsAttribute` once for each gate
  its test holds. The command below writes both.
- `FailsForAttribute` binds a test to its promise constant, and `GestureAttribute` marks a harness call that counts
  as a test's gesture. `BesideThePageAttribute` marks what the harness collected of a page's requests, console and
  uncaught errors, the one thing of the harness's that `Assert.That` reads.
- A seed is written through the typed builders of `Seed.cs`; an endpoint and a page of the World area are named once,
  in `WorldEndpoints.cs` and `WorldPages.cs`; a refusal route's page is named in `RefusalPages.cs`.
- It compiles for `net48` and `net10.0`, since the twin serves the same world.

```bash
python3 spec/tools/check.py --generate
dotnet build tests/Alis.Reactive.Conformance.World -c Release
```
