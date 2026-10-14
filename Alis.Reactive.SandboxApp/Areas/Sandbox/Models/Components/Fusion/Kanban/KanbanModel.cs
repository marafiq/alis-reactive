namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Kanban
{
    /// <summary>
    /// Maple Grove's maintenance board: the facilities lead's work orders for the Maple and Birch wings, by
    /// the column they are in.
    /// </summary>
    public sealed class MaintenanceBoardModel
    {
        /// <summary>The board shows the Done column; the lead hides it to see only the work that is left.</summary>
        public bool ShowFinishedWork { get; set; } = true;
    }

    /// <summary>The board's columns, by the status a work order is in.</summary>
    public static class WorkOrderStatus
    {
        public const string Open = "Open";
        public const string InProgress = "InProgress";
        public const string Done = "Done";

        /// <summary>The column a work order waits in while its part is on order; it appears when one is.</summary>
        public const string Blocked = "Blocked";
    }

    public static class WorkOrderPriority
    {
        public const string Urgent = "Urgent";
        public const string Routine = "Routine";
    }

    /// <summary>The board's rows, one for each wing.</summary>
    public static class MaintenanceWing
    {
        public const string Maple = "Maple";
        public const string Birch = "Birch";
    }

    /// <summary>
    /// The names the Kanban gives what it is doing: the request type of its action and change events
    /// (ej2-kanban crud.js), of its card dialog (dialog.js) and of the column header it renders
    /// (layout-render.js).
    /// </summary>
    public static class BoardRequestType
    {
        public const string Added = "cardCreated";
        public const string Changing = "cardChange";
        public const string Changed = "cardChanged";
        public const string Removing = "cardRemove";
        public const string Removed = "cardRemoved";
        public const string AddDialog = "Add";
        public const string EditDialog = "Edit";
        public const string DeleteDialog = "Delete";
        public const string HeaderRow = "headerRow";
    }

    /// <summary>A work order: what is wrong, in which room, who fixes it, and its place in its column's line.</summary>
    public sealed record WorkOrder
    {
        /// <summary>
        /// The number the work-order system gives it, the board's key for it. It is not named <c>Id</c>: the card
        /// dialog names each field's input by its member, and an input named <c>id</c> hides the dialog form's own
        /// <c>id</c>, which EJ2 looks the form up by before it saves (ej2-kanban dialog.js:334-335), so Save would
        /// do nothing.
        /// </summary>
        public int Number { get; init; }
        public string Status { get; init; } = WorkOrderStatus.Open;
        public string Summary { get; init; } = "";
        public string Room { get; init; } = "";
        public string Wing { get; init; } = "";
        public string WingName { get; init; } = "";
        public string Priority { get; init; } = WorkOrderPriority.Routine;
        public string Technician { get; init; } = "";

        /// <summary>Its place in the line of work orders in its column and wing, first at 0.</summary>
        public int Queue { get; init; }

        /// <summary>The resident has signed off on the finished work.</summary>
        public bool SignedOff { get; init; }

        /// <summary>The work was finished today; the board shows only today's finished work.</summary>
        public bool FinishedToday { get; init; }
    }

    /// <summary>The work orders the work-order system holds for the board, and what it says about a change.</summary>
    public sealed class MaintenanceBoard
    {
        public WorkOrder[] WorkOrders { get; init; } = [];
        public string Message { get; init; } = "";

        /// <summary>The change put a work order in the Blocked column: its part is on order.</summary>
        public bool BackOrdered { get; init; }
    }

    /// <summary>A change the board asks the work-order system to make, as the board reports it.</summary>
    public sealed class WorkOrderChange
    {
        public string RequestType { get; init; } = "";
        public List<WorkOrder> AddedRecords { get; init; } = new();
        public List<WorkOrder> ChangedRecords { get; init; } = new();
        public List<WorkOrder> DeletedRecords { get; init; } = new();

        /// <summary>The place in its column's line the change puts a work order, when the board gives one.</summary>
        public int? Index { get; init; }
    }

    /// <summary>Why the work-order system did not make a change.</summary>
    public sealed class WorkOrderRefusal
    {
        public string Message { get; init; } = "";
    }

    /// <summary>One work order, by the number in the path.</summary>
    public sealed class WorkOrderLookup
    {
        public int WorkOrderNumber { get; init; }
    }

    public sealed class WorkOrderHistory
    {
        public string History { get; init; } = "";
    }

    /// <summary>Who is updating a work order at another desk; empty when no one is.</summary>
    public sealed class WorkOrderHold
    {
        public string HeldBy { get; init; } = "";
        public string Message { get; init; } = "";
    }

    /// <summary>A new work order with its number reserved, for the lead to describe in the card dialog.</summary>
    public sealed class WorkOrderDraft
    {
        public WorkOrder WorkOrder { get; init; } = new();
    }

    /// <summary>The front desk's newest call, or why there is none.</summary>
    public sealed class FrontDeskCalls
    {
        public WorkOrder WorkOrder { get; init; } = new();
        public string Message { get; init; } = "";
    }

    /// <summary>The work order whose part arrived, ready to go back in progress.</summary>
    public sealed class PartArrival
    {
        public WorkOrder WorkOrder { get; init; } = new();
    }

    /// <summary>The lists the night technician gets: the work in progress and the Maple wing's work.</summary>
    public sealed class WorkLists
    {
        public List<WorkOrder> InProgress { get; init; } = new();
        public List<WorkOrder> MapleWing { get; init; } = new();
    }

    public sealed class WorkListsSent
    {
        public string Message { get; init; } = "";
    }
}
