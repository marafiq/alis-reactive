using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RespiteBookingValidator : ReactiveValidator<RespiteBookingModel>
    {
        public RespiteBookingValidator()
        {
            ClientRule(x => x.ResidentName).Required("Resident name is required.");
            ClientRule(x => x.CommunityId).Required("Choose a community.");
            ClientRule(x => x.DailyRate).Required("Daily rate is required.");
        }
    }
}
