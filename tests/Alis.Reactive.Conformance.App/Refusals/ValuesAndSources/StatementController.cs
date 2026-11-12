using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class StatementController : Controller
{
    private const decimal WhatIsDue = 120m;

    [HttpGet(RefusalPages.StatementOfABalanceWrittenAsText)]
    public IActionResult OfABalanceWrittenAsText() => View("/Refusals/ValuesAndSources/StatementOfABalanceWrittenAsText.cshtml", new Statement("Ada Hart"));

    [HttpGet(RefusalPages.StatementOfABalance)]
    public IActionResult OfABalance() => View("/Refusals/ValuesAndSources/StatementOfABalance.cshtml", new Statement("Ada Hart"));

    [HttpGet(Statements.BalanceWrittenAsText)]
    public IActionResult TheBalanceWrittenAsText() => Json(new BalanceWrittenAsText { Due = WhatIsDue });

    [HttpGet(Statements.Balance)]
    public IActionResult TheBalance() => Json(new Balance { Due = WhatIsDue });
}

public sealed record Statement(string Resident);

public sealed class BalanceWrittenAsText
{
    [JsonConverter(typeof(AmountAsText))]
    public decimal Due { get; init; }
}

public sealed class Balance
{
    public decimal Due { get; init; }
}

public sealed class AmountAsText : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => decimal.Parse(reader.GetString() ?? throw new JsonException("An amount is written as text."), CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
}

public static class Statements
{
    public const string BalanceWrittenAsText = RefusalPages.Area + "values-and-sources/statement/the-balance-written-as-text";

    public const string Balance = RefusalPages.Area + "values-and-sources/statement/the-balance";
}
