using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>An incident report, filed with the photos taken of it.</summary>
public sealed class IncidentReport
{
    public List<IFormFile>? Photos { get; set; }
}

/// <summary>The rules every incident report is filed by: at least one photo.</summary>
public sealed class IncidentReportRules : ReactiveValidator<IncidentReport>
{
    public IncidentReportRules()
    {
        RuleFor(x => x.Photos).NotEmpty().WithMessage("Attach a photo of the incident.");
    }
}
