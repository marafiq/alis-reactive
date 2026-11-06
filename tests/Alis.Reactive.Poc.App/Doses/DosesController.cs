using Alis.Reactive.Poc.App.Components;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Doses;

public sealed class DosesController : Controller
{
    public IActionResult Round() => View(DoseRound.Page());

    public IActionResult ComparedObjects() => View(DoseRound.Page());

    public IActionResult ComparedMemberObjects() => View(DoseRound.Page());

    public IActionResult ComparedDates() => View(DoseRound.Page());

    public IActionResult OrderedByName() => View(DoseRound.Page());

    public IActionResult OrderedMembers() => View(DoseRound.Page());

    public IActionResult NarrowingCast() => View(DoseRound.Page());

    public IActionResult ComputedViewValue() => View(DoseRound.Page());

    public IActionResult NullViewValue() => View(DoseRound.Page());

    public IActionResult Texts() => View(DoseRound.Page());

    public IActionResult TrimmedByChar() => View(DoseRound.Page());

    public IActionResult TrimmedByChars() => View(DoseRound.Page());

    public IActionResult StartsWithByCulture() => View(DoseRound.Page());

    public IActionResult StartsWithIgnoringCase() => View(DoseRound.Page());
}

public sealed record DoseRoundPage(string Ward, Carer Lead, Carer? Cover, DateTime Since);

public static class DoseLimits
{
    public static int Routine { get; } = 2;
}

public static class DoseRound
{
    public static DoseRoundPage Page() => new("North", new Carer("Ruth"), null, new DateTime(2026, 11, 6, 7, 0, 0));

    public static IReadOnlyList<MedicationDose> Doses()
    {
        return
        [
            new("Aspirin", "North", 1, DoseStatus.Given, DoseStatus.Given, 75, 'O', new DateTime(2026, 11, 6, 8, 0, 0), new Carer("Ruth"), null, "Aspirin\u0085", "Dr Weiß", "Crush"),
            new("Digoxin", "North", 2, DoseStatus.Due, DoseStatus.Due, 0.125, 'O', new DateTime(2026, 11, 6, 9, 0, 0), null, null, "Digoxin", "Dr Işık", ""),
            new("Insulin", "South", 3, DoseStatus.Due, DoseStatus.Due, 10, 'I', new DateTime(2026, 11, 6, 12, 0, 0), new Carer("Kofi"), null, "\u2003Insulin", "Dr Θρ\u1FB3ξ", null),
            new("Heparin", null, null, DoseStatus.Missed, DoseStatus.Due, 5000, 'I', new DateTime(2026, 11, 6, 6, 0, 0), new Carer("Abena"), null, "\uFEFFHeparin", "DR ΚΑΡΑΣ", "  "),
            new("Paracetamol", "West", 2, DoseStatus.Given, DoseStatus.Due, 500, 'O', new DateTime(2026, 11, 6, 10, 0, 0), new Carer("Esi"), null, "Paracetamol\u0085", "DR \u0130NCE", "\u0085"),
        ];
    }
}
