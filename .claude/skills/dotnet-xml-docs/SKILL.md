---
name: dotnet-xml-docs
description: This skill should be used when writing or reviewing XML documentation comments on C# types, interfaces, classes, methods, properties, or parameters. Also use when the user asks to "add xml docs", "document this class", "fix xml comments", "add summary", "improve documentation", or when auditing public API surface for missing documentation. Covers proper .NET tag syntax, formatting conventions, and Alis.Reactive-specific patterns.
---

# .NET XML Documentation Comments

Write accurate, concise XML documentation comments that help future developers understand
**what** a type or member does, **why** it exists, and **how** to use it correctly.

## The Standard

Every word, line, and sentence in `<summary>`, `<remarks>`, or `<param>` must be concise,
accurate, and written for the right audience. Each must earn its place. Write to genuine
intent — never to complete the task quickly in a shallow way.

## Core Rules

1. **Every public type and member gets a `<summary>`** — complete sentence, ends with full stop.
2. **Tag order convention** — `summary`, `remarks`, `typeparam`, `param`, `returns`, `value`, `exception`, `example`, `seealso`. Project convention, not compiler-enforced.
3. **`<summary>` answers "what does this do?"** — one to two sentences max. If it needs more, use `<remarks>`.
4. **`<remarks>` answers "why?" and "how?"** — design rationale, threading, usage guidance. Use `<para>` for paragraphs.
5. **`<param>` documents every parameter** — compiler warns on missing/mismatched names (CS1573). Describe what the parameter represents, not its type.
6. **`<returns>` describes the return value** — what it represents, not the type name.
7. **`<typeparam>` documents every type parameter** — explain the constraint or role.
8. **`<exception>` documents thrown exceptions** — `cref` must resolve; describe when it's thrown.
9. **`<value>` on properties** — optional. Describes what the property value represents. Most BCL code uses `<summary>` alone; add `<value>` when semantics need explanation beyond the summary (units, valid ranges, null meaning).
10. **Cross-references use `<see cref="...">`** — compiler validates. Generic types use braces: `cref="List{T}"`.

## Property Voice Convention

Follow the standard Microsoft BCL convention:
- **Read-write properties**: "Gets or sets the..." — this is standard, not noise.
- **Read-only properties**: "Gets the..."
- **What IS noise**: "Returns the value of the Foo property" or restating the type name.

## What NOT to Document

- **Type restating** — don't echo the parameter type or return type. Describe what it represents.
- **Implementation details** — document the contract, not the mechanism.
- **Filler phrases** — avoid "This method...", "This property...", "This class...". Lead with the action.
- **Extension method `this` parameter** — don't explain what an extension method receiver is.

## Interface Implementations

Use `<inheritdoc/>` when the base/interface documentation is complete and the
implementation adds no new semantics:

```csharp
/// <inheritdoc/>
public string Render() { ... }
```

Add implementation-specific detail in `<remarks>` when needed:

```csharp
/// <inheritdoc/>
/// <remarks>
/// Uses <c>System.Text.Json</c> with camelCase property naming.
/// </remarks>
public string Render() { ... }
```

Cherry-pick specific tags with `path`:

```csharp
/// <inheritdoc cref="TriggerBuilder{TModel}.CustomEvent" path="/param"/>
```

## Alis.Reactive Patterns

### Class-Level Summary

State the architectural role:

```csharp
/// <summary>
/// Collects behaviors (trigger → reaction graphs) and component registrations,
/// then serializes them as the JSON plan the browser runtime executes.
/// </summary>
/// <typeparam name="TModel">The view model type, providing compile-time expression paths.</typeparam>
```

### Builder Methods

State what the builder configures, not how:

```csharp
/// <summary>
/// Adds a CSS class to the target element when the reaction executes.
/// </summary>
/// <param name="className">The CSS class name to add.</param>
/// <returns>The builder instance for method chaining.</returns>
```

### Plan Model Classes (JSON-Serialized)

Note the JSON contract role:

```csharp
/// <summary>
/// A behavior pairing a <see cref="StartsWhen"/> trigger with a
/// <see cref="ReactionGraph"/> reaction. Serialized as one element of the
/// <c>behaviors</c> array in the plan JSON.
/// </summary>
```

### Internal-Use Members on Public Interfaces

When a member is public for serialization/interface reasons but not for direct developer use:

```csharp
/// <summary>
/// Registers a trigger-reaction pair in the plan. Called by
/// <see cref="TriggerBuilder{TModel}"/> — not intended for direct use in views.
/// </summary>
```

## Formatting Quick Reference

| Need | Tag | Context |
|------|-----|---------|
| Inline code | `<c>` | `<c>DomReady</c>` |
| Code block | `<code>` | Inside `<example>`, `<remarks>`, or any tag |
| Paragraph break | `<para>` | Inside `<summary>`, `<remarks>`, `<returns>` |
| Line break | `<br/>` | Single-spaced lines |
| Code reference | `<see cref="Type"/>` | Compiler-validated link |
| Code ref + text | `<see cref="X">text</see>` | Custom link text |
| Language keyword | `<see langword="null"/>` | `null`, `true`, `false`, `void`, any C# keyword |
| External link | `<see href="url">text</see>` | Web documentation |
| Bold/italic | `<b>`, `<i>`, `<u>` | HTML formatting, compiler-validated |
| Hyperlink | `<a href="url">text</a>` | Alternative to `<see href>` |
| Parameter ref | `<paramref name="x"/>` | Inside summary text |
| Type param ref | `<typeparamref name="T"/>` | Inside summary text |
| Angle brackets | `&lt;` `&gt;` | Escape `<` and `>` in text |

## Reference Files

- **`references/tag-reference.md`** — Complete tag syntax, attributes, and compiler behavior rules.
- **`references/csharp-spec-annex-d.md`** — C# Language Specification Annex D: the authoritative
  formal grammar, ID string format, output XML file structure, and `cref` encoding rules.

## Workflow

1. Read the type/member source to understand its role and responsibilities.
2. Write `<summary>` — one to two sentences answering "what does this do?"
3. Add `<remarks>` only if design rationale or usage guidance is needed.
4. For interface implementations, prefer `<inheritdoc/>` over duplicating docs.
5. Add `<typeparam>`, `<param>`, `<returns>`, `<exception>` as applicable.
6. Add `<see cref>` cross-references to related types.
7. Verify XML is well-formed and `cref` targets exist.
8. Build to confirm no CS1591 warnings on public members.

## Anti-Patterns

- **Restating the name**: `/// <summary>Gets the PlanId.</summary>` — say what PlanId represents instead.
- **Future features in docs**: Don't document capabilities that don't exist yet. Comments must match current source.
- **Walls of text in summary**: Keep summary tight. Move details to `<remarks>`.
- **Missing `<exception>`**: If a method throws, document it. Callers need to know.
- **Undocumented generics**: Every `<T>` needs a `<typeparam>`. Compiler warns, but be proactive.
- **Duplicating interface docs**: Use `<inheritdoc/>` on implementations instead of copy-pasting.
- **`<returns>` on void methods**: Void methods have no return value — don't add the tag.
- **Copy-pasted `<param>` names**: Verify parameter names match the actual signature. CS1572 warns on phantom params.
