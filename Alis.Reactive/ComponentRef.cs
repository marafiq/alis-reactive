using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;

namespace Alis.Reactive
{
    /// <summary>
    /// Typed reference to a component instance on the page.
    /// Returned by p.Component&lt;T&gt;(). Component-specific extension methods
    /// constrain TComponent to add member-level actions such as SetValue, Show, or Focus.
    ///
    /// The ref itself is component-family-agnostic. Extensions emit V2 set/call actions against the
    /// component object, while runtime contract resolution handles the component root specifics.
    /// </summary>
    public class ComponentRef<TComponent, TModel>
        where TComponent : IComponent, new()
        where TModel : class
    {
        private static readonly TComponent _instance = new TComponent();

        internal string TargetId { get; }
        internal PipelineBuilder<TModel> Emitter { get; }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
        {
            TargetId = targetId;
            Emitter = pipeline;
        }

        internal ComponentRef<TComponent, TModel> Set(
            string memberPath,
            object? value,
            string? coerceAs = null)
        {
            Emitter.SetComponentProperty(TargetId, _instance.Vendor, memberPath, value, coerceAs);
            return this;
        }

        internal ComponentRef<TComponent, TModel> SetFromPath(
            string memberPath,
            string valueMemberPath,
            string? coerceAs = null)
        {
            Emitter.SetComponentPropertyFromPath(TargetId, _instance.Vendor, memberPath, valueMemberPath, coerceAs);
            return this;
        }

        internal ComponentRef<TComponent, TModel> Set<TProp>(
            string memberPath,
            ValueExpression<TProp> value,
            string? coerceAs = null)
        {
            Emitter.SetComponentProperty(
                TargetId,
                _instance.Vendor,
                memberPath,
                value.ToValueExpr(Emitter.Authoring),
                value.CoercionType,
                coerceAs);
            return this;
        }

        internal ComponentRef<TComponent, TModel> Call(string memberPath, params object?[] args)
        {
            Emitter.CallComponentMember(TargetId, _instance.Vendor, memberPath, args);
            return this;
        }

        internal ComponentRef<TComponent, TModel> CallFromPath(
            string memberPath,
            string valueMemberPath,
            string? valueCoerceAs = null)
        {
            Emitter.CallComponentMemberFromPath(TargetId, _instance.Vendor, memberPath, valueMemberPath, valueCoerceAs);
            return this;
        }
    }
}
