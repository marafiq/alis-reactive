namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Move-in booking: the coordinator picks the day the resident moves in and, if the family wants
    /// one, the day of the welcome visit. The server books exactly the days picked, whatever the time
    /// zone of the browser that picked them.
    /// </summary>
    public class MoveInBookingModel
    {
        /// <summary>A calendar day with no time: the server binds it as a <see cref="DateOnly"/>.</summary>
        public DateOnly? MoveInDay { get; set; }

        /// <summary>A day held as a <see cref="DateTime"/>, as most existing models hold dates.</summary>
        public DateTime? WelcomeVisit { get; set; }
    }

    /// <summary>What the server books.</summary>
    public sealed record MoveInBooked(string Confirmation);
}
