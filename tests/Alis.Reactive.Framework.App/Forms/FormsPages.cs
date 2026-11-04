
namespace Alis.Reactive.Framework.App.Forms;

public sealed class OutingPage
{
    public string? Resident { get; set; }

    public string? Companion { get; set; }

    public string? Note { get; set; }
}

public sealed class OutingRules : ReactiveValidator<OutingPage>
{
    public OutingRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident going out.");
    }
}

public sealed class LunchPage
{
    public string? Resident { get; set; }

    public string? Companion { get; set; }
}

public sealed class LunchRules : ReactiveValidator<LunchPage>
{
    public LunchRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident at lunch.");
    }
}

public sealed class LunchCompanionRules : ReactiveValidator<LunchPage>
{
    public LunchCompanionRules()
    {
        RuleFor(x => x.Companion).NotEmpty().WithMessage("Name the companion.");
    }
}

public sealed class FrontDeskPage
{
    public string? Visitor { get; set; }

    public string? Parcel { get; set; }
}

public sealed class StayPage
{
    public string? Guest { get; set; }

    public string? StartsOn { get; set; }
}

public sealed class FormsEcho
{
    public string Summary { get; set; } = "";
}
