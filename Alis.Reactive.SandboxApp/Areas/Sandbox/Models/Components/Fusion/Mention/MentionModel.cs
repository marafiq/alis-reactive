namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Mention
{
    public sealed class MentionModel
    {
        public string? ShiftNote { get; set; }

        public string? NextNurse { get; set; }

        public IReadOnlyList<CareTeamMember> CareTeam { get; set; } = [];

        public IReadOnlyList<CareTeamMember> DayShift { get; set; } = [];
    }

    public sealed class CareTeamMember
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
