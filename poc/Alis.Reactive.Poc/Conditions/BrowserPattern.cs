using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Alis.Reactive.Conditions;

internal sealed class BrowserPattern
{
    private static readonly HashSet<char> Metacharacters = ['\\', '^', '$', '.', '|', '?', '*', '+', '(', ')', '[', ']', '{', '}', '/'];

    private const string EndAsDotnetReadsIt = "(?=\\n?$)";

    private static readonly Dictionary<char, string> SpelledOutsideAClass = new()
    {
        ['d'] = "[0-9]",
        ['D'] = "[^0-9]",
        ['w'] = "[a-zA-Z0-9_]",
        ['W'] = "[^a-zA-Z0-9_]",
        ['s'] = "[ \\t\\n\\r\\f\\v]",
        ['S'] = "[^ \\t\\n\\r\\f\\v]",
        ['A'] = "^",
        ['Z'] = "$",
    };

    private static readonly Dictionary<char, string> SpelledInsideAClass = new()
    {
        ['d'] = "0-9",
        ['w'] = "a-zA-Z0-9_",
        ['s'] = " \\t\\n\\r\\f\\v",
    };

    private static readonly Dictionary<char, char> ControlCharacters = new()
    {
        ['t'] = '\t',
        ['n'] = '\n',
        ['r'] = '\r',
        ['f'] = '\f',
        ['v'] = '\v',
    };

    private readonly string _pattern;
    private readonly Func<string, ArgumentException> _refused;
    private readonly StringBuilder _copy = new();
    private readonly Stack<bool> _lookarounds = new();
    private int _at;

    private BrowserPattern(string pattern, Func<string, ArgumentException> refused)
    {
        _pattern = pattern;
        _refused = refused;
    }

    internal static string Of(string pattern, Func<string, ArgumentException> refused) => new BrowserPattern(pattern, refused).Copy();

    private string Copy()
    {
        var repeatable = false;
        while (_at < _pattern.Length) repeatable = Token(repeatable);
        if (_lookarounds.Count > 0) throw _refused("opens a group it does not close; close it with ')'.");
        return _copy.ToString();
    }

    private bool Token(bool repeatable)
    {
        var token = _pattern[_at];
        switch (token)
        {
            case '\\':
                Escape();
                return true;
            case '[':
                Class();
                return true;
            case '(':
                Open();
                return false;
            case ')':
                return Close();
            case '|' or '^':
                Take();
                return false;
            case '$':
                _at++;
                _copy.Append(EndAsDotnetReadsIt);
                return false;
            case '.':
                throw Differs(".", "[^\\n]");
            case '*' or '+' or '?':
                Repeat(repeatable);
                return false;
            case '{':
                Count(repeatable);
                return false;
            case ']' or '}':
                throw _refused($"holds '{token}' outside the construct it closes; escape it as \\{token}.");
            default:
                Take();
                return true;
        }
    }

    private void Escape()
    {
        var escaped = Escaped();
        if (SpelledOutsideAClass.TryGetValue(escaped, out var spelling)) throw Differs("\\" + escaped, spelling);
        Escape(escaped);
    }

    private char Escape(char escaped)
    {
        if (Metacharacters.Contains(escaped)) return TakeEscape(escaped);
        if (ControlCharacters.TryGetValue(escaped, out var control))
        {
            TakeEscape(escaped);
            return control;
        }

        if (escaped == 'u') return Hex(escaped, 4);
        if (escaped == 'x') return Hex(escaped, 2);
        if (char.IsLetterOrDigit(escaped)) throw Unread("\\" + escaped);
        throw _refused($"holds '\\{escaped}', which needs no escape; write {escaped}.");
    }

    private char Escaped()
    {
        var endsInTheEscape = _at + 1 >= _pattern.Length;
        if (endsInTheEscape) throw _refused("ends in '\\', which escapes nothing; escape it as \\\\.");
        return _pattern[_at + 1];
    }

    private char TakeEscape(char escaped)
    {
        _copy.Append('\\').Append(escaped);
        _at += 2;
        return escaped;
    }

    private char Hex(char escaped, int digits)
    {
        var start = _at + 2;
        var hex = start + digits <= _pattern.Length ? _pattern.Substring(start, digits) : "";
        var hasTheDigits = hex.Length == digits && IsHex(hex);
        if (!hasTheDigits) throw _refused($"holds '\\{escaped}' without {digits} hex digits after it.");
        var code = Convert.ToInt32(hex, 16);
        _copy.Append(_pattern, _at, digits + 2);
        _at += digits + 2;
        return (char)code;
    }

