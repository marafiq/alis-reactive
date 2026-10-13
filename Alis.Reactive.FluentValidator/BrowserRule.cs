using System;
using System.Collections.Generic;
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
        private BrowserRule(IReadOnlyList<Func<string, ClientRule>> checks)
        {
            Checks = checks;
        }

        /// <summary>Each check, given the validator's message.</summary>
        internal IReadOnlyList<Func<string, ClientRule>> Checks { get; }

        /// <summary>An absolute <c>http</c> or <c>https</c> URL.</summary>
        public static BrowserRule Url { get; } = One(ClientRule.Url);

        /// <summary>An email address, as FluentValidation's <c>EmailAddress()</c> reads one.</summary>
        public static BrowserRule Email { get; } = One(ClientRule.Email);

        /// <summary>A card number that passes the Luhn check, as FluentValidation's <c>CreditCard()</c> reads one.</summary>
        public static BrowserRule CreditCard { get; } = One(ClientRule.CreditCard);

        /// <summary>Text the JavaScript regular expression <paramref name="pattern"/> matches.</summary>
        /// <param name="pattern">A pattern that reads the same in JavaScript and .NET.</param>
        /// <param name="flags">JavaScript flags: <c>i</c>, <c>m</c>, <c>s</c>.</param>
        public static BrowserRule Pattern(string pattern, string flags = "")
        {
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentException("A browser pattern needs its text.", nameof(pattern));
            return One(message => ClientRule.Pattern(pattern, flags ?? "", message));
        }

        /// <summary>Text at least <paramref name="min"/> and at most <paramref name="max"/> characters long.</summary>
        /// <param name="min">The fewest characters, or 0.</param>
        /// <param name="max">The most characters, or -1 for no limit.</param>
        public static BrowserRule Length(int min, int max)
        {
            var checks = new List<Func<string, ClientRule>>();
            if (min > 0) checks.Add(message => ClientRule.MinLength(min, message));
            if (max >= 0) checks.Add(message => ClientRule.MaxLength(max, message));
            return new BrowserRule(checks);
        }

        private static BrowserRule One(Func<string, ClientRule> check) =>
            new BrowserRule(new[] { check });
    }
}
