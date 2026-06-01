---
name: enforce-csharp8
enabled: false
event: file
action: block
conditions:
  - field: file_path
    operator: regex_match
    pattern: (Alis\.Reactive/|Alis\.Reactive\.Native/|Alis\.Reactive\.Fusion/|Alis\.Reactive\.FluentValidator/|Alis\.Reactive\.DesignSystem/)Alis\.Reactive.*\.csproj$
  - field: new_text
    operator: regex_match
    pattern: <LangVersion>[^8]
---

**BLOCKED: C# language version change on a core framework project.**

These four projects are locked to C# 8.0 (`<LangVersion>8</LangVersion>`):
- `Alis.Reactive`
- `Alis.Reactive.Native`
- `Alis.Reactive.Fusion`
- `Alis.Reactive.FluentValidator`

This ensures the framework can be consumed by any .NET app regardless of its language version.
Apps and test projects may use the latest C# version.
