namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Shift cards: the charge nurse opens a shift's card on today's floor schedule to acknowledge its handover.
    /// </summary>
    public class ShiftCardPageModel
    {
    }

    /// <summary>A shift on today's floor schedule.</summary>
    public sealed record ShiftCardShift(int Id, string Subject, DateTime StartTime, DateTime EndTime);

    /// <summary>Today's shifts, as the schedule loads them.</summary>
    public sealed record ShiftCardDay(IReadOnlyList<ShiftCardShift> Shifts, string Summary);

    /// <summary>The card a shift opens: its handover and the action that acknowledges it.</summary>
    public class ShiftCardModel
    {
        public int ShiftId { get; set; }
        public string Subject { get; set; } = "";
    }
}
