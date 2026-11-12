# Alis.Reactive.Native

A consumer of the core: the plain-DOM components and the app-level objects, PackageId AlisReactive.Native. It stands
on the core's public surface alone, as an app's own component does, and gets nothing an app does not. It references
no other consumer.

## The standard

- A slice is one folder named for its class, `Native` and what it renders: the declaration, the render helper and
  its TypeScript. `SliceTree.txt` orders the slices, and a row enters it with the first file of its folder; a slice
  shares code with no other slice, and duplication between slices beats a shared base class.
- A declaration mirrors the object's typed API member for member, by its JavaScript names. A slice needs no change
  to the core's runtime: one that seems to need it means the plan is missing information.
- The slice's browser code is held to the strictest lint and TypeScript options of `spec/REWRITE.md` section 2.7,
  with no exemption.
- A slice's tests prove only what its declaration adds: its members' names and shapes, its browser facts and its
  own defects.
- A file compiles for both hosts or for one, by the core's rule (`Alis.Reactive/CLAUDE.md`).
- U3 declares a slice with the first view that holds its component, and U23 builds it: the declaration is written
  member for member from its line of `spec/NativeComponents/module.md`, and its render helper throws
  `NotImplementedException` until then (`spec/CLAUDE.md`, the lead's line of 2026-11-12). The text box is the
  first: its declaration is `NativeComponents/NativeTextBox/NativeTextBox.cs`, and its render helpers with their
  builder are `NativeComponents/NativeTextBox/NativeTextBoxHtml.cs`. A file's namespace is its first folder's, so
  every slice is in the namespace of `NativeComponents/`, whose own file is the base the slices share,
  `NativeComponents/NativeComponent.cs`.

## Commands

```bash
dotnet build Alis.Reactive.Native -c Release
scripts/check.sh --no-spec
```
