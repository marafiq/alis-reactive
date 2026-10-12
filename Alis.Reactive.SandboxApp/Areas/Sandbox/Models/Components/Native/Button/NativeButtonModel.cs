namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// NativeButton uses string IDs instead of model binding and only needs
    /// a type for <c>Html.ReactivePlan&lt;T&gt;()</c>.
    /// </summary>
    public class NativeButtonModel
    {
    }

    /// <summary>The document events the NativeButton page raises and hears.</summary>
    public static class NativeButtonEvents
    {
        public static readonly AppEvent ResidentTransferred = AppEvent.Named("resident-transferred");
    }
}
