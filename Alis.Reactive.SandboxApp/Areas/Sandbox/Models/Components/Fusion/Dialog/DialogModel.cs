namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A resident's discharge review: it opens only when no care task is still open.</summary>
    public sealed class DialogModel
    {
        public bool CareTasksOpen { get; set; }
    }

    /// <summary>The discharge summary the review dialog loads when it opens.</summary>
    public sealed class DischargeSummaryModel
    {
    }
}
