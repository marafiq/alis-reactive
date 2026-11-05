namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Morning roster: the nurse checks residents in and refreshes the roster; the loader covers the
    /// row or panel the server is working on, and the roster is redrawn from the server's answer.
    /// </summary>
    public class ResidentRosterPageModel
    {
        public ResidentRoster Roster { get; init; } = new([]);
    }

    /// <summary>A resident on the morning roster.</summary>
    public sealed record RosterResident(int Id, string Name, bool CheckedIn);

    /// <summary>The roster rows the server draws.</summary>
    public sealed record ResidentRoster(IReadOnlyList<RosterResident> Residents);

    /// <summary>The care summary of the resident the roster opens with.</summary>
    public sealed record RosterCareSummary(string Summary);
}
