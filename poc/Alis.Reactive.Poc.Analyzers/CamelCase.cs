namespace Alis.Reactive.Poc.Analyzers;

internal static class CamelCase
{
    public static string Of(string name)
    {
        if (name.Length == 0 || !char.IsUpper(name[0])) return name;
        var characters = name.ToCharArray();
        for (var position = 0; position < characters.Length; position++)
        {
            if (position == 1 && !char.IsUpper(characters[position])) break;
            var hasNext = position + 1 < characters.Length;
            if (position > 0 && hasNext && !char.IsUpper(characters[position + 1]))
            {
                if (characters[position + 1] == ' ') characters[position] = char.ToLowerInvariant(characters[position]);
                break;
            }

            characters[position] = char.ToLowerInvariant(characters[position]);
        }

        return new string(characters);
    }
}
