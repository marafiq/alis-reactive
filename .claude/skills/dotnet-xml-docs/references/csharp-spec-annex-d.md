# C# Language Specification — Annex D: Documentation Comments

Source: ECMA-334 / C# Language Specification, Annex D (Informative)
URL: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments

This is the **authoritative formal specification** for XML documentation comments.

## Key Points

- The annex is **informative** — a conforming compiler is NOT required to check doc comment syntax.
- A conforming compiler IS **permitted** to check, and Roslyn does (CS1591, CS1572, CS1573).
- Tags are **recommendations**, not requirements. Any valid XML is allowed.
- The documentation file is a **flat list** of members, not a hierarchy.

## Comment Syntax

```antlr
Single_Line_Doc_Comment : '///' Input_Character* ;
Delimited_Doc_Comment   : '/**' Delimited_Comment_Section* ASTERISK+ '/' ;
```

- `///` — standard. Leading whitespace after `///` is stripped.
- `/** */` — multiline. Repeated `*` pattern at line starts is stripped.

## Output XML File Format

```xml
<?xml version="1.0"?>
<doc>
  <assembly>
    <name>AssemblyName</name>
  </assembly>
  <members>
    <member name="T:Namespace.TypeName">
      <!-- summary, remarks, etc. -->
    </member>
    <member name="M:Namespace.TypeName.MethodName(System.Int32)">
      <!-- param, returns, exception, etc. -->
    </member>
  </members>
</doc>
```

## ID String Format

| Prefix | Member Kind |
|--------|-------------|
| `N:` | Namespace |
| `T:` | Type (class, struct, enum, interface, delegate) |
| `F:` | Field |
| `P:` | Property (including indexers) |
| `M:` | Method (including constructors, finalizers, operators) |
| `E:` | Event |
| `!:` | Error (unresolvable reference) |

**Encoding rules:**
- Fully qualified name, dot-separated. Periods in names replaced with `#`.
- Parameters in parentheses, comma-separated.
- Generic types: backtick + count (e.g., `` MyList`1 ``).
- Generic method params: double-backtick (e.g., `` GetValues``1(``0) ``).
- `ref`/`out`/`in` params: `@` suffix on type.
- Arrays: `[]`, `[0:,0:]` for multi-dimensional.
- Pointers: `*` suffix.
- Conversion operators: `~ReturnType` suffix.

## Recommended Tags — Formal Syntax

### General

| Tag | Syntax | Purpose |
|-----|--------|---------|
| `<summary>` | `<summary>description</summary>` | Type or member description |
| `<remarks>` | `<remarks>description</remarks>` | Supplemental information |

### Members

| Tag | Syntax | Purpose |
|-----|--------|---------|
| `<param>` | `<param name="n">desc</param>` | Parameter description. Compiler-validated. |
| `<paramref>` | `<paramref name="n"/>` | Inline parameter reference |
| `<returns>` | `<returns>description</returns>` | Return value description |
| `<value>` | `<value>description</value>` | Property value description |
| `<exception>` | `<exception cref="T">desc</exception>` | Thrown exception. Compiler-validated. |
| `<permission>` | `<permission cref="T">desc</permission>` | Security accessibility |

### Generic Types

| Tag | Syntax | Purpose |
|-----|--------|---------|
| `<typeparam>` | `<typeparam name="T">desc</typeparam>` | Type parameter description |
| `<typeparamref>` | `<typeparamref name="T"/>` | Inline type parameter reference |

### Formatting

| Tag | Syntax | Purpose |
|-----|--------|---------|
| `<c>` | `<c>text</c>` | Inline code font |
| `<code>` | `<code>source</code>` | Multi-line code block |
| `<example>` | `<example>desc + code</example>` | Usage example |
| `<para>` | `<para>content</para>` | Paragraph structure |
| `<list>` | `<list type="bullet\|number\|table">` | Lists and tables |

### References

| Tag | Syntax | Purpose |
|-----|--------|---------|
| `<see>` | `<see cref="M" href="url" langword="kw"/>` | Inline link. Compiler-validated (`cref`). |
| `<seealso>` | `<seealso cref="M" href="url"/>` | "See Also" entry. Compiler-validated. |

### Reuse

| Tag | Syntax | Purpose |
|-----|--------|---------|
| `<include>` | `<include file="f" path="xpath"/>` | External XML inclusion |
| `<inheritdoc>` | `<inheritdoc [cref="M"] [path="xpath"]/>` | Inherit base/interface docs |

## `cref` Encoding

- Braces for generics: `cref="List{T}"` (not angle brackets)
- Full member signatures: `cref="Widget.M1(System.Char,System.Single@)"`
- Respects `using` directives — short names resolve if in scope
- Compiler validates existence and translates to canonical ID string

## Partial Types and Methods

- Partial type: doc comments from all parts concatenated (unspecified order).
- Partial method with implementing declaration: implementing docs win.
- Partial method without implementing declaration: docs ignored (declaration removed).

## Missing `<permission>` Tag

The spec defines `<permission>` (§D.3.11) for documenting security accessibility:

```xml
/// <permission cref="System.Security.PermissionSet">
/// Everyone can access this method.
/// </permission>
```

This tag is rarely used in modern .NET (CAS is deprecated), but it exists in the formal spec.