    private void Class()
    {
        Take();
        if (Next == '^') Take();
        if (Next == ']') throw _refused("holds ']' first in a class, which the browser reads as the end of an empty class; escape it as \\].");
        var afterARange = false;
        while (true)
        {
            if (_at >= _pattern.Length) throw _refused("opens a class it does not close; close it with ']'.");
            if (Next == ']')
            {
                Take();
                return;
            }

            if (afterARange && Next == '-') throw _refused("holds '-' right after a range; escape it as \\-.");
            var from = ClassItem();
            var startsARange = Next == '-' && _at + 1 < _pattern.Length && _pattern[_at + 1] != ']';
            afterARange = startsARange;
            if (!startsARange) continue;
            Take();
            var to = ClassItem();
            if (to < from) throw _refused($"holds a range from '{from}' to '{to}', which runs backwards.");
        }
    }

    private char ClassItem()
    {
        var item = _pattern[_at];
        if (item == '[') throw _refused("holds '[' inside a class, which .NET reads as a subtraction after '-'; escape it as \\[.");
        if (item != '\\')
        {
            Take();
            return item;
        }

        var escaped = Escaped();
        if (SpelledInsideAClass.TryGetValue(escaped, out var spelling)) throw _refused($"holds '\\{escaped}' inside a class, which .NET and the browser read differently; write {spelling}.");
        if (escaped == '-') return TakeEscape(escaped);
        return Escape(escaped);
    }

    private void Open()
    {
        var opensASpecialGroup = _at + 1 < _pattern.Length && _pattern[_at + 1] == '?';
        if (!opensASpecialGroup)
        {
            Take();
            _lookarounds.Push(false);
            return;
        }

        var opener = Opener();
        if (opener is null)
        {
            var shown = _pattern.Substring(_at, Math.Min(3, _pattern.Length - _at));
            throw Unread(shown);
        }

        _copy.Append(opener);
        _at += opener.Length;
        _lookarounds.Push(opener != "(?:");
    }

    private string? Opener()
    {
        foreach (var opener in new[] { "(?:", "(?=", "(?!", "(?<=", "(?<!" })
        {
            var opens = string.CompareOrdinal(_pattern, _at, opener, 0, opener.Length) == 0;
            if (opens) return opener;
        }

        return null;
    }

    private bool Close()
    {
        if (_lookarounds.Count == 0) throw _refused("holds ')' that closes no group; escape it as \\).");
        Take();
        var closedALookaround = _lookarounds.Pop();
        return !closedALookaround;
    }

    private void Repeat(bool repeatable)
    {
        var quantifier = _pattern[_at];
        if (!repeatable) throw _refused($"holds '{quantifier}' where nothing it can repeat comes before it; escape it as \\{quantifier}.");
        Take();
        if (Next == '?') Take();
    }

    private void Count(bool repeatable)
    {
        var close = _pattern.IndexOf('}', _at);
        var body = close < 0 ? "" : _pattern.Substring(_at + 1, close - _at - 1);
        var parts = body.Split(',');
        var isACount = parts.Length <= 2 && IsNumber(parts[0]) && (parts.Length == 1 || parts[1].Length == 0 || IsNumber(parts[1]));
        if (!isACount) throw _refused("holds '{' that is not a count such as {2} or {2,5}; escape it as \\{.");
        if (!repeatable) throw _refused($"holds '{{{body}}}' where nothing it can repeat comes before it.");
        var runsBackwards = parts.Length == 2 && parts[1].Length > 0 && int.Parse(parts[1], CultureInfo.InvariantCulture) < int.Parse(parts[0], CultureInfo.InvariantCulture);
        if (runsBackwards) throw _refused($"holds the count '{{{body}}}', whose least is above its most.");
        _copy.Append(_pattern, _at, close - _at + 1);
        _at = close + 1;
        if (Next == '?') Take();
    }

    private char? Next => _at < _pattern.Length ? _pattern[_at] : null;

    private void Take()
    {
        _copy.Append(_pattern[_at]);
        _at++;
    }

    private ArgumentException Differs(string construct, string spelling)
        => _refused($"holds '{construct}', which .NET and the browser read differently; write {spelling}.");

    private ArgumentException Unread(string construct)
        => _refused($"holds '{construct}', which the browser does not read as .NET does; move the work to the server.");

    private static bool IsNumber(string text) => text.Length is > 0 and <= 9 && text.All(digit => digit is >= '0' and <= '9');

    private static bool IsHex(string text) => text.All(Uri.IsHexDigit);
}
