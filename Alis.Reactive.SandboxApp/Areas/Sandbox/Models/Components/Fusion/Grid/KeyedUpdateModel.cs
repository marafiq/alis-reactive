namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The evening rounds board: the nurse on the wing tonight. The grid holds the residents, so the page has
    /// no fields of its own.
    /// </summary>
    public class KeyedUpdateModel
    {
        public string Nurse { get; set; } = "";
        public string Wing { get; set; } = "";
    }

    /// <summary>A resident on the evening rounds board, one row of the board's grid.</summary>
    public class EveningRoundsResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public string FallRisk { get; set; } = "";
        public int OpenTasks { get; set; }
    }

    /// <summary>The resident the nurse picked on the board.</summary>
    public class EveningRoundsRequest
    {
        public List<EveningRoundsResident> Residents { get; set; } = new();
    }

    /// <summary>A resident's open tasks after one was marked done.</summary>
    public class EveningRoundsTaskCount
    {
        public int ResidentId { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }

    /// <summary>A resident's fall risk after a fall was recorded.</summary>
    public class EveningRoundsFallRisk
    {
        public int ResidentId { get; set; }
        public string FallRisk { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    /// <summary>A resident as the chart has them now.</summary>
    public class EveningRoundsChart
    {
        public EveningRoundsResident Resident { get; set; } = new();
        public string Summary { get; set; } = "";
    }

    /// <summary>Why the nurse's entry was refused.</summary>
    public class EveningRoundsRefusal
    {
        public string Summary { get; set; } = "";
    }
}
