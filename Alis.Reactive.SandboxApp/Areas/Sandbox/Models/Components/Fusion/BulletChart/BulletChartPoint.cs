namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.BulletChart
{
    /// <summary>
    /// Bullet chart data row for the sandbox demo.
    /// </summary>
    public sealed class BulletChartPoint
    {
        public string Category { get; set; } = string.Empty;

        public int Value { get; set; }

        public int Target { get; set; }
    }
}
