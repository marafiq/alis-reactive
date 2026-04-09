using System;
using System.Collections.Generic;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // === View Model ===

    /// <summary>
    /// View model for the Point-in-Time Schedule sandbox page.
    /// Filters drive server-side data loading — the schedule never holds the full dataset.
    /// </summary>
    public class PointInTimeScheduleModel
    {
        public string? SelectedFacilityId { get; set; }
    }

    // === API DTOs ===

    /// <summary>
    /// A single shift assignment shown as an event on the schedule.
    /// Maps to SF Schedule eventSettings.dataSource items.
    /// </summary>
    public class ShiftAssignment
    {
        public int Id { get; set; }
        public string Subject { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public int ShiftId { get; set; }

        // Domain fields
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
    /// Shift resource — maps to SF Schedule resources[].dataSource items.
    /// </summary>
    public class ShiftResource
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public string Color { get; set; } = "";
    }

    /// <summary>
    /// Facility for the facility filter dropdown.
    /// </summary>
    public class Facility
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
    }

    /// <summary>
    /// Staff member for the assignment dialog dropdown.
    /// </summary>
    public class StaffMember
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    /// <summary>
    /// Response from GET /api/schedule/assignments — server-driven data loading.
    /// </summary>
    public class ScheduleDataResponse
    {
        public List<ShiftAssignment> Assignments { get; set; } = new();
        public List<ShiftResource> Shifts { get; set; } = new();
        public int UnassignedCount { get; set; }
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
    /// Request to POST /api/schedule/assign — assign staff to a shift slot.
    /// </summary>
    public class AssignStaffRequest
    {
        public int AssignmentId { get; set; }
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
    }
}
