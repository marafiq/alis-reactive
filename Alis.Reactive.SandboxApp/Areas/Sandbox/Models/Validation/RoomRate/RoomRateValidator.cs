using System.Globalization;
using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The rate sheet's rules. The community's rate policy is a service the app registers, so the
    /// validator takes it from DI, and the page's rules are read from the same validator the server runs.
    /// </summary>
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

            // Suites are priced by size, so a room named "... Suite" needs its square footage.
            When(x => x.RoomName != null && x.RoomName.EndsWith("Suite"), () =>
                RuleFor(x => x.SquareFeet).NotEmpty().WithMessage("Give the suite's square footage."));

            // The fee is measured against the rate once there is a rate to measure it against.
            When(x => x.MonthlyRate > 0, () =>
                RuleFor(x => x.CommunityFee)
                    .LessThan(x => x.MonthlyRate).WithMessage("The community fee must be less than the monthly rate."));
        }
    }
}
