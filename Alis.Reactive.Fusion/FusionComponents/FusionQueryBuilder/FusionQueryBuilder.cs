using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>Edits a filter over a local collection of rows.</summary>
/// <typeparam name="TRow">The type of the rows supplied to the query builder.</typeparam>
[Mirrors("@syncfusion/ej2-querybuilder", "QueryBuilder")]
public sealed class FusionQueryBuilder<TRow> : FusionComponent
{
    /// <summary>Checks the required fields of the current editors when validation is enabled.</summary>
    /// <returns>Whether the editors pass the check.</returns>
    public MethodResult<bool> ValidateFields() => Method<bool>();

    /// <summary>Gets the current tree with incomplete conditions and empty groups removed.</summary>
    /// <returns>The complete conditions, or an empty object when none remain.</returns>
    public MethodResult<RuleModel> GetValidRules() => Method<RuleModel>();

    /// <summary>Builds the EJ2 predicate for a tree of complete conditions.</summary>
    /// <param name="rules">A source of the rule tree to compile.</param>
    /// <returns>The record predicate. An empty tree produces no predicate.</returns>
    public Predicate GetPredicate(TypedSource<RuleModel> rules) => ReturnedObject<Predicate>([(Arg<RuleModel>)rules], nullable: true);

    /// <summary>Applies the current complete conditions to the local data source.</summary>
    /// <returns>The matching rows after the data manager finishes.</returns>
    public MethodResult<QueryResult> GetFilteredRecords() => MethodAsync<QueryResult>();

    /// <summary>The rule tree supplied by EJ2. Groups contain child rules.</summary>
    [Mirrors("@syncfusion/ej2-querybuilder", "RuleModel")]
    public sealed class RuleModel
    {
        /// <summary>Gets or sets the children of a group. A leaf or an empty result has no children.</summary>
        [JsonPropertyName("rules")]
        public IReadOnlyList<RuleModel>? Rules { get; set; }
    }

    /// <summary>An EJ2 predicate compiled from the filter.</summary>
    [Mirrors("@syncfusion/ej2-data", "Predicate")]
    public sealed class Predicate : JsObject
    {
        /// <summary>Checks one row against the compiled filter.</summary>
        /// <param name="record">The row to check.</param>
        /// <returns>Whether the record meets the conditions.</returns>
        public MethodResult<bool> Validate(Arg<TRow> record) => Method<bool>([record]);
    }

    /// <summary>The local data manager’s answer to a filter.</summary>
    public sealed class QueryResult
    {
        /// <summary>Gets or sets the rows that match the filter.</summary>
        [JsonPropertyName("result")]
        public required IReadOnlyList<TRow> Result { get; set; }
    }
}
