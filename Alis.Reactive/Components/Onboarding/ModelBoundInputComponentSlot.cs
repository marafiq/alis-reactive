using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    /// <summary>
    /// The deterministic slot where a Razor-rendered input component joins the Reactive Plan.
    /// Its controlled component id is the join key across DOM rendering, plan registration,
    /// validation, gather, partial load/unload, and runtime component lookup.
    /// </summary>
    internal sealed class ModelBoundInputComponentSlot
    {
        private ModelBoundInputComponentSlot(
            ComponentId componentId,
            BindingPath bindingPath,
            Shape valueShape)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            BindingPath = bindingPath ?? throw new ArgumentNullException(nameof(bindingPath));
            ValueShape = valueShape ?? throw new ArgumentNullException(nameof(valueShape));
        }

        internal ComponentId ComponentId { get; }

        internal BindingPath BindingPath { get; }

        internal Shape ValueShape { get; }

        internal string ElementId => ComponentId.Value;

        internal string BindingName => BindingPath.Value;

        internal InputComponentRenderTarget RenderTarget =>
            InputComponentRenderTarget.For(ComponentId, BindingPath);

        internal ComponentRegistration Register(InputComponentRegistrationProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            return profile.Register(this);
        }

        internal static ModelBoundInputComponentSlot For<TModel, TValue>(
            Expression<Func<TModel, TValue>> expression,
            string bindingPath)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return For<TValue>(
                IdGenerator.For(expression),
                bindingPath);
        }

        internal static ModelBoundInputComponentSlot For<TValue>(
            string componentId,
            string bindingPath) =>
            new ModelBoundInputComponentSlot(
                ComponentId.Of(componentId),
                BindingPath.Of(bindingPath),
                Shape.FromClrType(typeof(TValue)));
    }

    internal sealed class InputComponentRenderTarget
    {
        private InputComponentRenderTarget(ComponentId componentId, BindingPath bindingPath)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            BindingPath = bindingPath ?? throw new ArgumentNullException(nameof(bindingPath));
        }

        internal ComponentId ComponentId { get; }

        internal BindingPath BindingPath { get; }

        internal string ElementId => ComponentId.Value;

        internal string BindingName => BindingPath.Value;

        internal static InputComponentRenderTarget For(ComponentId componentId, BindingPath bindingPath) =>
            new InputComponentRenderTarget(componentId, bindingPath);
    }
}
