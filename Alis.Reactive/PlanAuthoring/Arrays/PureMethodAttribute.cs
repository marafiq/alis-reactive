using System;

namespace Alis.Reactive.PlanAuthoring.Arrays
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class PureMethodAttribute : Attribute
    {
        internal PureMethodAttribute(string javaScriptName)
        {
            JavaScriptName = javaScriptName ?? throw new ArgumentNullException(nameof(javaScriptName));
        }

        internal string JavaScriptName { get; }
    }
}
