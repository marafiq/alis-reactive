namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class SliderModel
    {
        public double PainScore { get; set; }

        public double[] PreferredRange { get; set; } = [];
    }

    public sealed class SliderEchoRequest
    {
        public double PainScore { get; set; }

        public double[] PreferredRange { get; set; } = [];
    }

    public sealed class SliderEchoResponse
    {
        public double PainScore { get; set; }

        public double[] PreferredRange { get; set; } = [];

        public string Summary { get; set; } = string.Empty;
    }
}
