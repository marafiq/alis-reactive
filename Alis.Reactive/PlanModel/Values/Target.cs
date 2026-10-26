using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The object a read, a write or a call goes to: an element, a plugin, or data a callback was handed
    /// (the event's payload, a response body, an array item).
    /// </summary>
    internal abstract class Target
    {
        private protected Target() { }

        internal static Target Element(string id) => new ElementTarget(id);

        internal static Target Plugin(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new PluginTarget(objects => objects.Plugin(name));
        }

        /// <summary>The stream object at a URL: <c>alis.stream</c> keyed by it.</summary>
        internal static Target Stream(string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            return new PluginTarget(objects => objects.Stream(url));
        }

        /// <summary>The hub object at a URL: <c>alis.hub</c> keyed by it.</summary>
        internal static Target Hub(string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            return new PluginTarget(objects => objects.Hub(url));
        }

        internal static Target Event { get; } = new EventTarget();

        internal static Target Response(ResponseHandle handle) => new ResponseTarget(handle);

        internal static Target Item { get; } = new ItemTarget();

        /// <summary>The object a method call returns.</summary>
        internal static Target Returned(ValueExpression call) => new ReturnedTarget(call);

        /// <summary>Data a callback received rather than an object the page holds; shown as it arrived.</summary>
        internal virtual bool IsData => false;

        /// <summary>The expression for the object, and the path from it to where its members start.</summary>
        internal abstract (Expr Root, IReadOnlyList<string> Path) Write(Scope scope);

        private sealed class ElementTarget : Target
        {
            private readonly string _id;

            internal ElementTarget(string id)
            {
                _id = id ?? throw new ArgumentNullException(nameof(id));
            }

            internal override (Expr, IReadOnlyList<string>) Write(Scope scope) => (G.Obj(scope.Objects.Element(_id)), G.NoPath);
        }

        // An object a registered plugin makes: a plugin by its name, a stream or a hub by its URL.
        private sealed class PluginTarget : Target
        {
            private readonly Func<ObjectTable, string> _object;

            internal PluginTarget(Func<ObjectTable, string> @object)
            {
                _object = @object;
            }

            internal override (Expr, IReadOnlyList<string>) Write(Scope scope) => (G.Obj(_object(scope.Objects)), G.NoPath);
        }

        private sealed class EventTarget : Target
        {
            internal override bool IsData => true;

            internal override (Expr, IReadOnlyList<string>) Write(Scope scope) => (G.Ref(scope.EventParam), G.NoPath);
        }

        private sealed class ResponseTarget : Target
        {
            private static readonly IReadOnlyList<string> Body = new[] { "body" };
            private readonly ResponseHandle _handle;

            internal ResponseTarget(ResponseHandle handle)
            {
                _handle = handle ?? throw new ArgumentNullException(nameof(handle));
            }

            internal override bool IsData => true;

            internal override (Expr, IReadOnlyList<string>) Write(Scope scope)
            {
                return (G.Ref(scope.Response(_handle)), Body);
            }
        }

        private sealed class ItemTarget : Target
        {
            internal override bool IsData => true;

            internal override (Expr, IReadOnlyList<string>) Write(Scope scope) => (G.Ref(scope.Item), G.NoPath);
        }

        private sealed class ReturnedTarget : Target
        {
            private readonly ValueExpression _call;

            internal ReturnedTarget(ValueExpression call)
            {
                _call = call ?? throw new ArgumentNullException(nameof(call));
            }

            internal override (Expr, IReadOnlyList<string>) Write(Scope scope) => (_call.Write(scope), G.NoPath);
        }
    }
}
