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

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class FormsEcho
{
    public string Summary { get; set; } = "";
}
