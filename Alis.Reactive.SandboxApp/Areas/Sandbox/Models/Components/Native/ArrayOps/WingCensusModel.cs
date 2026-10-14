namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ArrayOps
{
    /// <summary>The morning stand-up: the programs the director ticks as running on the wing today.</summary>
    public sealed class WingCensusModel
    {
        /// <summary>The programs running today.</summary>
        public List<string> ProgramsToday { get; set; } = new List<string>();
    }

    /// <summary>A wing's census as this morning's records hold it.</summary>
    public sealed class WingCensusResponse
    {
        /// <summary>The wing's name, as the stand-up reads it.</summary>
        public string Wing { get; set; } = "";

        public CensusResident[] Residents { get; set; } = Array.Empty<CensusResident>();
    }

    /// <summary>One resident on a wing's census.</summary>
    public sealed class CensusResident
    {
        public string Name { get; set; } = "";

        /// <summary>Where the resident is this morning: one of <see cref="CensusStatus"/>.</summary>
        public string Status { get; set; } = "";

        public int Age { get; set; }

        /// <summary>Morse fall scale score: 45 or more is high risk, 24 or less low risk.</summary>
        public int MorseScore { get; set; }

        /// <summary>Doses on the morning medication pass.</summary>
        public int MorningDoses { get; set; }

        /// <summary>This month's charges, in dollars and cents.</summary>
        public decimal MonthlyCharges { get; set; }

        /// <summary>Hours of personal care a week.</summary>
        public double CareHours { get; set; }

        /// <summary>Days until the resident's next care review.</summary>
        public int DaysToReview { get; set; }
    }

    /// <summary>Where a resident on the census is this morning.</summary>
    public static class CensusStatus
    {
        public const string InResidence = "In residence";
        public const string InHospital = "In hospital";
        public const string OnLeave = "On leave";
    }

    /// <summary>The programs a wing can run in a day, in the words the director ticks.</summary>
    public static class StandUpPrograms
    {
        public const string PhysicalTherapy = "Physical therapy";
        public const string ChairYoga = "Chair yoga";
        public const string MusicHour = "Music hour";
        public const string GardenClub = "Garden club";
        public const string MovieNight = "Movie night";
    }
}
