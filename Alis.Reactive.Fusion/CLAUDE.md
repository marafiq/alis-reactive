# Alis.Reactive.Fusion

A consumer of the core: the Syncfusion slices, the templates and the Fusion app-level objects, PackageId
AlisReactive.Fusion. It stands on the core's public surface alone and gets nothing an app does not. Syncfusion is
pinned at one version for NuGet and npm together (`scripts/pin-syncfusion.mjs`); the hold is the owner's.

## The standard

- A slice is one folder named for its class, `Fusion` and the EJ2 class it renders: the declaration, the render
  helper and its TypeScript. `SliceTree.txt` orders the slices, and a row enters it with the first file of its
  folder; a slice shares code with no other slice.
- A declaration holds a vendor member only with high confidence: the vendor's TypeScript declares it public with a
  concrete type, or a browser probe under `spec/probes/` pins what the browser hands or takes. A member the vendor
  marks private is a gap to name, never a door.
- A slice's tests prove only what its declaration adds, and no test of a core module runs through a Fusion slice.
- A file compiles for both hosts or for one, by the core's rule (`Alis.Reactive/CLAUDE.md`).
- U3 declares a slice with the first view that holds its component; U25 builds the package and the reference, and
  U26 the other slices. The declaration is written member for member from its line of
  `spec/FusionComponents/module.md`, and its render helpers throw `NotImplementedException` until its unit builds
  them. The multi-select box is the exemplar: its declaration, with its payloads nested, is
  `FusionComponents/FusionMultiSelect/FusionMultiSelect.cs`, and its render helpers with `Fields` on EJ2's builder
  are `FusionComponents/FusionMultiSelect/FusionMultiSelectHtml.cs`. Every slice is in the namespace of
  `FusionComponents/`, whose own file is the base the slices share, `FusionComponents/FusionComponent.cs`.

## Commands

```bash
dotnet build Alis.Reactive.Fusion -c Release
node scripts/pin-syncfusion.mjs --check
scripts/check.sh --no-spec
```
