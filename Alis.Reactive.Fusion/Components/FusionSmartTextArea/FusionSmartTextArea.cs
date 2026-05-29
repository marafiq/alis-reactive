namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion EJ2 SmartTextArea. Syncfusion does not ship an MVC builder for this
    /// package version, so this vertical slice owns the typed render helper.
    /// </summary>
    public sealed class FusionSmartTextArea : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionSmartTextArea(), "smarttextarea");

        public string ValueMember => "value";
    }
}
