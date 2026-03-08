namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ScorePayload
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
    }

    public class RolePayload
    {
        public string Role { get; set; } = "";
    }

    public class ConditionsShowcaseModel
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
