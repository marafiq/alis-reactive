namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentRosterPageModel
    {
        public MorningRosterRows Roster { get; init; } = new([]);
    }

    public sealed record MorningRosterResident(int Id, string Name, bool CheckedIn);

    public sealed record MorningRosterRows(IReadOnlyList<MorningRosterResident> Residents);

    public sealed record RosterCareSummary(string Summary);
}
