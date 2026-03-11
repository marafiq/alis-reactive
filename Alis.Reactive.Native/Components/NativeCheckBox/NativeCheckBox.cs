namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;input type="checkbox"&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// </summary>
    [ReadExpr("checked")]
    public sealed class NativeCheckBox : NativeComponent, IReadableComponent
    {
    }
}
