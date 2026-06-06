using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Reads the emitted Reactive Plan and provides typed component locators.
/// </summary>
/// <remarks>
/// Test bodies stay at the model-expression and locator level while this helper
/// handles the plan JSON shape.
/// </remarks>
public sealed class PagePlan<TModel> where TModel : class
{
    private readonly IPage _page;
    private readonly Dictionary<string, BoundComponent> _components;

    private PagePlan(IPage page, Dictionary<string, BoundComponent> components)
    {
        _page = page;
        _components = components;
    }

    public static async Task<PagePlan<TModel>> FromPage(IPage page)
    {
        var json = await page.EvalOnSelectorAsync<string>(
            "[data-reactive-plan]",
            "el => el.textContent");

        var doc = JsonDocument.Parse(json);
        var components = new Dictionary<string, BoundComponent>(StringComparer.OrdinalIgnoreCase);
        var root = doc.RootElement;

        if (root.TryGetProperty("components", out var componentMap))
        {
            foreach (var componentProperty in componentMap.EnumerateObject())
            {
                var componentKey = componentProperty.Name;
                var component = componentProperty.Value;
                var id = component.GetProperty("id").GetString()!;
                var vendor = component.GetProperty("vendor").GetString()!;
                var typeKey = component.GetProperty("type").GetString()!;

                var bindingPath = ExtractBindingPath(componentKey);

                components[bindingPath] = new BoundComponent(
                    ElementId: id,
                    BindingPath: bindingPath,
                    ComponentKey: componentKey,
                    Vendor: vendor,
                    TypeKey: typeKey);
            }
        }

        return new PagePlan<TModel>(page, components);
    }

    public IReadOnlyCollection<string> ComponentNames => _components.Keys;

    public AutoCompleteLocator AutoComplete(Expression<Func<TModel, object?>> expr)
        => new AutoCompleteLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public DropDownListLocator DropDownList(Expression<Func<TModel, object?>> expr)
        => new DropDownListLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public NumericTextBoxLocator NumericTextBox(Expression<Func<TModel, object?>> expr)
        => new NumericTextBoxLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public SwitchLocator Switch(Expression<Func<TModel, object?>> expr)
        => new SwitchLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public NativeTextBoxLocator TextBox(Expression<Func<TModel, object?>> expr)
        => new NativeTextBoxLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public DatePickerLocator DatePicker(Expression<Func<TModel, object?>> expr)
        => new DatePickerLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public TimePickerLocator TimePicker(Expression<Func<TModel, object?>> expr)
        => new TimePickerLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public DateTimePickerLocator DateTimePicker(Expression<Func<TModel, object?>> expr)
        => new DateTimePickerLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public DateRangePickerLocator DateRangePicker(Expression<Func<TModel, object?>> expr)
        => new DateRangePickerLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public MultiColumnComboBoxLocator MultiColumnComboBox(Expression<Func<TModel, object?>> expr)
        => new MultiColumnComboBoxLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public InputMaskLocator InputMask(Expression<Func<TModel, object?>> expr)
        => new InputMaskLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public RichTextEditorLocator RichTextEditor(Expression<Func<TModel, object?>> expr)
        => new RichTextEditorLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public MultiSelectLocator MultiSelect(Expression<Func<TModel, object?>> expr)
        => new MultiSelectLocator(_page, Resolve(ToBindingPath(expr)).ElementId);

    public AutoCompleteLocator AutoComplete(string bindingPath)
        => new AutoCompleteLocator(_page, Resolve(bindingPath).ElementId);

    public BoundComponent? FindComponent(string bindingPath)
        => _components.TryGetValue(bindingPath, out var component) ? component : FindBySuffix(bindingPath);

    public BoundComponent? FindComponent(Expression<Func<TModel, object?>> expr)
        => FindComponent(ToBindingPath(expr));

    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");

    public ILocator ErrorFor(Expression<Func<TModel, object?>> expr)
        => _page.Locator($"span[data-valmsg-for='{ToBindingPath(expr)}']");

    private BoundComponent Resolve(string bindingPath)
    {
        if (_components.TryGetValue(bindingPath, out var component))
            return component;

        var suffixMatch = FindBySuffix(bindingPath);
        if (suffixMatch != null)
            return suffixMatch;

        throw new InvalidOperationException(
            $"Component '{bindingPath}' not found in plan. Available: [{string.Join(", ", _components.Keys)}]");
    }

    private BoundComponent? FindBySuffix(string bindingPath)
    {
        foreach (var componentEntry in _components)
        {
            if (componentEntry.Value.ComponentKey.EndsWith("__" + bindingPath, StringComparison.OrdinalIgnoreCase) ||
                componentEntry.Value.ComponentKey.EndsWith(bindingPath, StringComparison.OrdinalIgnoreCase) ||
                componentEntry.Key.EndsWith(bindingPath, StringComparison.OrdinalIgnoreCase))
                return componentEntry.Value;
        }
        return null;
    }

    private static string ExtractBindingPath(string componentKey)
    {
        var separatorIndex = componentKey.LastIndexOf("__", StringComparison.Ordinal);
        return separatorIndex >= 0 ? componentKey.Substring(separatorIndex + 2) : componentKey;
    }

    private static string ToBindingPath(Expression<Func<TModel, object?>> expr)
    {
        var member = expr.Body;
        if (member is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            member = unary.Operand;

        return member switch
        {
            MemberExpression m => BuildPath(m),
            _ => throw new ArgumentException($"Expression must be property access, got: {expr}")
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

/// <summary>Component resolved from the emitted Reactive Plan.</summary>
public sealed record BoundComponent(
    string ElementId,
    string BindingPath,
    string ComponentKey,
    string Vendor,
    string TypeKey);
