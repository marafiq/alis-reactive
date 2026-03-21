using System.Linq.Expressions;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Compile-time component locator scoped to TModel.
///
/// Element IDs are deterministic: {TypeScope}__{PropertyName}.
/// Same IdGenerator the framework uses. No plan JSON reading needed.
/// TModel is the shared contract between view and test.
///
/// Developer workflow:
///   1. Domain team defines TModel (plain class)
///   2. View developer: Html.InputField(plan, m => m.Physician, ...)
///   3. Test developer: scope.AutoComplete(m => m.Physician)
///   4. Rename Physician → both break at compile time
///
/// Usage:
///   var resident = PageScope.For&lt;ResidentModel&gt;(Page);
///   var physician = resident.AutoComplete(m => m.Physician);
///   var rate = resident.NumericTextBox(m => m.MonthlyRate);
///
///   await physician.TypeAndSelect("smi", "Dr. Smith");
///   await Expect(resident.Element("save-status")).ToContainTextAsync("saved");
/// </summary>
public sealed class PageScope<TModel> where TModel : class
{
    private readonly IPage _page;
    private readonly string _typeScope;

    private PageScope(IPage page)
    {
        _page = page;
        _typeScope = typeof(TModel).FullName!.Replace(".", "_");
    }

    /// <summary>Create a scope for TModel on the given page.</summary>
    public static PageScope<TModel> Of(IPage page) => new(page);

    /// <summary>
    /// Compute the element ID for a model property — same as IdGenerator.
    /// {TypeScope}__{PropertyPath}
    /// </summary>
    public string IdFor(Expression<Func<TModel, object?>> expr)
        => _typeScope + "__" + ToPropertyPath(expr);

    // ─── Component Locators (Fusion) ───

    /// <summary>Syncfusion AutoComplete.</summary>
    public AutoCompleteLocator AutoComplete(Expression<Func<TModel, object?>> expr)
        => new(_page, IdFor(expr));

    // Future: DropDownList, MultiSelect, NumericTextBox, Switch, DatePicker, etc.

    // ─── Page Surfaces ───

    /// <summary>Any element by explicit ID — buttons, status spans, echo divs.</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");

    /// <summary>The raw input/wrapper for a model property (when component type doesn't matter).</summary>
    public ILocator Input(Expression<Func<TModel, object?>> expr)
        => _page.Locator($"#{IdFor(expr)}");

    // ─── Expression Resolution ───

    private static string ToPropertyPath(Expression<Func<TModel, object?>> expr)
    {
        var body = expr.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } unary)
            body = unary.Operand;

        if (body is not MemberExpression member)
            throw new ArgumentException($"Expression must be a property access, got: {expr}");

        var parts = new List<string>();
        var current = member;
        while (current != null)
        {
            parts.Add(current.Member.Name);
            current = current.Expression as MemberExpression;
        }
        parts.Reverse();
        return string.Join("_", parts);  // Address_City for nested
    }
}

/// <summary>Convenience factory.</summary>
public static class PageScope
{
    public static PageScope<TModel> For<TModel>(IPage page) where TModel : class
        => PageScope<TModel>.Of(page);
}
