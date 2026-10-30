using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckBox
{
    /// <summary>
    /// A care aide's evening shower log for one resident: the shower chair her care plan calls for, whether
    /// she declined the shower, the water temperature checked before it, and any skin concern passed to
    /// the nurse.
    /// </summary>
    public sealed class ShowerLog
    {
        /// <summary>The care plan's answer, opened as it is on file.</summary>
        public bool UsesShowerChair { get; set; }

        public bool ShowerDeclined { get; set; }

        public bool WaterTemperatureChecked { get; set; }

        public bool SkinConcern { get; set; }

        public bool NurseTold { get; set; }
    }

    /// <summary>What the server says once the shower log is saved.</summary>
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
