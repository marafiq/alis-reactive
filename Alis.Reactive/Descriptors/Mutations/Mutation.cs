using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;
using Alis.Reactive.Descriptors.Values;

namespace Alis.Reactive.Descriptors.Mutations
{
    /// <summary>
    /// Base descriptor for an effect applied to a resolved target object.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Mutation>))]
    public abstract class Mutation { }

    /// <summary>
    /// Sets a property on the resolved target object.
    /// </summary>
    public sealed class SetPropMutation : Mutation
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "set-prop";

        /// <summary>Gets the property name assigned on the resolved target object.</summary>
        public string Prop { get; }

        /// <summary>Gets the value descriptor resolved and assigned to <see cref="Prop"/>.</summary>
        public CommandValue Value { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal SetPropMutation(string prop, CommandValue value)
        {
            Prop = prop;
            Value = value;
        }
    }

    /// <summary>
    /// Invokes a method on the resolved target object.
    /// </summary>
    public sealed class CallMutation : Mutation
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "call";

        /// <summary>Gets the method name invoked on the resolved target object.</summary>
        public string Method { get; }

        /// <summary>
        /// Gets the optional property path navigated before invoking <see cref="Method"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        /// <summary>Gets the arguments resolved and passed into the method call.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CommandValue[]? Args { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal CallMutation(string method, string? chain = null, CommandValue[]? args = null)
        {
            Method = method;
            Chain = chain;
            Args = args;
        }
    }
}
