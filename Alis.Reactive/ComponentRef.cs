using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

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
        private static readonly ComponentMetadata _component = ReactiveComponentMetadata.For<TComponent>();

        internal string TargetId { get; }
        internal PipelineBuilder<TModel> Emitter { get; }

        internal ComponentRef(string targetId, PipelineBuilder<TModel> pipeline)
        {
            TargetId = targetId;
            Emitter = pipeline;
        }

        internal ComponentRef<TComponent, TModel> Set(
            CapabilityProperty member,
            object? value,
            ValueShape? assignedShape = null)
        {
            Emitter.SetComponentProperty(TargetId, _component, member, value, assignedShape);
            return this;
        }

        internal ComponentRef<TComponent, TModel> Set<TProp>(
            CapabilityProperty member,
            ReactiveValue<TProp> value,
            ValueShape? assignedShape = null)
        {
            Emitter.SetComponentProperty(
                TargetId,
                _component,
                member,
                value.ToPlanValue(Emitter.Authoring.Values),
                value.ValueShape,
                assignedShape);
            return this;
        }

        internal ReactiveValue<TProp> CreateValue<TProp>()
        {
            var binding = _component.Binding
                ?? throw new InvalidOperationException($"{typeof(TComponent).Name} does not declare a bindable component member.");

            return ReactiveValue<TProp>.FromComponentValue(TargetId, _component, binding);
        }

        internal ReactiveValue<TProp> CreateValue<TProp>(CapabilityProperty member) =>
            ReactiveValue<TProp>.FromComponentValue(TargetId, _component, member);

        internal ComponentRef<TComponent, TModel> SetFromEvent<TSource>(
            CapabilityProperty member,
            Expression<Func<TSource, object?>> path,
            ValueShape? assignedShape = null)
        {
            var payload = Emitter.DescribeEventPayload(path);
            Emitter.SetComponentProperty(
                TargetId,
                _component,
                member,
                payload.Expression,
                payload.Shape,
                assignedShape);
            return this;
        }

        internal ComponentRef<TComponent, TModel> SetFromResponse<TSource>(
            CapabilityProperty member,
            Expression<Func<TSource, object?>> path,
            ValueShape? assignedShape = null)
        {
            var payload = Emitter.Authoring.Values.DescribeResponsePayload(path);
            Emitter.SetComponentProperty(
                TargetId,
                _component,
                member,
                payload.Expression,
                payload.Shape,
                assignedShape);
            return this;
        }

        internal ComponentRef<TComponent, TModel> Call(CapabilityMethod member, params object?[] args)
        {
            Emitter.CallComponentMember(TargetId, _component, member, args);
            return this;
        }

        internal ComponentRef<TComponent, TModel> CallFromEvent<TSource>(
            CapabilityMethod member,
            Expression<Func<TSource, object?>> path)
        {
            var payload = Emitter.DescribeEventPayload(path);
            Emitter.CallComponentMember(
                TargetId,
                _component,
                member,
                new[] { payload.Expression },
                new[] { payload.Shape });
            return this;
        }

        internal ComponentRef<TComponent, TModel> CallFromResponse<TSource>(
            CapabilityMethod member,
            Expression<Func<TSource, object?>> path)
        {
            var payload = Emitter.Authoring.Values.DescribeResponsePayload(path);
            Emitter.CallComponentMember(
                TargetId,
                _component,
                member,
                new[] { payload.Expression },
                new[] { payload.Shape });
            return this;
        }

    }
}
