# Alis.Reactive

The core: one assembly, PackageId AlisReactive, compiled for `net48` and `net10.0`. It names no consumer and takes no
vendor library. A module is one folder and one namespace of the same name, and `SliceTree.txt` orders them: a slice
uses only the rows before its own. `spec/REWRITE.md` section 2.3 says why each row stands where it does.

## The standard

- A door is public only when a view author, a validator author or a component author needs it; everything else is
  `internal`. A new door is a line of `PublicAPI/<tfm>/PublicAPI.Unshipped.txt` in the commit that writes it, with its
  XML doc (`.claude/rules/docs.md`).
- A door is typed. A deliberate exception carries `Declarations/TypedDslExemptionAttribute.cs` with its reason.
- U3 declares every door `spec/api` compiles, and a door's module's unit builds it. A door that is declared and not
  built throws `NotImplementedException`, so it is built or it fails loud: none renders a page it wrote no plan
  for. Its XML doc states what the spec decides for it, and its unit holds the doc true of what it builds.
- The grammar's nodes are sealed, with `internal` constructors, and a unit never changes the grammar. They are
  `Schema/`'s, written from `spec/Core/schema.md`: a union is an abstract class that names each kind's class, a
  kind's class is its kind and then its union (`SetStatement`), a constructor takes every field and none is
  nullable, and a closed choice is a sealed class with a private constructor and one instance a case, written as
  its text (`Schema/Choice.cs`). A member that holds a kind is typed as its union, so the plan writes its kind.
  `Schema/PlanJson.cs` holds the options the plan is written with.
- The TypeScript contract and the plan's JSON schema are generated from the grammar as System.Text.Json writes it
  (`tools/PlanContractGenerator`), never edited by hand, and `scripts/check.sh` fails on a grammar the tracked
  contract differs from.
- The core discovers and invokes no member at run time. `BannedSymbols.txt` is what the build refuses in both hosts:
  a use of `System.Reflection`, `Activator`, a type found by its name, a compiled expression and a regular
  expression. A read of a type's identity is the owner's, listed by file and member in
  `tests/Alis.Reactive.ArchitectureTests/WhenTheCoreIsCompiled.cs`, which fails on any other and on an entry the
  core no longer reads; a read enters that list with the owner's yes (`spec/CLAUDE.md`). The ban reads uses, so a
  reflection type named in a declaration is the list's to refuse. The list reads the files the net10 host
  compiles; a file of the net48 host alone is read from U4 on, by N18.1's analyzer in each host's build.
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
| A use of reflection, of `Activator`, of a type found by its name, of a compiled expression or of a regular expression | the build (RS0030, `BannedSymbols.txt`), each entry held by `WhenTheCoreIsCompiled` |
| A read of type information off the owner's list | `WhenTheCoreIsCompiled` |
| A node left open, a nullable field, a field typed as a kind's class, a kind written twice or by a class not named for it, a choice a case of which can be made outside it | the contract generator, held by `WhenThePlanContractIsGenerated` |
| A grammar the tracked contract differs from | `scripts/check.sh`, its first step |
