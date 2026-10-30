using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>An admission: a member of each type an entry's text converts to.</summary>
public sealed class AdmitPage
{
    public string? Name { get; set; }

    public int Age { get; set; }

    public decimal? Weight { get; set; }

    public DateTime Admitted { get; set; }

    public DateTimeOffset Arrives { get; set; }
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class AdmitEcho
{
    public string Summary { get; set; } = "";
}

/// <summary>The rules every admission is checked by, written once: the browser checks what it can.</summary>
public sealed class AdmitRules : ReactiveValidator<AdmitPage>
{
    public AdmitRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident.");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Give an age from 18 to 120.");
    }
}
