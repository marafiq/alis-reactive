using System;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// A custom FluentValidation validator the browser evaluates too: it names the browser check that
    /// decides what its <c>IsValid</c> decides. A validator without it stays on the server.
    /// </summary>
    /// <example>
    /// <code>
    /// public sealed class WristbandCodeValidator&lt;T&gt; : PropertyValidator&lt;T, string?&gt;, IBrowserRule
    /// {
    ///     public BrowserRule Browser =&gt; BrowserRule.Pattern(@"^WB-\d{6}$");
    ///     public override bool IsValid(ValidationContext&lt;T&gt; context, string? value) =&gt;
    ///         string.IsNullOrEmpty(value) || Regex.IsMatch(value, @"^WB-\d{6}$");
    ///     ..
    /// }
    /// </code>
    /// </example>
    public interface IBrowserRule
    {
        /// <summary>The check the browser runs for this validator, with the validator's own message.</summary>
        BrowserRule Browser { get; }
    }

    /// <summary>A check the browser evaluates. Each passes an empty value, as FluentValidation's own do.</summary>
    public sealed class BrowserRule
    {
        private BrowserRule(IReadOnlyList<(RuleName Name, RuleOperand Operand)> checks)
        {
            Checks = checks;
        }

        internal IReadOnlyList<(RuleName Name, RuleOperand Operand)> Checks { get; }

        /// <summary>An absolute <c>http</c> or <c>https</c> URL.</summary>
        public static BrowserRule Url { get; } = One(RuleName.Url, RuleOperand.None);

        /// <summary>An email address, as FluentValidation's <c>EmailAddress()</c> reads one.</summary>
        public static BrowserRule Email { get; } = One(RuleName.Email, RuleOperand.None);

        /// <summary>A card number that passes the Luhn check, as FluentValidation's <c>CreditCard()</c> reads one.</summary>
        public static BrowserRule CreditCard { get; } = One(RuleName.CreditCard, RuleOperand.None);

        /// <summary>Text the JavaScript regular expression <paramref name="pattern"/> matches.</summary>
        /// <param name="pattern">A pattern that reads the same in JavaScript and .NET.</param>
        /// <param name="flags">JavaScript flags: <c>i</c>, <c>m</c>, <c>s</c>.</param>
        public static BrowserRule Pattern(string pattern, string flags = "")
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A browser pattern needs its text.", nameof(pattern));
            return One(RuleName.Regex, RuleOperand.Pattern(pattern, flags ?? ""));
        }

        /// <summary>Text at least <paramref name="min"/> and at most <paramref name="max"/> characters long.</summary>
        /// <param name="min">The fewest characters, or 0.</param>
        /// <param name="max">The most characters, or -1 for no limit.</param>
        public static BrowserRule Length(int min, int max)
        {
            var checks = new List<(RuleName, RuleOperand)>();
            if (min > 0) checks.Add((RuleName.MinLength, RuleOperand.Literal(min, Shape.None)));
            if (max >= 0) checks.Add((RuleName.MaxLength, RuleOperand.Literal(max, Shape.None)));
            return new BrowserRule(checks);
        }

        private static BrowserRule One(RuleName name, RuleOperand operand) =>
            new BrowserRule(new[] { (name, operand) });
    }
}
