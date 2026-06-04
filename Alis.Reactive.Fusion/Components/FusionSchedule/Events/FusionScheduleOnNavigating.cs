namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before Syncfusion Schedule changes date or view.
    /// Set <see cref="Cancel"/> to prevent navigation.
    /// </summary>
    public class FusionScheduleNavigatingArgs
    {
        /// <summary>Navigation action, either <c>date</c> or <c>view</c>.</summary>
        public string Action { get; set; } = "";

        /// <summary>Target date as an ISO string reported by Syncfusion.</summary>
        public string CurrentDate { get; set; } = "";

        /// <summary>Previous date as an ISO string reported by Syncfusion.</summary>
        public string PreviousDate { get; set; } = "";

        /// <summary>Target view, for example <c>Day</c> or <c>Week</c>. Empty on date navigation.</summary>
        public string CurrentView { get; set; } = "";

        /// <summary>Previous view when switching views. Empty on date navigation.</summary>
        public string PreviousView { get; set; } = "";

        /// <summary>Zero-based index of the target view when switching views.</summary>
        public int ViewIndex { get; set; }

        /// <summary>Set to true before the callback returns to cancel navigation.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleNavigatingArgs() { }
    }
}
