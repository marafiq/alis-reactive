using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Values
{
    /// <summary>
    /// Describes a value that a command consumes at reaction execution time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A command value is the horizontal contract shared by property writes, method arguments,
    /// and dispatch payload fields. Each instance either carries a literal JSON value or
    /// identifies a <see cref="BindSource"/> that the browser runtime resolves from the
    /// current execution context.
    /// </para>
    /// <para>
    /// Optional <c>coerce</c> travels with the value descriptor so shaping happens before the
    /// consuming command applies the value.
    /// </para>
    /// </remarks>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<CommandValue>))]
    public abstract class CommandValue
    {
        internal static CommandValue FromLiteral(object? value, string? coerce = null)
            => new LiteralValue(value, coerce);

        internal static CommandValue FromSource(BindSource source, string? coerce = null)
            => new SourceValue(source, coerce);
    }

    /// <summary>
    /// Carries a literal value directly in the plan.
    /// </summary>
    public sealed class LiteralValue : CommandValue
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "literal";

        /// <summary>Gets the literal JSON value.</summary>
        public object? Value { get; }

        /// <summary>
        /// Gets the optional coercion applied before the consuming command uses the value.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal LiteralValue(object? value, string? coerce = null)
        {
            Value = value;
            Coerce = coerce;
        }
    }

    /// <summary>
    /// Resolves a value from an existing source at execution time.
    /// </summary>
    public sealed class SourceValue : CommandValue
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "source";

        /// <summary>Gets the source descriptor resolved in the browser runtime.</summary>
        public BindSource Source { get; }

        /// <summary>
        /// Gets the optional coercion applied after source resolution and before command use.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal SourceValue(BindSource source, string? coerce = null)
        {
            Source = source;
            Coerce = coerce;
        }
    }
}
