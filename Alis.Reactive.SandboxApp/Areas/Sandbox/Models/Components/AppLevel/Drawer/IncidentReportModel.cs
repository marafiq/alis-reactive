using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Incident reports: the nurse files a report from the drawer while the page stays in view.
    /// </summary>
    public class IncidentReportPageModel
    {
    }

    /// <summary>The incident report a nurse files from the drawer.</summary>
    public class IncidentReportModel
    {
        public string? Summary { get; set; }
    }

    /// <summary>A report must say what happened before it is filed.</summary>
    public class IncidentReportValidator : ReactiveValidator<IncidentReportModel>
    {
        public IncidentReportValidator()
        {
            ClientRule(x => x.Summary).Required("Describe what happened.");
        }
    }

    /// <summary>The confirmation the server returns once the report is filed.</summary>
    public sealed record IncidentReportFiled(string Confirmation);
}
