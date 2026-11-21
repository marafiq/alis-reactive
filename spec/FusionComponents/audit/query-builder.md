# What does Query Builder execute?

The resident review worklist uses Syncfusion’s EJ2 QueryBuilder to select residents by wing, days overdue and escalation. The official example imports and exports the editor’s JSON. This screen also executes that filter over local records and checks an incoming referral against its complete conditions. The npm umbrella and both ASP.NET builder packages are 33.1.47. The lock resolves `@syncfusion/ej2-querybuilder` to 33.1.46 and `@syncfusion/ej2-data` to 33.1.45. [Official demo](https://ej2.syncfusion.com/aspnetcore/querybuilder/defaultfunctionalities#/bootstrap5) · [Worklist](../../../tests/Alis.Reactive.Conformance.App/Care/Residents/ReviewWorklist.cshtml)

## What travels in the editor’s JSON?

| Position | Pinned contract and observations |
|---|---|
| Group | `condition` is `and` or `or` in the standard editor. `rules` contains leaves and nested groups. `getRules()` returns the editing tree, including a wing condition whose value was cleared. |
| Leaf | `field` is the record’s field name, `label` is the display text, `type` selects an editor, and `operator` identifies the comparison. String, number, boolean and date have different operator menus. The view derives each field from a C# member with the plan’s JSON naming. |
| Value | The declared union contains string, number, boolean, string array and number array. The live tree also contains null for null/empty operators. Zero and false survive pruning. An `in` list contains choices, and `between` takes two endpoints. The probe checks membership, both inclusive numeric endpoints and a date-only value against two actual Date records. Date editor settings also accept Date values at the column boundary. |
| Flags | With `enableNotCondition` enabled, an explicit group `not: true` survives normalization. An explicit `isLocked: false` on the group and `true` on a leaf both survive. With separate connectors enabled a leaf can carry its own `condition`. The local predicate implementation at this pin still combines children by the containing group’s condition and does not consult group negation. The probe preserves a NOT on “wing equals North” and still finds North. A leaf’s OR inside an AND group still excludes a South record matching the other leaf. The worklist offers ordinary AND/OR groups. |

`RuleModel` declares all nine members optional. The tested objects distinguish an omitted key, a null value, an empty string, a zero and a false. The Fusion declaration exposes the optional children for a completeness check. It passes the original normalized tree as a typed source to EJ2, preserving the heterogeneous leaf values. The C# slice leaves direct construction and editing of heterogeneous leaf values to the vendor’s builder and editor. [RuleModel declaration](../../../node_modules/@syncfusion/ej2-querybuilder/src/query-builder/query-builder-model.d.ts#L109) · [Normalization](../../../node_modules/@syncfusion/ej2-querybuilder/src/query-builder/query-builder.js#L5662) · [Observed trees and counterexamples](../../probes/query-builder-rules.json)

## Which rules validate a filter?

| Operation | What it decides |
|---|---|
| `validateFields()` | When `allowValidation` is on, checks the rendered field selection and, for required columns, the operator and value. Empty strings and empty arrays fail the required-value check. Value-free null/empty operators have their own branch. When validation is off, an empty required editor returned true in the probe. `ColumnsModel.validation` carries `isRequired` and optional numeric `min` and `max`. The numeric editor consumes those bounds. |
| `getValidRules()` | Clones and normalizes the current tree, removing incomplete leaves and empty groups. A cleared wing disappears while the complete nested group remains. Empty `in` lists and ranges with a missing endpoint disappear. An entirely incomplete tree produced `{}`. A nonempty leaf naming an unlisted field and an unrecognized operator survived the same call. This operation has no catalogue check for arbitrary submitted rule JSON. |
| `getPredicate(tree)` | Recurses through groups and converts comparisons to EJ2 Data predicates. `matchCase` changes the string comparison. An empty normalized tree produced JavaScript `undefined`. A compiled predicate’s `validate(record)` returns a boolean for one record. The referral preview supplies a record that is outside the roster. |
| `getFilteredRecords()` | Calls `getValidRules(this.rule)`, builds a predicate, constructs a Query and returns the data manager’s `executeQuery` result. Without a predicate the vendor builds an unfiltered Query. With the local array source the return was a Promise whose resolved `result` held the matched rows. The typed door awaits that Promise. |
| `getDataManagerQuery(tree)` | Compiles the supplied tree and projects the configured column fields. In the probe the nonempty query contained `onSelect` and `onWhere`. Executing it with EJ2’s local DataManager produced the same resident names as `getFilteredRecords()` and `Predicate.validate`. |
| `getRules()` and `setRules(tree)` | Read the current editing tree and import a supplied tree. The probe uses these directly. The typed worklist starts its saved filter through the vendor’s ASP.NET `Rule` option. |

The record predicate decides membership. The column validation settings govern the filter editor. The application controls what happens with those decisions. Here an invalid edit leaves the last applied worklist on screen while the referral preview uses the complete clauses of the current draft. Business-model validation in ASP.NET has a separate contract. [Editor validation](../../../node_modules/@syncfusion/ej2-querybuilder/src/query-builder/query-builder.js#L1380) · [Numeric editor bounds](../../../node_modules/@syncfusion/ej2-querybuilder/src/query-builder/query-builder.js#L3326) · [Local filtering and projection](../../../node_modules/@syncfusion/ej2-querybuilder/src/query-builder/query-builder.js#L5879) · [Record predicate](../../../node_modules/@syncfusion/ej2-data/src/query.js#L653)

## How much of EJ2 does the slice expose?

The compiler inventory found 93 public instance members including inheritance. These comprised 42 read/write properties, 41 methods and 10 events. Four root methods have typed live doors here, plus the predicate’s record check, the tree’s child collection and the local result envelope. Startup options stay on the ASP.NET builder and the render helper accepts typed local rows. Both host manifests contain the declarations. [Live declarations](../../../Alis.Reactive.Fusion/FusionComponents/FusionQueryBuilder/FusionQueryBuilder.cs) · [Per-member crosswalk](query-builder-crosswalk.tsv)

The crosswalk gives 172 member occurrences across the root, seven event-payload contexts, RuleModel, Predicate, ColumnsModel and Validation. Rows include inherited owners, package-relative locations and optional method arguments. The full catalog links return types and event arguments recursively. The remaining root methods, properties and all ten events have separate verdicts. SQL/Mongo conversion, remote adaptors, custom editors, templates, drag events and live mutation/import of arbitrary saved rule documents have no typed worklist proof here. The member list excludes private, protected, static and vendor-hidden declarations using the existing audit’s rules. [Inventory generator](inventory.mjs) · [Crosswalk generator](query-builder-crosswalk.mjs)

## How can the evidence be replayed?

Run from the worktree with the pinned browser package restored by the conformance project. The first two commands used below the toolchain setup extract public declarations and reproduce the TSV. The raw probe uses an HTTP origin and the shipped EJ2 bundle. The conformance runner drives the typed C# screen. [Raw browser probe](../../probes/query-builder-rules.mjs) · [Behavioral tests](../../../tests/Alis.Reactive.Conformance.PlaywrightTests/FusionComponents/FusionQueryBuilder/WhenTheCoordinatorPreparesAResidentReview.cs)

```bash
. scripts/toolchain-env.sh
build_lock="$HOME/.local/share/alis-toolchain/build.lock"
/usr/bin/lockf -k "$build_lock" caffeinate -i node spec/FusionComponents/audit/inventory.mjs .landing/FusionQueryBuilder/catalog.json .landing/FusionQueryBuilder/slice-rows.tsv < /dev/null
node spec/FusionComponents/audit/query-builder-crosswalk.mjs .landing/FusionQueryBuilder/catalog.json > .landing/FusionQueryBuilder/crosswalk.tsv
cmp spec/FusionComponents/audit/query-builder-crosswalk.tsv .landing/FusionQueryBuilder/crosswalk.tsv
PLAYWRIGHT_PACKAGE="$PWD/tests/Alis.Reactive.Conformance.PlaywrightTests/bin/Release/net10.0/.playwright/package" /usr/bin/lockf -k "$build_lock" caffeinate -i node spec/probes/query-builder-rules.mjs < /dev/null > .landing/FusionQueryBuilder/probe.json
cmp spec/probes/query-builder-rules.json .landing/FusionQueryBuilder/probe.json
/usr/bin/lockf -k "$build_lock" caffeinate -i scripts/playwright.sh --screen review-worklist < /dev/null
```
