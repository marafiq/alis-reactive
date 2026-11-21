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
  marks private is a gap to name, never a door. One exception, the owner's of 2026-11-17 (spec/CLAUDE.md):
  FusionUploader's code raises FilesChanged through EJ2's private trigger, open for the owner's revisit.
- A slice's tests prove only what its declaration adds, and no test of a core module runs through a Fusion slice.
- A slice's browser code is held to the runtime's strict compiler options and typed lint. It reaches the page
  through `fusion.ts`, the bundle's entry, which calls each slice's mount, and reaches the core only as an app's code
  does, on `alis:mount`; `FusionComponents/mounted.ts`, the tree's first row, mounts a slice once on each element its
  marker names. `build.mjs` writes the bundle, alis-reactive-fusion.js, which git ignores, and the Fusion layout
  links it after EJ2's script manager and before the framework's script. A component EJ2 gives no MVC 5 builder is
  built there from the start its render helper writes: the smart text area's code,
  `FusionComponents/FusionSmartTextArea/fusion-smart-text-area.ts`.
- A file compiles for both hosts or for one, by the core's rule (`Alis.Reactive/CLAUDE.md`).
- A template condition holds no reader of its own: a value the view holds, or a literal, is read and written by the
  core's `ReactivePlan.ViewValueJson`, D6.7's reader, as the plan writes the item's member it is compared with, and
  refused where a list's lambda refuses it, so the package holds no reflection and no compiled expression (the owner,
  2026-11-17, `spec/NOTES.md`). An ordering of two members of the item is held to the list lambda's rule by the core's
  `ReactivePlan.RequireMembersTheBrowserOrders`.
- U3 declares a slice with the first view that holds its component; U25 builds the package and the reference, and U26
  the other slices. The declaration is written member for member from its line of `spec/FusionComponents/module.md`, and
  its render helpers throw `NotImplementedException` until its unit builds them. The multi-select box is the exemplar:
  its declaration, with its payloads nested, is `FusionComponents/FusionMultiSelect/FusionMultiSelect.cs`, and its
  render helpers with `Fields` on EJ2's builder are `FusionComponents/FusionMultiSelect/FusionMultiSelectHtml.cs`. A
  file's namespace is its first folder's: a component slice is in the namespace of `FusionComponents/`, whose own file
  is the base the slices share, `FusionComponents/FusionComponent.cs`; module 22's template types are in
  `FusionTemplates/`; and module 23's app-level slices, built by U27, `FusionAppLevelObjects/FusionToast/` and
  `FusionAppLevelObjects/FusionConfirm/`, are in the namespace of `FusionAppLevelObjects/`, on the same base or, for the
  confirm, the core's `Component` with a vendor of its own.

## Commands

```bash
dotnet build Alis.Reactive.Fusion -c Release
node scripts/pin-syncfusion.mjs --check
scripts/check.sh --no-spec
```
