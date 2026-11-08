using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Components;
using Alis.Reactive.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Poc.App.Components;

[Mirrors("Components/HandoverBoard/handover-board", "HandoverBoard")]
public sealed class HandoverBoard : Component
{
    public static ComponentVendor BoardVendor { get; } = ComponentVendor.Define("handover", objectPath: "board", subscribe: "addEventListener", unsubscribe: "removeEventListener");

    public override ComponentVendor Vendor => BoardVendor;

    public ReadOnly<int> Round => ReadOnly<int>();

    public ReadOnly<decimal> Dose => ReadOnly<decimal>();

    public ReadOnly<double> Rate => ReadOnly<double>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: a_boolean_member_holding_text_sends_no_request proves the browser stops on it.")]
    public ReadOnly<bool> Due => ReadOnly<bool>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: a_text_member_holding_a_number_sends_no_request proves the browser stops on it.")]
    public ReadOnly<string> Ward => ReadOnly<string>();

    public ReadOnly<DateTime> PlannedFor => ReadOnly<DateTime>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: an_instant_member_holding_text_that_names_no_moment_sends_no_request proves the browser stops on it.")]
    public ReadOnly<DateTimeOffset> StartsAt => ReadOnly<DateTimeOffset>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: a_list_member_holding_one_number_sends_no_request proves the browser stops on it.")]
    public ReadOnly<IReadOnlyList<int>> Beds => ReadOnly<IReadOnlyList<int>>();

    public ReadOnly<IReadOnlyList<int>> Late => ReadOnly<IReadOnlyList<int>>();

    public ReadOnly<IReadOnlyList<int?>> Waiting => ReadOnly<IReadOnlyList<int?>>();

    public ReadOnly<IReadOnlyList<string?>> Leads => ReadOnly<IReadOnlyList<string?>>();

    [SuppressMessage("Alis.Reactive.Contract", "ALIS025", Justification = "Drift on purpose: a_shift_member_holding_a_record_sends_no_request proves the browser stops on it.")]
    public ReadOnly<Shift> Shift => ReadOnly<Shift>();

    public ReadOnly<Handover?> Current => ReadOnly<Handover?>();
}

public enum Shift
{
    Day,
    Twilight,
    Night,
}

public sealed class Handover
{
    public int Round { get; init; }

    public DateTimeOffset StartsAt { get; init; }

    public DateTimeOffset? EndsAt { get; init; }

    public IReadOnlyList<int> Beds { get; init; } = [];

    public string? Lead { get; init; }

    public int? Target { get; init; }
}

public static class HandoverBoardHtml
{
    public static Rendered<HandoverBoard, TModel> HandoverBoard<TModel>(this ReactivePlan<TModel> plan, string id)
    {
        var board = new TagBuilder("section");
        board.Attributes["id"] = id;
        board.Attributes["data-handover-board"] = "";
        return plan.Render<HandoverBoard>(id, board);
    }
}
