using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Reads a validator's FluentValidation rules into the browser's rules (grammar.md 7.3). Each rule
    /// component the browser can evaluate becomes a browser check with the same message; every other
    /// component stays on the server.
    /// </summary>
    internal sealed class ClientRuleProjection
    {
        private readonly ClientValidationRuleSet _rules = new ClientValidationRuleSet();
        private readonly Func<IValidationRule, ClientScope> _scopeOf;

        private ClientRuleProjection(Func<IValidationRule, ClientScope> scopeOf)
        {
            _scopeOf = scopeOf;
        }

        internal static IReadOnlyList<ClientValidationField> Project(
            IEnumerable<IValidationRule> rules,
            Func<IValidationRule, ClientScope> scopeOf)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (scopeOf == null) throw new ArgumentNullException(nameof(scopeOf));

            var projection = new ClientRuleProjection(scopeOf);
            foreach (var rule in rules)
                projection.Add(rule);
            return projection._rules.ToFields();
        }

        private void Add(IValidationRule rule)
        {
            var scope = _scopeOf(rule);
            if (!RunsInBrowser(rule, scope)) return;

            var components = rule.Components.Where(RunsInBrowser).ToArray();
            var children = components.Select(component => component.Validator).OfType<IChildValidatorAdaptor>().ToArray();

            if (rule is IIncludeRule)
            {
                if (children.Length == 0) return;
                var activation = scope.Activation(_rules);
                foreach (var child in children)
                    _rules.AddRulesFrom(ChildFields(child), ValidationFieldPath.Empty, activation);
                return;
            }

            if (!BindsAMember(rule)) return;

            var field = FieldOf(rule);
            if (IsCollectionRule(rule))
            {
                if (children.Length == 0) return;
                var activation = scope.Activation(_rules);
                foreach (var child in children)
                    _rules.AddItemFields(field, ChildFields(child), activation);
                return;
            }

            AddProperty(rule, field, components, scope);
        }

        private void AddProperty(IValidationRule rule, ClientValidationFieldReference field, IRuleComponent[] components, ClientScope scope)
        {
            var checks = components
                .Select(component => (component, checks: ChecksOf(rule, field, component)))
                .ToArray();
            var hasChecks = checks.Any(entry => entry.checks.Count > 0);
            var hasChildren = checks.Any(entry => entry.component.Validator is IChildValidatorAdaptor);
            if (!hasChecks && !hasChildren) return;

            var activation = scope.Activation(_rules);
            if (hasChecks) _rules.EnsureField(field);
            foreach (var (component, componentChecks) in checks)
            {
                if (component.Validator is IChildValidatorAdaptor child)
                {
                    _rules.AddRulesFrom(ChildFields(child), field.Path, activation);
                    continue;
                }

                foreach (var check in componentChecks)
                    _rules.AddRule(field, check.ToRule(activation));
            }
        }

        // The browser checks one FluentValidation component stands for; none when it is server-only.
        private static IReadOnlyList<BrowserCheck> ChecksOf(
            IValidationRule rule,
            ClientValidationFieldReference field,
            IRuleComponent component)
        {
            if (!TryMessageTemplateOf(component, out var template)) return Array.Empty<BrowserCheck>();

            var message = new ClientMessage(template, () => DisplayNameOf(rule));
            switch (component.Validator)
            {
                case INotEmptyValidator _:
                    var isCollection = field.Shape.Kind == "array";
                    return One(isCollection ? RuleName.AtLeastOne : RuleName.Required, message);

                // A cleared input reaches the server as null (a JSON "" is sent as null; form data binds
                // "" as null), so NotNull fails exactly when the input is empty. An empty collection is
                // not null, so NotNull on a collection stays on the server.
                case INotNullValidator _ when field.Shape.Kind != "array":
                    return One(RuleName.Required, message);

                case IPropertyValidator validator when validator.Name == "EmptyValidator":
                    return One(RuleName.Empty, message);

                case ILengthValidator length when BoundsReadFromModel(length):
                    return Array.Empty<BrowserCheck>();

                case ILengthValidator length:
                    var lengthMessage = message.With("MinLength", length.Min).With("MaxLength", length.Max);
                    var checks = new List<BrowserCheck>();
                    if (length.Min > 0)
                        checks.Add(new BrowserCheck(RuleName.MinLength, lengthMessage, RuleOperand.Literal(length.Min, Shape.None), Shape.None));
                    if (length.Max >= 0)
                        checks.Add(new BrowserCheck(RuleName.MaxLength, lengthMessage, RuleOperand.Literal(length.Max, Shape.None), Shape.None));
                    return checks;

                case IEmailValidator _:
                    return One(RuleName.Email, message);

                case IRegularExpressionValidator pattern:
                    return PatternCheck(pattern, message);

                case ICreditCardValidator _:
                    return One(RuleName.CreditCard, message);

                case IHttpUrlValidator _:
                    return One(RuleName.Url, message);

                case IComparisonValidator comparison:
                    return Comparison(rule, field, comparison, message);

                case IBetweenValidator between:
                    var bounds = ValidationRangeBounds.FromClientLiteral(between.From, between.To);
                    var name = between is IInclusiveBetweenValidator ? RuleName.Range : RuleName.ExclusiveRange;
                    var rangeMessage = message.With("From", between.From).With("To", between.To);
                    return new[] { new BrowserCheck(name, rangeMessage, RuleOperand.Range(bounds), bounds.Shape) };

                default:
                    return Array.Empty<BrowserCheck>();
            }
        }

        // Length(x => ..., x => ...) reads its bounds from the model, which does not exist when the page
        // renders; its Min and Max are then placeholders. The bounds live in the public MinFunc and MaxFunc
        // of FluentValidation's LengthValidator<T>, which ILengthValidator does not expose.
        private static bool BoundsReadFromModel(ILengthValidator length) =>
            length.GetType().GetProperty("MaxFunc")?.GetValue(length) != null;

        // RegexOptions with no JavaScript flag: the browser cannot read such a pattern the same way.
        private const RegexOptions NoJavaScriptForm = RegexOptions.IgnorePatternWhitespace | RegexOptions.RightToLeft;

        // The regex FluentValidation tests with, as a JavaScript pattern and flags. A pattern built from
        // the model (Matches(x => ...)) has no text until a model exists, and an option JavaScript lacks
        // would read the text differently: both stay on the server.
        private static IReadOnlyList<BrowserCheck> PatternCheck(IRegularExpressionValidator validator, ClientMessage message)
        {
            var builtFromModel = validator.Expression == null;
            if (builtFromModel) return Array.Empty<BrowserCheck>();

            var regex = FixedRegexOf(validator);
            if ((regex.Options & NoJavaScriptForm) != 0) return Array.Empty<BrowserCheck>();

            var flags = (regex.Options.HasFlag(RegexOptions.IgnoreCase) ? "i" : "")
                + (regex.Options.HasFlag(RegexOptions.Multiline) ? "m" : "")
                + (regex.Options.HasFlag(RegexOptions.Singleline) ? "s" : "");
            return new[] { new BrowserCheck(RuleName.Regex, message, RuleOperand.Pattern(regex.ToString(), flags), Shape.None) };
        }

        // The interface exposes the pattern's text, not its options. The validator keeps the regex in
        // its one Func<T, Regex> field, which ignores the model when the pattern is fixed.
        private static Regex FixedRegexOf(IRegularExpressionValidator validator)
        {
            var regexOf = validator.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(field => field.GetValue(validator))
                .OfType<Delegate>()
                .SingleOrDefault(candidate => candidate.Method.ReturnType == typeof(Regex))
                ?? throw new InvalidOperationException(
                    $"'{validator.GetType().Name}' keeps no Func<T, Regex>; Alis.Reactive cannot read this FluentValidation version's patterns.");
            return (Regex)regexOf.DynamicInvoke(new object?[] { null })!;
        }

        private static IReadOnlyList<BrowserCheck> One(RuleName name, ClientMessage message) =>
            new[] { new BrowserCheck(name, message, RuleOperand.None, Shape.None) };

        private static IReadOnlyList<BrowserCheck> Comparison(
            IValidationRule rule,
            ClientValidationFieldReference field,
            IComparisonValidator comparison,
            ClientMessage message)
        {
            if (comparison.MemberToCompare != null)
            {
                var peer = PeerOf(rule, field, comparison.MemberToCompare);
                var peerName = SplitPascalCase(comparison.MemberToCompare.Name);
                var peerMessage = message.With("ComparisonValue", peerName).With("ComparisonProperty", peerName);
                var name = comparison.Comparison == FluentValidation.Validators.Comparison.NotEqual
                    ? RuleName.NotEqualTo
                    : RuleNameOf(comparison.Comparison);
                return new[] { new BrowserCheck(name, peerMessage, RuleOperand.PeerField(peer.Path, peer.Shape), peer.Shape) };
            }

            if (comparison.ValueToCompare == null) return Array.Empty<BrowserCheck>();

            var literal = ClientValidationLiteral.From(comparison.ValueToCompare);
            return new[]
            {
                new BrowserCheck(
                    RuleNameOf(comparison.Comparison),
                    message.With("ComparisonValue", comparison.ValueToCompare),
                    RuleOperand.Literal(literal.Value, literal.Shape),
                    literal.Shape),
            };
        }

        // FluentValidation keeps only a peer's member, so a peer is read as the rule's sibling. A peer
        // that is not a member of the rule's own owner cannot be located, and is refused here.
        private static ClientValidationFieldReference PeerOf(IValidationRule rule, ClientValidationFieldReference field, MemberInfo peer)
        {
            var owner = ((MemberExpression)StripConvert(rule.Expression.Body)).Expression?.Type;
            var isSibling = owner != null && peer.DeclaringType != null && peer.DeclaringType.IsAssignableFrom(owner);
            if (!isSibling)
                throw new InvalidOperationException(
                    $"The rule on '{field.Path.Value}' compares it with '{peer.Name}', which is not a member of the same object. " +
                    "A browser rule compares a member with a sibling; declare the rule in the validator of the object that owns both, " +
                    "or end it with .ServerOnly() to run it on the server alone.");

            var segments = field.Path.Value.Split('.');
            var siblingPath = string.Join(".", segments.Take(segments.Length - 1).Concat(new[] { peer.Name }));
            return ClientValidationFieldReference.Of(ValidationFieldPath.Of(siblingPath), Shape.FromClrType(MemberType(peer)));
        }

        private static Type MemberType(MemberInfo member) => member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new InvalidOperationException($"A comparison peer '{member.Name}' must be a property or a field."),
        };

        private static RuleName RuleNameOf(Comparison comparison) => comparison switch
        {
            FluentValidation.Validators.Comparison.Equal => RuleName.EqualTo,
            FluentValidation.Validators.Comparison.NotEqual => RuleName.NotEqual,
            FluentValidation.Validators.Comparison.GreaterThan => RuleName.Gt,
            FluentValidation.Validators.Comparison.GreaterThanOrEqual => RuleName.Min,
            FluentValidation.Validators.Comparison.LessThan => RuleName.Lt,
            _ => RuleName.Max,
        };

        // The fields of the child validator FluentValidation runs. SetValidator(validator), Include(validator)
        // and ChildRules hold that validator, and it is read as written. A validator a factory chooses per
        // model (SetValidator(x => ...)) or by the value's type (SetInheritanceValidator) has not been
        // chosen when the page renders, so its rules stay on the server.
        private static IReadOnlyList<ClientValidationField> ChildFields(IChildValidatorAdaptor child)
        {
            return ChildValidatorOf(child) switch
            {
                IClientValidationMetadataSource reactive => reactive.GetClientRules(),
                IEnumerable<IValidationRule> rules => Project(rules, _ => ClientScope.Unconditional),
                _ => Array.Empty<ClientValidationField>(),
            };
        }

        private static readonly MethodInfo ChildValidatorMethod =
            typeof(ClientRuleProjection).GetMethod(nameof(ChildValidator), BindingFlags.NonPublic | BindingFlags.Static)!;

        // The adaptor is generic over the parent model and the property; FluentValidation asks for both.
        private static IValidator? ChildValidatorOf(IChildValidatorAdaptor child)
        {
            var adaptorType = child.GetType();
            while (!(adaptorType.IsGenericType && adaptorType.GetGenericTypeDefinition() == typeof(ChildValidatorAdaptor<,>)))
                adaptorType = adaptorType.BaseType!;
            return (IValidator?)ChildValidatorMethod.MakeGenericMethod(adaptorType.GetGenericArguments()).Invoke(null, new object[] { child });
        }

        // No model exists at render: a held validator is returned as is; a factory that reads the model
        // fails on the missing model, and a validator chosen by the value's type finds no value.
        private static IValidator? ChildValidator<TModel, TProperty>(ChildValidatorAdaptor<TModel, TProperty> adaptor)
        {
            try
            {
                return adaptor.GetValidator(new ValidationContext<TModel>(default!), default!);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        // A rule the browser can run: in the default rule set, and under no condition but a recorded
        // When or Unless. A When written as a method group conditions the rule, yet records nothing.
        private static bool RunsInBrowser(IValidationRule rule, ClientScope scope)
        {
            var inDefaultRuleSet = rule.RuleSets == null || rule.RuleSets.Length == 0
                || rule.RuleSets.Contains(RulesetValidatorSelector.DefaultRuleSetName, StringComparer.OrdinalIgnoreCase);
            var unrecordedCondition = rule.HasCondition && scope.IsUnconditional;
            return inDefaultRuleSet && !scope.IsServerOnly && !rule.HasAsyncCondition && !unrecordedCondition;
        }

        // A component's own .When or .Unless is a C# predicate the browser cannot run.
        private static bool RunsInBrowser(IRuleComponent component) =>
            !component.HasCondition && !component.HasAsyncCondition;

        // RuleForEach validates each element: its expression yields the collection, its type is the element's.
        private static bool IsCollectionRule(IValidationRule rule) =>
            rule.TypeToValidate != rule.Expression.ReturnType;

        // A rule reaches the browser only through a model member it names: RuleFor(x => x.A.B). A rule
        // over the whole object or a computed value (x => x.Name.Trim()) has no input to bind.
        private static bool BindsAMember(IValidationRule rule) =>
            rule.Expression != null
            && StripConvert(rule.Expression.Body) is MemberExpression
            && ExpressionPathHelper.IsMemberChain(rule.Expression.Body);

        private static Expression StripConvert(Expression expression)
        {
            while (expression is UnaryExpression unary
                   && (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
                expression = unary.Operand;
            return expression;
        }

        private static ClientValidationFieldReference FieldOf(IValidationRule rule)
        {
            return ClientValidationFieldReference.Of(
                ValidationFieldPath.Of(ExpressionPathHelper.ToMemberName(rule.Expression.Body)),
                Shape.FromClrType(ExpressionPathHelper.ToPropertyType(rule.Expression)));
        }

        // The component's message template. A message written from the model (WithMessage(x => ...))
        // fails on the missing model when the page renders; the browser cannot say it, so the
        // component stays on the server.
        private static bool TryMessageTemplateOf(IRuleComponent component, out string template)
        {
            try
            {
                template = component.GetUnformattedErrorMessage();
                return true;
            }
            catch (NullReferenceException)
            {
                template = "";
                return false;
            }
        }

        // The rule's display name, for a message template that names it. A name computed from the
        // model (WithName(x => ...)) has no value when the page renders: the member's name stands in.
        private static string DisplayNameOf(IValidationRule rule)
        {
            try
            {
                return rule.GetDisplayName(null);
            }
            catch (NullReferenceException)
            {
                return SplitPascalCase(rule.PropertyName);
            }
        }

        // FluentValidation's own display-name rule: "AdmissionDate" reads "Admission Date".
        private static string SplitPascalCase(string input)
        {
            var text = new StringBuilder(input.Length + 5);
            for (var i = 0; i < input.Length; ++i)
            {
                var current = input[i];
                var startsWord = char.IsUpper(current)
                    && ((i > 1 && !char.IsUpper(input[i - 1])) || (i + 1 < input.Length && !char.IsUpper(input[i + 1])));
                if (startsWord) text.Append(' ');

                var isDroppedPeriod = current == '.' && i + 1 < input.Length && char.IsUpper(input[i + 1]);
                if (!isDroppedPeriod) text.Append(current);
            }

            return text.ToString().Trim();
        }

        private sealed class BrowserCheck
        {
            private readonly RuleName _name;
            private readonly ClientMessage _message;
            private readonly RuleOperand _operand;
            private readonly Shape _shape;

            internal BrowserCheck(RuleName name, ClientMessage message, RuleOperand operand, Shape shape)
            {
                _name = name;
                _message = message;
                _operand = operand;
                _shape = shape;
            }

            internal ClientRule ToRule(ClientRuleActivation activation) =>
                new ClientRule(_name, ValidationMessage.Of(_message.Text), _operand, activation, _shape);
        }

        // A message template, formatted with what is known when the page renders. A sentence that names a
        // value only validation knows ({PropertyValue}, {TotalLength}) is dropped.
        private readonly struct ClientMessage
        {
            private static readonly Regex RuntimeOnlySentence =
                new Regex(@"\s*[^.]*\{(PropertyValue|TotalLength)\}[^.]*\.", RegexOptions.CultureInvariant);

            internal ClientMessage(string template, Func<string> displayName)
            {
                var text = RuntimeOnlySentence.Replace(template, "").Trim();
                Text = text.Contains("{PropertyName}") ? text.Replace("{PropertyName}", displayName()) : text;
            }

            private ClientMessage(string text)
            {
                Text = text;
            }

            internal string Text { get; }

            internal ClientMessage With(string placeholder, object? value) =>
                new ClientMessage(Text.Replace("{" + placeholder + "}", Convert.ToString(value, CultureInfo.CurrentCulture)));
        }
    }
}
