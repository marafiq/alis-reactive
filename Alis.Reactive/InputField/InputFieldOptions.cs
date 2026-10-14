namespace Alis.Reactive.InputField
{
    /// <summary>
    /// Configures the label, the required marker and the visibility of an input field wrapper.
    /// </summary>
    /// <remarks>
    /// Passed to <c>Html.InputField</c> when the field wrapper needs label text
    /// or a required marker. Fields render without either option by default.
    /// </remarks>
    public class InputFieldOptions
    {
        internal string? LabelText { get; private set; }

        internal bool IsRequired { get; private set; }

        internal bool IsHidden { get; private set; }

        /// <summary>
        /// Marks the field as required, showing a <c>*</c> indicator next to the label.
        /// </summary>
        public InputFieldOptions Required() { IsRequired = true; return this; }

        /// <summary>
        /// Sets the label text displayed above the input component.
        /// </summary>
        /// <param name="label">Label text.</param>
        public InputFieldOptions Label(string label) { LabelText = label; return this; }

        /// <summary>
        /// Renders the component alone, without the label and message wrapper: for a value the page
        /// carries but the user does not see, such as a hidden input.
        /// </summary>
        public InputFieldOptions Hidden() { IsHidden = true; return this; }
    }
}
