using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Forms;

/// <summary>
/// An outing booked at the desk: the resident and the companion inside the booking's form, the companion in a
/// section of its own, and a note for the desk written beside the form.
/// </summary>
public sealed class OutingPage
{
    public string? Resident { get; set; }

    public string? Companion { get; set; }

    public string? Note { get; set; }
}

/// <summary>The booking's rules: a resident the browser checks is named.</summary>
public sealed class OutingRules : ReactiveValidator<OutingPage>
{
    public OutingRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident going out.");
    }
}

/// <summary>
/// A lunch booked for a resident: the resident inside the booking's form, and the companion in a section of the
/// form that is saved on its own, by rules of its own.
/// </summary>
public sealed class LunchPage
{
    public string? Resident { get; set; }

    public string? Companion { get; set; }
}

/// <summary>The booking's rules: a resident the browser checks is named; a companion is not the booking's to require.</summary>
public sealed class LunchRules : ReactiveValidator<LunchPage>
{
    public LunchRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident at lunch.");
    }
}

/// <summary>The companion section's own rules, for when it is saved on its own.</summary>
public sealed class LunchCompanionRules : ReactiveValidator<LunchPage>
{
    public LunchCompanionRules()
    {
        RuleFor(x => x.Companion).NotEmpty().WithMessage("Name the companion.");
    }
}

/// <summary>The front desk's log: a visit and a parcel, each logged by a form of its own.</summary>
public sealed class FrontDeskPage
{
    public string? Visitor { get; set; }

    public string? Parcel { get; set; }
}

/// <summary>
/// A stay booked for a guest: the stay's form holds only the guest's line, which the desk loads when it picks a
/// guest; when the stay starts is the server's to ask for.
/// </summary>
public sealed class StayPage
{
    public string? Guest { get; set; }

    public string? StartsOn { get; set; }
}

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class FormsEcho
{
    public string Summary { get; set; } = "";
}
