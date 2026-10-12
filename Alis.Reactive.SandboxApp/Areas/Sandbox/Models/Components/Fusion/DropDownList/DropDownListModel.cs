namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DropDownListModel
    {
        public string? Category { get; set; }
    }

    /// <summary>The document events the DropDownList page raises and hears.</summary>
    public static class DropDownListEvents
    {
        public static readonly AppEvent DoGather = AppEvent.Named("do-gather");
    }
}
