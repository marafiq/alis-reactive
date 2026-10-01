using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomRateValidator : ReactiveValidator<RoomRateModel>
    {
        public RoomRateValidator()
        {
            ClientRule(x => x.RoomName).Required("Enter the room name.");
            ClientRule(x => x.MonthlyRate).Required("Enter the monthly rate.");
            ClientRule(x => x.CommunityFee).Required("Enter the community fee, or 0 if it is waived.");
        }
    }
}
