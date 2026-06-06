using System;
using System.Collections.Generic;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Point-in-Time Schedule state for server-filtered data loading.
    /// Filters drive each data-source request; the schedule never holds the full dataset.
    /// </summary>
    public class PointInTimeScheduleModel
    {
        public string? SelectedFacilityId { get; set; }
    }

    /// <summary>
    /// Shift assignment event rendered by the Schedule data source.
    /// Maps to Syncfusion Schedule eventSettings.dataSource items.
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
    /// Schedule data-source response for server-filtered assignment loading.
    /// </summary>
    public class ScheduleDataResponse
    {
        public List<ShiftAssignment> Assignments { get; set; } = new();
        public List<ShiftResource> Shifts { get; set; } = new();
        public int UnassignedCount { get; set; }
    }

    public class ScheduleViewRouteResponse
    {
        public string CurrentView { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    public class ScheduleEventsAuditRequest
    {
        public List<FusionScheduleEventData> Events { get; set; } = new();
    }

    public class ScheduleEventsAuditResponse
    {
        public int Count { get; set; }
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
        public string? Notes { get; set; }
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
}
