namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RatingModel
    {
        public double SatisfactionScore { get; set; }
    }

    public sealed class RatingEchoRequest
    {
        public double SatisfactionScore { get; set; }
    }

    public sealed class RatingEchoResponse
    {
        public double SatisfactionScore { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
