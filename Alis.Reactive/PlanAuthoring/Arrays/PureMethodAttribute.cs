using System;

namespace Alis.Reactive.Builders.Arrays
{
    /// <summary>
    /// Marks a method a lambda over an array's items may call, by the JavaScript method that gives the same
    /// value without changing the item. The array compiler writes the call as that method on the item's value.
    /// </summary>
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
