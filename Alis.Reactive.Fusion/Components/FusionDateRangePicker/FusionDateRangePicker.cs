namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion DateRangePicker component.
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionDateRangePicker&gt;(m => m.StayPeriod) to unlock
    /// the DateRangePicker-specific extension methods.
    ///
    /// ReadExpr is "value" — Syncfusion ej2.value returns [Date, Date] (start + end).
    /// Model property is DateTime[]? — the framework's existing array handling (emitArray,
    /// CoercionTypes "array" + elementCoerceAs "date") supports this natively.
    ///
    /// For targeted access to individual dates, use comp.StartDate() (readExpr "startDate")
    /// or comp.EndDate() (readExpr "endDate") — these are independent of ReadExpr.
    /// </summary>
    public sealed class FusionDateRangePicker : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
