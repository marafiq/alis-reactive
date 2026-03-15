namespace Alis.Reactive.Native
{
    /// <summary>
    /// Base marker class for all Native (DOM/HTML) components.
    /// Sealed subclasses act as phantom types — constraining which
    /// vertical slice extension methods are available at compile time.
    /// </summary>
    public abstract class NativeComponent : IComponent
    {
        /// <inheritdoc />
        public string Vendor => "native";
    }

    /// <summary>
    /// Marker interface for native input components that can be bound to model properties.
    /// </summary>
    public interface INativeInputComponent { }
}
