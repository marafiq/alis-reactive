using System;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Generates collision-free HTML element IDs from model type and property expression.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Format: <c>{Namespace_TypeName}__{MemberPath}</c> — double underscore separates
    /// the model scope from the property path. Example:
    /// <c>Alis_Reactive_SandboxApp_Models_OrderModel__Address_City</c>.
    /// </para>
    /// <para>
    /// All component vendors (Syncfusion, native) produce the same ID for the same
    /// expression, so cross-component references resolve correctly. The <c>name</c>
    /// attribute for MVC model binding is not affected — only the <c>id</c> attribute.
    /// </para>
    /// </remarks>
    public static class IdGenerator
    {
        /// <summary>
        /// Generates an element ID from a model expression using untyped <c>object?</c> projection.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <param name="expression">The model property expression (e.g. <c>m =&gt; m.Address.City</c>).</param>
        /// <returns>A scoped element ID like <c>Namespace_Model__Address_City</c>.</returns>
        public static string For<TModel>(Expression<Func<TModel, object?>> expression)
        {
            var scope = TypeScope(typeof(TModel));
            var elementId = ExpressionPathHelper.ToElementId<TModel>(expression);
            return scope + "__" + elementId;
        }

        /// <summary>
        /// Generates an element ID from a typed model expression, avoiding boxing for value types.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The property type, preserving type safety for value types.</typeparam>
        /// <param name="expression">The model property expression (e.g. <c>m =&gt; m.FacilityId</c>).</param>
        /// <returns>A scoped element ID like <c>Namespace_Model__FacilityId</c>.</returns>
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
