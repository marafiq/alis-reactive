using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>An incident report for a resident who declined to be photographed, filed with no photos.</summary>
public sealed class UnphotographedReport
{
    public List<IFormFile>? Photos { get; set; }
}

/// <summary>The rules a report for a resident who declined photographs is filed by: no photo at all.</summary>
public sealed class UnphotographedReportRules : ReactiveValidator<UnphotographedReport>
{
    public UnphotographedReportRules()
    {
        RuleFor(x => x.Photos).Empty().WithMessage("This resident declined photographs; attach none.");
    }
}
