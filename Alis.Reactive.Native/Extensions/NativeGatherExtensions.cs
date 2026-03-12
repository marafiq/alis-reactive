using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Adds Include() and Include&lt;TComponent&gt;() to GatherBuilder for native DOM components.
    /// Vendor and readExpr derived from component instances — no hardcoded strings.
    /// </summary>
    public static class NativeGatherExtensions
    {
        private static readonly NativeTextBox _defaultComponent = new NativeTextBox();

        /// <summary>
        /// Gathers a native text input bound to a model property.
        /// Shorthand for Include&lt;NativeTextBox, TModel&gt;(expr).
        /// </summary>
        public static GatherBuilder<TModel> Include<TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TModel : class
        {
            var elementId = IdGenerator.For<TModel>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                _defaultComponent.Vendor,
                propertyName,
                _defaultComponent.ReadExpr));
            return self;
        }

        /// <summary>
        /// Gathers the value of a native DOM component bound to a model property.
        /// ReadExpr is resolved from the component type's IInputComponent.ReadExpr property.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            Expression<Func<TModel, object?>> expr)
            where TComponent : NativeComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            var elementId = IdGenerator.For<TModel>(expr);
            var propertyName = ExpressionPathHelper.ToPropertyName(expr);
            self.AddItem(new ComponentGather(
                elementId,
                component.Vendor,
                propertyName,
                component.ReadExpr));
            return self;
        }

        /// <summary>
        /// Gathers the value of a native DOM component identified by string ref.
        /// </summary>
        public static GatherBuilder<TModel> Include<TComponent, TModel>(
            this GatherBuilder<TModel> self,
            string refId,
            string name)
            where TComponent : NativeComponent, IInputComponent, new()
            where TModel : class
        {
            var component = new TComponent();
            self.AddItem(new ComponentGather(
                refId,
                component.Vendor,
                name,
                component.ReadExpr));
            return self;
        }
    }
}
