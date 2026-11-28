using Alis.Reactive.Poc.App.Components;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Rounds;

public sealed class RoundsController : Controller
{
    [HttpGet]
    public IActionResult Round() => View(new RoundForm { Dose = 0.125m, Note = "with food" });

    [HttpPost]
    public IActionResult Round(RoundForm form)
    {
        if (form.Dose <= RoundForm.MaximumDose) return View("RoundSaved", form);
        ModelState.AddModelError(nameof(RoundForm.Dose), "The dose is above the ward's maximum, so the round keeps the maximum.");
        form.Dose = RoundForm.MaximumDose;
        form.Note = form.Note.Trim();
        return View(form);
    }
}

public sealed class RoundForm
{
    public const decimal MaximumDose = 0.25m;

    public decimal Dose { get; set; }

    public RescueDose? Rescue { get; set; }

    public string Note { get; set; } = "";
}

public sealed class RescueDose
{
    public decimal Dose { get; set; }
}

public static class RoundSchedule
{
    public static DoseCardOptions Digoxin { get; } = new()
    {
        Step = 0.125m,
        Unit = "mg",
        Route = MedicationRoute.Oral,
        Due = new DateTime(2026, 11, 7),
        Times = ["08:00", "20:00"],
    };
}
