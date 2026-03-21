using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Reads the plan JSON from the page and provides strongly-typed component locators.
///
/// The plan JSON already contains componentId, vendor, readExpr, and bindingPath
/// for every component on the page. TModel is a phantom — it provides compile-time
/// expression safety without referencing the app.
///
/// Usage:
///   var plan = await ReactivePlan&lt;ResidentModel&gt;.FromPage(Page);
///   var physician = plan.AutoComplete(m => m.Physician);
///   await physician.TypeAndSelect("smi", "Dr. Smith");
///
/// Rename Physician → PrimaryPhysician on the model:
///   - View breaks at compile time (Html.InputField uses same expression)
///   - Test breaks at compile time (plan.AutoComplete uses same expression)
///   - Coupled to domain, decoupled from implementation.
/// </summary>
public sealed class PagePlan<TModel> where TModel : class
{
    private readonly IPage _page;
    private readonly Dictionary<string, ComponentEntry> _components;

    private PagePlan(IPage page, Dictionary<string, ComponentEntry> components)
    {
        _page = page;
        _components = components;
    }

    /// <summary>
    /// Read the plan JSON from [data-reactive-plan] on the page.
    /// Call AFTER the page has loaded and booted.
    /// </summary>
    public static async Task<PagePlan<TModel>> FromPage(IPage page)
    {
        var json = await page.EvalOnSelectorAsync<string>(
            "[data-reactive-plan]",
            "el => el.textContent");

        var doc = JsonDocument.Parse(json);
        var components = new Dictionary<string, ComponentEntry>(StringComparer.OrdinalIgnoreCase);

        if (doc.RootElement.TryGetProperty("components", out var comps))
        {
            foreach (var prop in comps.EnumerateObject())
            {
                var bindingPath = prop.Name;
                var obj = prop.Value;
                components[bindingPath] = new ComponentEntry(
                    Id: obj.GetProperty("id").GetString()!,
                    Vendor: obj.GetProperty("vendor").GetString()!,
                    ReadExpr: obj.GetProperty("readExpr").GetString()!,
                    BindingPath: bindingPath,
                    ComponentType: obj.GetProperty("componentType").GetString()!);
            }
        }

        return new PagePlan<TModel>(page, components);
    }

    /// <summary>All component binding paths discovered in the plan.</summary>
    public IReadOnlyCollection<string> ComponentNames => _components.Keys;

    // ─── Typed Component Locators (expression-based) ───

    /// <summary>AutoComplete — resolved from plan via model expression.</summary>
    public AutoCompleteLocator AutoComplete(Expression<Func<TModel, object?>> expr)
    {
        var entry = Resolve(ToBindingPath(expr), expectedComponentType: "autocomplete");
        return new AutoCompleteLocator(_page, entry.Id);
    }

    /// <summary>DropDownList — resolved from plan via model expression.</summary>
    public DropDownListLocator DropDownList(Expression<Func<TModel, object?>> expr)
    {
        var entry = Resolve(ToBindingPath(expr), expectedComponentType: "dropdownlist");
        return new DropDownListLocator(_page, entry.Id);
    }

    /// <summary>NumericTextBox — resolved from plan via model expression.</summary>
    public NumericTextBoxLocator NumericTextBox(Expression<Func<TModel, object?>> expr)
    {
        var entry = Resolve(ToBindingPath(expr), expectedComponentType: "numerictextbox");
        return new NumericTextBoxLocator(_page, entry.Id);
    }

    /// <summary>Switch — resolved from plan via model expression.</summary>
    public SwitchLocator Switch(Expression<Func<TModel, object?>> expr)
    {
        var entry = Resolve(ToBindingPath(expr), expectedComponentType: "switch");
        return new SwitchLocator(_page, entry.Id);
    }

    /// <summary>Native TextBox — resolved from plan via model expression.</summary>
    public NativeTextBoxLocator TextBox(Expression<Func<TModel, object?>> expr)
    {
        var entry = Resolve(ToBindingPath(expr), expectedComponentType: "textbox");
        return new NativeTextBoxLocator(_page, entry.Id);
    }

    // ─── String-based overloads (for non-model elements) ───

    /// <summary>AutoComplete — by binding path string (when expression isn't available).</summary>
    public AutoCompleteLocator AutoComplete(string bindingPath)
    {
        var entry = Resolve(bindingPath, expectedComponentType: "autocomplete");
        return new AutoCompleteLocator(_page, entry.Id);
    }

    /// <summary>Look up any component entry by binding path.</summary>
    public ComponentEntry? FindComponent(string bindingPath)
        => _components.TryGetValue(bindingPath, out var entry) ? entry : null;

    /// <summary>Look up any component entry by model expression.</summary>
    public ComponentEntry? FindComponent(Expression<Func<TModel, object?>> expr)
        => FindComponent(ToBindingPath(expr));

    // ─── Page-Level Surfaces ───

    /// <summary>Any element by raw ID — for status spans, echo divs, results.</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");

    /// <summary>Validation error message for a model property. Encapsulates data-valmsg-for selector.</summary>
    public ILocator ErrorFor(Expression<Func<TModel, object?>> expr)
        => _page.Locator($"span[data-valmsg-for='{ToBindingPath(expr)}']");

    // ─── Internal ───

    private ComponentEntry Resolve(string bindingPath, string expectedComponentType)
    {
        if (!_components.TryGetValue(bindingPath, out var entry))
        {
            var available = string.Join(", ", _components.Keys);
            throw new InvalidOperationException(
                $"Component '{bindingPath}' not found in plan. Available: [{available}]");
        }

        if (entry.ComponentType != expectedComponentType)
        {
            throw new InvalidOperationException(
                $"Component '{bindingPath}' is '{entry.ComponentType}', expected '{expectedComponentType}'. " +
                $"The view uses a different component type than the test expects.");
        }

        return entry;
    }

    private static string ToBindingPath(Expression<Func<TModel, object?>> expr)
    {
        var member = expr.Body;

        // Unwrap Convert (boxing for value types)
        if (member is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            member = unary.Operand;

        return member switch
        {
            MemberExpression m => BuildPath(m),
            _ => throw new ArgumentException($"Expression must be a property access (m => m.Prop), got: {expr}")
        };
    }

    private static string BuildPath(MemberExpression expr)
    {
        var parts = new List<string>();
        var current = expr;
        while (current != null)
        {
            parts.Add(current.Member.Name);
            current = current.Expression as MemberExpression;
        }
        parts.Reverse();
        return string.Join(".", parts);
    }
}

/// <summary>A component registration from the plan JSON.</summary>
public sealed record ComponentEntry(
    string Id,
    string Vendor,
    string ReadExpr,
    string BindingPath,
    string ComponentType);
