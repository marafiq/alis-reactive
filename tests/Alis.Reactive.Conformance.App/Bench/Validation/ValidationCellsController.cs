using System.Linq;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class ValidationCellsController : Controller
{
    [HttpGet(BenchPages.ValidationRule)]
    public IActionResult ValidationRule() => View("/Bench/Validation/CodeForm.cshtml", new ConformanceForm());

    [HttpGet(BenchPages.ValidationLive)]
    public IActionResult ValidationLive() => View("/Bench/Validation/CodeForm.cshtml", new ConformanceForm());

    [HttpGet(BenchPages.ValidationSummary)]
    public IActionResult ValidationSummary() => View("/Bench/Validation/CodeForm.cshtml", new ConformanceForm());

    [HttpGet(BenchPages.ValidationCondition)]
    public IActionResult ValidationCondition() => View("/Bench/Validation/StrictCodeForm.cshtml", new ConformanceForm());

    [HttpGet(BenchPages.ValidationListItem)]
    public IActionResult ValidationListItem() => View("/Bench/Validation/EntriesForm.cshtml", new ConformanceEntries { Entries = [new(), new()] });

    [HttpGet(BenchPages.ValidationPrefix)]
    public IActionResult ValidationPrefix() => View("/Bench/Validation/FirstEntryForm.cshtml", new ConformanceFirst());

    [HttpPost(BenchEndpoints.Form)]
    public IActionResult Form([FromForm] ConformanceForm form) => Content(form.Code is { } code ? $"Saved {code}" : "Saved");

    [HttpPost(BenchEndpoints.FormEntries)]
    public IActionResult FormEntries([FromForm] ConformanceEntries form) => Content($"Saved {string.Join(" and ", form.Entries.Select(entry => entry.Code))}");

    [HttpPost(BenchEndpoints.FormFirst)]
    public IActionResult FormFirst([FromForm] ConformanceFirst form) => Content($"Saved {form.First.Code}");
}
