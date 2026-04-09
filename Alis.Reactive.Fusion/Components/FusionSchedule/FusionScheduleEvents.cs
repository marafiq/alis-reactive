namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionSchedule.
    /// Singleton instance — used with .Reactive() event selector lambda.
    /// </summary>
    public sealed class FusionScheduleEvents
    {
        public static readonly FusionScheduleEvents Instance = new FusionScheduleEvents();
        private FusionScheduleEvents() { }

        /// <summary>Fires when a time cell is clicked (SF "cellClick" event).
        /// Payload: startTime, endTime, groupIndex, isAllDay.</summary>
        public TypedEvent<FusionScheduleCellClickArgs> CellClicked =>
            new TypedEvent<FusionScheduleCellClickArgs>(
                "cellClick", new FusionScheduleCellClickArgs());

        /// <summary>Fires when an event/appointment is clicked (SF "eventClick" event).</summary>
        public TypedEvent<FusionScheduleEventClickArgs> EventClicked =>
            new TypedEvent<FusionScheduleEventClickArgs>(
                "eventClick", new FusionScheduleEventClickArgs());

        /// <summary>Fires before a scheduler action begins (SF "actionBegin" event).
        /// requestType: "eventCreate", "eventChange", "eventRemove", "dateNavigate", "viewNavigate".</summary>
        public TypedEvent<FusionScheduleActionBeginArgs> ActionBegin =>
            new TypedEvent<FusionScheduleActionBeginArgs>(
                "actionBegin", new FusionScheduleActionBeginArgs());

        /// <summary>Fires after a scheduler action completes (SF "actionComplete" event).
        /// Contains addedRecords, changedRecords, deletedRecords.</summary>
        public TypedEvent<FusionScheduleActionCompleteArgs> ActionComplete =>
            new TypedEvent<FusionScheduleActionCompleteArgs>(
                "actionComplete", new FusionScheduleActionCompleteArgs());

        /// <summary>Fires before date or view navigation (SF "navigating" event).
        /// action: "date" or "view". Cancelable.</summary>
        public TypedEvent<FusionScheduleNavigatingArgs> Navigating =>
            new TypedEvent<FusionScheduleNavigatingArgs>(
                "navigating", new FusionScheduleNavigatingArgs());

        /// <summary>Fires before a popup opens (SF "popupOpen" event).
        /// type: "QuickInfo", "Editor", "DeleteAlert". Cancelable.</summary>
        public TypedEvent<FusionSchedulePopupOpenArgs> PopupOpen =>
            new TypedEvent<FusionSchedulePopupOpenArgs>(
                "popupOpen", new FusionSchedulePopupOpenArgs());

        /// <summary>Fires when a popup closes (SF "popupClose" event).</summary>
        public TypedEvent<FusionSchedulePopupCloseArgs> PopupClose =>
            new TypedEvent<FusionSchedulePopupCloseArgs>(
                "popupClose", new FusionSchedulePopupCloseArgs());

        /// <summary>Fires after data is loaded and rendered (SF "dataBound" event).</summary>
        public TypedEvent<FusionScheduleDataBoundArgs> DataBound =>
            new TypedEvent<FusionScheduleDataBoundArgs>(
                "dataBound", new FusionScheduleDataBoundArgs());

        /// <summary>Fires before each event renders (SF "eventRendered" event).
        /// Used for custom styling (e.g., unassigned shifts in red). Cancelable.</summary>
        public TypedEvent<FusionScheduleEventRenderedArgs> EventRendered =>
            new TypedEvent<FusionScheduleEventRenderedArgs>(
                "eventRendered", new FusionScheduleEventRenderedArgs());
    }
}
