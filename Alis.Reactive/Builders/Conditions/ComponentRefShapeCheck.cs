using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Conditions
{
    /// <summary>
    /// Shared cross-check helper used by every typed property accessor (Value, SetValue,
    /// StartDate, EndDate, CurrentView, SelectedDate, ...) on every input component.
    /// Routes all shape mismatches through one throw formatter so messages are uniform
    /// across the DSL surface.
    /// </summary>
    /// <remarks>
    /// Two entry points distinguish where the expected Shape comes from:
    /// <list type="bullet">
    /// <item>
    /// <see cref="ResolveAndVerifyForValueMember{TComponent, TModel}"/> — for accessors that
    /// read or write the component's ValueMember (the model-property-bound value). Expected
    /// shape resolves from the current <see cref="PlanModel.PlanBuildContext"/>'s registration
    /// (primary) or the ref's <see cref="ComponentRef{TComponent, TModel}.ExpressionClrType"/>
    /// captured at the factory (fallback for cross-scope refs).
    /// </item>
    /// <item>
    /// <see cref="ResolveAndVerifyForDeclaredProperty{TComponent, TModel}"/> — for accessors
    /// that read a vendor-declared non-ValueMember property (e.g. FusionDateRangePicker's
    /// startDate, FusionSchedule's currentView). Expected shape is the vendor's fixed contract
    /// passed explicitly by the accessor; registration and expression paths are irrelevant.
    /// </item>
    /// </list>
    /// </remarks>
    internal static class ComponentRefShapeCheck
    {
        /// <summary>
        /// Verifies the requested TProp shape against the component's registered ValueMember
        /// shape (or the ExpressionClrType fallback for cross-scope refs). Returns the expected
        /// shape for the caller to pass to EnsureProperty and the ValueProducer construction.
        /// Throws on mismatch with a message naming the component, the property, and the
        /// expected CLR type.
        /// </summary>
        public static Shape ResolveAndVerifyForValueMember<TComponent, TModel>(
            this ComponentRef<TComponent, TModel> self,
            Shape requested,
            string componentName,
            string propertyName)
            where TComponent : IComponent, new()
            where TModel : class
        {
            var expected = ResolveFromRegistrationOrExpression(self, componentName, propertyName);
            AssertMatch(requested, expected, self.TargetId, componentName, propertyName);
            return expected;
        }

        /// <summary>
        /// Verifies the requested TProp shape against a vendor-declared fixed shape for a
        /// non-ValueMember property. Used by named reads like StartDate, EndDate, CurrentView,
        /// SelectedDate where the expected shape is fixed by the component's vendor contract
        /// and does not depend on the model binding.
        /// </summary>
        public static Shape ResolveAndVerifyForDeclaredProperty<TComponent, TModel>(
            this ComponentRef<TComponent, TModel> self,
            Shape requested,
            Shape declared,
            string componentName,
            string propertyName)
            where TComponent : IComponent, new()
            where TModel : class
        {
            AssertMatch(requested, declared, self.TargetId, componentName, propertyName);
            return declared;
        }

        private static Shape ResolveFromRegistrationOrExpression<TComponent, TModel>(
            ComponentRef<TComponent, TModel> self,
            string componentName,
            string propertyName)
            where TComponent : IComponent, new()
            where TModel : class
        {
            if (self.Pipeline.Context.TryFindRegistrationById(self.TargetId, out var reg) && reg != null)
                return reg.Shape;
            if (self.ExpressionClrType != null)
                return Shape.FromClrType(self.ExpressionClrType);
            throw new InvalidOperationException(
                $"{componentName}.{propertyName}<T>() on component '{self.TargetId}' has no " +
                $"registration in the current plan context and no expression-captured TProp. " +
                $"Register via Html.InputField(plan, m => m.X).{componentName}(...) or address " +
                $"via p.Component<{componentName}>(m => m.X) / " +
                $"p.Component<{componentName}, TOtherModel>(m => m.X).");
        }

        private static void AssertMatch(
            Shape requested,
            Shape expected,
            string targetId,
            string componentName,
            string propertyName)
        {
            // Accept when shapes are equal OR when the caller's requested shape can be
            // safely assigned into the expected slot (e.g. passing a non-null DateTime
            // into a DateTime?-registered field is always safe — non-null subset of nullable).
            if (expected.Accepts(requested)) return;
            throw new InvalidOperationException(
                $"{componentName}.{propertyName}<T>() on '{targetId}' expected shape {expected}. " +
                $"Got {requested}. Use a generic argument whose Shape.FromClrType matches {expected}.");
        }
    }
}
