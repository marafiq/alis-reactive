using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Sources
{
    /// <summary>
    /// Abstract base for source binding descriptors in the JSON plan. Each concrete subclass
    /// serializes with a <c>kind</c> discriminator that tells the JavaScript runtime where
    /// to resolve a value from: an event payload or a component instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used as the left-hand side of <see cref="Guards.ValueGuard"/> conditions, as
    /// <see cref="Guards.ValueGuard.RightSource"/> for source-vs-source comparisons, and as
    /// argument sources in mutation commands.
    /// </para>
    /// <para>
    /// Serialized via <see cref="WriteOnlyPolymorphicConverter{T}"/> so each subclass writes
    /// its own <c>kind</c> property without <c>JsonDerivedType</c> attributes on the base.
    /// </para>
    /// </remarks>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<BindSource>))]
    public abstract class BindSource { }

    /// <summary>
    /// A source that resolves a value from the current event payload via a dot-path
    /// expression. Serialized as <c>kind: "event"</c> in the JSON plan.
    /// </summary>
    /// <remarks>
    /// The <see cref="Path"/> is a dot-notation string produced by
    /// <c>ExpressionPathHelper</c> from a C# lambda (e.g., <c>x =&gt; x.Address.City</c>
    /// becomes <c>"evt.address.city"</c>). The runtime walks the execution context using
    /// this path to resolve the value.
    /// </remarks>
    internal sealed class EventSource : BindSource
    {
        /// <summary>Gets the type discriminator. Always <c>"event"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "event";

        /// <summary>Gets the dot-notation path into the event payload (e.g., <c>"evt.address.city"</c>).</summary>
        public string Path { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal EventSource(string path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// A source that resolves a value by reading a component instance at runtime.
    /// Serialized as <c>kind: "component"</c> with <c>componentId</c>, <c>vendor</c>,
    /// and <c>readExpr</c> fields in the JSON plan.
    /// </summary>
    /// <remarks>
    /// The runtime locates the DOM element by <see cref="ComponentId"/>, resolves the
    /// vendor root via <c>resolveRoot</c> (e.g., <c>ej2_instances[0]</c> for Fusion),
    /// then walks <see cref="ReadExpr"/> to read the current value (e.g., <c>"value"</c>,
    /// <c>"checked"</c>).
    /// </remarks>
    public sealed class ComponentSource : BindSource
    {
        /// <summary>Gets the type discriminator. Always <c>"component"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "component";

        /// <summary>Gets the DOM element ID of the component to read from.</summary>
        public string ComponentId { get; }

        /// <summary>Gets the vendor identifier (e.g., <c>"native"</c> or <c>"fusion"</c>) used to resolve the component root.</summary>
        public string Vendor { get; }

        /// <summary>Gets the property path walked on the resolved root to read the component value (e.g., <c>"value"</c>, <c>"checked"</c>).</summary>
        public string ReadExpr { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ComponentSource(string componentId, string vendor, string readExpr)
        {
            ComponentId = componentId;
            Vendor = vendor;
            ReadExpr = readExpr;
        }
    }
}
