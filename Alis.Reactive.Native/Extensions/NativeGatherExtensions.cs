using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Descriptors.Requests;
using Alis.Reactive.Native.Components;

namespace Alis.Reactive.Native.Extensions
{
    /// <summary>
    /// Native-specific gather shorthand. The untyped Include() defaults to NativeTextBox.
    /// Typed Include&lt;TComponent&gt;() overloads live in core GatherExtensions (vendor-agnostic).
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
    }
}
