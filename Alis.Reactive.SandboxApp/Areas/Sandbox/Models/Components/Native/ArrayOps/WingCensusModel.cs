namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ArrayOps
{
    public sealed class WingCensusModel
    {
        public List<string> ProgramsToday { get; set; } = new List<string>();
    }

    public sealed class WingCensusResponse
    {
        public string Wing { get; set; } = "";

        public CensusResident[] Residents { get; set; } = Array.Empty<CensusResident>();
    }

    public sealed class CensusResident
    {
        public string Name { get; set; } = "";

        public string Status { get; set; } = "";

        public int Age { get; set; }

        public int MorseScore { get; set; }

        public int MorningDoses { get; set; }

        public decimal MonthlyCharges { get; set; }

        public double CareHours { get; set; }

        public int DaysToReview { get; set; }
    }

    public static class CensusStatus
    {
        public const string InResidence = "In residence";
        public const string InHospital = "In hospital";
        public const string OnLeave = "On leave";
    }

    public static class StandUpPrograms
    {
        public const string PhysicalTherapy = "Physical therapy";
        public const string ChairYoga = "Chair yoga";
        public const string MusicHour = "Music hour";
        public const string GardenClub = "Garden club";
        public const string MovieNight = "Movie night";
    }
}
