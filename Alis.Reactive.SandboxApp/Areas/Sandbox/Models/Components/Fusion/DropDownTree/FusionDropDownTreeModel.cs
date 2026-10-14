using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownTree
{
    /// <summary>
    /// A move-in coordinator at Maple Grove assigns a resident moving in this week to a room, chosen on the
    /// building plan by building, wing and room, or by typing the room's number.
    /// </summary>
    public sealed class FusionDropDownTreeModel
    {
        /// <summary>The resident moving in, by resident id.</summary>
        public string? Resident { get; set; }

        /// <summary>The room's number as the coordinator types it, such as Oak 110.</summary>
        public string? RoomNumber { get; set; }

        /// <summary>The room chosen on the building plan, as the plan's value: one node id.</summary>
        public string[]? Room { get; set; }

        /// <summary>The residents moving in this week.</summary>
        public IReadOnlyList<MovingInResident> MovingIn { get; init; } = [];

        /// <summary>The buildings, their wings and the wings' rooms, each node naming its parent.</summary>
        public IReadOnlyList<BuildingPlanNode> BuildingPlan { get; init; } = [];
    }

    /// <summary>A resident moving in this week: the id the page keeps and the name it shows.</summary>
    public sealed class MovingInResident
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>A building, wing or room on the building plan.</summary>
    public sealed class BuildingPlanNode
    {
        public string Value { get; init; } = "";

        public string Text { get; init; } = "";

        public string? ParentValue { get; init; }

        public bool HasChildren { get; init; }

        public bool Expanded { get; init; }
    }

    /// <summary>Asks which room, if any, a resident's deposit holds.</summary>
    public sealed class DepositQuery
    {
        public string? Resident { get; set; }
    }

    /// <summary>The room a resident's deposit holds, as the building plan's value; empty when none does.</summary>
    public sealed class DepositHold
    {
        public string[] Room { get; init; } = [];
    }

    /// <summary>The room the building plan held before a change, and the one it holds now, for a resident.</summary>
    public sealed class RoomHoldRequest
    {
        public string? Resident { get; set; }

        public string[]? From { get; set; }

        public string[]? To { get; set; }
    }

    /// <summary>What the front office says after a hold changes, or why it refused one.</summary>
    public sealed class RoomHoldNote
    {
        public string Message { get; init; } = "";
    }

    /// <summary>The move-in the coordinator books for the resident and the room on the plan.</summary>
    public sealed class RoomAssignmentBooked
    {
        public string Message { get; init; } = "";
    }

    /// <summary>A room is assigned only to a resident, and only once a room is chosen.</summary>
    public sealed class RoomAssignmentValidator : ReactiveValidator<FusionDropDownTreeModel>
    {
        public RoomAssignmentValidator()
        {
            RuleFor(x => x.Resident).NotEmpty().WithMessage("Choose who is moving in.");
            RuleFor(x => x.Room).NotEmpty().WithMessage("Choose a room on the building plan.");
        }
    }
}
