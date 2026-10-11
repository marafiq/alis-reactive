using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RespiteBookingValidator : ReactiveValidator<RespiteBookingModel>
    {
        public RespiteBookingValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Resident name is required.");
            RuleFor(x => x.CommunityId).NotEmpty().WithMessage("Choose a community.");
            RuleFor(x => x.DailyRate).NotEmpty().WithMessage("Daily rate is required.");
        }
    }
}
