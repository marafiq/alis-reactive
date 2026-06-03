namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionSchedule"/> component.
    /// </summary>
    public sealed class FusionScheduleEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionScheduleEvents Instance = new FusionScheduleEvents();
        private FusionScheduleEvents() { }

        /// <summary>Fires when a time cell is clicked.
        /// Includes start/end time, group index, and all-day state.</summary>
        public TypedEvent<FusionScheduleCellClickArgs> CellClicked =>
            new TypedEvent<FusionScheduleCellClickArgs>(
                "cellClick", new FusionScheduleCellClickArgs());

        /// <summary>Fires when an event/appointment is clicked.</summary>
        public TypedEvent<FusionScheduleEventClickArgs> EventClicked =>
            new TypedEvent<FusionScheduleEventClickArgs>(
                "eventClick", new FusionScheduleEventClickArgs());

        /// <summary>Fires before a scheduler action begins.
        /// Use <c>RequestType</c> to distinguish <c>eventCreate</c>, <c>eventChange</c>, etc.</summary>
        public TypedEvent<FusionScheduleActionBeginArgs> ActionBegin =>
            new TypedEvent<FusionScheduleActionBeginArgs>(
                "actionBegin", new FusionScheduleActionBeginArgs());

        /// <summary>Fires after a scheduler action completes.
        /// Use <c>RequestType</c> to identify the completed action.</summary>
        public TypedEvent<FusionScheduleActionCompleteArgs> ActionComplete =>
            new TypedEvent<FusionScheduleActionCompleteArgs>(
                "actionComplete", new FusionScheduleActionCompleteArgs());

        /// <summary>Fires before date or view navigation.
        /// Use <c>Action</c> to distinguish <c>date</c> and <c>view</c> navigation. Cancelable.</summary>
        public TypedEvent<FusionScheduleNavigatingArgs> Navigating =>
            new TypedEvent<FusionScheduleNavigatingArgs>(
                "navigating", new FusionScheduleNavigatingArgs());

        /// <summary>Fires before a popup opens.
        /// Use <c>Type</c> to distinguish <c>QuickInfo</c>, <c>Editor</c>, etc. Cancelable.</summary>
        public TypedEvent<FusionSchedulePopupOpenArgs> PopupOpen =>
            new TypedEvent<FusionSchedulePopupOpenArgs>(
                "popupOpen", new FusionSchedulePopupOpenArgs());

        /// <summary>Fires when a popup closes.</summary>
        public TypedEvent<FusionSchedulePopupCloseArgs> PopupClose =>
            new TypedEvent<FusionSchedulePopupCloseArgs>(
                "popupClose", new FusionSchedulePopupCloseArgs());

        /// <summary>Fires after data is loaded and rendered.</summary>
        public TypedEvent<FusionScheduleDataBoundArgs> DataBound =>
            new TypedEvent<FusionScheduleDataBoundArgs>(
                "dataBound", new FusionScheduleDataBoundArgs());

        /// <summary>Fires before each event renders.
        /// Used for custom styling (e.g., unassigned shifts in red). Cancelable.</summary>
        public TypedEvent<FusionScheduleEventRenderedArgs> EventRendered =>
            new TypedEvent<FusionScheduleEventRenderedArgs>(
                "eventRendered", new FusionScheduleEventRenderedArgs());
    }
}
