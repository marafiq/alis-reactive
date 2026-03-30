# Component Registration Enforcement

**Severity:** Medium
**SOLID Principle:** Fail-Fast (Rule #9 in CLAUDE.md)
**Priority:** P0

## What is the issue?

Every input component must call `plan.AddToComponentsMap()` in its `HtmlExtensions` factory method. If this call is missing, the component renders HTML but is invisible to validation and gather. The framework silently skips unregistered components — no error, no warning.

## Why is it an issue?

CLAUDE.md Rule #9 says: "No Fallbacks — Fail Fast. If a component is not registered in the plan, throw immediately with a clear error message telling the developer what they forgot to register. Fallbacks hide bugs."

A missing `AddToComponentsMap` call means:
- `IncludeAll()` skips the component — the field's value is never sent to the server
- Validation rules for the field are unenriched — they either silently pass or block with a generic summary error
- The developer sees "my field isn't validating" with zero guidance

This is the most dangerous gap because it's **silent and intermittent** — the form appears to work until someone tries to validate or submit.

## Evidence

**File:** `Alis.Reactive.Native/Components/NativeTextBox/NativeTextBoxHtmlExtensions.cs`

```csharp
public static void NativeTextBox<TModel, TProp>(
    this InputFieldSetup<TModel, TProp> setup,
    Action<NativeTextBoxBuilder<TModel, TProp>> configure)
{
    // THIS LINE IS MANDATORY but not enforced:
    setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
        setup.ElementId, _component.Vendor, setup.BindingPath,
        _component.ReadExpr, "textbox"));
    // ...
}
```

**What happens if the line is removed:** The component renders. The user sees a text input. But:

1. `g.IncludeAll()` in a POST pipeline skips this field — value not sent
2. `Validate<MyValidator>("form")` has a rule for this field but the rule's `fieldId` is never enriched — validation blocks with a summary error "unenriched field" or silently passes (depending on fail-closed vs fail-open configuration)
3. No compile error, no runtime error, no console warning

**Proof of silent failure:** The framework has 23 components. Each has this registration call. If any ONE is missing, only that component fails — and only when someone tries to validate or gather. The other 22 work fine, making the bug hard to isolate.

## How to solve it

**Option A: Roslyn analyzer (compile-time)**

Create a diagnostic analyzer that checks: any method on `InputFieldSetup<TModel, TProp>` that calls `Render()` (closing the field wrapper) must also call `AddToComponentsMap()` in the same method body.

```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ComponentRegistrationAnalyzer : DiagnosticAnalyzer
{
    // ALIS004: InputFieldSetup.Render() called without AddToComponentsMap()
    // Severity: Error
}
```

**Option B: Unit test guard (test-time)**

Add a test that uses reflection to find every static extension method on `InputFieldSetup<,>` across all assemblies, and verifies each one calls `AddToComponentsMap`:

```csharp
[Test]
public void Every_input_component_factory_registers_in_ComponentsMap()
{
    var factoryMethods = typeof(InputFieldExtensions).Assembly
        .GetTypes()
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
        .Where(m => m.GetParameters().Any(p =>
            p.ParameterType.IsGenericType &&
            p.ParameterType.GetGenericTypeDefinition() == typeof(InputFieldSetup<,>)));

    foreach (var method in factoryMethods)
    {
        // Verify IL contains call to AddToComponentsMap
        // (or use a simpler pattern: invoke and check ComponentsMap)
    }
}
```

**Option C: Framework enforcement (runtime)**

Make `InputFieldSetup.Render()` (which closes the field wrapper HTML) throw if `AddToComponentsMap` wasn't called:

```csharp
public void Render(IHtmlContent content)
{
    if (!_registered)
        throw new InvalidOperationException(
            $"Component for '{BindingPath}' was rendered without calling " +
            $"plan.AddToComponentsMap(). Validation and gather will not work. " +
            $"Add: plan.AddToComponentsMap(\"{BindingPath}\", ...) in your HtmlExtensions factory.");
    // ... render
}
```

## Why the solution is better

Option C is the strongest — it catches the bug at the exact moment the developer sees the rendered output, with a message that tells them exactly what to do. No test needed, no analyzer to install. The framework enforces its own invariant.

Options A and B are defense-in-depth: the analyzer catches it before build, the test catches it in CI.

## Summary of improvement

| Before | After (Option C) |
|--------|-----------------|
| Missing registration = silent failure | Missing registration = immediate throw with fix instructions |
| Developer debugs "why isn't my field validating?" | Developer sees "add AddToComponentsMap() in your factory" |
| Bug surfaces only when someone validates/gathers | Bug surfaces at render time (always) |
| No feedback on which component is missing | Error names the exact binding path |
| 23 components rely on convention | Framework enforces the invariant |
