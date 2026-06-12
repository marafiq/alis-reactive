# .NET XML Documentation Tags — Complete Reference

Source: [Microsoft Learn — Recommended XML tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)

## General Tags (Any Element)

### `<summary>`

```xml
/// <summary>
/// One sentence describing what this type or member does.
/// </summary>
```

- Required on every publicly visible type and member.
- IntelliSense displays this text.
- Write complete sentences ending with full stops.

### `<remarks>`

```xml
/// <remarks>
/// Supplemental information not suitable for the summary.
/// Use <para> tags to create paragraphs.
/// </remarks>
```

- Appears in Object Browser.
- Use for design rationale, threading notes, usage guidance.

## Member Tags

### `<param>`

```xml
/// <param name="bindingPath">The model expression path that identifies this component.</param>
```

- One `<param>` per parameter — compiler warns if missing (CS1573) or mismatched (CS1572).
- `name` attribute must exactly match the parameter name.

### `<paramref>`

```xml
/// <summary>
/// Registers <paramref name="entry"/> in the components map.
/// </summary>
```

- Inline reference to a parameter within `<summary>` or `<remarks>`.

### `<returns>`

```xml
/// <returns>The serialized JSON plan ready for embedding in a <c>data-alis-plan</c> script element.</returns>
```

- One per method with a return value. Do not add to void methods.
- Displayed in IntelliSense.

### `<value>`

```xml
/// <value>The vendor identifier: <c>"native"</c> or <c>"fusion"</c>.</value>
```

- Describes what a property value represents.
- Optional in practice — most BCL code uses `<summary>` alone on properties.
- Most useful when semantics need explanation beyond the summary (units, valid ranges, null meaning).
- Properties typically have `<summary>` ("Gets or sets the...") and optionally `<value>`.

### `<exception>`

```xml
/// <exception cref="InvalidOperationException">
/// Thrown when <paramref name="bindingPath"/> is already registered with different component metadata.
/// </exception>
```

- `cref` must reference a real exception type — compiler validates.
- Document every exception that callers should handle.

## Generic Type Tags

### `<typeparam>`

```xml
/// <typeparam name="TModel">The view model type providing expression-based property access.</typeparam>
```

- One per type parameter — compiler warns if missing.
- IntelliSense displays this text.

### `<typeparamref>`

```xml
/// <summary>
/// Builds a plan for <typeparamref name="TModel"/> and renders to JSON.
/// </summary>
```

- Inline reference to a type parameter.

## Formatting Tags

### `<para>`

```xml
/// <remarks>
/// <para>First paragraph — what it does.</para>
/// <para>Second paragraph — design rationale.</para>
/// </remarks>
```

- Creates double-spaced paragraphs inside `<summary>`, `<remarks>`, `<returns>`.

### `<c>` (Inline Code)

```xml
/// <summary>Serializes entries to JSON using <c>System.Text.Json</c>.</summary>
```

- Single words or short inline code spans.

### `<code>` (Code Block)

```xml
/// <remarks>
/// <code>
/// Html.On(plan, t => t.DomReady(p =>
/// {
///     p.Element("status").AddClass("active");
/// }));
/// </code>
/// </remarks>
```

- Multi-line code blocks. Use inside `<example>` for standalone samples, or directly
  inside `<remarks>` for inline code illustrations.

### `<example>`

```xml
/// <example>
/// Dispatch a custom event from a DomReady trigger:
/// <code>
/// p.Dispatch("data-loaded");
/// </code>
/// </example>
```

- Wraps `<code>` with optional descriptive text.
- Adds "Example" section heading in documentation generators.

### `<list>`

```xml
/// <list type="bullet">
/// <item><description>AddClass — adds a CSS class</description></item>
/// <item><description>RemoveClass — removes a CSS class</description></item>
/// </list>
```

- Types: `bullet`, `number`, `table`.
- Use `<term>` + `<description>` for table/definition list.

### `<br/>`

- Single line break (vs `<para>` double-spaced paragraph).

### HTML Formatting Tags

```xml
/// <remarks>
/// This is <b>bold</b>, <i>italic</i>, and <u>underlined</u> text.
/// Visit <a href="https://example.com">our docs</a> for details.
/// </remarks>
```

- `<b>`, `<i>`, `<u>` — compiler-validated text formatting.
- `<a href="url">text</a>` — compiler-validated hyperlink (alternative to `<see href>`).
- All work in IntelliSense tooltips and generated documentation.

### Angle Bracket Escaping

```xml
/// <summary>
/// Returns a value &lt; 1 when the plan is empty.
/// </summary>
```

- Escape `<` as `&lt;` and `>` as `&gt;` in text content.
- Required because XML doc comments must be well-formed XML.

## Cross-Reference Tags

### `<see>`

```xml
/// <see cref="ReactivePlan{TModel}"/>
/// <see cref="PipelineBuilder{TModel}.Dispatch(string)">Dispatch</see>
/// <see langword="null"/>
/// <see href="https://learn.microsoft.com/dotnet">Link text</see>
```

- `cref` — code reference (compiler-validated). Supports custom link text via closing tag.
- `langword` — language keyword (`null`, `true`, `false`, `void`, `static`, or any C# keyword).
- `href` — external URL (clickable in docs). Use `href` for URLs, NOT `cref`.
- Generic types use braces: `cref="List{T}"` (not angle brackets).

### `<seealso>`

```xml
/// <seealso cref="TriggerBuilder{TModel}"/>
/// <seealso href="https://example.com">External docs</seealso>
```

- Generates "See Also" section in documentation.
- Cannot be nested inside `<summary>`.

### `<inheritdoc>`

```xml
/// <inheritdoc/>
/// <inheritdoc cref="IReactivePlan{TModel}.Render"/>
/// <inheritdoc cref="MyParentMethod" path="/returns"/>
```

- Inherits documentation from base class/interface.
- `cref` — inherit from specific member.
- `path` — XPath to inherit only specific tags.
- For public library APIs, use explicitly rather than relying on Visual Studio's automatic inheritance
  (which only works in the IDE, not in generated XML documentation files).

## Reuse Tags

### `<include>`

```xml
/// <include file='docs.xml' path='docs/members[@name="MyType"]/MyMethod/*'/>
```

- Pulls documentation from an external XML file. Compiler-validated.
- Used by the .NET Runtime team for large codebases where inline docs clutter source.
- `file` — path to the XML file (relative to source).
- `path` — XPath expression selecting the documentation nodes.
- For most projects, inline docs are preferred for discoverability.

## Tag Ordering Convention

Apply tags in this order for consistency (project convention, not compiler-enforced):

```xml
/// <summary>...</summary>
/// <remarks>...</remarks>
/// <typeparam name="T">...</typeparam>
/// <param name="x">...</param>
/// <returns>...</returns>
/// <value>...</value>
/// <exception cref="T">...</exception>
/// <example>...</example>
/// <seealso cref="T"/>
```

## Compiler Behavior

- CS1591 — missing XML comment for publicly visible type or member.
- CS1572 — `<param>` tag for nonexistent parameter name.
- CS1573 — parameter has no matching `<param>` tag.
- `<typeparam>` — compiler warns if name doesn't match.
- `<exception>` — compiler warns if cref type doesn't exist.
- `<see cref>` / `<seealso cref>` — compiler warns if target doesn't exist.
- All XML must be well-formed — compiler warns on malformed XML.
- `using` directives are respected when resolving `cref` attributes.
