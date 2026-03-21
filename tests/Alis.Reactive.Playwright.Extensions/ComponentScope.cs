using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Scoped entry point for locating Alis.Reactive components on a page.
/// Knows the IdGenerator prefix pattern: {TypeScope}__{PropertyName}
///
/// Usage:
///   var scope = new ComponentScope(Page, typeof(ResidentModel));
///   var physician = scope.AutoComplete("Physician");
///   await physician.TypeAndSelect("Smith");
/// </summary>
public class ComponentScope
{
    private readonly IPage _page;
    private readonly string _prefix;

    public ComponentScope(IPage page, Type modelType)
    {
        _page = page;
        _prefix = modelType.FullName!.Replace(".", "_") + "__";
    }

    public ComponentScope(IPage page, string prefix)
    {
        _page = page;
        _prefix = prefix;
    }

    /// <summary>The generated element ID for a model property.</summary>
    public string IdFor(string propertyName) => _prefix + propertyName;

    /// <summary>Locate a FusionAutoComplete component by model property name.</summary>
    public AutoCompleteLocator AutoComplete(string propertyName)
        => new(_page, IdFor(propertyName));

    /// <summary>Locate a display element by raw ID (for status spans, echo divs).</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");
}
