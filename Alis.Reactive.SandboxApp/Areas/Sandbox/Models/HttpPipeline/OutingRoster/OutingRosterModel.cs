namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Garden outing: the activities coordinator finds the residents who use the mobility aids chosen
    /// (everyone when none is chosen) and books the bus with boarding help for those aids.
    /// </summary>
    public class OutingRosterModel
    {
        /// <summary>The codes of the mobility aids chosen; null until the coordinator chooses one.</summary>
        public string[]? MobilityAids { get; set; }
    }

    /// <summary>A mobility aid the coordinator can choose: the code the server matches, the name people read.</summary>
    public sealed record MobilityAidOption(string Code, string Name);

    /// <summary>The residents who can join, as the server lists them.</summary>
    public sealed record OutingRosterAnswer(string Residents);

    /// <summary>The bus booking the server confirms.</summary>
    public sealed record OutingBookingAnswer(string Confirmation);

    /// <summary>Why the server cannot book the bus.</summary>
    public sealed record OutingBookingRefusal(string Reason);
}
