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

    public IActionResult Checks() => View(DoseRound.Page());

    public IActionResult Lookups() => View(DoseRound.Page());

    public IActionResult LookedUpInACaseBlindSet() => View(DoseRound.Page());

    public IActionResult LookedUpInAHashSet() => View(DoseRound.Page());

    public IActionResult LookedUpInADosesHours() => View(DoseRound.Page());

    public IActionResult LookedUpAnObject() => View(DoseRound.Page());

    public IActionResult LookedUpInANullList() => View(DoseRound.Page());

    public IActionResult LookedUpWithAComparer() => View(DoseRound.Page());

    public IActionResult TrimmedByChar() => View(DoseRound.Page());

    public IActionResult TrimmedByChars() => View(DoseRound.Page());

    public IActionResult StartsWithByCulture() => View(DoseRound.Page());

    public IActionResult StartsWithIgnoringCase() => View(DoseRound.Page());

    public IActionResult Sums() => View(DoseRound.Page());

    public IActionResult Casing() => View(DoseRound.Page());

    public IActionResult Sorts() => View(DoseRound.Page());

    public IActionResult SortedByCulture() => View(DoseRound.Page());

    public IActionResult SortedByStatus() => View(DoseRound.Page());

    public IActionResult SortedBySchedule() => View(DoseRound.Page());

    public IActionResult SortedByCarer() => View(DoseRound.Page());

    public IActionResult SortedIgnoringCase() => View(DoseRound.Page());

    public IActionResult Absences() => View(DoseRound.Page());

    public IActionResult RequiredParameters() => View(DoseRound.Page());
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
            new("Aspirin", "North", 1, DoseStatus.Given, DoseStatus.Given, 75, 'O', new DateTime(2026, 11, 6, 8, 0, 0), new Carer("Ruth"), null, "Aspirin\u0085", "Dr Weiß", "Crush", [new Carer("Ruth")], [8], 0.1m, 0.1, 2147483000, 50000000000000000000000000000m, false, 0.10m) { Refills = 2, Unit = "mg", Maker = "Bayer", Store = new DoseStore { Shelf = new DoseShelf { Name = "trolley" }, Spare = new DoseShelf { Name = "trolley" }, Locker = { Name = "trolley" } }, Batch = new DoseBatch { Lot = "A1", Expiry = "2027-01" } },
            new("Digoxin", "North", 2, DoseStatus.Due, DoseStatus.Due, 0.125, 'O', new DateTime(2026, 11, 6, 9, 0, 0), null, null, "Digoxin", "Dr Işık", "", [new Carer("Kofi"), new Carer("Ruth")], [9, 21], 0.2m, 0.2, 1000, 50000000000000000000000000000m, true, 0.20m) { Unit = "mcg", Store = new DoseStore { Shelf = new DoseShelf { Name = "fridge" }, Spare = new DoseShelf { Name = "cupboard" } }, Batch = new DoseBatch { Expiry = "2027-03" } },
            new("Insulin", "South", 3, DoseStatus.Due, DoseStatus.Due, 10, 'I', new DateTime(2026, 11, 6, 12, 0, 0), new Carer("Kofi"), null, "\u2003Insulin", "Dr Θρ\u1FB3ξ", null, [new Carer("Abena")], [12, 18, 22], 0m, 0, -1000, 0m, false, 0m) { Refills = 1, Unit = "units", Store = new DoseStore { Shelf = new DoseShelf { Name = null }, Spare = new DoseShelf { Name = null }, Locker = { Name = null } }, Delivery = new DoseInfusion { Site = null } },
            new("Heparin", null, null, DoseStatus.Missed, DoseStatus.Due, 5000, 'I', new DateTime(2026, 11, 6, 6, 0, 0), new Carer("Abena"), null, "\uFEFFHeparin", "DR ΚΑΡΑΣ", "  ", [new Carer("Kofi"), new Carer("Abena")], [6], 0m, 0, 0, 0m, true, 0m) { Unit = null, Seal = DoseSeal.Of("S4", breaker: null) },
            new("Paracetamol", "West", 2, DoseStatus.Given, DoseStatus.Due, 500, 'O', new DateTime(2026, 11, 6, 10, 0, 0), new Carer("Esi"), null, "Paracetamol\u0085", "DR \u0130NCE", "\u0085", [], [], 0m, 0, 0, 0m, false, 0m) { Refills = 3, Seal = DoseSeal.Of("S5", breaker: "Esi") },
        ];
    }
}
