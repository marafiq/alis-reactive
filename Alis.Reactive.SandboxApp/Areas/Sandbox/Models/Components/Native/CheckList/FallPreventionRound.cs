using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList
{
    /// <summary>
    /// A night nurse's fall-prevention round for one resident: her fall risk factors, the precautions in
    /// place tonight, and the nurse who checked.
    /// </summary>
    public sealed class FallPreventionRound
    {
        /// <summary>The fall risk factors that apply, opened with the ones on file.</summary>
        public List<string> RiskFactors { get; set; } = new List<string>();

        /// <summary>The precautions in place tonight.</summary>
        public List<string> Precautions { get; set; } = new List<string>();

        /// <summary>The initials of the nurse who checked.</summary>
        public string? CheckedBy { get; set; }
    }

    /// <summary>The risk factors and precautions the round lists, in the words the nurse ticks.</summary>
    public static class FallPrevention
    {
        public const string RecentFall = "Fell in the last 30 days";
        public const string NightSedative = "Takes a sedative at night";
        public const string NightBathroomTrips = "Gets up to the bathroom at night";
        public const string UsesWalker = "Uses a walker";

        public const string LowBed = "Bed in its lowest position";
        public const string CallLight = "Call light within reach";
        public const string NonSlipSocks = "Non-slip socks on";
        public const string BedAlarm = "Bed alarm on";
        public const string NightLight = "Night light on";
    }

    /// <summary>The precautions recorded on the resident's round last night.</summary>
    public sealed class LastNightPrecautions
    {
        public string[] Precautions { get; set; } = Array.Empty<string>();
    }

    /// <summary>What the server says once the round is recorded.</summary>
    public sealed class FallRoundRecorded
    {
        public string Summary { get; set; } = "";
    }

    public sealed class FallPreventionRoundValidator : ReactiveValidator<FallPreventionRound>
    {
        public FallPreventionRoundValidator()
        {
            RuleFor(x => x.Precautions).NotEmpty().WithMessage("Tick the precautions in place tonight.");

            // The community's policy after a fall lives on the server: a Must the browser cannot run.
            RuleFor(x => x.Precautions)
                .Must((round, precautions) => precautions.Count == 0
                    || !round.RiskFactors.Contains(FallPrevention.RecentFall)
                    || precautions.Contains(FallPrevention.BedAlarm))
                .WithMessage("A resident who fell in the last 30 days needs the bed alarm on.");

            RuleFor(x => x.CheckedBy).NotEmpty().WithMessage("Enter your initials.");
        }
    }
}
