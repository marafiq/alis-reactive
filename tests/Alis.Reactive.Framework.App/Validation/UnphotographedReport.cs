
namespace Alis.Reactive.Framework.App.Validation;

public sealed class UnphotographedReport
{
    public List<IFormFile>? Photos { get; set; }
}

public sealed class UnphotographedReportRules : ReactiveValidator<UnphotographedReport>
{
    public UnphotographedReportRules()
    {
        RuleFor(x => x.Photos).Empty().WithMessage("This resident declined photographs; attach none.");
    }
}
