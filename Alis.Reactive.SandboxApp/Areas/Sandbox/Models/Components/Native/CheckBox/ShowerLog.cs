
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckBox
{
    public sealed class ShowerLog
    {
        public bool UsesShowerChair { get; set; }

        public bool ShowerDeclined { get; set; }

        public bool WaterTemperatureChecked { get; set; }

        public bool SkinConcern { get; set; }

        public bool NurseTold { get; set; }
    }

    public sealed class ShowerLogSaved
    {
        public string Summary { get; set; } = "";
    }

    public sealed class ShowerLogValidator : ReactiveValidator<ShowerLog>
    {
        public ShowerLogValidator()
        {
            RuleFor(x => x.WaterTemperatureChecked).Equal(true).When(x => !x.ShowerDeclined)
                .WithMessage("Check the water temperature before Dorothy's shower.");

            RuleFor(x => x.NurseTold).Equal(true).When(x => x.SkinConcern)
                .WithMessage("Tell the nurse on duty about the skin concern before you save.");
        }
    }
}
