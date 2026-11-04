namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.CheckList
{
    public sealed class FallPreventionRound
    {
        public List<string> RiskFactors { get; set; } = new List<string>();

        public List<string> Precautions { get; set; } = new List<string>();

        public string? CheckedBy { get; set; }
    }

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

    public sealed class LastNightPrecautions
    {
        public string[] Precautions { get; set; } = Array.Empty<string>();
    }

    public sealed class FallRoundRecorded
    {
        public string Summary { get; set; } = "";
    }

    public sealed class FallPreventionRoundValidator : ReactiveValidator<FallPreventionRound>
    {
        public FallPreventionRoundValidator()
        {
            RuleFor(x => x.Precautions).NotEmpty().WithMessage("Tick the precautions in place tonight.");

            RuleFor(x => x.Precautions)
                .Must((round, precautions) => precautions.Count == 0
                    || !round.RiskFactors.Contains(FallPrevention.RecentFall)
                    || precautions.Contains(FallPrevention.BedAlarm))
                .WithMessage("A resident who fell in the last 30 days needs the bed alarm on.");

            RuleFor(x => x.CheckedBy).NotEmpty().WithMessage("Enter your initials.");
        }
    }
}
