namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.Navigating.
    /// Fires before date or view navigation. Set cancel to prevent.
    /// action: "date" for arrow navigation, "view" for Day/Week/Month tab switch.
    /// </summary>
    public class FusionScheduleNavigatingArgs
    {
        /// <summary>The navigation action: "date" or "view".</summary>
        public string Action { get; set; } = "";

        /// <summary>The date being navigated TO (ISO string from SF).</summary>
        public string CurrentDate { get; set; } = "";

        /// <summary>The date being navigated FROM (ISO string from SF).</summary>
        public string PreviousDate { get; set; } = "";

        /// <summary>The view being navigated TO: "Day", "Week", "WorkWeek", "Month", "Agenda".
        /// Present on view switch (action="view"). Empty on date navigation.</summary>
        public string CurrentView { get; set; } = "";

        /// <summary>The view being navigated FROM.
        /// Present on view switch (action="view"). Empty on date navigation.</summary>
        public string PreviousView { get; set; } = "";

        /// <summary>Zero-based index of the target view in the views array.
        /// Present on view switch (action="view").</summary>
        public int ViewIndex { get; set; }

        /// <summary>Set to true to cancel the navigation.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleNavigatingArgs() { }
    }
}
