using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomRateValidator : ReactiveValidator<RoomRateModel>
    {
        public RoomRateValidator()
        {
            RuleFor(x => x.RoomName).NotEmpty().WithMessage("Enter the room name.");
            RuleFor(x => x.MonthlyRate).NotEmpty().WithMessage("Enter the monthly rate.");
            RuleFor(x => x.CommunityFee).NotEmpty().WithMessage("Enter the community fee, or 0 if it is waived.");
        }
    }
}
