using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alis.Reactive.Conditions;

namespace Alis.Reactive.Validation;

internal abstract class Check<T, TValue>
{
    internal abstract string Template { get; }
    internal abstract Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run);
}

internal sealed class ValueCheck<T, TValue>(Func<TValue, bool> passes, string template) : Check<T, TValue>
{
    internal override string Template => template;
    internal override Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run) => Task.FromResult(passes(value));
}

internal static class Emptiness
{
    internal static bool IsEmpty<TValue>(TValue value)
    {
        if (value is null) return true;
        if (value is string text) return string.IsNullOrWhiteSpace(text);
        if (value is ICollection collection) return collection.Count == 0;
        if (value is not IEnumerable sequence) return EqualityComparer<TValue>.Default.Equals(value, default!);
        var iterator = sequence.GetEnumerator();
        try { return !iterator.MoveNext(); }
        finally { (iterator as IDisposable)?.Dispose(); }
    }
}

internal sealed class LengthCheck<T> : Check<T, string?>
{
    private readonly int _minimum;
    private readonly int _maximum;

    internal LengthCheck(int minimum, int maximum, string template)
    {
        if (maximum != -1 && maximum < minimum) throw new ArgumentOutOfRangeException("max", "Max should be larger than min.");
        _minimum = minimum;
        _maximum = maximum;
        Template = template;
    }

    internal override string Template { get; }

    internal override Task<bool> Passes(T model, string? value, MessageArguments arguments, ValidationRun run)
    {
        arguments.With("MinLength", _minimum).With("MaxLength", _maximum).With("TotalLength", value?.Length ?? 0);
        return Task.FromResult(value is null || value.Length >= _minimum && (_maximum == -1 || value.Length <= _maximum));
    }
}

internal sealed class PatternCheck<T>(string pattern, RegexOptions options) : Check<T, string?>
{
    private readonly Func<string?, bool> _matches = BrowserPattern.OnServer(pattern, options);
    internal override string Template => Templates.Matches;

    internal override Task<bool> Passes(T model, string? value, MessageArguments arguments, ValidationRun run)
    {
        arguments.With("RegularExpression", pattern);
        return Task.FromResult(_matches(value));
    }
}

internal static class TextChecks
{
    internal static bool Email(string? value)
    {
        if (value is null) return true;
        var at = value.IndexOf('@');
        return at > 0 && at < value.Length - 1 && at == value.LastIndexOf('@');
    }

    internal static bool Url(string? value)
        => string.IsNullOrEmpty(value) || Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    internal static bool CreditCard(string? value)
    {
        if (value is null) return true;
        var sum = 0;
        var doubled = false;
        for (var at = value.Length - 1; at >= 0; at--)
        {
            var digit = value[at];
            if (digit is ' ' or '-') continue;
            if (digit is < '0' or > '9') return false;
            var weighted = (digit - '0') * (doubled ? 2 : 1);
            sum += weighted / 10 + weighted % 10;
            doubled = !doubled;
        }
        return sum % 10 == 0;
    }
}

internal sealed class Comparison
{
    private readonly Func<object?, object?, bool> _passes;
    private Comparison(string template, Func<object?, object?, bool> passes)
    {
        Template = template;
        _passes = passes;
    }

    internal string Template { get; }
    internal bool Passes(object? value, object? other) => _passes(value, other);
    internal static Comparison Equal { get; } = new(Templates.Equal, Equals);
    internal static Comparison NotEqual { get; } = new(Templates.NotEqual, (value, other) => !Equals(value, other));
    internal static Comparison GreaterThan { get; } = Ordered(Templates.GreaterThan, order => order > 0);
    internal static Comparison GreaterThanOrEqual { get; } = Ordered(Templates.GreaterThanOrEqual, order => order >= 0);
    internal static Comparison LessThan { get; } = Ordered(Templates.LessThan, order => order < 0);
    internal static Comparison LessThanOrEqual { get; } = Ordered(Templates.LessThanOrEqual, order => order <= 0);

    private static Comparison Ordered(string template, Func<int, bool> accepts)
        => new(template, (value, other) => value is null || other is not null && accepts(Comparer.Default.Compare(value, other)));
}

