namespace Alis.Reactive.InputField
{
    public class InputFieldOptions
    {
        internal string? LabelText { get; private set; }
        internal bool IsRequired { get; private set; }

        public InputFieldOptions Required() { IsRequired = true; return this; }
        public InputFieldOptions Label(string label) { LabelText = label; return this; }
    }
}
