
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownTree
{
    public sealed class FusionDropDownTreeModel
    {
        public string? Resident { get; set; }

        public string? RoomNumber { get; set; }

        public string[]? Room { get; set; }

        public IReadOnlyList<MovingInResident> MovingIn { get; init; } = [];

        public IReadOnlyList<BuildingPlanNode> BuildingPlan { get; init; } = [];
    }

    public sealed class MovingInResident
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class BuildingPlanNode
    {
        public string Value { get; init; } = "";

        public string Text { get; init; } = "";

        public string? ParentValue { get; init; }

        public bool HasChildren { get; init; }

        public bool Expanded { get; init; }
    }

    public sealed class DepositQuery
    {
        public string? Resident { get; set; }
    }

    public sealed class DepositHold
    {
        public string[] Room { get; init; } = [];
    }

    public sealed class RoomHoldRequest
    {
        public string? Resident { get; set; }

        public string[]? From { get; set; }

        public string[]? To { get; set; }
    }

    public sealed class RoomHoldNote
    {
        public string Message { get; init; } = "";
    }

    public sealed class RoomAssignmentBooked
    {
        public string Message { get; init; } = "";
    }

    public sealed class RoomAssignmentValidator : ReactiveValidator<FusionDropDownTreeModel>
    {
        public RoomAssignmentValidator()
        {
            RuleFor(x => x.Resident).NotEmpty().WithMessage("Choose who is moving in.");
            RuleFor(x => x.Room).NotEmpty().WithMessage("Choose a room on the building plan.");
        }
    }
}
