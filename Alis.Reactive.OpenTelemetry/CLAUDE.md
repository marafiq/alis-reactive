# Alis.Reactive.OpenTelemetry

A consumer of the core: the trace sink a page registers and its layout helper, PackageId AlisReactive.OpenTelemetry.
It stands on the core's diagnostics port alone; the core takes no OpenTelemetry package and records nothing while no
sink is registered.

## The standard

- Its code is one slice, the folder Tracing; the row enters `SliceTree.txt` with the folder's first file.
- The bundle holds the three OpenTelemetry pins of `spec/REWRITE.md` section 2.7, which move together.
- A file compiles for both hosts or for one, by the core's rule (`Alis.Reactive/CLAUDE.md`).

## Commands

```bash
dotnet build Alis.Reactive.OpenTelemetry -c Release
scripts/check.sh --no-spec
```
