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

## Commands

```bash
dotnet build Alis.Reactive.Native -c Release
scripts/check.sh --no-spec
```
