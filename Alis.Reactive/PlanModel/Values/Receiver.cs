using System;
using System.Collections.Generic;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The object a read, a write or a call goes to: an element, a plugin, or data a callback was handed
    /// (the event's payload, a response body, an array item).
    /// </summary>
    internal abstract class Receiver
    {
        private protected Receiver() { }

        internal static Receiver Element(string id) => new ElementReceiver(id);

        internal static Receiver Plugin(string name) => new PluginReceiver(name);

        internal static Receiver Event { get; } = new EventReceiver();

        internal static Receiver Response(ResponseHandle handle) => new ResponseReceiver(handle);

        internal static Receiver Item { get; } = new ItemReceiver();

        /// <summary>Data a callback received rather than an object the page holds; shown as it arrived.</summary>
        internal virtual bool IsData => false;

        /// <summary>The expression for the object, and the path from it to where its members start.</summary>
        internal abstract (Expr Root, IReadOnlyList<string> Path) Locate(Scope scope);

        private sealed class ElementReceiver : Receiver
        {
            private readonly string _id;

            internal ElementReceiver(string id)
            {
                _id = id ?? throw new ArgumentNullException(nameof(id));
            }

            internal override (Expr, IReadOnlyList<string>) Locate(Scope scope) => (G.Obj(scope.Objects.Element(_id)), G.NoPath);
        }

        private sealed class PluginReceiver : Receiver
        {
            private readonly string _name;

            internal PluginReceiver(string name)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
            }

            internal override (Expr, IReadOnlyList<string>) Locate(Scope scope) => (G.Obj(scope.Objects.Plugin(_name)), G.NoPath);
        }

        private sealed class EventReceiver : Receiver
        {
            internal override bool IsData => true;

            internal override (Expr, IReadOnlyList<string>) Locate(Scope scope) => (G.Ref(scope.EventParam), G.NoPath);
        }

        private sealed class ResponseReceiver : Receiver
        {
            private static readonly IReadOnlyList<string> Body = new[] { "body" };
            private readonly ResponseHandle _handle;

            internal ResponseReceiver(ResponseHandle handle)
            {
                _handle = handle ?? throw new ArgumentNullException(nameof(handle));
            }

            internal override bool IsData => true;

            internal override (Expr, IReadOnlyList<string>) Locate(Scope scope)
            {
                var response = scope.ResponseOf(_handle)
                    ?? throw new InvalidOperationException(
                        $"Plan '{scope.PlanId}' reads a response body outside the routes of the request that receives it.");
                return (G.Ref(response), Body);
            }
        }

        private sealed class ItemReceiver : Receiver
        {
            internal override bool IsData => true;

            internal override (Expr, IReadOnlyList<string>) Locate(Scope scope) => (G.Ref(scope.Item), G.NoPath);
        }
    }
}
