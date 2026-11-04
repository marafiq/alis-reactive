namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Kanban
{
    public sealed class MaintenanceBoardModel
    {
        public bool ShowFinishedWork { get; set; } = true;
    }

    public static class WorkOrderStatus
    {
        public const string Open = "Open";
        public const string InProgress = "InProgress";
        public const string Done = "Done";

        public const string Blocked = "Blocked";
    }

    public static class WorkOrderPriority
    {
        public const string Urgent = "Urgent";
        public const string Routine = "Routine";
    }

    public static class MaintenanceWing
    {
        public const string Maple = "Maple";
        public const string Birch = "Birch";
    }

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

    public sealed record WorkOrder
    {
        public int Number { get; init; }
        public string Status { get; init; } = WorkOrderStatus.Open;
        public string Summary { get; init; } = "";
        public string Room { get; init; } = "";
        public string Wing { get; init; } = "";
        public string WingName { get; init; } = "";
        public string Priority { get; init; } = WorkOrderPriority.Routine;
        public string Technician { get; init; } = "";

        public int Queue { get; init; }

        public bool SignedOff { get; init; }

        public bool FinishedToday { get; init; }
    }

    public sealed class MaintenanceBoard
    {
        public WorkOrder[] WorkOrders { get; init; } = [];
        public string Message { get; init; } = "";

        public bool BackOrdered { get; init; }
    }

    public sealed class WorkOrderChange
    {
        public string RequestType { get; init; } = "";
        public List<WorkOrder> AddedRecords { get; init; } = new();
        public List<WorkOrder> ChangedRecords { get; init; } = new();
        public List<WorkOrder> DeletedRecords { get; init; } = new();

        public int? Index { get; init; }
    }

    public sealed class WorkOrderRefusal
    {
        public string Message { get; init; } = "";
    }

    public sealed class WorkOrderLookup
    {
        public int WorkOrderNumber { get; init; }
    }

    public sealed class WorkOrderHistory
    {
        public string History { get; init; } = "";
    }

    public sealed class WorkOrderHold
    {
        public string HeldBy { get; init; } = "";
        public string Message { get; init; } = "";
    }

    public sealed class WorkOrderDraft
    {
        public WorkOrder WorkOrder { get; init; } = new();
    }

    public sealed class FrontDeskCalls
    {
        public WorkOrder WorkOrder { get; init; } = new();
        public string Message { get; init; } = "";
    }

    public sealed class PartArrival
    {
        public WorkOrder WorkOrder { get; init; } = new();
    }

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
