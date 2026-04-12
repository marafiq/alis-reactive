using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Mutable in-memory store for Point-in-Time Schedule sandbox.
    /// Data is generated on first access per facility+week, then mutations persist
    /// for the lifetime of the process so POST/PUT changes are visible on GET refresh.
    /// </summary>
    public static class FakeScheduleData
    {
        public static readonly List<Facility> Facilities = new()
        {
            new Facility { Id = "mystery-manor", Name = "Mystery Manor" },
            new Facility { Id = "sunrise-gardens", Name = "Sunrise Gardens" },
            new Facility { Id = "oak-ridge", Name = "Oak Ridge Senior Living" },
        };

        public static readonly List<ShiftResource> Shifts = new()
        {
            new ShiftResource { Id = 1, Text = "AM Shift (6:00 AM - 2:00 PM)", Color = "#1aaa55" },
            new ShiftResource { Id = 2, Text = "PM Shift (2:00 PM - 10:00 PM)", Color = "#357cd2" },
            new ShiftResource { Id = 3, Text = "Night Shift (10:00 PM - 6:00 AM)", Color = "#7fa900" },
        };

        public static readonly List<StaffMember> Staff = new()
        {
            new StaffMember { Id = 1, Name = "Jane Blonde", Role = "CNA", Phone = "555-0101" },
            new StaffMember { Id = 2, Name = "Mike Ross", Role = "RN", Phone = "555-0102" },
            new StaffMember { Id = 3, Name = "Sarah Connor", Role = "CNA", Phone = "555-0103" },
            new StaffMember { Id = 4, Name = "Tom Hardy", Role = "LPN", Phone = "555-0104" },
            new StaffMember { Id = 5, Name = "Lisa Chen", Role = "RN", Phone = "555-0105" },
            new StaffMember { Id = 6, Name = "Pat Night", Role = "CNA", Phone = "555-0106" },
            new StaffMember { Id = 7, Name = "Maria Garcia", Role = "RN", Phone = "555-0107" },
            new StaffMember { Id = 8, Name = "James Wilson", Role = "LPN", Phone = "555-0108" },
        };

        private static readonly string[][] ResidentGroups = new[]
        {
            new[] { "Adams, Baker, Clark" },
            new[] { "Davis, Evans" },
            new[] { "Foster, Grant, Hall" },
            new[] { "Irving, King" },
            new[] { "Lewis, Moore, Nelson" },
            new[] { "Parker, Quinn" },
            new[] { "Roberts, Smith" },
        };

        // Mutable store: key = "facilityId|weekSunday"
        private static readonly ConcurrentDictionary<string, List<ShiftAssignment>> Store = new();

        /// <summary>
        /// Gets (or generates) shift assignments for a facility within a date range.
        /// Spans multiple generated weeks if needed (e.g., Month view covers 6 weeks).
        /// Returned data is filtered to the requested range.
        /// </summary>
        public static ScheduleDataResponse GetAssignments(string facilityId, DateTime rangeStart, DateTime rangeEnd)
        {
            var assignments = new List<ShiftAssignment>();

            // Walk through each week that overlaps the range
            var current = rangeStart.AddDays(-(int)rangeStart.DayOfWeek);
            while (current < rangeEnd)
            {
                var key = $"{facilityId}|{current:yyyy-MM-dd}";
                var weekData = Store.GetOrAdd(key, _ => GenerateWeek(facilityId, current));
                assignments.AddRange(weekData.Where(a => a.StartTime >= rangeStart && a.StartTime < rangeEnd));
                current = current.AddDays(7);
            }

            return new ScheduleDataResponse
            {
                Assignments = assignments,
                Shifts = Shifts,
                UnassignedCount = assignments.Count(a => a.IsUnassigned),
            };
        }

        /// <summary>
        /// Finds a specific assignment by ID across all facility/week stores.
        /// </summary>
        public static ShiftAssignment? FindAssignment(int assignmentId)
        {
            foreach (var kvp in Store)
            {
                var assignment = kvp.Value.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null) return assignment;
            }
            return null;
        }

        /// <summary>
        /// Assigns a staff member to a specific assignment slot.
        /// Mutates the in-memory store so the next GET reflects the change.
        /// </summary>
        public static ShiftAssignment? AssignStaff(int assignmentId, int staffId)
        {
            var staff = Staff.FirstOrDefault(s => s.Id == staffId);
            if (staff == null) return null;

            foreach (var kvp in Store)
            {
                var assignment = kvp.Value.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null)
                {
                    assignment.StaffName = staff.Name;
                    assignment.StaffRole = staff.Role;
                    assignment.StaffPhone = staff.Phone;
                    assignment.IsUnassigned = false;
                    assignment.Subject = $"{staff.Name} ({staff.Role})";
                    assignment.Description = $"{assignment.CareItems} care items | {assignment.EstimatedMinutes} min | Residents: {assignment.ResidentNames}";
                    assignment.CategoryColor = null;
                    return assignment;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes staff from a specific assignment slot (makes it unassigned).
        /// Mutates the in-memory store so the next GET reflects the change.
        /// </summary>
        public static ShiftAssignment? UnassignStaff(int assignmentId)
        {
            foreach (var kvp in Store)
            {
                var assignment = kvp.Value.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment != null)
                {
                    var shiftLabel = Shifts.FirstOrDefault(s => s.Id == assignment.ShiftId)?.Text?.Split(' ')[0] ?? "Shift";
                    assignment.StaffName = null;
                    assignment.StaffRole = null;
                    assignment.StaffPhone = null;
                    assignment.IsUnassigned = true;
                    assignment.Subject = $"UNASSIGNED - {shiftLabel}";
                    assignment.Description = $"{assignment.CareItems} care items | {assignment.EstimatedMinutes} min | NEEDS COVERAGE";
                    assignment.CategoryColor = "#dc3545";
                    return assignment;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new shift assignment in the correct facility+week bucket.
        /// Used when user clicks an empty cell and assigns staff via the dialog.
        /// </summary>
        public static ShiftAssignment? CreateAssignment(string facilityId, DateTime startTime, DateTime endTime, int shiftId, int staffId)
        {
            var staff = Staff.FirstOrDefault(s => s.Id == staffId);
            if (staff == null) return null;

            var sunday = startTime.AddDays(-(int)startTime.DayOfWeek);
            var key = $"{facilityId}|{sunday:yyyy-MM-dd}";

            var assignments = Store.GetOrAdd(key, _ => GenerateWeek(facilityId, sunday));
            var nextId = assignments.Count > 0 ? assignments.Max(a => a.Id) + 1 : 1;
            var shiftLabel = Shifts.FirstOrDefault(s => s.Id == shiftId)?.Text?.Split(' ')[0] ?? "Shift";

            var assignment = new ShiftAssignment
            {
                Id = nextId,
                Subject = $"{staff.Name} ({staff.Role})",
                StartTime = startTime,
                EndTime = endTime,
                ShiftId = shiftId,
                StaffName = staff.Name,
                StaffRole = staff.Role,
                StaffPhone = staff.Phone,
                IsUnassigned = false,
                CareItems = 0,
                EstimatedMinutes = 0,
                ResidentNames = "",
                Description = $"New assignment | {staff.Name} ({staff.Role})",
            };

            assignments.Add(assignment);
            return assignment;
        }

        private static List<ShiftAssignment> GenerateWeek(string facilityId, DateTime sunday)
        {
            var rng = new Random(facilityId.GetHashCode() ^ sunday.GetHashCode());
            var assignments = new List<ShiftAssignment>();
            var id = Math.Abs(facilityId.GetHashCode() ^ sunday.GetHashCode()) % 10000;

            for (var dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var date = sunday.AddDays(dayOffset);

                foreach (var shift in Shifts)
                {
                    id++;
                    var (startHour, endHour) = GetShiftHours(shift.Id);
                    var startTime = date.AddHours(startHour);
                    var endTime = shift.Id == 3
                        ? date.AddDays(1).AddHours(endHour)
                        : date.AddHours(endHour);

                    var isUnassigned = rng.NextDouble() < 0.3;
                    var staffIndex = rng.Next(Staff.Count);
                    var staff = isUnassigned ? null : Staff[staffIndex];
                    var careItems = rng.Next(2, 8);
                    var estimatedMinutes = careItems * rng.Next(10, 20);
                    var residents = ResidentGroups[rng.Next(ResidentGroups.Length)][0];
                    var shiftLabel = shift.Text.Split(' ')[0];

                    assignments.Add(new ShiftAssignment
                    {
                        Id = id,
                        Subject = isUnassigned
                            ? $"UNASSIGNED - {shiftLabel}"
                            : $"{staff!.Name} ({staff.Role})",
                        StartTime = startTime,
                        EndTime = endTime,
                        ShiftId = shift.Id,
                        StaffName = staff?.Name,
                        StaffRole = staff?.Role,
                        StaffPhone = staff?.Phone,
                        IsUnassigned = isUnassigned,
                        CareItems = careItems,
                        EstimatedMinutes = estimatedMinutes,
                        ResidentNames = residents,
                        Description = isUnassigned
                            ? $"{careItems} care items | {estimatedMinutes} min | NEEDS COVERAGE"
                            : $"{careItems} care items | {estimatedMinutes} min | Residents: {residents}",
                        CategoryColor = isUnassigned ? "#dc3545" : null,
                    });
                }
            }

            return assignments;
        }

        private static (int start, int end) GetShiftHours(int shiftId) => shiftId switch
        {
            1 => (6, 14),
            2 => (14, 22),
            3 => (22, 6),
            _ => (6, 14),
        };
    }
}
