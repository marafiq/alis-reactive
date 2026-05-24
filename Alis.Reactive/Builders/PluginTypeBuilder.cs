using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Configures a plugin's JsType members during plan construction.
    /// Used by <c>plan.RegisterPlugin("name", p =&gt; p.Method&lt;string&gt;("getToken"))</c>.
    /// </summary>
    public sealed class PluginTypeBuilder
    {
        private readonly PluginName _pluginName;
        private readonly System.Collections.Generic.List<PluginPropertyContract> _properties =
            new System.Collections.Generic.List<PluginPropertyContract>();
        private readonly System.Collections.Generic.List<PluginOperationContract> _operations =
            new System.Collections.Generic.List<PluginOperationContract>();

        internal PluginTypeBuilder(string pluginName)
        {
            _pluginName = PluginName.Of(pluginName);
        }

        /// <summary>Declares a method that returns a typed value. Shape inferred from T.</summary>
        public PluginTypeBuilder Method<T>(string name)
        {
            return AddMethod<T>(name, PluginMethodArguments.Open);
        }

        /// <summary>Declares a readable plugin object property.</summary>
        public PluginTypeBuilder Property<T>(string name)
        {
            EnsureName(name);
            _properties.Add(PluginPropertyContract.Create(
                PluginPropertyId.Of(_pluginName, MemberName.Of(name)),
                Shape.FromClrType(typeof(T))));
            return this;
        }

        /// <summary>Declares a method that returns a typed value with an exact argument contract.</summary>
        public PluginTypeBuilder Method<TReturn>(string name, Action<PluginArgumentTypes> arguments)
        {
            return AddMethod<TReturn>(
                name,
                PluginMethodArguments.From(arguments));
        }

        /// <summary>Declares the plugin root function as returning a typed value.</summary>
        public PluginTypeBuilder Function<T>()
        {
            return AddMethod<T>(PluginOperationId.Root(_pluginName), PluginMethodArguments.Open);
        }

        /// <summary>Declares the plugin root function with an exact argument contract.</summary>
        public PluginTypeBuilder Function<TReturn>(Action<PluginArgumentTypes> arguments)
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.From(arguments));
        }

        /// <summary>Declares the plugin root function with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Function<TReturn, TArg1>()
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.Exact(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares the plugin root function with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Function<TReturn, TArg1, TArg2>()
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares the plugin root function with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Function<TReturn, TArg1, TArg2, TArg3>()
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        /// <summary>Declares a method with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Method<TReturn, TArg1>(string name)
        {
            return AddMethod<TReturn>(
                name,
                PluginMethodArguments.Exact(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares a method with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Method<TReturn, TArg1, TArg2>(string name)
        {
            return AddMethod<TReturn>(
                name,
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares a method with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Method<TReturn, TArg1, TArg2, TArg3>(string name)
        {
            return AddMethod<TReturn>(
                name,
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        /// <summary>Declares a void method (no return value).</summary>
        public PluginTypeBuilder Void(string name)
        {
            return AddVoid(name, PluginMethodArguments.Open);
        }

        /// <summary>Declares a void method with an exact argument contract.</summary>
        public PluginTypeBuilder Void(string name, Action<PluginArgumentTypes> arguments)
        {
            return AddVoid(
                name,
                PluginMethodArguments.From(arguments));
        }

        /// <summary>Declares the plugin root function as a void command.</summary>
        public PluginTypeBuilder Void()
        {
            return AddVoid(PluginOperationId.Root(_pluginName), PluginMethodArguments.Open);
        }

        /// <summary>Declares the plugin root command with an exact argument contract.</summary>
        public PluginTypeBuilder Void(Action<PluginArgumentTypes> arguments)
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.From(arguments));
        }

        /// <summary>Declares the plugin root command with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Void<TArg1>()
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.Exact(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares the plugin root command with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2>()
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares the plugin root command with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2, TArg3>()
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        /// <summary>Declares a void method with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Void<TArg1>(string name)
        {
            return AddVoid(
                name,
                PluginMethodArguments.Exact(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares a void method with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2>(string name)
        {
            return AddVoid(
                name,
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares a void method with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2, TArg3>(string name)
        {
            return AddVoid(
                name,
                PluginMethodArguments.Exact(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        internal PluginContract Build() =>
            PluginContract.Create(_pluginName, _properties, _operations);

        private PluginTypeBuilder AddMethod<TReturn>(string name, PluginMethodArguments args)
        {
            EnsureName(name);
            return AddMethod<TReturn>(
                PluginOperationId.Of(_pluginName, MemberName.Of(name)),
                args);
        }

        private PluginTypeBuilder AddMethod<TReturn>(PluginOperationId operation, PluginMethodArguments args)
        {
            if (operation == null) throw new System.ArgumentNullException(nameof(operation));
            if (args == null) throw new System.ArgumentNullException(nameof(args));
            var returns = Shape.FromClrType(typeof(TReturn));
            _operations.Add(PluginOperationContract.Create(
                operation,
                args.SignatureFor(returns)));
            return this;
        }

        private PluginTypeBuilder AddVoid(string name, PluginMethodArguments args)
        {
            EnsureName(name);
            return AddVoid(
                PluginOperationId.Of(_pluginName, MemberName.Of(name)),
                args);
        }

        private PluginTypeBuilder AddVoid(PluginOperationId operation, PluginMethodArguments args)
        {
            if (operation == null) throw new System.ArgumentNullException(nameof(operation));
            if (args == null) throw new System.ArgumentNullException(nameof(args));
            _operations.Add(PluginOperationContract.Create(
                operation,
                args.SignatureFor(Shape.None)));
            return this;
        }

        private static void EnsureName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Method name required.", nameof(name));
        }

    }

    internal abstract class PluginMethodArguments
    {
        internal static PluginMethodArguments Open { get; } =
            new OpenPluginMethodArguments();

        internal static PluginMethodArguments From(Action<PluginArgumentTypes> configure)
        {
            if (configure == null) throw new System.ArgumentNullException(nameof(configure));
            var builder = new PluginArgumentTypes();
            configure(builder);
            return Exact(builder.Shapes);
        }

        internal static PluginMethodArguments Exact(params Shape[] args)
        {
            if (args == null) throw new System.ArgumentNullException(nameof(args));
            return new ExactPluginMethodArguments(args);
        }

        internal static PluginMethodArguments Exact(System.Collections.Generic.IEnumerable<Shape> args)
        {
            if (args == null) throw new System.ArgumentNullException(nameof(args));
            return new ExactPluginMethodArguments(args);
        }

        internal abstract MethodSignature SignatureFor(Shape returns);
    }

    internal sealed class OpenPluginMethodArguments : PluginMethodArguments
    {
        internal override MethodSignature SignatureFor(Shape returns) =>
            MethodSignature.Open(returns);
    }

    internal sealed class ExactPluginMethodArguments : PluginMethodArguments
    {
        private readonly System.Collections.Generic.IReadOnlyList<Shape> _args;

        internal ExactPluginMethodArguments(System.Collections.Generic.IEnumerable<Shape> args)
        {
            if (args == null) throw new System.ArgumentNullException(nameof(args));

            var snapshot = new System.Collections.Generic.List<Shape>();
            foreach (var arg in args)
            {
                if (arg == null)
                    throw new System.ArgumentException("Plugin method argument shape must not be null.", nameof(args));

                snapshot.Add(arg);
            }

            _args = snapshot;
        }

        internal override MethodSignature SignatureFor(Shape returns) =>
            MethodSignature.Exact(_args, returns);
    }

    /// <summary>Builds an exact plugin argument contract without imposing an arity limit.</summary>
    public sealed class PluginArgumentTypes
    {
        private readonly System.Collections.Generic.List<Shape> _shapes =
            new System.Collections.Generic.List<Shape>();

        internal System.Collections.Generic.IReadOnlyList<Shape> Shapes => _shapes;

        /// <summary>Appends one typed JavaScript argument to the contract.</summary>
        public PluginArgumentTypes Arg<T>()
        {
            _shapes.Add(Shape.FromClrType(typeof(T)));
            return this;
        }
    }
}
