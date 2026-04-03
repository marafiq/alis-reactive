using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Reads the plan JSON from the page and provides strongly-typed component locators.
///
/// The V2 plan carries binding-to-object links plus contract metadata. This helper
/// resolves those pieces into DOM-backed component bindings so tests can stay
/// expression-driven without scraping implementation details from the page.
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
    private readonly Dictionary<string, BoundComponent> _components;

    private PagePlan(IPage page, Dictionary<string, BoundComponent> components)
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
        var components = new Dictionary<string, BoundComponent>(StringComparer.OrdinalIgnoreCase);
        var root = doc.RootElement;
        var objects = root.GetProperty("objects");
        var contracts = root.GetProperty("contracts");

        foreach (var prop in root.GetProperty("bindings").EnumerateObject())
        {
            var bindingPath = prop.Name;
            var binding = prop.Value;
            var objectName = binding.GetProperty("object").GetString()!;
            var valueMember = binding.GetProperty("valueMember").GetString()!;

            if (!objects.TryGetProperty(objectName, out var runtimeObject))
                throw new InvalidOperationException(
                    $"Binding '{bindingPath}' points to missing object '{objectName}'.");

            if (!runtimeObject.TryGetProperty("elementId", out var elementIdProp))
                continue;

            var contractKey = runtimeObject.GetProperty("contract").GetString()!;
            if (!contracts.TryGetProperty(contractKey, out var contract))
                throw new InvalidOperationException(
                    $"Binding '{bindingPath}' points to missing contract '{contractKey}'.");

            components[bindingPath] = new BoundComponent(
                ElementId: elementIdProp.GetString()!,
                BindingPath: bindingPath,
                ComponentType: InferComponentType(contractKey),
                Resolver: contract.GetProperty("resolver").GetString()!,
                ValueMember: valueMember,
                ContractKey: contractKey);
        }

        return new PagePlan<TModel>(page, components);
    }

    /// <summary>All component binding paths discovered in the plan.</summary>
    public IReadOnlyCollection<string> ComponentNames => _components.Keys;

    // ─── Typed Component Locators (expression-based) ───

    /// <summary>AutoComplete — resolved from plan via model expression.</summary>
    public AutoCompleteLocator AutoComplete(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "autocomplete");
        return new AutoCompleteLocator(_page, componentRef.ElementId);
    }

    /// <summary>DropDownList — resolved from plan via model expression.</summary>
    public DropDownListLocator DropDownList(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "dropdownlist");
        return new DropDownListLocator(_page, componentRef.ElementId);
    }

    /// <summary>NumericTextBox — resolved from plan via model expression.</summary>
    public NumericTextBoxLocator NumericTextBox(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "numerictextbox");
        return new NumericTextBoxLocator(_page, componentRef.ElementId);
    }

    /// <summary>Switch — resolved from plan via model expression.</summary>
    public SwitchLocator Switch(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "switch");
        return new SwitchLocator(_page, componentRef.ElementId);
    }

    /// <summary>Native TextBox — resolved from plan via model expression.</summary>
    public NativeTextBoxLocator TextBox(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "textbox");
        return new NativeTextBoxLocator(_page, componentRef.ElementId);
    }

    /// <summary>DatePicker — resolved from plan via model expression.</summary>
    public DatePickerLocator DatePicker(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "datepicker");
        return new DatePickerLocator(_page, componentRef.ElementId);
    }

    /// <summary>TimePicker — resolved from plan via model expression.</summary>
    public TimePickerLocator TimePicker(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "timepicker");
        return new TimePickerLocator(_page, componentRef.ElementId);
    }

    /// <summary>DateTimePicker — resolved from plan via model expression.</summary>
    public DateTimePickerLocator DateTimePicker(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "datetimepicker");
        return new DateTimePickerLocator(_page, componentRef.ElementId);
    }

    /// <summary>DateRangePicker — resolved from plan via model expression.</summary>
    public DateRangePickerLocator DateRangePicker(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "daterangepicker");
        return new DateRangePickerLocator(_page, componentRef.ElementId);
    }

    /// <summary>MultiColumnComboBox — resolved from plan via model expression.</summary>
    public MultiColumnComboBoxLocator MultiColumnComboBox(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "multicolumncombobox");
        return new MultiColumnComboBoxLocator(_page, componentRef.ElementId);
    }

    /// <summary>InputMask — resolved from plan via model expression.</summary>
    public InputMaskLocator InputMask(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "inputmask");
        return new InputMaskLocator(_page, componentRef.ElementId);
    }

    /// <summary>RichTextEditor — resolved from plan via model expression.</summary>
    public RichTextEditorLocator RichTextEditor(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "richtexteditor");
        return new RichTextEditorLocator(_page, componentRef.ElementId);
    }

    /// <summary>MultiSelect — resolved from plan via model expression.</summary>
    public MultiSelectLocator MultiSelect(Expression<Func<TModel, object?>> expr)
    {
        var componentRef = Resolve(ToBindingPath(expr), expectedComponentType: "multiselect");
        return new MultiSelectLocator(_page, componentRef.ElementId);
    }

    // ─── String-based overloads (for non-model elements) ───

    /// <summary>AutoComplete — by binding path string (when expression isn't available).</summary>
    public AutoCompleteLocator AutoComplete(string bindingPath)
    {
        var componentRef = Resolve(bindingPath, expectedComponentType: "autocomplete");
        return new AutoCompleteLocator(_page, componentRef.ElementId);
    }

    /// <summary>Look up any bound component by binding path.</summary>
    public BoundComponent? FindComponent(string bindingPath)
        => _components.TryGetValue(bindingPath, out var componentRef) ? componentRef : null;

    /// <summary>Look up any bound component by model expression.</summary>
    public BoundComponent? FindComponent(Expression<Func<TModel, object?>> expr)
        => FindComponent(ToBindingPath(expr));

    // ─── Page-Level Surfaces ───

    /// <summary>Any element by raw ID — for status spans, echo divs, results.</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");

    /// <summary>Validation error message for a model property. Encapsulates data-valmsg-for selector.</summary>
    public ILocator ErrorFor(Expression<Func<TModel, object?>> expr)
        => _page.Locator($"span[data-valmsg-for='{ToBindingPath(expr)}']");

    // ─── Internal ───

    private BoundComponent Resolve(string bindingPath, string expectedComponentType)
    {
        if (!_components.TryGetValue(bindingPath, out var componentRef))
        {
            var available = string.Join(", ", _components.Keys);
            throw new InvalidOperationException(
                $"Component '{bindingPath}' not found in plan. Available: [{available}]");
        }

        if (componentRef.ComponentType != expectedComponentType)
        {
            throw new InvalidOperationException(
                $"Component '{bindingPath}' is '{componentRef.ComponentType}', expected '{expectedComponentType}'. " +
                $"The view uses a different component type than the test expects.");
        }

        return componentRef;
    }

    private static string InferComponentType(string contractKey)
    {
        var delimiter = contractKey.IndexOf('.');
        if (delimiter < 0 || delimiter == contractKey.Length - 1)
            return contractKey;

        var typeKey = contractKey.Substring(delimiter + 1);
        return typeKey.StartsWith("component.", StringComparison.Ordinal)
            ? typeKey.Substring("component.".Length)
            : typeKey;
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

/// <summary>A DOM-backed component binding resolved from the V2 plan.</summary>
public sealed record BoundComponent(
    string ElementId,
    string BindingPath,
    string ComponentType,
    string Resolver,
    string ValueMember,
    string ContractKey);
