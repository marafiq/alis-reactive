using System;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Generates collision-free element IDs from typeof(TModel).FullName + expression member path.
    /// Format: "{Namespace_TypeName}__{MemberPath}" — double underscore separates scope from property.
    ///
    /// Example: Alis_Reactive_SandboxApp_Models_OrderModel__Address_City
    ///          ↑ model scope (guaranteed unique)           ↑ property path
    ///
    /// Split on "__" to recover: [0] = model scope, [1] = property path.
    /// All vendors (SF, native) produce the same ID for the same expression.
    /// The name attribute (for MVC model binding) is NOT affected — only the id attribute.
    /// </summary>
    public static class IdGenerator
    {
        public static string For<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var scope = TypeScope(typeof(TModel));
            var elementId = ExpressionPathHelper.ToElementId<TModel>(expression);
            return scope + "__" + elementId;
        }

        public static string For<TModel, TProp>(Expression<Func<TModel, TProp>> expression)
        {
            var scope = TypeScope(typeof(TModel));
            var elementId = ExpressionPathHelper.ToElementId<TModel, TProp>(expression);
            return scope + "__" + elementId;
        }

        /// <summary>
        /// Generates element ID from a model Type and property path string.
        /// Matches the format of For&lt;TModel, TProp&gt;() — "{Scope}__{PropertyPath}".
        /// Dots in property path become underscores (matching Html.IdFor convention).
        /// </summary>
        public static string For(Type modelType, string propertyPath)
        {
            var scope = TypeScope(modelType);
            return scope + "__" + propertyPath.Replace(".", "_");
        }

        /// <summary>
        /// Converts typeof(TModel).FullName to an HTML-safe scope string.
        /// Dots and plus signs become underscores: "Alis.Reactive.Models.OrderModel" → "Alis_Reactive_Models_OrderModel".
        /// </summary>
        public static string TypeScope(Type type)
        {
            return type.FullName!.Replace('.', '_').Replace('+', '_');
        }
    }
}
