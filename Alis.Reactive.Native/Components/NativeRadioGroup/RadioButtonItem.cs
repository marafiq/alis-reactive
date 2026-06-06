namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A single radio or checkbox option with a value, display text, and optional description.
    /// </summary>
    /// <remarks>
    /// Created in the controller and passed to the builder via <c>.Items()</c>.
    /// Used by both <see cref="NativeRadioGroupBuilder{TModel,TProp}"/> and
    /// <see cref="NativeCheckListBuilder{TModel,TProp}"/>.
    /// </remarks>
    public class RadioButtonItem
    {
        private readonly OptionDescription _description;

        /// <summary>
        /// Option value written to form posts and component reads.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Display text shown next to the radio button or checkbox.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Optional secondary description shown below the display text.
        /// </summary>
        public string? Description => _description.ValueForRender;

        /// <summary>
        /// Creates a new option without secondary description text.
        /// </summary>
        /// <param name="value">Submitted option value.</param>
        /// <param name="text">Display text.</param>
        public RadioButtonItem(string value, string text)
            : this(value, text, OptionDescription.None)
        {
        }

        /// <summary>
        /// Creates a new option with secondary description text.
        /// </summary>
        /// <param name="value">Submitted option value.</param>
        /// <param name="text">Display text.</param>
        /// <param name="description">Secondary description text.</param>
        public RadioButtonItem(string value, string text, string description)
            : this(value, text, OptionDescription.Text(description))
        {
        }

        private RadioButtonItem(string value, string text, OptionDescription description)
        {
            Value = value;
            Text = text;
            _description = description;
        }
    }

    internal abstract class OptionDescription
    {
        private protected OptionDescription() { }

        internal static OptionDescription None { get; } =
            new MissingOptionDescription();

        internal static OptionDescription Text(string value) =>
            new TextOptionDescription(value);

        internal abstract string? ValueForRender { get; }
    }

    internal sealed class MissingOptionDescription : OptionDescription
    {
        internal override string? ValueForRender => null;
    }

    internal sealed class TextOptionDescription : OptionDescription
    {
        private readonly string _value;

        internal TextOptionDescription(string value)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));
            _value = value;
        }

        internal override string ValueForRender => _value;
    }
}
