# Sandbox Demo Templates (Full Vertical Slice)

A sandbox demo is a vertical slice across 3 files. All 3 must be updated.

## File 1: Model (`Areas/Sandbox/Models/Components/Fusion/{ComponentName}/XxxModel.cs`)

Add a model property for the new capability. For server-filtered events, also add the item class
and response class:

```csharp
// Model property
public string[]? Supplies { get; set; }

// Item class for DataSource
public class SupplyItem
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
    public string Category { get; set; } = "";
}

// Response class for HTTP endpoint
public class SupplySearchResponse
{
    public List<SupplyItem> Supplies { get; set; } = new();
    public int Count { get; set; }
}
```

## File 2: Controller (`Areas/Sandbox/Controllers/Components/Fusion/XxxController.cs`)

The controller uses `[Area("Sandbox")]` and `[Route("Sandbox/Components/Xxx")]`:

For server-filtered events, add an HTTP GET endpoint with server-side text filtering:

```csharp
[HttpGet]
public IActionResult Supplies([FromQuery] string? Supplies)
{
    var all = new List<SupplyItem> { /* ... */ };

    // Gather sends "null" string when nothing selected — treat same as empty
    var search = Supplies == "null" ? null : Supplies;
    var filtered = string.IsNullOrEmpty(search)
        ? all
        : all.Where(s => s.Text.Contains(search, StringComparison.OrdinalIgnoreCase)
                      || s.Value.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

    return Ok(new SupplySearchResponse { Supplies = filtered, Count = filtered.Count });
}
```

## File 3: View (`Areas/Sandbox/Views/Components/Fusion/Xxx/Index.cshtml`)

Add a numbered section. Include `<span id="xxx">` elements for Playwright assertions:

```html
<section class="rounded-lg border border-border bg-white p-6 shadow-sm">
    <h2 class="text-base font-semibold mb-4">N. Filtering Event (Server-Filtered HTTP)</h2>
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        @{ Html.InputField(plan, m => m.Supplies, o => o.Label("Supplies (Server-Filtered)"))
            .FusionMultiSelect(b => b
                .Fields<SupplyItem>(t => t.Text, v => v.Value)
                .AllowFiltering(true)       <!-- REQUIRED for MultiSelect filtering -->
                .Reactive(plan, evt => evt.Filtering, (args, p) =>
                {
                    args.PreventDefault(p);
                    p.Get("/Sandbox/Components/Xxx/Supplies")
                     .Gather(g => g.FromEvent(args, x => x.Text, "Supplies"))
                     .Response(r => r.OnSuccess<SupplySearchResponse>((json, s) =>
                     {
                         args.UpdateData(s, json, j => j.Supplies);
                         s.Element("filter-status").SetText("results loaded");
                     }));
                })); }
    </div>
    <div class="font-mono text-sm mt-2">
        <p>Filter status: <span id="filter-status" class="text-text-muted">&mdash;</span></p>
    </div>
</section>
```

## AllowFiltering Note

SF AutoComplete has filtering built-in. SF MultiSelect and DropDownList
require `.AllowFiltering(true)` explicitly — without it, the filtering event never fires.
