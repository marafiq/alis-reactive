namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered when the BulletChart is clicked.
    /// </summary>
    public sealed class FusionBulletChartMouseClickArgs
    {
        /// <summary>Clicked target element id.</summary>
        public string Target { get; set; } = string.Empty;

        /// <summary>Mouse X coordinate in chart space.</summary>
        public double X { get; set; }

        /// <summary>Mouse Y coordinate in chart space.</summary>
        public double Y { get; set; }
    }
}
