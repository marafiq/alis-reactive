using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownTree;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Maple Grove's room assignment for the week's move-ins: the residents moving in, the building plan,
    /// the room a deposit holds, the hold the plan's choice makes, and the move-in it books.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionDropDownTree")]
    public sealed class DropDownTreeController : Controller
    {
        private static readonly IReadOnlyList<Arrival> Arrivals =
        [
            new("r-2041", "Ruth Alvarez", "October 21", "maple-106"),
            new("r-2042", "Walter Chen", "October 22", null),
            new("r-2043", "Edith Moreau", "October 23", "oak-112")
        ];

        private static readonly IReadOnlyList<BuildingPlanNode> Plan =
        [
            new() { Value = "maple", Text = "Maple House", HasChildren = true, Expanded = true },
            new() { Value = "maple-north", Text = "North wing", ParentValue = "maple", HasChildren = true, Expanded = true },
            new() { Value = "maple-104", Text = "Maple 104", ParentValue = "maple-north" },
            new() { Value = "maple-106", Text = "Maple 106", ParentValue = "maple-north" },
            new() { Value = "maple-south", Text = "South wing", ParentValue = "maple", HasChildren = true, Expanded = true },
            new() { Value = "maple-121", Text = "Maple 121", ParentValue = "maple-south" },
            new() { Value = "maple-123", Text = "Maple 123", ParentValue = "maple-south" },
            new() { Value = "oak", Text = "Oak House", HasChildren = true, Expanded = true },
            new() { Value = "oak-memory", Text = "Memory care wing", ParentValue = "oak", HasChildren = true, Expanded = true },
            new() { Value = "oak-110", Text = "Oak 110", ParentValue = "oak-memory" },
            new() { Value = "oak-112", Text = "Oak 112", ParentValue = "oak-memory" }
        ];

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/DropDownTree/Index.cshtml", new FusionDropDownTreeModel
            {
                MovingIn = Arrivals.Select(arrival => new MovingInResident { Id = arrival.Id, Name = arrival.Name }).ToList(),
                BuildingPlan = Plan
            });

        [HttpGet("Deposit")]
        public IActionResult Deposit([FromQuery] DepositQuery query)
        {
            var arrival = FindArrival(query.Resident);
            return arrival is null
                ? NotFound()
                : Ok(new DepositHold { Room = arrival.DepositRoom is null ? [] : [arrival.DepositRoom] });
        }

        [HttpPost("Hold")]
        public IActionResult Hold([FromBody] RoomHoldRequest request)
        {
            var arrival = FindArrival(request.Resident);
            if (arrival is null)
                return BadRequest(new RoomHoldNote { Message = "Choose who is moving in before holding a room." });

            var to = FindNode(request.To);
            if (to is not null && to.HasChildren)
                return BadRequest(new RoomHoldNote { Message = $"{to.Text} is not a room. Choose one of its rooms." });

            var from = FindNode(request.From);
            var released = from is null || from.HasChildren ? null : from;
            var message = (released, to) switch
            {
                ({ } was, { } now) => $"{was.Text} is released; {now.Text} is held for {arrival.Name}.",
                (null, { } now) => $"{now.Text} is held for {arrival.Name}.",
                ({ } was, null) => $"{was.Text} is released.",
                _ => "No room is held."
            };
            return Ok(new RoomHoldNote { Message = message });
        }

        [HttpPost("Assign")]
        public IActionResult Assign([FromBody] FusionDropDownTreeModel assignment)
        {
            var arrival = FindArrival(assignment.Resident);
            var room = FindNode(assignment.Room);
            if (arrival is null || room is null || room.HasChildren)
                return BadRequest(new RoomAssignmentBooked { Message = "Choose who is moving in and one room for them." });

            return Ok(new RoomAssignmentBooked { Message = $"{arrival.Name} moves into {room.Text} on {arrival.MoveInDay}." });
        }

        private static Arrival? FindArrival(string? residentId) =>
            Arrivals.FirstOrDefault(arrival => arrival.Id == residentId);

        // The building plan holds one node at a time; its value is that node's id.
        private static BuildingPlanNode? FindNode(string[]? value) =>
            Plan.FirstOrDefault(node => node.Value == value?.FirstOrDefault());

        private sealed record Arrival(string Id, string Name, string MoveInDay, string? DepositRoom);
    }
}
