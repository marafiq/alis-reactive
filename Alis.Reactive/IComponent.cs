using System;
using System.Reflection;

namespace Alis.Reactive
{
    /// <summary>
    /// Marker interface for all reactive components (Fusion SF, Native DOM).
    /// Constrains p.Component&lt;T&gt;() to only accept component types.
    /// </summary>
    public interface IComponent { }

    /// <summary>
    /// Marker for components that can be read — constrained by [ReadExpr] attribute.
    /// Used by gather and validation extensions as a generic constraint.
    /// </summary>
    public interface IReadableComponent : IComponent { }

    /// <summary>
    /// Declares the property path from the vendor-determined root for reading.
    /// Applied to component phantom types that implement IReadableComponent.
    /// Examples: [ReadExpr("value")], [ReadExpr("checked")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ReadExprAttribute : Attribute
    {
        public string Expression { get; }
        public ReadExprAttribute(string expression) => Expression = expression;
    }

    /// <summary>
    /// Resolves ReadExpr from component type attribute at plan-build time.
    /// </summary>
    internal static class ComponentHelper
    {
        internal static string GetReadExpr<TComponent>() where TComponent : IReadableComponent
        {
            var attr = typeof(TComponent).GetCustomAttribute<ReadExprAttribute>();
            if (attr == null)
                throw new InvalidOperationException(
                    $"{typeof(TComponent).Name} must have [ReadExpr] attribute");
            return attr.Expression;
        }
    }

    /// <summary>
    /// Marker for app-level components that have a well-known element ID.
    /// Enables the parameterless overload: p.Component&lt;FusionConfirm&gt;()
    /// </summary>
    public interface IAppLevelComponent : IComponent
    {
        string DefaultId { get; }
    }
}
