using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class PointInTimeScheduleModel
    {
        public string? SelectedFacilityId { get; set; }

        public bool ScheduleLocked { get; set; }

        public bool NeedsCoverageOnly { get; set; }
    }

    public class ShiftAssignment
    {
        public int Id { get; set; }
        public string Subject { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public int ShiftId { get; set; }

        public string? StaffName { get; set; }
        public string? StaffRole { get; set; }
        public string? StaffPhone { get; set; }
        public bool IsUnassigned { get; set; }
        public int CareItems { get; set; }
        public int EstimatedMinutes { get; set; }
        public string? ResidentNames { get; set; }
        public string Description { get; set; } = "";
        public string? CategoryColor { get; set; }

        public bool IsInService { get; set; }

        public string OpenBy { get; set; } = "";

        public bool IsStaffedShift => !IsUnassigned && !IsInService;
    }

    public class ShiftResource
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public string Color { get; set; } = "";
    }

    public class Facility
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class StaffMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    public class ScheduleDataResponse
    {
        public List<ShiftAssignment> Assignments { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public sealed class ScheduleAssignmentsQuery
    {
        public string CurrentView { get; set; } = "";
        public DateTime CurrentDate { get; set; }
        public string? SelectedFacilityId { get; set; }
    }

    public sealed class EditAssignmentQuery
    {
        public int AssignmentId { get; set; }
    }

    public sealed class NewAssignmentQuery
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int GroupIndex { get; set; }
        public string? SelectedFacilityId { get; set; }
    }

    public sealed class AgencyCoverRequest
    {
        public List<ShiftAssignment> Shifts { get; set; } = new();
    }

    public sealed class AgencyCoverAnswer
    {
        public string Summary { get; set; } = "";
    }

    public sealed class InServiceClaimRequest
    {
        public int InServiceId { get; set; }
    }

    public sealed class InServiceChange
    {
        public int InServiceId { get; set; }
        public string Subject { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public sealed class InServiceRemoval
    {
        public int InServiceId { get; set; }
    }

    public sealed class InServiceAnswer
    {
        public string Note { get; set; } = "";
    }

    public class ScheduleActionPayload
    {
        public int Id { get; set; }
    }

    public class StaffOption
    {
        public string Text { get; set; } = "";
        public int Value { get; set; }
    }

    public class AssignStaffRequest
    {
        public int AssignmentId { get; set; }
        public int? StaffId { get; set; }
    }

    public sealed class CancelShiftRequest
    {
        public int AssignmentId { get; set; }
    }

    public class NewAssignmentModel
    {
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int ShiftId { get; set; }
        public string? FacilityId { get; set; }
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
    }

    public static class PointInTimeScheduleEvents
    {
        public static readonly AppEvent<ScheduleActionPayload> ScheduleEdit = AppEvent.Declare<ScheduleActionPayload>();
        public static readonly AppEvent<ScheduleActionPayload> ScheduleReassign = AppEvent.Declare<ScheduleActionPayload>();
    }
}
