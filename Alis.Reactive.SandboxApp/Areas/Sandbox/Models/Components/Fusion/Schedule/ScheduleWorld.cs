using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// One browser's schedule: each facility week it opens starts from the seed
    /// (<see cref="FakeScheduleData.GenerateWeek"/>) and keeps the browser's changes in its session, so a
    /// staff change one browser makes is never seen by another.
    /// </summary>
    public sealed class ScheduleWorld
    {
        private const string WeekKeyPrefix = "schedule-week:";
        private readonly ISession _session;

        public ScheduleWorld(ISession session)
        {
            _session = session;
        }

        /// <summary>The facility's shifts that start in the range, across every week it touches.</summary>
        public ScheduleDataResponse GetAssignments(string facilityId, DateTime rangeStart, DateTime rangeEnd)
        {
            var assignments = new List<ShiftAssignment>();
            for (var sunday = SundayOf(rangeStart); sunday < rangeEnd; sunday = sunday.AddDays(7))
                assignments.AddRange(Week(facilityId, sunday).Where(a => a.StartTime >= rangeStart && a.StartTime < rangeEnd));

            return new ScheduleDataResponse
            {
                Assignments = assignments,
                Shifts = FakeScheduleData.Shifts,
                UnassignedCount = assignments.Count(a => a.IsUnassigned),
            };
        }

        /// <summary>The shift with this id, in any week this browser opened.</summary>
        public ShiftAssignment? FindAssignment(int assignmentId) =>
            OpenedWeeks().SelectMany(week => week.Assignments).FirstOrDefault(a => a.Id == assignmentId);

        /// <summary>Puts the staff member on the shift; null when either is unknown.</summary>
        public ShiftAssignment? AssignStaff(int assignmentId, int staffId)
        {
            var staff = FakeScheduleData.Staff.FirstOrDefault(s => s.Id == staffId);
            return staff == null ? null : Change(assignmentId, assignment => FakeScheduleData.Assign(assignment, staff));
        }

        /// <summary>Takes the shift off the schedule and returns it; null when the shift is unknown.</summary>
        public ShiftAssignment? CancelAssignment(int assignmentId)
        {
            foreach (var (key, assignments) in OpenedWeeks())
            {
                var assignment = assignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment == null) continue;

                assignments.Remove(assignment);
                Save(key, assignments);
                return assignment;
            }
            return null;
        }

        /// <summary>Adds a staffed shift to the facility's week; null when the staff member is unknown.</summary>
        public ShiftAssignment? CreateAssignment(string facilityId, DateTime startTime, DateTime endTime, int shiftId, int staffId)
        {
            var staff = FakeScheduleData.Staff.FirstOrDefault(s => s.Id == staffId);
            if (staff == null) return null;

            var sunday = SundayOf(startTime);
            var week = Week(facilityId, sunday);
            var nextId = week.Count > 0 ? week.Max(a => a.Id) + 1 : 1;
            var assignment = FakeScheduleData.NewAssignment(nextId, startTime, endTime, shiftId, staff);
            week.Add(assignment);
            Save(WeekKey(facilityId, sunday), week);
            return assignment;
        }

        private ShiftAssignment? Change(int assignmentId, Action<ShiftAssignment> change)
        {
            foreach (var (key, assignments) in OpenedWeeks())
            {
                var assignment = assignments.FirstOrDefault(a => a.Id == assignmentId);
                if (assignment == null) continue;

                change(assignment);
                Save(key, assignments);
                return assignment;
            }
            return null;
        }

        private List<ShiftAssignment> Week(string facilityId, DateTime sunday)
        {
            var key = WeekKey(facilityId, sunday);
            var json = _session.GetString(key);
            if (json != null) return JsonSerializer.Deserialize<List<ShiftAssignment>>(json)!;

            var seeded = FakeScheduleData.GenerateWeek(facilityId, sunday);
            Save(key, seeded);
            return seeded;
        }

        private IEnumerable<(string Key, List<ShiftAssignment> Assignments)> OpenedWeeks() =>
            _session.Keys
                .Where(key => key.StartsWith(WeekKeyPrefix, StringComparison.Ordinal))
                .ToList()
                .Select(key => (key, JsonSerializer.Deserialize<List<ShiftAssignment>>(_session.GetString(key)!)!));

        private void Save(string key, List<ShiftAssignment> week) =>
            _session.SetString(key, JsonSerializer.Serialize(week));

        private static string WeekKey(string facilityId, DateTime sunday) => $"{WeekKeyPrefix}{facilityId}|{sunday:yyyy-MM-dd}";

        private static DateTime SundayOf(DateTime day) => day.Date.AddDays(-(int)day.DayOfWeek);
    }
}
