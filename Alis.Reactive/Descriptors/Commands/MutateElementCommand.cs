using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Mutations;

namespace Alis.Reactive.Descriptors.Commands
{
    /// <summary>
    /// Applies a mutation to a DOM element or component root resolved in the browser.
    /// </summary>
    public sealed class MutateElementCommand : Command
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "mutate-element";

        /// <summary>Gets the DOM element identifier targeted by the mutation.</summary>
        public string Target { get; }

        /// <summary>Gets the mutation applied after the target root is resolved.</summary>
        public Mutation Mutation { get; }

        /// <summary>Gets the optional vendor hint used to resolve the component root.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Vendor { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal MutateElementCommand(
            string target,
            Mutation mutation,
            string? vendor = null)
        {
            Target = target;
            Mutation = mutation;
            Vendor = vendor;
        }
    }
}
