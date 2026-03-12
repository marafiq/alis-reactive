namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;button&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Not an IInputComponent — buttons have no form value to read.
    /// </summary>
    public sealed class NativeButton : NativeComponent
    {
    }
}
