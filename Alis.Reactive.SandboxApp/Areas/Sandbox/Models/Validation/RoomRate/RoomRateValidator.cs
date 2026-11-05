using System.Globalization;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomRateValidator : ReactiveValidator<RoomRateModel>
    {
        public RoomRateValidator(RateSheetPolicy policy)
        {
            RuleFor(x => x.RoomName).NotEmpty().WithMessage("Enter the room name.");
            RuleFor(x => x.MonthlyRate).NotEmpty().WithMessage("Enter the monthly rate.");
            RuleFor(x => x.MonthlyRate)
                .LessThanOrEqualTo(policy.MaxMonthlyRate)
                .WithMessage($"{policy.Community} caps a room at ${policy.MaxMonthlyRate.ToString("#,##0", CultureInfo.InvariantCulture)} a month.");
            RuleFor(x => x.CommunityFee).NotEmpty().WithMessage("Enter the community fee, or 0 if it is waived.");

            When(x => x.RoomName != null && x.RoomName.EndsWith("Suite"), () =>
                RuleFor(x => x.SquareFeet).NotEmpty().WithMessage("Give the suite's square footage."));

            When(x => x.MonthlyRate > 0, () =>
                RuleFor(x => x.CommunityFee)
                    .LessThan(x => x.MonthlyRate).WithMessage("The community fee must be less than the monthly rate."));
        }
    }
}
