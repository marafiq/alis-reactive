using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Scoped entry point for locating Alis.Reactive components on a page.
/// Knows the IdGenerator prefix pattern: {TypeScope}__{PropertyName}
///
/// Usage:
///   var scope = new ComponentScope(Page, typeof(ResidentModel));
///   var physician = scope.AutoComplete("Physician");
///   await physician.TypeAndSelect("smi", "Dr. Smith");
///   await scope.ExpectText("echo-id", "smith");
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

    // ─── Component Locators ───

    /// <summary>Locate a FusionAutoComplete component by model property name.</summary>
    public AutoCompleteLocator AutoComplete(string propertyName)
        => new(_page, IdFor(propertyName));

    // ─── Page-Level Locators ───

    /// <summary>Locate a display element by raw ID (for status spans, echo divs).</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");

    // ─── Page-Level Assertions ───

    /// <summary>Assert a display element contains expected text. For echo spans, status divs.</summary>
    public async Task ExpectText(string elementId, string expectedText, int timeoutMs = 5000)
    {
        await Assertions.Expect(_page.Locator($"#{elementId}"))
            .ToContainTextAsync(expectedText, new() { Timeout = timeoutMs });
    }

    /// <summary>Assert a display element is visible.</summary>
    public async Task ExpectVisible(string elementId, int timeoutMs = 5000)
    {
        await Assertions.Expect(_page.Locator($"#{elementId}"))
            .ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    /// <summary>Assert a display element is hidden.</summary>
    public async Task ExpectHidden(string elementId, int timeoutMs = 5000)
    {
        await Assertions.Expect(_page.Locator($"#{elementId}"))
            .Not.ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }
}
