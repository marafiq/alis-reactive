using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Kanban;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Maple Grove's work-order system behind the facilities lead's maintenance board: it holds the work
    /// orders, makes or refuses each change the board sends, and answers questions about one work order.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/Kanban")]
    public class KanbanController : Controller
    {
        // Each browser keeps its own work orders in its session, seeded on first read, so one test's
        // changes are never another's.
        private const string BoardKey = "maintenanceBoard";
        private const string NextIdKey = "maintenanceNextWorkOrderId";
        private const string FrontDeskCallTakenKey = "maintenanceFrontDeskCallTaken";
        private const string BrakeCableArrivedKey = "maintenanceBrakeCableArrived";
        private const int FirstNewWorkOrderId = 110;

        // Walter Finch's wheelchair waits on a brake cable that is back-ordered until Friday.
        private const int WheelchairWorkOrderId = 101;

        // Ana Ruiz is updating the call light work order at her desk.
        private const int HeldWorkOrderId = 102;

        // The family in Birch 106 withdrew the faucet request this morning; the board does not know yet.
        private const int WithdrawnWorkOrderId = 105;

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Kanban/Index.cshtml",
                new MaintenanceBoardModel());
        }

        [HttpGet("Board")]
        public IActionResult Board()
        {
            return Ok(new MaintenanceBoard { WorkOrders = [.. LoadBoard()] });
        }

        [HttpPost("Commit")]
        public IActionResult Commit([FromBody] WorkOrderChange change)
        {
            return change.RequestType switch
            {
                BoardRequestType.Added => Add(change),
                BoardRequestType.Changed => Change(change),
                BoardRequestType.Removed => FileAway(change),
                _ => BadRequest(new WorkOrderRefusal { Message = $"The work-order system does not know the change '{change.RequestType}'." })
            };
        }

        [HttpGet("History/{workOrderId:int}")]
        public IActionResult History(int workOrderId)
        {
            return Ok(new WorkOrderHistory { History = HistoryOf(workOrderId) });
        }

        [HttpGet("Hold/{workOrderId:int}")]
        public IActionResult Hold(int workOrderId)
        {
            return Ok(workOrderId == HeldWorkOrderId
                ? new WorkOrderHold { HeldBy = "Ana Ruiz", Message = "Ana Ruiz is updating this work order; try again when she is done." }
                : new WorkOrderHold());
        }

        // The number is reserved now; the work order is filed when the board adds it.
        [HttpPost("Draft")]
        public IActionResult Draft()
        {
            return Ok(new WorkOrderDraft
            {
                WorkOrder = new WorkOrder
                {
                    Id = TakeNextId(),
                    Status = WorkOrderStatus.Open,
                    Wing = MaintenanceWing.Maple,
                    WingName = WingNameOf(MaintenanceWing.Maple)
                }
            });
        }

        // The front desk took one urgent call this shift; it reaches the board when the lead asks for it.
        [HttpGet("FrontDeskCall")]
        public IActionResult FrontDeskCall()
        {
            if (HttpContext.Session.GetString(FrontDeskCallTakenKey) != null)
                return NotFound(new FrontDeskCalls { Message = "No new calls from the front desk." });

            HttpContext.Session.SetString(FrontDeskCallTakenKey, "taken");
            return Ok(new FrontDeskCalls
            {
                WorkOrder = new WorkOrder
                {
                    Id = TakeNextId(),
                    Status = WorkOrderStatus.Open,
                    Summary = "Toilet running nonstop",
                    Room = "Birch 110",
                    Wing = MaintenanceWing.Birch,
                    WingName = WingNameOf(MaintenanceWing.Birch),
                    Priority = WorkOrderPriority.Urgent,
                    Technician = "Ana Ruiz"
                }
            });
        }

        // The brake cable is in: the wheelchair's work order is ready to go back in progress.
        [HttpPost("PartArrived")]
        public IActionResult PartArrived()
        {
            HttpContext.Session.SetString(BrakeCableArrivedKey, "arrived");
            var wheelchair = LoadBoard().Single(workOrder => workOrder.Id == WheelchairWorkOrderId);
            return Ok(new PartArrival { WorkOrder = wheelchair with { Status = WorkOrderStatus.InProgress } });
        }

        [HttpPost("WorkLists")]
        public IActionResult SendWorkLists([FromBody] WorkLists lists)
        {
            return Ok(new WorkListsSent
            {
                Message = $"Sent the night technician {Count(lists.InProgress.Count)} in progress and the Maple wing's list of {lists.MapleWing.Count}."
            });
        }

        private IActionResult Add(WorkOrderChange change)
        {
            if (change.AddedRecords.Count == 0)
                return BadRequest(new WorkOrderRefusal { Message = "The board added no work order." });

            var board = LoadBoard();
            foreach (var added in change.AddedRecords)
                board = PlaceInLine(board, added with { WingName = WingNameOf(added.Wing) }, change.Index);
            SaveBoard(board);

            var first = change.AddedRecords[0];
            return Ok(new MaintenanceBoard
            {
                WorkOrders = [.. board],
                Message = $"Added work order {first.Id}, {first.Summary}, to {ColumnName(first.Status)}."
            });
        }

        private IActionResult Change(WorkOrderChange change)
        {
            if (change.ChangedRecords.Count == 0)
                return BadRequest(new WorkOrderRefusal { Message = "The board changed no work order." });

            var board = LoadBoard();
            var changed = change.ChangedRecords[0];
            var existing = board.SingleOrDefault(workOrder => workOrder.Id == changed.Id);
            if (existing == null)
                return Conflict(new WorkOrderRefusal { Message = $"Work order {changed.Id} is no longer on the board." });

            if (existing.Id == WithdrawnWorkOrderId)
                return Conflict(new WorkOrderRefusal
                {
                    Message = "The family in Birch 106 withdrew the faucet request this morning, so the work order stays where it was."
                });

            if (WaitsForItsPart(existing, changed))
            {
                board = PlaceInLine(board, existing with { Status = WorkOrderStatus.Blocked }, null);
                SaveBoard(board);
                return Ok(new MaintenanceBoard
                {
                    WorkOrders = [.. board],
                    BackOrdered = true,
                    Message = "The brake cable for Maple 118's wheelchair is back-ordered until Friday, so its work order waits for the part."
                });
            }

            // Work that has just been finished waits for the resident's sign-off.
            var moved = existing.Status != changed.Status;
            var finishedNow = moved && changed.Status == WorkOrderStatus.Done;
            var updated = changed with
            {
                WingName = WingNameOf(changed.Wing),
                SignedOff = !finishedNow && existing.SignedOff,
                FinishedToday = finishedNow || existing.FinishedToday
            };
            // Done lists work in the order it was finished; the board does not draw earlier finished work, so a
            // place the board gives there would not count it.
            var place = finishedNow ? null : change.Index ?? (moved ? null : (int?)existing.Queue);
            board = PlaceInLine(board, updated, place);
            SaveBoard(board);

            return Ok(new MaintenanceBoard
            {
                WorkOrders = [.. board],
                Message = moved
                    ? $"Moved work order {updated.Id}, {updated.Summary}, to {ColumnName(updated.Status)}."
                    : $"Saved work order {updated.Id}: {updated.Summary}."
            });
        }

        private IActionResult FileAway(WorkOrderChange change)
        {
            if (change.DeletedRecords.Count == 0)
                return BadRequest(new WorkOrderRefusal { Message = "The board filed away no work order." });

            var unsigned = change.DeletedRecords.FirstOrDefault(workOrder => !workOrder.SignedOff);
            if (unsigned != null)
                return Conflict(new WorkOrderRefusal { Message = $"Work order {unsigned.Id} has no sign-off yet, so nothing was filed away." });

            var filed = change.DeletedRecords.Select(workOrder => workOrder.Id).ToHashSet();
            var board = LoadBoard().Where(workOrder => !filed.Contains(workOrder.Id)).ToList();
            SaveBoard(board);

            return Ok(new MaintenanceBoard
            {
                WorkOrders = [.. board],
                Message = $"Filed away {Count(filed.Count)} of finished work."
            });
        }

        // Starting the wheelchair repair before the brake cable arrives parks it in the Blocked column.
        private bool WaitsForItsPart(WorkOrder existing, WorkOrder changed) =>
            existing.Id == WheelchairWorkOrderId
            && existing.Status == WorkOrderStatus.Open
            && changed.Status == WorkOrderStatus.InProgress
            && HttpContext.Session.GetString(BrakeCableArrivedKey) == null;

        // Puts the work order at its place in its column's line in its wing, the end when no place is given,
        // and numbers that line from 0.
        private static List<WorkOrder> PlaceInLine(List<WorkOrder> board, WorkOrder workOrder, int? place)
        {
            var others = board.Where(other => other.Id != workOrder.Id).ToList();
            bool InLine(WorkOrder other) => other.Status == workOrder.Status && other.Wing == workOrder.Wing;

            var line = others.Where(InLine).OrderBy(other => other.Queue).ToList();
            line.Insert(Math.Clamp(place ?? line.Count, 0, line.Count), workOrder);

            return others.Where(other => !InLine(other))
                .Concat(line.Select((inLine, queue) => inLine with { Queue = queue }))
                .ToList();
        }

        private static string Count(int workOrders) => workOrders == 1 ? "1 work order" : $"{workOrders} work orders";

        private static string ColumnName(string status) => status switch
        {
            WorkOrderStatus.InProgress => "In Progress",
            WorkOrderStatus.Blocked => "Waiting on parts",
            _ => status
        };

        private static string WingNameOf(string wing) => $"{wing} wing";

        private static string HistoryOf(int workOrderId) => workOrderId switch
        {
            101 => "Walter Finch's daughter reported it Monday; Dev Patel took it Monday afternoon.",
            102 => "The night nurse reported it at 2 a.m.; Dev Patel took it at the morning huddle.",
            103 => "Physical therapy asked for it Tuesday; Ana Ruiz started it this morning.",
            108 => "The front desk reported it yesterday; no technician has taken it yet.",
            _ => "Reported this week."
        };

        private List<WorkOrder> LoadBoard()
        {
            var json = HttpContext.Session.GetString(BoardKey);
            return json == null ? SeededBoard() : JsonSerializer.Deserialize<List<WorkOrder>>(json)!;
        }

        private void SaveBoard(List<WorkOrder> board) =>
            HttpContext.Session.SetString(BoardKey, JsonSerializer.Serialize(board));

        private int TakeNextId()
        {
            var next = HttpContext.Session.GetInt32(NextIdKey) ?? FirstNewWorkOrderId;
            HttpContext.Session.SetInt32(NextIdKey, next + 1);
            return next;
        }

        // Maple has nothing in progress, so a work order dropped there is first in line.
        private static List<WorkOrder> SeededBoard() =>
        [
            Seed(101, WorkOrderStatus.Open, 0, "Wheelchair brakes slipping", "Maple 118", MaintenanceWing.Maple, WorkOrderPriority.Routine, "Dev Patel"),
            Seed(102, WorkOrderStatus.Open, 1, "Call light not working", "Maple 214", MaintenanceWing.Maple, WorkOrderPriority.Urgent, "Dev Patel"),
            Seed(104, WorkOrderStatus.Done, 0, "Window sticks when opened", "Maple 210", MaintenanceWing.Maple, WorkOrderPriority.Routine, "Dev Patel") with { SignedOff = true, FinishedToday = true },
            Seed(103, WorkOrderStatus.InProgress, 0, "Replace bathroom grab bar", "Birch 205", MaintenanceWing.Birch, WorkOrderPriority.Urgent, "Ana Ruiz"),
            Seed(105, WorkOrderStatus.Open, 0, "Faucet drips in the kitchenette", "Birch 106", MaintenanceWing.Birch, WorkOrderPriority.Routine, "Ana Ruiz"),
            Seed(108, WorkOrderStatus.Open, 1, "Door closer squeaks", "Birch 104", MaintenanceWing.Birch, WorkOrderPriority.Routine, ""),
            Seed(109, WorkOrderStatus.Open, 2, "Shower drain backed up", "Birch 112", MaintenanceWing.Birch, WorkOrderPriority.Urgent, "Ana Ruiz"),
            Seed(106, WorkOrderStatus.Done, 0, "Replace TV remote batteries", "Birch 112", MaintenanceWing.Birch, WorkOrderPriority.Routine, "Ana Ruiz") with { SignedOff = true },
            Seed(107, WorkOrderStatus.Done, 1, "Hang new curtains", "Birch 101", MaintenanceWing.Birch, WorkOrderPriority.Routine, "Ana Ruiz") with { SignedOff = true, FinishedToday = true }
        ];

        private static WorkOrder Seed(int id, string status, int queue, string summary, string room, string wing, string priority, string technician) =>
            new()
            {
                Id = id,
                Status = status,
                Queue = queue,
                Summary = summary,
                Room = room,
                Wing = wing,
                WingName = WingNameOf(wing),
                Priority = priority,
                Technician = technician
            };
    }
}
