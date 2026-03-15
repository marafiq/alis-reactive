using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    public sealed class ValidationErrorsCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "validation-errors";

        public string FormId { get; }

        public ValidationErrorsCommand(string formId)
        {
            FormId = formId;
        }
    }
}
