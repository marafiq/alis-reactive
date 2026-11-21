using System;

namespace Alis.Reactive.Components;

internal static class JavaScriptNames
{
    internal static bool IsRegistryName(string name)
    {
        var valid = name.Length > 0 && IsLetter(name[0]);
        foreach (var character in name)
            valid = valid && (IsLetter(character) || IsDigit(character) || character == '_' || character == '-');
        return valid;
    }

    internal static string Checked(string name)
    {
        var valid = name.Length > 0 && (IsLetter(name[0]) || name[0] == '_' || name[0] == '$');
        foreach (var character in name)
            valid = valid && (IsLetter(character) || IsDigit(character) || character == '_' || character == '$');
        if (!valid) throw new ArgumentException($"'{name}' is not a JavaScript name: a letter, '_' or '$', then letters, digits, '_' or '$'.", nameof(name));
        return name;
    }

    private static bool IsLetter(char character) => (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z');

    private static bool IsDigit(char character) => character >= '0' && character <= '9';
}
