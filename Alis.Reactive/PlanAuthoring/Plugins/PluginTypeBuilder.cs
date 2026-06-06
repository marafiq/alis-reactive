using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Declares the functions, properties, and commands a Reactive Plan may use on a named plugin.
    /// For example, <c>plan.RegisterPlugin("auth", p =&gt; p.Method&lt;string&gt;("getToken"))</c>.
    /// Use the argument builder when a host plugin member has fixed argument types.
    /// </summary>
    public sealed class PluginTypeBuilder
    {
        private readonly PluginName _pluginName;
        private readonly PluginMemberDeclarations _members = new PluginMemberDeclarations();

        internal PluginTypeBuilder(string pluginName)
        {
            _pluginName = PluginName.Of(pluginName);
        }

        /// <summary>Declares a value-returning plugin function with an open argument contract.</summary>
        /// <typeparam name="T">CLR type declared as the plugin function's return shape.</typeparam>
        /// <param name="name">Host-page plugin member to call for this method.</param>
        public PluginTypeBuilder Method<T>(string name)
        {
            return AddMethod<T>(name, MethodArgumentContract.Open);
        }

        /// <summary>Declares a readable plugin property.</summary>
        /// <typeparam name="T">CLR type declared as the readable property shape.</typeparam>
        /// <param name="name">Host-page plugin member to read for this property.</param>
        public PluginTypeBuilder Property<T>(string name)
        {
            EnsureName(name);
            _members.Add(PluginPropertyContract.Create(
                PluginPropertyId.Of(_pluginName, MemberName.Of(name)),
                Shape.FromClrType(typeof(T))));
            return this;
        }

        /// <summary>Declares a value-returning plugin function with an exact argument contract.</summary>
        /// <typeparam name="TReturn">CLR type declared as the plugin function's return shape.</typeparam>
        /// <param name="name">Host-page plugin member to call for this method.</param>
        /// <param name="arguments">Ordered argument types accepted by the function.</param>
        public PluginTypeBuilder Method<TReturn>(string name, Action<PluginArgumentTypes> arguments)
        {
            return AddMethod<TReturn>(
                name,
                ExactArguments(arguments));
        }

        /// <summary>Declares the plugin root function as returning a typed value.</summary>
        /// <typeparam name="T">CLR type declared as the plugin root function's return shape.</typeparam>
        public PluginTypeBuilder Function<T>()
        {
            return AddMethod<T>(PluginOperationId.Root(_pluginName), MethodArgumentContract.Open);
        }

        /// <summary>Declares the plugin root function with a return type and exact argument contract.</summary>
        /// <typeparam name="TReturn">CLR type declared as the plugin root function's return shape.</typeparam>
        /// <param name="arguments">Ordered argument types accepted by the root function.</param>
        public PluginTypeBuilder Function<TReturn>(Action<PluginArgumentTypes> arguments)
        {
            return AddMethod<TReturn>(
                PluginOperationId.Root(_pluginName),
                ExactArguments(arguments));
        }

        /// <summary>Declares a command member with no return value and an open argument contract.</summary>
        /// <param name="name">Host-page plugin member to invoke for this command.</param>
        public PluginTypeBuilder Void(string name)
        {
            return AddVoid(name, MethodArgumentContract.Open);
        }

        /// <summary>Alias for declaring a no-return plugin command member with an open argument contract.</summary>
        /// <param name="name">Host-page plugin member to invoke for this command.</param>
        public PluginTypeBuilder Command(string name) =>
            Void(name);

        /// <summary>Declares a command member with an exact argument contract.</summary>
        /// <param name="name">Host-page plugin member to invoke for this command.</param>
        /// <param name="arguments">Ordered argument types accepted by the command.</param>
        public PluginTypeBuilder Void(string name, Action<PluginArgumentTypes> arguments)
        {
            return AddVoid(
                name,
                ExactArguments(arguments));
        }

        /// <summary>Alias for declaring a no-return plugin command member with an exact argument contract.</summary>
        /// <param name="name">Host-page plugin member to invoke for this command.</param>
        /// <param name="arguments">Ordered argument types accepted by the command.</param>
        public PluginTypeBuilder Command(string name, Action<PluginArgumentTypes> arguments) =>
            Void(name, arguments);

        /// <summary>Declares the plugin root command with no return value.</summary>
        public PluginTypeBuilder Void()
        {
            return AddVoid(PluginOperationId.Root(_pluginName), MethodArgumentContract.Open);
        }

        /// <summary>Alias for declaring the plugin root command with no return value.</summary>
        public PluginTypeBuilder Command() =>
            Void();

        /// <summary>Declares the plugin root command with an exact argument contract.</summary>
        /// <param name="arguments">Ordered argument types accepted by the root command.</param>
        public PluginTypeBuilder Void(Action<PluginArgumentTypes> arguments)
        {
            return AddVoid(
                PluginOperationId.Root(_pluginName),
                ExactArguments(arguments));
        }

        /// <summary>Alias for declaring the plugin root command with an exact argument contract.</summary>
        /// <param name="arguments">Ordered argument types accepted by the root command.</param>
        public PluginTypeBuilder Command(Action<PluginArgumentTypes> arguments) =>
            Void(arguments);

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
            var returns = Shape.FromClrType(typeof(TReturn));
            _members.Add(PluginOperationContract.Create(
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
            _members.Add(PluginOperationContract.Create(
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
    }

    /// <summary>Builds an exact plugin argument contract without imposing an arity limit.</summary>
    public sealed class PluginArgumentTypes
    {
        private readonly System.Collections.Generic.List<Shape> _shapes =
            new System.Collections.Generic.List<Shape>();

        internal System.Collections.Generic.IReadOnlyList<Shape> Shapes => _shapes;

        /// <summary>Appends one argument type to the plugin contract.</summary>
        /// <typeparam name="T">Argument type accepted by the plugin member.</typeparam>
        public PluginArgumentTypes Arg<T>()
        {
            _shapes.Add(Shape.FromClrType(typeof(T)));
            return this;
        }
    }
}
