using System;
using System.Linq;

namespace Alis.Reactive
{
    /// <summary>
    /// The class names every input field renders with: the wrapper, its label, the required marker, the
    /// message slot, and the class a refused input carries while its message shows. The defaults are the
    /// names the framework's stylesheets style; an app that styles its own chooses them with
    /// <c>services.AddReactiveClassNames(...)</c>, and each name it leaves unset keeps its default.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddReactiveClassNames(names =>
    /// {
    ///     names.Field = "mb-3";
    ///     names.FieldLabel = "form-label";
    ///     names.FieldRequired = "text-danger";
    ///     names.FieldMessage = "invalid-feedback";
    ///     names.InvalidInput = "is-invalid";
    /// });
    /// </code>
    /// </example>
    public sealed class ReactiveClassNames
    {
        private string _field = "alis-field";
        private string _fieldLabel = "alis-field__label";
        private string _fieldRequired = "alis-field__required";
        private string _fieldMessage = "alis-field__message";
        private string _invalidInput = "alis-has-error";

        internal ReactiveClassNames()
        {
        }

        /// <summary>The wrapper around a field's label, input and message. Default <c>alis-field</c>.</summary>
        public string Field
        {
            get => _field;
            set => _field = Classes(value, nameof(Field));
        }

        /// <summary>A field's label. Default <c>alis-field__label</c>.</summary>
        public string FieldLabel
        {
            get => _fieldLabel;
            set => _fieldLabel = Classes(value, nameof(FieldLabel));
        }

        /// <summary>The marker beside a required field's label. Default <c>alis-field__required</c>.</summary>
        public string FieldRequired
        {
            get => _fieldRequired;
            set => _fieldRequired = Classes(value, nameof(FieldRequired));
        }

        /// <summary>The slot a field's validation message shows in. Default <c>alis-field__message</c>.</summary>
        public string FieldMessage
        {
            get => _fieldMessage;
            set => _fieldMessage = Classes(value, nameof(FieldMessage));
        }

        /// <summary>
        /// The one class a refused input's element carries while its message shows, added when a form refuses
        /// the input and removed when the input is corrected. Default <c>alis-has-error</c>.
        /// </summary>
        public string InvalidInput
        {
            get => _invalidInput;
            set => _invalidInput = OneClass(value, nameof(InvalidInput));
        }

        private static string Classes(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{name} needs a class name; leave it unset to keep the default.", name);
            return value;
        }

        private static string OneClass(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsWhiteSpace))
                throw new ArgumentException(
                    $"{name} is one class name, added to and removed from a refused input on its own; '{value}' is not one.", name);
            return value;
        }
    }
}
