using System.Text.Json;
using Microsoft.Playwright;

namespace Alis.Reactive.Playwright.Extensions;

/// <summary>
/// Reads the plan JSON from the page and provides typed component locators.
/// The plan is the single source of truth — it contains every componentId,
/// vendor, readExpr, and bindingPath on the page. No hardcoded IDs needed.
///
/// Usage:
///   var plan = await ReactivePlan.FromPage(Page);
///   var physician = plan.AutoComplete("Physician");
///   await physician.TypeAndSelect("smi", "Dr. Smith");
///   await Expect(plan.Element("status")).ToContainTextAsync("selected");
/// </summary>
public sealed class ReactivePlan
{
    private readonly IPage _page;
    private readonly Dictionary<string, ComponentEntry> _components;

    private ReactivePlan(IPage page, Dictionary<string, ComponentEntry> components)
    {
        _page = page;
        _components = components;
    }

    /// <summary>
    /// Read the plan JSON from [data-reactive-plan] on the page.
    /// Call AFTER page has loaded and booted.
    /// </summary>
    public static async Task<ReactivePlan> FromPage(IPage page)
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
                    BindingPath: bindingPath);
            }
        }

        return new ReactivePlan(page, components);
    }

    /// <summary>All component binding paths discovered in the plan.</summary>
    public IReadOnlyCollection<string> ComponentNames => _components.Keys;

    /// <summary>Look up a component entry by binding path (model property name).</summary>
    public ComponentEntry? FindComponent(string bindingPath)
        => _components.TryGetValue(bindingPath, out var entry) ? entry : null;

    // ─── Typed Component Locators ───

    /// <summary>
    /// AutoComplete locator resolved from plan. Throws if component not found.
    /// </summary>
    public AutoCompleteLocator AutoComplete(string bindingPath)
    {
        var entry = Resolve(bindingPath, "fusion");
        return new AutoCompleteLocator(_page, entry.Id);
    }

    // Future: DropDownList, MultiSelect, NumericTextBox, Switch, etc.

    // ─── Page-Level Surfaces ───

    /// <summary>Any element by raw ID — for status spans, echo divs, results.</summary>
    public ILocator Element(string elementId) => _page.Locator($"#{elementId}");

    // ─── Internal ───

    private ComponentEntry Resolve(string bindingPath, string expectedVendor)
    {
        if (!_components.TryGetValue(bindingPath, out var entry))
        {
            var available = string.Join(", ", _components.Keys);
            throw new InvalidOperationException(
                $"Component '{bindingPath}' not found in plan. Available: [{available}]");
        }

        if (entry.Vendor != expectedVendor)
        {
            throw new InvalidOperationException(
                $"Component '{bindingPath}' is vendor '{entry.Vendor}', expected '{expectedVendor}'");
        }

        return entry;
    }
}

/// <summary>
/// A component registration from the plan JSON.
/// </summary>
public sealed record ComponentEntry(
    string Id,
    string Vendor,
    string ReadExpr,
    string BindingPath);
