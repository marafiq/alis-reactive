using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The reservations office's rules for a hold: a plain FluentValidation validator, not a reactive
    /// one, so its rules never reach the page. The server checks them when a hold is placed.
    /// </summary>
    public class ReservationsOfficeValidator : AbstractValidator<RoomHoldModel>
    {
        public const string ProspectWithHold = "Ruth Carter";

        public ReservationsOfficeValidator()
        {
            // One hold per prospect: a family releases its hold before placing another.
            RuleFor(x => x.ProspectName)
                .NotEqual(ProspectWithHold)
                .WithMessage($"{ProspectWithHold} already holds Maple 218. Release that hold before placing another.");
        }
    }
}
