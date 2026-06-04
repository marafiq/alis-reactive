namespace Alis.Reactive.Native
{
    /// <summary>
    /// Base class for framework-owned native HTML components such as text inputs and checkboxes.
    /// </summary>
    /// <remarks>
    /// Sealed subclasses constrain which extension methods are available at compile time.
    /// For example, <c>SetChecked</c> only appears on <see cref="ComponentRef{TComponent,TModel}"/>
    /// when <c>TComponent</c> is <c>NativeCheckBox</c>.
    /// </remarks>
    public abstract class NativeComponent : IComponent
    {
        /// <inheritdoc />
        public string Vendor => "native";
    }
}
