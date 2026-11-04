using Alis.Reactive;

namespace Net48MvcSmoke.Models
{
    public class SmokeModelRules : ReactiveValidator<SmokeModel>
    {
        public SmokeModelRules()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Enter your name.");
        }
    }
}
