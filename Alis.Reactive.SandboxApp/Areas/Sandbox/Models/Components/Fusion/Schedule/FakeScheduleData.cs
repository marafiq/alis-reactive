namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The schedule's seed: facilities, shifts, staff, and each facility's generated week. What a
    /// browser changes lives in its own <see cref="ScheduleWorld"/>.
    /// </summary>
    public static class FakeScheduleData
    {
        public static readonly List<Facility> Facilities = new()
        {
            new Facility { Id = "maple-grove", Name = "Maple Grove" },
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
            new StaffMember { Id = 1, Name = "Janet Brooks", Role = "CNA", Phone = "555-0101" },
            new StaffMember { Id = 2, Name = "Marcus Reid", Role = "RN", Phone = "555-0102" },
            new StaffMember { Id = 3, Name = "Sarah Collins", Role = "CNA", Phone = "555-0103" },
            new StaffMember { Id = 4, Name = "Tom Harding", Role = "LPN", Phone = "555-0104" },
            new StaffMember { Id = 5, Name = "Lisa Chen", Role = "RN", Phone = "555-0105" },
            new StaffMember { Id = 6, Name = "Pat Nguyen", Role = "CNA", Phone = "555-0106" },
            new StaffMember { Id = 7, Name = "Maria Garcia", Role = "RN", Phone = "555-0107" },
            new StaffMember { Id = 8, Name = "James Wilson", Role = "LPN", Phone = "555-0108" },
        };

        /// <summary>The staff educator who teaches the in-services.</summary>
        public const string StaffEducator = "Dana Whitfield";

        /// <summary>The facility whose roster is full: every shift of its weeks starts with someone on it.</summary>
        private const string FullyStaffedFacilityId = "oak-ridge";

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

        /// <summary>Puts a staff member on a shift.</summary>
        public static void Assign(ShiftAssignment assignment, StaffMember staff)
        {
            assignment.StaffName = staff.Name;
            assignment.StaffRole = staff.Role;
            assignment.StaffPhone = staff.Phone;
            assignment.IsUnassigned = false;
            assignment.Subject = $"{staff.Name} ({staff.Role})";
            assignment.Description = $"{assignment.CareItems} care items | {assignment.EstimatedMinutes} min | Residents: {assignment.ResidentNames}";
            assignment.CategoryColor = null;
        }

        /// <summary>A shift a coordinator adds in an open hour, staffed from the start.</summary>
        public static ShiftAssignment NewAssignment(int id, DateTime startTime, DateTime endTime, int shiftId, StaffMember staff) => new()
        {
            Id = id,
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

        private static string ShiftLabel(int shiftId) =>
            Shifts.FirstOrDefault(s => s.Id == shiftId)?.Text?.Split(' ')[0] ?? "Shift";

        // string.GetHashCode is randomized per process, so seeding from it gave every sandbox start
        // a different week. A seed built from the facility's characters and the week's date gives
        // every run, machine, and CI shard the same week.
        private static int StableSeed(string facilityId, DateTime sunday)
        {
            var seed = sunday.Year * 400 + sunday.DayOfYear;
            foreach (var c in facilityId) seed = unchecked(seed * 31 + c);
            return seed;
        }

        /// <summary>
        /// The week a facility starts with, the same on every run, machine and CI shard: three shifts a day,
        /// some of them open except at Oak Ridge, whose roster is full, and two in-services the AM staff attend
        /// after their shift, on Tuesday and Thursday. The staff educator has Thursday's open to change it.
        /// </summary>
        public static List<ShiftAssignment> GenerateWeek(string facilityId, DateTime sunday)
        {
            var seed = StableSeed(facilityId, sunday);
            var rng = new Random(seed);
            var assignments = new List<ShiftAssignment>();
            var id = (seed & int.MaxValue) % 10000;

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

                    var isUnassigned = facilityId != FullyStaffedFacilityId && rng.NextDouble() < 0.3;
                    var staffIndex = rng.Next(Staff.Count);
                    var staff = isUnassigned ? null : Staff[staffIndex];
                    var careItems = rng.Next(2, 8);
                    var estimatedMinutes = careItems * rng.Next(10, 20);
                    var residents = ResidentGroups[rng.Next(ResidentGroups.Length)][0];

                    assignments.Add(new ShiftAssignment
                    {
                        Id = id,
                        Subject = isUnassigned
                            ? $"UNASSIGNED - {ShiftLabel(shift.Id)}"
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

            assignments.Add(InService(++id, "In-service: Falls prevention", sunday.AddDays(2), openBy: ""));
            assignments.Add(InService(++id, "In-service: Safe resident transfers", sunday.AddDays(4), openBy: StaffEducator));
            return assignments;
        }

        // An hour of training for the AM staff, from 2:30 PM, just after their shift ends.
        private static ShiftAssignment InService(int id, string subject, DateTime day, string openBy) => new()
        {
            Id = id,
            Subject = subject,
            StartTime = day.AddHours(14.5),
            EndTime = day.AddHours(15.5),
            ShiftId = 1,
            IsInService = true,
            OpenBy = openBy,
            Description = $"Maple wing day room | for the AM shift | taught by {StaffEducator}",
        };

        private static (int start, int end) GetShiftHours(int shiftId) => shiftId switch
        {
            1 => (6, 14),
            2 => (14, 22),
            3 => (22, 6),
            _ => (6, 14),
        };
    }
}
