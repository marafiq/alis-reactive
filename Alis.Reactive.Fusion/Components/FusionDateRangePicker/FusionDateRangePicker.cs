namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion DateRangePicker component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionDateRangePicker&gt;(m => m.StayStart) to unlock
    /// the DateRangePicker-specific extension methods.
    ///
    /// UNIQUE: exposes TWO readable properties (startDate, endDate) on the ej2 instance.
    /// ReadExpr is "startDate" (primary). Extensions provide StartDate() and EndDate()
    /// as separate typed sources.
    /// </summary>
    public sealed class FusionDateRangePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "startDate";
    }
}
