namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ShiftCardPageModel
    {
    }

    public sealed record ShiftCardShift(int Id, string Subject, DateTime StartTime, DateTime EndTime);

    public sealed record ShiftCardDay(IReadOnlyList<ShiftCardShift> Shifts, string Summary);

    public class ShiftCardQuery
    {
        public int ShiftId { get; set; }
    }

    public class ShiftCardModel
    {
        public int ShiftId { get; set; }
        public string Subject { get; set; } = "";
        public int Revision { get; set; }
    }

    public sealed record ShiftCardSignOffs(string Summary);
}
