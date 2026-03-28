using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Requests;

namespace Alis.Reactive.Descriptors.Reactions
{
    /// <summary>
    /// A reaction that executes an HTTP request with optional pre-fetch commands.
    /// Serialized as <c>kind: "http"</c> in the JSON plan.
    /// </summary>
    /// <remarks>
    /// <see cref="PreFetch"/> contains commands that run before the request fires (e.g.,
    /// showing a loading spinner). These are reverted after the response arrives. The
    /// <see cref="Request"/> descriptor carries the URL, method, gather items, and
    /// response handlers.
    /// </remarks>
    public sealed class HttpReaction : Reaction
    {
        /// <summary>Gets the type discriminator. Always <c>"http"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "http";

        /// <summary>
        /// Gets the commands to execute before the HTTP request, or <see langword="null"/>
        /// when no pre-fetch commands exist. These commands are reverted after the response arrives.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Command>? PreFetch { get; }

        /// <summary>Gets the HTTP request descriptor containing URL, method, gather items, and response handlers.</summary>
        public RequestDescriptor Request { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        /// <param name="preFetch">Commands to execute before the HTTP request, or <see langword="null"/> when none exist.</param>
        /// <param name="request">The HTTP request descriptor containing URL, method, gather, and response handlers.</param>
        internal HttpReaction(List<Command>? preFetch, RequestDescriptor request)
        {
            PreFetch = preFetch;
            Request = request;
        }
    }
}
