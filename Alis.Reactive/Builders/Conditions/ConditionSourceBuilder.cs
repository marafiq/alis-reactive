using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders.Conditions
{
    internal enum CompositionMode { None, All, Any }

    /// <summary>
    /// Exposes typed condition operators that test a source value against a literal or
    /// another source. Reached by calling <c>When()</c> on the pipeline or branch builder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Operators are grouped by category:
    /// </para>
    /// <para><b>Comparison:</b> <c>Eq</c>, <c>NotEq</c>, <c>Gt</c>, <c>Gte</c>, <c>Lt</c>, <c>Lte</c></para>
    /// <para><b>Presence:</b> <c>Truthy</c>, <c>Falsy</c>, <c>IsNull</c>, <c>NotNull</c>, <c>IsEmpty</c>, <c>NotEmpty</c></para>
    /// <para><b>Membership:</b> <c>In</c>, <c>NotIn</c></para>
    /// <para><b>Range:</b> <c>Between</c></para>
    /// <para><b>Text (string only):</b> <c>Contains</c>, <c>StartsWith</c>, <c>EndsWith</c>, <c>Matches</c>, <c>MinLength</c></para>
    /// <para><b>Array:</b> <c>ArrayContains</c></para>
    /// <para><b>Source-vs-source:</b> <c>Eq</c>, <c>NotEq</c>, <c>Gt</c>, <c>Gte</c>, <c>Lt</c>, <c>Lte</c>
    /// (overloads that accept a <see cref="TypedSource{TProp}"/> instead of a literal).</para>
    /// <para>
    /// Every operator returns a <see cref="GuardBuilder{TModel}"/> that can be terminated
    /// with <c>Then</c>, composed with <c>And</c>/<c>Or</c>/<c>Not</c>, or both.
    /// </para>
    /// <code>
    /// // Comparison:
    /// p.When(args, x =&gt; x.Score).Gte(80).Then(t =&gt; t.Element("pass").Show());
    ///
    /// // Presence:
    /// p.When(comp.Value()).NotEmpty().Then(t =&gt; t.Element("filled").Show());
    ///
    /// // Membership:
    /// p.When(args, x =&gt; x.Status).In("active", "pending")
    ///  .Then(t =&gt; t.Element("badge").AddClass("visible"));
    ///
    /// // Text:
    /// p.When(args, x =&gt; x.Email).Contains("@").Then(t =&gt; t.Element("valid").Show());
    ///
    /// // Source-vs-source:
    /// var rate = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Rate);
    /// var budget = p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Budget);
    /// p.When(rate.Value()).Gt(budget.Value()).Then(t =&gt; t.Element("warning").Show());
    /// </code>
    /// </remarks>
    /// <typeparam name="TModel">The view model type.</typeparam>
    /// <typeparam name="TProp">The source property type. All operator operands must match this type at compile time.</typeparam>
    public sealed class ConditionSourceBuilder<TModel, TProp> where TModel : class
    {
        private readonly TypedSource<TProp> _typedSource;
        private readonly string _coerceAs;

        // Composition state for direct And/Or chaining
        private readonly CompositionMode _mode;
        private readonly Guard? _existingGuard;

        // Back-references — only one of these is set depending on the entry path.
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        /// <summary>
        /// NEVER make public. Constructed by <see cref="PipelineBuilder{TModel}.When{TPayload, TProp}"/>
        /// when starting a condition from the pipeline.
        /// </summary>
        internal ConditionSourceBuilder(TypedSource<TProp> source, PipelineBuilder<TModel> pipeline)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _pipeline = pipeline;
            _mode = CompositionMode.None;
        }

        /// <summary>
        /// NEVER make public. Constructed by <see cref="ConditionStart{TModel}"/> for inner
        /// guards inside <c>And</c>/<c>Or</c> lambdas.
        /// </summary>
        internal ConditionSourceBuilder(TypedSource<TProp> source)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _mode = CompositionMode.None;
        }

        /// <summary>
        /// NEVER make public. Constructed by <see cref="BranchBuilder{TModel}.ElseIf{TPayload, TProp}"/>
        /// when continuing a condition chain.
        /// </summary>
        internal ConditionSourceBuilder(TypedSource<TProp> source, BranchBuilder<TModel> branchBuilder)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _branchBuilder = branchBuilder;
            _mode = CompositionMode.None;
        }

        /// <summary>
        /// NEVER make public. Constructed by <see cref="GuardBuilder{TModel}.And{TPayload, TProp}"/>
        /// and <see cref="GuardBuilder{TModel}.Or{TPayload, TProp}"/> when composing guards.
        /// </summary>
        internal ConditionSourceBuilder(TypedSource<TProp> source, CompositionMode mode,
            Guard existingGuard, PipelineBuilder<TModel>? pipeline, BranchBuilder<TModel>? branchBuilder)
        {
            _typedSource = source;
            _coerceAs = source.CoercionType;
            _mode = mode;
            _existingGuard = existingGuard;
            _pipeline = pipeline;
            _branchBuilder = branchBuilder;
        }

        // ── Comparison operators (typed operand) ──

        /// <summary>
        /// Tests whether the source value equals <paramref name="operand"/>.
        /// </summary>
        /// <param name="operand">The value to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining <c>Then</c>, <c>And</c>, <c>Or</c>, or <c>Not</c>.</returns>
        public GuardBuilder<TModel> Eq(TProp operand) =>
            Build(GuardOp.Eq, operand);

        /// <summary>
        /// Tests whether the source value does not equal <paramref name="operand"/>.
        /// </summary>
        /// <param name="operand">The value to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> NotEq(TProp operand) =>
            Build(GuardOp.Neq, operand);

        /// <summary>
        /// Tests whether the source value is greater than <paramref name="operand"/>.
        /// </summary>
        /// <param name="operand">The lower bound (exclusive).</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Gt(TProp operand) =>
            Build(GuardOp.Gt, operand);

        /// <summary>
        /// Tests whether the source value is greater than or equal to <paramref name="operand"/>.
        /// </summary>
        /// <param name="operand">The lower bound (inclusive).</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Gte(TProp operand) =>
            Build(GuardOp.Gte, operand);

        /// <summary>
        /// Tests whether the source value is less than <paramref name="operand"/>.
        /// </summary>
        /// <param name="operand">The upper bound (exclusive).</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Lt(TProp operand) =>
            Build(GuardOp.Lt, operand);

        /// <summary>
        /// Tests whether the source value is less than or equal to <paramref name="operand"/>.
        /// </summary>
        /// <param name="operand">The upper bound (inclusive).</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Lte(TProp operand) =>
            Build(GuardOp.Lte, operand);

        // ── Presence operators (no operand) ──

        /// <summary>
        /// Tests whether the source value is truthy (non-null, non-zero, non-empty string, non-false).
        /// </summary>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Truthy() =>
            Build(GuardOp.Truthy);

        /// <summary>
        /// Tests whether the source value is falsy (null, zero, empty string, or false).
        /// </summary>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Falsy() =>
            Build(GuardOp.Falsy);

        /// <summary>
        /// Tests whether the source value is null or undefined.
        /// </summary>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> IsNull() =>
            Build(GuardOp.IsNull);

        /// <summary>
        /// Tests whether the source value is neither null nor undefined.
        /// </summary>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> NotNull() =>
            Build(GuardOp.NotNull);

        /// <summary>
        /// Tests whether the source value is null or an empty string.
        /// </summary>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> IsEmpty() =>
            Build(GuardOp.IsEmpty);

        /// <summary>
        /// Tests whether the source value is neither null nor an empty string.
        /// </summary>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> NotEmpty() =>
            Build(GuardOp.NotEmpty);

        // ── Membership operators ──

        /// <summary>
        /// Tests whether the source value matches any of the supplied <paramref name="values"/>.
        /// </summary>
        /// <param name="values">The set of allowed values.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> In(params TProp[] values) =>
            Build(GuardOp.In, values);

        /// <summary>
        /// Tests whether the source value does not match any of the supplied <paramref name="values"/>.
        /// </summary>
        /// <param name="values">The set of disallowed values.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> NotIn(params TProp[] values) =>
            Build(GuardOp.NotIn, values);

        // ── Range ──

        /// <summary>
        /// Tests whether the source value falls between <paramref name="low"/> and
        /// <paramref name="high"/> (inclusive on both ends).
        /// </summary>
        /// <param name="low">The lower bound (inclusive).</param>
        /// <param name="high">The upper bound (inclusive).</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Between(TProp low, TProp high) =>
            Build(GuardOp.Between, new object?[] { low, high });

        // ── Text operators (string source) ──

        /// <summary>
        /// Tests whether the source string contains <paramref name="substring"/>.
        /// </summary>
        /// <param name="substring">The substring to search for.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Contains(string substring) =>
            Build(GuardOp.Contains, substring);

        /// <summary>
        /// Tests whether the source string starts with <paramref name="prefix"/>.
        /// </summary>
        /// <param name="prefix">The expected prefix.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> StartsWith(string prefix) =>
            Build(GuardOp.StartsWith, prefix);

        /// <summary>
        /// Tests whether the source string ends with <paramref name="suffix"/>.
        /// </summary>
        /// <param name="suffix">The expected suffix.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> EndsWith(string suffix) =>
            Build(GuardOp.EndsWith, suffix);

        /// <summary>
        /// Tests whether the source string matches the regular expression <paramref name="pattern"/>.
        /// </summary>
        /// <param name="pattern">A regular expression pattern.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Matches(string pattern) =>
            Build(GuardOp.Matches, pattern);

        /// <summary>
        /// Tests whether the source string has at least <paramref name="length"/> characters.
        /// </summary>
        /// <param name="length">The minimum required length.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> MinLength(int length) =>
            Build(GuardOp.MinLength, length);

        // ── Array operators ──

        /// <summary>
        /// Tests whether the source array contains <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item to search for in the array.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> ArrayContains(object item)
        {
            var bindSource = _typedSource.ToBindSource();
            var guard = new ValueGuard(bindSource, _coerceAs, GuardOp.ArrayContains,
                item, _typedSource.ElementCoercionType);
            return ComposeAndWrap(guard);
        }

        // ── Source-vs-source comparison (right side is a TypedSource, not a literal) ──

        /// <summary>
        /// Tests whether the source value equals another source's current value.
        /// </summary>
        /// <param name="right">The other source to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Eq(TypedSource<TProp> right) => BuildVsSource(GuardOp.Eq, right);

        /// <summary>
        /// Tests whether the source value does not equal another source's current value.
        /// </summary>
        /// <param name="right">The other source to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> NotEq(TypedSource<TProp> right) => BuildVsSource(GuardOp.Neq, right);

        /// <summary>
        /// Tests whether the source value is greater than another source's current value.
        /// </summary>
        /// <param name="right">The other source to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Gt(TypedSource<TProp> right) => BuildVsSource(GuardOp.Gt, right);

        /// <summary>
        /// Tests whether the source value is greater than or equal to another source's current value.
        /// </summary>
        /// <param name="right">The other source to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Gte(TypedSource<TProp> right) => BuildVsSource(GuardOp.Gte, right);

        /// <summary>
        /// Tests whether the source value is less than another source's current value.
        /// </summary>
        /// <param name="right">The other source to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Lt(TypedSource<TProp> right) => BuildVsSource(GuardOp.Lt, right);

        /// <summary>
        /// Tests whether the source value is less than or equal to another source's current value.
        /// </summary>
        /// <param name="right">The other source to compare against.</param>
        /// <returns>A <see cref="GuardBuilder{TModel}"/> for chaining.</returns>
        public GuardBuilder<TModel> Lte(TypedSource<TProp> right) => BuildVsSource(GuardOp.Lte, right);

        private GuardBuilder<TModel> BuildVsSource(string op, TypedSource<TProp> right)
        {
            var leftSource = _typedSource.ToBindSource();
            var rightSource = right.ToBindSource();
            var guard = new ValueGuard(leftSource, _coerceAs, op, rightSource);
            return ComposeAndWrap(guard);
        }

        // --- Internal ---

        private GuardBuilder<TModel> Build(string op, object? operand = null)
        {
            var bindSource = _typedSource.ToBindSource();
            var guard = new ValueGuard(bindSource, _coerceAs, op, operand);
            return ComposeAndWrap(guard);
        }

        /// <summary>
        /// Composes a new guard with the existing guard (if in And/Or mode),
        /// then wraps it in a GuardBuilder. Single composition point — all
        /// operator methods (Build, BuildVsSource, ArrayContains) delegate here.
        /// </summary>
        private GuardBuilder<TModel> ComposeAndWrap(Guard newGuard)
        {
            if (_mode == CompositionMode.None || _existingGuard == null)
                return WrapGuard(newGuard);

            var guards = new System.Collections.Generic.List<Guard>();
            if (_mode == CompositionMode.All)
                GuardBuilder<TModel>.FlattenAllStatic(_existingGuard, guards);
            else
                GuardBuilder<TModel>.FlattenAnyStatic(_existingGuard, guards);

            guards.Add(newGuard);
            Guard combined = _mode == CompositionMode.All
                ? new AllGuard(guards)
                : (Guard)new AnyGuard(guards);

            return WrapGuard(combined);
        }

        private GuardBuilder<TModel> WrapGuard(Guard guard)
        {
            if (_pipeline != null)
                return new GuardBuilder<TModel>(guard, _pipeline);

            if (_branchBuilder != null)
                return new GuardBuilder<TModel>(guard, _branchBuilder);

            // Inner guard (no pipeline or branch) — used in And/Or lambdas
            return new GuardBuilder<TModel>(guard);
        }
    }
}
