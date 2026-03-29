# Code smells — canonical (descriptor planning)

**Applies to:** Every **mergeable task** in Issues **A–F** (and nested tasks **F1–F4**, **B1/B2**, **C1/C2**, **Pre-C**, etc.). Waive only with a row in that issue’s **Discussion & decisions** table (owner + reason).

**Related:** [issue-review-protocol.md](issue-review-protocol.md), [INVEST-rubric.md](INVEST-rubric.md), [descriptor-design-target-state.md](../descriptor-design-target-state.md) (no silent fallbacks). **Sonar:** [§5 SonarQube Community (C#) alignment](#sonar-community-csharp). **Merge:** [IMPLEMENTATION-GUARDRAILS.md](IMPLEMENTATION-GUARDRAILS.md) (anti-drift checklist).

### C# language version (mandatory for core descriptor work)

<a id="csharp-language-version"></a>

**Alis.Reactive** and related vertical slices pin **C# 8** — see [`Alis.Reactive.csproj`](../../../Alis.Reactive/Alis.Reactive.csproj) (`<LangVersion>8</LangVersion>`); same for [`Alis.Reactive.Native`](../../../Alis.Reactive.Native/Alis.Reactive.Native.csproj), [`Alis.Reactive.Fusion`](../../../Alis.Reactive.Fusion/Alis.Reactive.Fusion.csproj), [`Alis.Reactive.FluentValidator`](../../../Alis.Reactive.FluentValidator/Alis.Reactive.FluentValidator.csproj).

**Do not** use **C# 9+** features in planning examples or implementation for those projects unless an issue **explicitly** raises `LangVersion` and documents why. Examples of **disallowed** here: **`record`** types, **`init`** accessors, **primary constructors** (C# 12).

**Use instead:** `sealed` immutable classes, `readonly struct`, get-only properties + constructor, or builders — all valid in **C# 8**.

---

## 1. Constructor arity

| Rule | Smell |
|------|--------|
| **>4 parameters** | Instance constructors with **five or more** positional parameters **before** `params` / trailing optional groups. Count each required + optional parameter in the **same** parameter list. (If the language version allowed `record` primary constructors, those long parameter lists would count the same — **this repo uses C# 8**, so use immutable **classes** / **structs** for parameter objects.) |

**Mitigation:** Options/parameter object (**`sealed` immutable class** or **`readonly struct`**), builder, factory, or nested graph types. **Document** in the issue discussion log if a public API constraint forces a waiver.

**Pattern (Issue C):** An immutable **value object** holds the full HTTP request payload; `RequestDescriptor` exposes **≤4** parameters (e.g. `RequestDescriptor(RequestDescriptorSpec spec)`). **Public fluent DSL** in views can stay unchanged while builders populate the VO — see [issue-c-http-validation.md](issue-c-http-validation.md) **Target**.

**Examples (RSPEC-style — bad vs good)**

```csharp
// Noncompliant — planning smell: 5+ positional parameters (stricter than Sonar S107 default in many profiles)
public sealed class DispatchCommand
{
    public DispatchCommand(string target, string kind, string? prop, string? method, string value)
    {
        // ...
    }
}
```

```csharp
// Compliant — C# 8: nested immutable types; each ctor ≤4 params; DispatchCommand takes one arg
public sealed class CommandPayload
{
    public string? Prop { get; }
    public string? Method { get; }
    public string Value { get; }

    public CommandPayload(string? prop, string? method, string value)
    {
        Prop = prop;
        Method = method;
        Value = value;
    }
}

public sealed class DispatchMutation
{
    public string Target { get; }
    public string Kind { get; }
    public CommandPayload Payload { get; }

    public DispatchMutation(string target, string kind, CommandPayload payload)
    {
        Target = target;
        Kind = kind;
        Payload = payload;
    }
}

public sealed class DispatchCommand
{
    public DispatchCommand(DispatchMutation mutation) { /* ... */ }
}
```

---

## 2. SOLID-related smells

Use as a **checklist** during review; not every row applies to every file.

| Letter | Smell indicators (examples) |
|--------|-----------------------------|
| **S** — Single responsibility | One type owns unrelated concerns (e.g. mode gate + HTTP + conditions + segment flush); a change to **one** feature constantly edits **unrelated** sections of the same file. |
| **O** — Open/closed | New behavior requires **editing** many `switch` / `if` chains on string/`kind` instead of **adding** a descriptor + schema + handler row **where the architecture expects extension**. |
| **L** — Liskov | Subtype weakens contracts (throws where callers assume success); test doubles that don’t honor real serialization or guard rules. |
| **I** — Interface segregation | Callers depend on a **wide** surface (`AddCommand`, HTTP, conditions) when they only need **emit command** or **one** pipeline slice. |
| **D** — Dependency inversion | Core builders **directly `new`** low-level services **outside** composition roots; scattered `new ConcreteResolver()` instead of injected or single factory. |

**Examples (bad vs good)**

```csharp
// Noncompliant — SRP: one method mixes unrelated steps (signals S138 / S3776 on real codebases)
public void BuildEverything()
{
    ValidateMode();
    FlushAllSegments();
    SerializeHttpDescriptor();
    WriteTraceFile();
}
```

```csharp
// Compliant — single responsibility per unit; façade only wires
public void BuildEverything()
{
    _modeGate.EnsureValid();
    _segments.Flush();
    _http.EnsureDescriptor();
}
```

```csharp
// Noncompliant — ISP: caller only needs “emit”, but receives entire pipeline
public void AddVendorCommand(PipelineBuilder<object> fullPipeline) { fullPipeline.AddCommand(...); }
```

```csharp
// Compliant — narrow port (Issue E direction)
public void AddVendorCommand(ICommandEmitter emitter) { emitter.Emit(new DispatchCommand(...)); }
```

---

## 3. Dead code

| Smell | Examples |
|-------|----------|
| Unreachable branches | `if (false)`, duplicated conditions, legacy enum values never constructed. |
| Unused members | `private` methods/fields never referenced (verify with grep + coverage). |
| Commented-out blocks | Large **commented** production paths left in repo without ticket to delete or restore. |
| Duplicate APIs | Two public paths for the same effect where **one** is never called. |

**Examples (Sonar S1144 — bad vs good)**

```csharp
// Noncompliant — dead member (unused private)
private static void LegacyGuardMerge(Guard? a, Guard? b) { /* never called */ }
```

```csharp
// Compliant — remove or use; if kept for one call site, prove it with a test
private static Guard MergeGuards(Guard? a, Guard? b) => /* ... */;
```

---

## 4. Fallbacks (fail fast — policy smell)

<a id="fallbacks-fail-fast"></a>

Aligned with repo policy: **silent** recovery is a smell when it hides misconfiguration.

| Smell | Examples |
|-------|----------|
| Silent defaults | “Missing component id → use first on page”; unknown `vendor` → assume `native`. |
| Dual / compat paths | Try old serializer then new; “if merge fails, ignore” without explicit migration issue. |
| Catch-all handlers | Swallow exceptions in `ResolveAll` / `Render` / runtime `execute` without surfacing to developer. |

**Not a smell:** Explicit **documented** migration PR, feature flag with **end date**, or **user-visible** error message (throw).

**Examples (Sonar S2486 — bad vs good)**

```csharp
// Noncompliant — exception swallowed (policy + S2486)
try { plan.Render(); }
catch (Exception) { /* silent */ }
```

```csharp
// Compliant — fail fast with context (or let bubble from Render)
try { plan.Render(); }
catch (Exception ex)
{
    throw new InvalidOperationException("Plan render failed after ResolveAll.", ex);
}
```

```csharp
// Noncompliant — silent default hides bad registration (planning smell; not always a Sonar rule)
var root = ResolveRoot(el, vendor ?? "native");
```

```csharp
// Compliant — validate first; no silent default for unknown vendor (C# 8 + BCL)
if (vendor is null) throw new ArgumentNullException(nameof(vendor));
var root = ResolveRoot(el, vendor);
// if unknown vendor string: throw with message — do not assume "native"
```

---

## 5. SonarQube Community (C#) — alignment

<a id="sonar-community-csharp"></a>

**Source:** [SonarSource **sonar-dotnet**](https://github.com/SonarSource/sonar-dotnet) analyzer (bundled with **SonarQube Community** and **SonarCloud** for C#). Rule titles and keys (`csharpsquid:Sxxxx`) come from published RSPEC JSON. Sonar does **not** label rules “SOLID” by name — the table below maps **our** planning smells to **closest** Sonar rules as **objective** reinforcement for every task.

**Threshold note:** [S107](https://rules.sonarsource.com/csharp/RSPEC-107) (*Methods should not have too many parameters*) applies to methods **including constructors**; Sonar’s **default** maximum is often **7** parameters (configurable per profile). Our [§1 Constructor arity](#1-constructor-arity) (**5+** = smell) is **stricter** — keep both: use Sonar to catch gross signatures; use **§1** for this program’s bar unless the Discussion log waives.

### 5.1 SOLID (proxy rules)

| SOLID letter | What we look for (planning) | Sonar C# rules | RSPEC |
|--------------|-----------------------------|------------------|-------|
| **S** — Single responsibility | Types/methods doing unrelated jobs; “god” pipeline class | **S138** Functions should not have too many lines of code · **S3776** Cognitive Complexity of methods should not be too high · **S1200** Classes should not be coupled to too many other classes | RSPEC-138, RSPEC-3776, RSPEC-1200 |
| **O** — Open/closed | New `kind` / behavior forces editing central switches vs extending plan | No single Sonar rule; **S3776** / **S138** on giant `switch` methods indicate refactor pressure | — |
| **L** — Liskov | Subtypes that break serialization or guard contracts | **Primarily tests + review**; **S3655** *Empty nullable value should not be accessed* flags one class of contract bug (not a full Liskov check) | RSPEC-3655 |
| **I** — Interface segregation | Fat API (`AddCommand` + everything) | **S3215** “interface” instances should not be cast to concrete types (dependency leak) | RSPEC-3215 |
| **D** — Dependency inversion | High-level code `new`’s low-level concretes everywhere | **S1200** (coupling) + **S3215** (casts) as signals; composition-root pattern is review, not one rule | — |

Tags on these rules often include **brain-overload** (S138, S3776, S1200) or **design** (S3215).

### 5.2 Encapsulation

| Encapsulation smell | Sonar C# rules | RSPEC |
|---------------------|----------------|-------|
| Public mutable state | **S1104** Fields should not have public accessibility | RSPEC-1104 |
| Leaking internals via casts | **S3215** “interface” instances should not be cast to concrete types | RSPEC-3215 |
| Type coupling | **S1200** Classes should not be coupled to too many other classes | RSPEC-1200 |

### 5.3 Constructor / method arity

| Planning § | Sonar C# rules | RSPEC |
|------------|----------------|-------|
| [§1 Constructor arity](#1-constructor-arity) | **S107** Methods should not have too many parameters | RSPEC-107 |

### 5.4 Dead code

| Planning § | Sonar C# rules | RSPEC |
|------------|----------------|-------|
| [§3 Dead code](#3-dead-code) | **S1144** Unused private types or members should be removed | RSPEC-1144 |

### 5.5 Fallbacks and silent failure

| Planning § | Sonar C# rules | RSPEC |
|------------|----------------|-------|
| [§4 Fallbacks](#4-fallbacks-fail-fast--policy-smell) | **S2486** Generic exceptions should not be ignored (empty catch / swallow) | RSPEC-2486 |

### 5.6 Extra signals (clarity / conditions)

| Situation | Sonar C# rules | RSPEC |
|-----------|----------------|-------|
| Nested / confusing conditionals (e.g. **Issue F3** ternary cleanup) | **S3358** Ternary operators should not be nested | RSPEC-3358 |

### 5.7 Reference examples (Sonar RSPEC style — noncompliant vs compliant)

Same pattern as [Sonar rule pages](https://rules.sonarsource.com/csharp/): **Noncompliant** raises the rule (or matches planning smell); **Compliant** shows the fix. Rule keys reference [§5.1–5.6](#51-solid-proxy-rules).

**S107 — too many parameters**

```csharp
// Noncompliant
public void Emit(string a, string b, string c, string d, string e, string f) { }
```

```csharp
// Compliant — C# 8: Emit has one parameter; split nested ctor so each stays ≤4 params (this repo’s §1)
public sealed class EmitTail
{
    public string C { get; }
    public string D { get; }
    public string E { get; }
    public EmitTail(string c, string d, string e) { C = c; D = d; E = e; }
}
public sealed class EmitArgs
{
    public string A { get; }
    public string B { get; }
    public EmitTail Tail { get; }
    public EmitArgs(string a, string b, EmitTail tail) { A = a; B = b; Tail = tail; }
}
public void Emit(EmitArgs args) { }
```

**S3215 — interface cast to concrete**

```csharp
// Noncompliant
void Use(IComponentHost host)
{
    var fusion = (FusionHost)host;
    fusion.Ej2Call();
}
```

```csharp
// Compliant — depend on abstraction or factory that returns the right capability
void Use(IComponentHost host)
{
    host.ApplyToVendor(VendorKind.Fusion, h => h.Ej2Call());
}
```

**S3358 — nested ternary**

```csharp
// Noncompliant
var x = a ? (b ? c : d) : e;
```

```csharp
// Compliant
string x;
if (a)
    x = b ? c : d;
else
    x = e;
```

**S1104 — public fields**

```csharp
// Noncompliant
public class Options { public string Mode; }
```

```csharp
// Compliant — C# 8 (no init setter)
public sealed class Options
{
    public string Mode { get; }
    public Options() { Mode = ""; }
}
```

**Using this in CI:** Any **new** issue on **changed** files in these categories should be **fixed or triaged** with a PR comment linking the waiver row in **Discussion & decisions** (same bar as [INVEST-rubric.md](INVEST-rubric.md) merge gate).

---

## 6. Per-issue pointers

Each **issue-A** … **issue-F** file repeats **Issue-specific smells** under **§2** (subsection *Code smells (task gate — every task)*) and links here for the **canonical** list above. See the planning [README child table](README.md).
