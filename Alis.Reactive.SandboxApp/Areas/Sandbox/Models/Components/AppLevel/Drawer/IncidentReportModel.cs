
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class IncidentReportPageModel
    {
    }

    public class IncidentReportModel
    {
        public string? Summary { get; set; }
    }

    public class IncidentReportValidator : ReactiveValidator<IncidentReportModel>
    {
        public IncidentReportValidator()
        {
            RuleFor(x => x.Summary).NotEmpty().WithMessage("Describe what happened.");
        }
    }

    public sealed record IncidentReportFiled(string Confirmation);
}
