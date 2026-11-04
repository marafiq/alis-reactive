
namespace Alis.Reactive.Framework.App.Validation;

public sealed class IncidentReport
{
    public List<IFormFile>? Photos { get; set; }
}

public sealed class IncidentReportRules : ReactiveValidator<IncidentReport>
{
    public IncidentReportRules()
    {
        RuleFor(x => x.Photos).NotEmpty().WithMessage("Attach a photo of the incident.");
    }
}
