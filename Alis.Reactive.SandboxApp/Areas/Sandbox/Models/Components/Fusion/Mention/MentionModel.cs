namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Mention
{
    /// <summary>
    /// The night nurse's shift handoff note for one resident: the note, which names with @ the people it
    /// notifies, and the day nurse who takes the resident over.
    /// </summary>
    public sealed class MentionModel
    {
        /// <summary>What happened on the shift; each @name in it notifies that person.</summary>
        public string? ShiftNote { get; set; }

        /// <summary>The day-shift nurse who takes the resident over.</summary>
        public string? NextNurse { get; set; }

        /// <summary>The resident's care team, who the note can notify.</summary>
        public IReadOnlyList<CareTeamMember> CareTeam { get; set; } = [];

        /// <summary>The nurses on the day shift, who the resident can be handed to.</summary>
        public IReadOnlyList<CareTeamMember> DayShift { get; set; } = [];
    }

    /// <summary>Someone the note can name: their staff ID, their name as the list shows it, and their role.</summary>
    public sealed class CareTeamMember
    {
        /// <summary>The staff ID.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>The name as the list shows it and the note writes it.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>What they do for the resident, for example "charge nurse".</summary>
        public string Role { get; set; } = string.Empty;
    }
}