internal sealed class CompareCheck<T, TValue, TPeer>(Comparison comparison, Func<T, TPeer> read, string displayName) : Check<T, TValue>
{
    internal override string Template => comparison.Template;
    internal override Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run)
    {
        var other = read(model);
        arguments.With("ComparisonValue", other).With("ComparisonProperty", displayName);
        return Task.FromResult(comparison.Passes(value, other));
    }
}

internal sealed class BetweenCheck<T, TValue, TBound> : Check<T, TValue>
{
    private readonly TBound _from;
    private readonly TBound _to;
    private readonly bool _inclusive;

    internal BetweenCheck(TBound from, TBound to, bool inclusive)
    {
        if (from is null) throw new ArgumentNullException(nameof(from));
        if (to is null) throw new ArgumentNullException(nameof(to));
        if (Comparer.Default.Compare(to, from) < 0) throw new ArgumentOutOfRangeException(nameof(to), "To should be larger than from.");
        _from = from;
        _to = to;
        _inclusive = inclusive;
    }

    internal override string Template => _inclusive ? Templates.InclusiveBetween : Templates.ExclusiveBetween;
    internal override Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run)
    {
        arguments.With("From", _from).With("To", _to);
        if (value is null) return Task.FromResult(true);
        var from = Comparer.Default.Compare(value, _from);
        var to = Comparer.Default.Compare(value, _to);
        return Task.FromResult(_inclusive ? from >= 0 && to <= 0 : from > 0 && to < 0);
    }
}

internal sealed class PrecisionScaleCheck<T, TValue> : Check<T, TValue>
{
    private readonly int _precision;
    private readonly int _scale;
    private readonly bool _ignoreTrailingZeros;
    private readonly Func<TValue, decimal?> _number;

    internal PrecisionScaleCheck(int precision, int scale, bool ignoreTrailingZeros, Func<TValue, decimal?> number)
    {
        if (precision < 0) throw new ArgumentOutOfRangeException(nameof(precision), $"Precision must be a positive integer. [value:{precision}].");
        if (scale < 0) throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be a positive integer. [value:{scale}].");
        if (scale > precision) throw new ArgumentOutOfRangeException(nameof(scale), $"Scale must be less than precision. [scale:{scale}, precision:{precision}].");
        _precision = precision;
        _scale = scale;
        _ignoreTrailingZeros = ignoreTrailingZeros;
        _number = number;
    }

    internal override string Template => Templates.PrecisionScale;
    internal override Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run)
    {
        if (_number(value) is not { } number) return Task.FromResult(true);
        var bits = decimal.GetBits(number);
        var scale = (bits[3] >> 16) & 0x7F;
        var mantissa = new decimal(bits[0], bits[1], bits[2], false, 0);
        if (_ignoreTrailingZeros)
        {
            while (scale > 0 && mantissa % 10 == 0)
            {
                scale--;
                mantissa /= 10;
            }
        }
        var digits = 0;
        while (mantissa >= 1) { digits++; mantissa /= 10; }
        var integerDigits = digits - scale;
        var printedScale = Math.Max(scale, _scale);
        arguments.With("ExpectedPrecision", _precision).With("ExpectedScale", _scale)
            .With("Digits", Math.Max(integerDigits, 1) + printedScale).With("ActualScale", printedScale);
        return Task.FromResult(scale <= _scale && integerDigits <= _precision - _scale);
    }
}

internal sealed class PredicateCheck<T, TValue>(Func<T, TValue, bool> predicate) : Check<T, TValue>
{
    internal override string Template => Templates.Predicate;
    internal override Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run) => Task.FromResult(predicate(model, value));
}

internal sealed class AsyncPredicateCheck<T, TValue>(Func<T, TValue, CancellationToken, Task<bool>> predicate) : Check<T, TValue>
{
    internal override string Template => Templates.Predicate;
    internal override Task<bool> Passes(T model, TValue value, MessageArguments arguments, ValidationRun run)
    {
        run.RequireAsync();
        return predicate(model, value, run.Cancellation);
    }
}
