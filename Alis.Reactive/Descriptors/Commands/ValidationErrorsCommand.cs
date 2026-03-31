using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    public sealed class ValidationErrorsCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "validation-errors";

        public string FormId { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ValidationErrorsCommand(string formId)
        {
            FormId = formId;
        }
    }
}
