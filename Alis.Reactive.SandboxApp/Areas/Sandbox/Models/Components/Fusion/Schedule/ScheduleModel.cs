using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The staffing coordinator's shift schedule: the facility whose shifts it shows, and the two switches
    /// the coordinator keeps beside it.
    /// </summary>
    public class PointInTimeScheduleModel
    {
        public string? SelectedFacilityId { get; set; }

        /// <summary>While payroll runs, the schedule stays on its week and no shift opens.</summary>
        public bool ScheduleLocked { get; set; }

        /// <summary>Shows only the shifts no one covers yet.</summary>
        public bool NeedsCoverageOnly { get; set; }
    }

    /// <summary>
    /// One entry of the schedule: a shift (staffed, or open until someone covers it) or an in-service the
    /// shift's staff attend. Maps to Syncfusion Schedule eventSettings.dataSource items.
    /// </summary>
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

        /// <summary>An in-service: training the shift's staff attend. It opens in the schedule's own editor.</summary>
        public bool IsInService { get; set; }

        /// <summary>Who else has the in-service open to change it right now; empty when no one does.</summary>
        public string OpenBy { get; set; } = "";

        /// <summary>A shift someone covers; its details offer to edit or reassign it.</summary>
        public bool IsStaffedShift => !IsUnassigned && !IsInService;
    }

    /// <summary>
    /// Shift resource mapped to Syncfusion Schedule resources[].dataSource items.
    /// </summary>
    public class ShiftResource
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public string Color { get; set; } = "";
    }

    /// <summary>
    /// Facility option for the Schedule filter dropdown.
    /// </summary>
    public class Facility
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    /// <summary>
    /// Staff option for assignment dialog dropdowns.
    /// </summary>
    public class StaffMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    /// <summary>
    /// The shifts of the dates a view shows, and the server's summary of them: the facility, the dates it
    /// answered for, and how many shifts still need someone.
    /// </summary>
    public class ScheduleDataResponse
    {
        public List<ShiftAssignment> Assignments { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>
    /// The request of <c>GET Schedule/Shifts/{currentView}</c> as the page reads it from the schedule: the
    /// view (in the path), the date it shows and the facility.
    /// </summary>
    public sealed class ScheduleAssignmentsQuery
    {
        public string CurrentView { get; set; } = "";
        public DateTime CurrentDate { get; set; }
        public string? SelectedFacilityId { get; set; }
    }

    /// <summary>
    /// The request of <c>GET Schedule/Shifts/{currentView}</c> as the schedule navigates, whose navigating
    /// event hands the date it moves to as text.
    /// </summary>
    public sealed class ScheduleNavigationQuery
    {
        public string CurrentView { get; set; } = "";
        public string CurrentDate { get; set; } = "";
        public string? SelectedFacilityId { get; set; }
    }

    /// <summary>The query of <c>GET Schedule/EditForm</c>: the assignment whose edit form loads.</summary>
    public sealed class EditAssignmentQuery
    {
        public int AssignmentId { get; set; }
    }

    /// <summary>
    /// The query of <c>GET Schedule/NewAssignmentForm</c>: the clicked cell's times, the shift column it is
    /// in (0-based, as the schedule numbers its groups), and the facility.
    /// </summary>
    public sealed class NewAssignmentQuery
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int GroupIndex { get; set; }
        public string? SelectedFacilityId { get; set; }
    }

    /// <summary>The request of <c>POST Schedule/AgencyRequest</c>: the shifts the schedule holds.</summary>
    public sealed class AgencyCoverRequest
    {
        public List<ShiftAssignment> Shifts { get; set; } = new();
    }

    /// <summary>The agency's answer: which of the sent shifts it was asked to cover.</summary>
    public sealed class AgencyCoverAnswer
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>The request of <c>POST Schedule/InServiceClaim</c>: the in-service the coordinator opened.</summary>
    public sealed class InServiceClaimRequest
    {
        public int InServiceId { get; set; }
    }

    /// <summary>The server's answer to opening an in-service: whether it is the coordinator's to change.</summary>
    public sealed class InServiceClaimAnswer
    {
        public string Note { get; set; } = "";
    }

    /// <summary>
    /// Payload for schedule action CustomEvents dispatched by QuickInfo template buttons.
    /// Matches the { detail: { id: ... } } shape from EventButton.
    /// </summary>
    public class ScheduleActionPayload
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// Staff option for the edit form dropdown.
    /// </summary>
    public class StaffOption
    {
        public string Text { get; set; } = "";
        public int Value { get; set; }
    }

    /// <summary>
    /// Assign-staff request for a shift slot.
    /// </summary>
    public class AssignStaffRequest
    {
        public int AssignmentId { get; set; }
        public int? StaffId { get; set; }
    }

    /// <summary>The request of <c>POST api/schedule/cancel-shift</c>: the shift to take off the schedule.</summary>
    public sealed class CancelShiftRequest
    {
        public int AssignmentId { get; set; }
    }

    /// <summary>
    /// New-assignment form model loaded into FusionDialog via Into().
    /// CellClicked provides startTime/endTime/groupIndex — controller maps groupIndex to shiftId.
    /// </summary>
    public class NewAssignmentModel
    {
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int ShiftId { get; set; }
        public string? FacilityId { get; set; }
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>The document events the PointInTimeSchedule page raises and hears.</summary>
    public static class PointInTimeScheduleEvents
    {
        public static readonly AppEvent<ScheduleActionPayload> ScheduleEdit = AppEvent.Named<ScheduleActionPayload>("schedule:edit");
        public static readonly AppEvent<ScheduleActionPayload> ScheduleReassign = AppEvent.Named<ScheduleActionPayload>("schedule:reassign");
    }
}
