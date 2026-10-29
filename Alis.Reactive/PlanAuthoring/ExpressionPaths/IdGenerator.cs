using System;
using System.Linq.Expressions;

namespace Alis.Reactive
{
    /// <summary>
    /// Generates the stable HTML element ID of the input a member binds: scoped by the class that declares the
    /// member, then the member's path.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Format: <c>{Namespace_DeclaringClass}__{MemberPath}</c>. Double underscore separates the scope from the
    /// path. The scope is the class that declares the member, by its base definition, not the model the
    /// expression starts at: <c>m =&gt; m.Address.City</c> of an <c>OrderModel</c> is
    /// <c>Alis_Reactive_SandboxApp_Models_Address__Address_City</c>.
    /// </para>
    /// <para>
    /// A page, its validator, and the editor template or partial that renders the member reach the same ID
    /// without knowing about each other: an input a partial renders under a field prefix carries the prefix in
    /// its path (<c>..._ContactCard__Primary_Name</c>). These methods take no prefix, so they give the ID of an
    /// input rendered without one. The <c>name</c> attribute for MVC model binding is not affected: only the
    /// <c>id</c> attribute.
    /// </para>
    /// </remarks>
    public static class IdGenerator
    {
        /// <summary>
        /// Generates an element ID from a model expression using an untyped <c>object?</c> lambda.
        /// </summary>
        /// <typeparam name="TModel">View model the property path starts at.</typeparam>
        /// <param name="expression">Model property expression, for example <c>m =&gt; m.Address.City</c>.</param>
        /// <returns>A scoped element ID like <c>Namespace_Address__Address_City</c>.</returns>
        public static string For<TModel>(Expression<Func<TModel, object?>> expression) =>
            ForMember(ExpressionPathHelper.ToDeclarer(expression), ExpressionPathHelper.ToMemberName(expression.Body));

        /// <summary>
        /// Generates an element ID from a typed model expression, avoiding boxing for value types.
        /// </summary>
        /// <typeparam name="TModel">View model the property path starts at.</typeparam>
        /// <typeparam name="TProp">Property type, preserving type safety for value types.</typeparam>
        /// <param name="expression">Model property expression, for example <c>m =&gt; m.FacilityId</c>.</param>
        /// <returns>A scoped element ID like <c>Namespace_Model__FacilityId</c>.</returns>
        public static string For<TModel, TProp>(Expression<Func<TModel, TProp>> expression) =>
            ForMember(ExpressionPathHelper.ToDeclarer(expression), ExpressionPathHelper.ToMemberName(expression.Body));

        /// <summary>
        /// The one rule every input id follows, wherever it is derived: the rendered input, a validator's member,
        /// a component read by its member and a named gather. Dots and brackets in the name become underscores,
        /// matching the <c>Html.IdFor</c> convention.
        /// </summary>
        /// <param name="declarer">The class that declares the bound member (<see cref="ExpressionPathHelper.ToDeclarer"/>).</param>
        /// <param name="mvcName">The member's full MVC name, the view's field prefix included: <c>NextOfKin.Name</c>.</param>
        internal static string ForMember(Type declarer, string mvcName) =>
            Scope(declarer) + "__" + ExpressionPathHelper.ToMvcElementId(mvcName);

        // The class's full name, its dots and the plus of a class nested in another as underscores:
        // Alis.Reactive.Models.Ward+Room is Alis_Reactive_Models_Ward_Room.
        private static string Scope(Type declarer) => declarer.FullName!.Replace('.', '_').Replace('+', '_');
    }
}
