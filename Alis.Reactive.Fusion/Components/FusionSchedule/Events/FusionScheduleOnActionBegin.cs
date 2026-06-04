namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before Syncfusion Schedule starts an action.
    /// Set <see cref="Cancel"/> to prevent the action. <see cref="RequestType"/>
    /// identifies the action, for example <c>eventCreate</c>, <c>eventChange</c>, etc.
    /// </summary>
    public class FusionScheduleActionBeginArgs
    {
        /// <summary>Syncfusion action name, for example <c>eventCreate</c> or <c>dateNavigate</c>.</summary>
        public string RequestType { get; set; } = "";

        /// <summary>Set to true before the callback returns to cancel the action.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleActionBeginArgs() { }
    }
}
