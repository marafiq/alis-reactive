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
        /// <summary>
        /// Gets the option value submitted in the form.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the display text shown next to the radio button or checkbox.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the optional secondary description shown below the display text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Creates a new option.
        /// </summary>
        /// <param name="value">The option value submitted in the form.</param>
        /// <param name="text">The display text shown to the user.</param>
        /// <param name="description">Optional secondary description text.</param>
        public RadioButtonItem(string value, string text, string? description = null)
        {
            Value = value;
            Text = text;
            Description = description;
        }
    }
}
