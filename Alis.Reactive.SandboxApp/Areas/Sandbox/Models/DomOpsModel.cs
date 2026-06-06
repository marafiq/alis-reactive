namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Native DOM collection state for array-operation flows.</summary>
    public class DomOpsModel
    {
    }

    /// <summary>
    /// Typed placeholder for a DOM child element so <c>x =&gt; x.GetAttribute("data-risk")</c>
    /// type-checks in C#. At runtime the value is the live DOM node and the stub body is never executed.
    /// </summary>
    public sealed class DomChild
    {
        public string? GetAttribute(string name) => null;
    }
}
