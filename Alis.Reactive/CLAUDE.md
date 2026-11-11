# Alis.Reactive

The core: one assembly, PackageId AlisReactive, compiled for `net48` and `net10.0`. It names no consumer and takes no
vendor library. A module is one folder and one namespace of the same name, and `SliceTree.txt` orders them: a slice
uses only the rows before its own. `spec/REWRITE.md` section 2.3 says why each row stands where it does.

## The standard

- A door is public only when a view author, a validator author or a component author needs it; everything else is
  `internal`. A new door is a line of `PublicAPI/<tfm>/PublicAPI.Unshipped.txt` in the commit that writes it, with its
  XML doc (`.claude/rules/docs.md`).
- A door is typed. A deliberate exception carries `Declarations/TypedDslExemptionAttribute.cs` with its reason.
- The grammar's nodes are sealed, with `internal` constructors, and a unit never changes the grammar.
- The core discovers and invokes no member at run time. A read of type information is on the owner's list
  (`spec/CLAUDE.md`); U3 lands the tool that refuses any other, with the core's first code that reads a type.
- A module's two halves are written against the schema (`spec/Core/schema.md`): this folder writes the nodes, and
  the folder of the same name under `Alis.Reactive.Assets/runtime/` executes them.

## One rule for two hosts

A file compiles for both hosts, or for one. A one-host file says so in its name, `".Net48.cs"` or `".Net10.cs"`, and
is wrapped whole in `#if NET48` or `#if NET10_0_OR_GREATER`: the directive is its first line and `#endif` its last. No
other conditional directive is written. A member whose signature or body differs by host is a partial member,
declared once in the file both hosts compile and implemented in each host's file; a type only one host has lives in
that host's file. `WhenCodeCompilesForBothHosts` refuses a directive inside a file, a file wrapped in another
condition, a wrapped file not named for its host and a host-named file not wrapped. The rule is the same in every
shipped project. It replaces the 265 `#if NET48` blocks main wrote inside 79 files and the redesign's 147 inside 53.
A net48 language error means rework to what PolySharp polyfills, and realtime is net10's alone, so each of its
files is a net10 file.

## Commands

```bash
dotnet build Alis.Reactive -c Release
dotnet test tests/Alis.Reactive.ArchitectureTests
scripts/check.sh --no-spec
```

## Refused here

| Forbidden | Refused by |
|---|---|
| An untyped public door | `ALIS009` |
| A use of a later row or of the file's own row; a namespace off its folder; an open public class; a folder with no row | `ALIS010`, `ALIS011`, `ALIS012`, `ALIS013` |
| A public symbol outside the PublicAPI text | the build (RS0016, RS0017) |
| A conditional directive inside a file | `WhenCodeCompilesForBothHosts` |
| A comment; an XML doc off the public surface | `WhenCodeIsWritten` |
