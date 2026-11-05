namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class KeyedUpdateModel
    {
        public string Nurse { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    public class EveningRoundsResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public string FallRisk { get; set; } = "";
        public int OpenTasks { get; set; }
    }

    public class EveningRoundsRequest
    {
        public List<EveningRoundsResident> Residents { get; set; } = new();
    }

    public class EveningRoundsTaskCount
    {
        public int ResidentId { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }

    public class EveningRoundsFallRisk
    {
        public int ResidentId { get; set; }
        public string FallRisk { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    public class EveningRoundsChart
    {
        public EveningRoundsResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    public class EveningRoundsRefusal
    {
        public string Summary { get; set; } = "";
    }
}
