
namespace Alis.Reactive.Framework.App.Inputs;

public sealed class AdmitPage
{
    public string? Name { get; set; }

    public int Age { get; set; }

    public decimal? Weight { get; set; }

    public DateTime Admitted { get; set; }

    public DateTimeOffset Arrives { get; set; }
}

public sealed class AdmitEcho
{
    public string Summary { get; set; } = "";
}

public sealed class AdmitRules : ReactiveValidator<AdmitPage>
{
    public AdmitRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident.");
        RuleFor(x => x.Age).InclusiveBetween(18, 120).WithMessage("Give an age from 18 to 120.");
    }
}
