namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.BulletChart
{
    /// <summary>
    /// Model for the FusionBulletChart sandbox demo.
    /// </summary>
    public sealed class FusionBulletChartModel
    {
    }

    public sealed class FusionBulletChartClickAuditRequest
    {
        public string Target { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
    }

    public sealed class FusionBulletChartClickAuditResponse
    {
        public string Message { get; set; } = "";
        public string Wing { get; set; } = "";
        public string Coordinates { get; set; } = "";
    }
}
