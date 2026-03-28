using System;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Generates collision-free HTML element IDs from model type and property expression.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Format: <c>{Namespace_TypeName}__{MemberPath}</c>. Double underscore separates
    /// the model scope from the property path. Example:
    /// <c>Alis_Reactive_SandboxApp_Models_OrderModel__Address_City</c>.
    /// </para>
    /// <para>
    /// All component vendors (Syncfusion, native) produce the same ID for the same
    /// expression, so cross-component references resolve correctly. The <c>name</c>
    /// attribute for MVC model binding is not affected: only the <c>id</c> attribute.
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
        /// Generates an element ID from a model type and property path string.
        /// </summary>
        /// <remarks>
        /// Matches the format of <see cref="For{TModel, TProp}"/>: <c>{Scope}__{PropertyPath}</c>.
        /// Dots in the property path become underscores, matching the <c>Html.IdFor</c> convention.
        /// </remarks>
        /// <param name="modelType">The model type used to derive the scope prefix.</param>
        /// <param name="propertyPath">The dot-separated property path (e.g. <c>"Address.City"</c>).</param>
        /// <returns>A scoped element ID like <c>Namespace_Model__Address_City</c>.</returns>
        public static string For(Type modelType, string propertyPath)
        {
            var scope = TypeScope(modelType);
            return scope + "__" + propertyPath.Replace(".", "_");
        }

        /// <summary>
        /// Converts a type's full name to an HTML-safe scope string.
        /// </summary>
        /// <remarks>
        /// Dots and plus signs become underscores:
        /// <c>Alis.Reactive.Models.OrderModel</c> becomes <c>Alis_Reactive_Models_OrderModel</c>.
        /// </remarks>
        /// <param name="type">The type whose full name provides the scope.</param>
        /// <returns>An HTML-safe scope string with dots and plus signs replaced by underscores.</returns>
        public static string TypeScope(Type type)
        {
            return type.FullName!.Replace('.', '_').Replace('+', '_');
        }
    }
}
