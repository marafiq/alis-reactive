using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Configures a plugin's BrowserObjectContract members during plan construction.
    /// Used by <c>plan.RegisterPlugin("name", p =&gt; p.Method&lt;string&gt;("getToken"))</c>.
    /// </summary>
    public sealed class PluginTypeBuilder
    {
        private readonly PluginName _pluginName;
        private readonly PluginMemberDeclarations _members = new PluginMemberDeclarations();

        internal PluginTypeBuilder(string pluginName)
        {
            _pluginName = PluginName.Of(pluginName);
        }

        /// <summary>Declares a method that returns a typed value. Shape inferred from T.</summary>
        public PluginTypeBuilder Method<T>(string name)
        {
            return AddMethod<T>(name, MethodArgumentContract.Open);
        }

        /// <summary>Declares a readable plugin object property.</summary>
        public PluginTypeBuilder Property<T>(string name)
        {
            EnsureName(name);
            _members.Add(_pluginName, PluginPropertyContract.Create(
                PluginPropertyId.Of(_pluginName, MemberName.Of(name)),
                Shape.FromClrType(typeof(T))));
            return this;
        }

        /// <summary>Declares a method that returns a typed value with an exact argument contract.</summary>
        public PluginTypeBuilder Method<TReturn>(string name, Action<PluginArgumentTypes> arguments)
        {
            return AddMethod<TReturn>(
                name,
                ExactArguments(arguments));
        }

        /// <summary>Declares the plugin root function as returning a typed value.</summary>
        public PluginTypeBuilder Function<T>()
        {
            return AddMethod<T>(PluginOperationId.Root(_pluginName), MethodArgumentContract.Open);
        }

        /// <summary>Declares the plugin root function with an exact argument contract.</summary>
        public PluginTypeBuilder Function<TReturn>(Action<PluginArgumentTypes> arguments)
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                ExactArguments(arguments));
        }

        /// <summary>Declares the plugin root function with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Function<TReturn, TArg1>()
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                ExactArguments(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares the plugin root function with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Function<TReturn, TArg1, TArg2>()
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares the plugin root function with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Function<TReturn, TArg1, TArg2, TArg3>()
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        /// <summary>Declares a method with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Method<TReturn, TArg1>(string name)
        {
            return AddMethod<TReturn>(
                name,
                ExactArguments(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares a method with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Method<TReturn, TArg1, TArg2>(string name)
        {
            return AddMethod<TReturn>(
                name,
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares a method with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Method<TReturn, TArg1, TArg2, TArg3>(string name)
        {
            return AddMethod<TReturn>(
                name,
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        /// <summary>Declares a void method (no return value).</summary>
        public PluginTypeBuilder Void(string name)
        {
            return AddVoid(name, MethodArgumentContract.Open);
        }

        /// <summary>Declares a command method (no return value).</summary>
        public PluginTypeBuilder Command(string name) =>
            Void(name);

        /// <summary>Declares a void method with an exact argument contract.</summary>
        public PluginTypeBuilder Void(string name, Action<PluginArgumentTypes> arguments)
        {
            return AddVoid(
                name,
                ExactArguments(arguments));
        }

        /// <summary>Declares a command method with an exact argument contract.</summary>
        public PluginTypeBuilder Command(string name, Action<PluginArgumentTypes> arguments) =>
            Void(name, arguments);

        /// <summary>Declares the plugin root function as a void command.</summary>
        public PluginTypeBuilder Void()
        {
            return AddVoid(PluginOperationId.Root(_pluginName), MethodArgumentContract.Open);
        }

        /// <summary>Declares the plugin root function as a command.</summary>
        public PluginTypeBuilder Command() =>
            Void();

        /// <summary>Declares the plugin root command with an exact argument contract.</summary>
        public PluginTypeBuilder Void(Action<PluginArgumentTypes> arguments)
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                ExactArguments(arguments));
        }

        /// <summary>Declares the plugin root command with an exact argument contract.</summary>
        public PluginTypeBuilder Command(Action<PluginArgumentTypes> arguments) =>
            Void(arguments);

        /// <summary>Declares the plugin root command with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Void<TArg1>()
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                ExactArguments(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares the plugin root command with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2>()
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares the plugin root command with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2, TArg3>()
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        /// <summary>Declares a void method with one typed JavaScript argument.</summary>
        public PluginTypeBuilder Void<TArg1>(string name)
        {
            return AddVoid(
                name,
                ExactArguments(Shape.FromClrType(typeof(TArg1))));
        }

        /// <summary>Declares a void method with two typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2>(string name)
        {
            return AddVoid(
                name,
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2))));
        }

        /// <summary>Declares a void method with three typed JavaScript arguments.</summary>
        public PluginTypeBuilder Void<TArg1, TArg2, TArg3>(string name)
        {
            return AddVoid(
                name,
                ExactArguments(
                    Shape.FromClrType(typeof(TArg1)),
                    Shape.FromClrType(typeof(TArg2)),
                    Shape.FromClrType(typeof(TArg3))));
        }

        internal PluginContract Build() => _members.ToContract(_pluginName);

        private PluginTypeBuilder AddMethod<TReturn>(string name, MethodArgumentContract arguments)
        {
            EnsureName(name);
            return AddMethod<TReturn>(
                PluginOperationId.Of(_pluginName, MemberName.Of(name)),
                arguments);
        }

        private PluginTypeBuilder AddMethod<TReturn>(PluginOperationId operation, MethodArgumentContract arguments)
        {
            if (operation == null) throw new System.ArgumentNullException(nameof(operation));
            if (arguments == null) throw new System.ArgumentNullException(nameof(arguments));
            var returns = Shape.FromClrType(typeof(TReturn));
            _members.Add(_pluginName, PluginOperationContract.Create(
                operation,
                MethodSignature.WithArguments(arguments, returns)));
            return this;
        }

        private PluginTypeBuilder AddVoid(string name, MethodArgumentContract arguments)
        {
            EnsureName(name);
            return AddVoid(
                PluginOperationId.Of(_pluginName, MemberName.Of(name)),
                arguments);
        }

        private PluginTypeBuilder AddVoid(PluginOperationId operation, MethodArgumentContract arguments)
        {
            if (operation == null) throw new System.ArgumentNullException(nameof(operation));
            if (arguments == null) throw new System.ArgumentNullException(nameof(arguments));
            _members.Add(_pluginName, PluginOperationContract.Create(
                operation,
                MethodSignature.WithArguments(arguments, Shape.None)));
            return this;
        }

        private static void EnsureName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new System.ArgumentException("Method name required.", nameof(name));
        }

        private static MethodArgumentContract ExactArguments(Action<PluginArgumentTypes> configure)
        {
            if (configure == null) throw new System.ArgumentNullException(nameof(configure));
            var builder = new PluginArgumentTypes();
            configure(builder);
            return MethodArgumentContract.Exact(builder.Shapes);
        }

        private static MethodArgumentContract ExactArguments(params Shape[] args)
        {
            if (args == null) throw new System.ArgumentNullException(nameof(args));
            return MethodArgumentContract.Exact(args);
        }
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
