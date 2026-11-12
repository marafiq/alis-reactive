using System.Globalization;
using Alis.Reactive.Schema;

namespace Alis.Reactive.PathsAndWire;

internal static partial class ViewCulture
{
    private const string CollationOfTheInvariantCulture = "en";

    private static readonly NumberText AnInteger = new(group: false, exponent: false);

    internal static Culture Of(CultureInfo culture)
        => new(culture.Name, CollationOf(culture), culture.NumberFormat.NumberDecimalSeparator, culture.NumberFormat.NumberGroupSeparator, AnInteger, AFraction());

    private static string CollationOf(CultureInfo culture) => culture.Name.Length == 0 ? CollationOfTheInvariantCulture : culture.Name;

    private static partial NumberText AFraction();
}
