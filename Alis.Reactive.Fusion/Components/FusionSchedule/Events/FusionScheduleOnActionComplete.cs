namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered after Syncfusion Schedule completes an action successfully.
    /// </summary>
    public class FusionScheduleActionCompleteArgs
    {
        /// <summary>Syncfusion action name that completed, for example <c>eventChange</c> or <c>viewNavigate</c>.</summary>
        public string RequestType { get; set; } = "";
    }
}
