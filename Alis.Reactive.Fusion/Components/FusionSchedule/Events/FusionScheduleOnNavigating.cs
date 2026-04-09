namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.Navigating (SF "navigating" event).
    /// Fires before date or view navigation. Set cancel to prevent.
    /// Verified: sf-schedule-test.html — action is "date" or "view",
    /// currentDate/previousDate are ISO date strings.
    /// Use this to trigger server-side data loading for the new date range.
    /// </summary>
    public class FusionScheduleNavigatingArgs
    {
        /// <summary>The navigation action: "date" or "view".</summary>
        public string Action { get; set; } = "";

        /// <summary>The date being navigated TO (ISO string from SF).</summary>
        public string CurrentDate { get; set; } = "";

        /// <summary>The date being navigated FROM (ISO string from SF).</summary>
        public string PreviousDate { get; set; } = "";

        /// <summary>Set to true to cancel the navigation.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleNavigatingArgs() { }
    }
}
