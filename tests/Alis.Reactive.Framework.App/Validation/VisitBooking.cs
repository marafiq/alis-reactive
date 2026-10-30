using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A visit booked at the front desk.</summary>
public sealed class VisitBooking
{
    public string? Visitor { get; set; }

    public string? Relation { get; set; }

    public string? Company { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? ConfirmEmail { get; set; }

    public int Guests { get; set; } = 1;

    public string? Notes { get; set; }

    /// <summary>The visiting slot, set by the page and not an input: only the server checks it.</summary>
    public string? Slot { get; set; }
}

public sealed class Booked
{
    public string Summary { get; set; } = "";
}

/// <summary>The rules every visit is checked by, written once: the browser checks what it can, the server all.</summary>
public sealed class VisitRules : ReactiveValidator<VisitBooking>
{
    public VisitRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor.");
        RuleFor(x => x.Visitor).Must(name => name != "Mallory").WithMessage("Mallory may not visit.");
        RuleFor(x => x.Phone).Matches(@"^\d{3}-\d{4}$").WithMessage("Write the phone as 555-0100.");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Give an email we can write to.");
        RuleFor(x => x.ConfirmEmail).Equal(x => x.Email).WithMessage("Write the same email twice.");
        RuleFor(x => x.Guests).InclusiveBetween(1, 4).WithMessage("Bring one to four guests.");
        RuleFor(x => x.Notes).MaximumLength(20).WithMessage("Keep the note to 20 characters.");
        When(x => x.Relation == "Contractor", () =>
            RuleFor(x => x.Company).NotEmpty().WithMessage("Name the contractor's company."));
        RuleFor(x => x.Slot).Must(slot => slot != "Midnight").WithMessage("The desk is closed at midnight.");
    }
}
