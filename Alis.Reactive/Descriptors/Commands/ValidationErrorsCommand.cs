using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Commands
{
    public sealed class ValidationErrorsCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "validation-errors";

        public string FormId { get; }

        public ValidationErrorsCommand(string formId, Guard? when = null)
            : base(when)
        {
            FormId = formId;
        }

        protected override Command CloneWithGuard(Guard guard)
        {
            return new ValidationErrorsCommand(FormId, guard);
        }
    }
}
