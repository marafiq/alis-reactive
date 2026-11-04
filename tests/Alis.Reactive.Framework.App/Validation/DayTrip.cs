namespace Alis.Reactive.Framework.App.Validation;

public sealed class DayTrip
{
    public List<TripGuest> Guests { get; set; } = new()
    {
        new TripGuest { Name = "Ada", Relation = "Daughter" },
        new TripGuest { Name = "Bea", Relation = "Friend" },
    };

    public List<TripCompanion> Companions { get; set; } = new()
    {
        new TripCompanion { Badge = "V-1", Phone = "555-0111" },
        new TripCompanion { Badge = "V-2", Phone = "555-0122" },
    };

    public string? Transport { get; set; } = "Walk";

    public string? Driver { get; set; }

    public string? Licence { get; set; }

    public TripPharmacy Pharmacy { get; set; } = new();

    public string? Courier { get; set; } = "Cy";

    public List<string?> Stops { get; set; } = new() { "Pier", "Park" };

    public string? ReturnBy { get; set; } = "16:00";
}

public sealed class TripGuest
{
    public string? Name { get; set; }

    public string? Relation { get; set; }
}

public sealed class TripCompanion
{
    public string? Badge { get; set; }

    public string? Phone { get; set; }
}

public sealed class TripPharmacy
{
    public string? Phone { get; set; } = "555-0101";
}

public sealed class TripPharmacyRules : ReactiveValidator<TripPharmacy>
{
    public TripPharmacyRules()
    {
        RuleFor(x => x.Phone).Must(phone => phone != null && phone.StartsWith("555-")).WithMessage("Use a 555 number.");
    }
}

public sealed class DayTripRules : ReactiveValidator<DayTrip>
{
    public DayTripRules()
    {
        RuleForEach(x => x.Guests).ChildRules(guest =>
            guest.RuleFor(x => x.Name).NotEmpty().WithMessage("Name each guest.").DependentRules(() =>
                guest.RuleFor(x => x.Relation).NotEmpty().WithMessage("Say how each guest is related.")));
        RuleForEach(x => x.Companions).ChildRules(companion =>
            companion.RuleFor(x => x.Badge).Must(badge => badge != null && badge.StartsWith("V-")).WithMessage("Visitor badges start with V-.").DependentRules(() =>
                companion.RuleFor(x => x.Phone).NotEmpty().WithMessage("Give each badge holder's phone.")));
        When(x => x.Transport == "Van", () =>
            RuleFor(x => x.Driver).NotEmpty().WithMessage("Name the driver.").DependentRules(() =>
                RuleFor(x => x.Licence).NotEmpty().WithMessage("Give the driver's licence.")));
        RuleFor(x => x.Pharmacy).SetValidator(new TripPharmacyRules()).DependentRules(() =>
            RuleFor(x => x.Courier).NotEmpty().WithMessage("Name the courier."));
        RuleForEach(x => x.Stops).Must(stop => stop is "Pier" or "Park" or "Museum").WithMessage("Pick a stop on the route.").DependentRules(() =>
            RuleFor(x => x.ReturnBy).NotEmpty().WithMessage("Say when the trip returns."));
    }
}
