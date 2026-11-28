using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Poc.App.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Poc.App.Stations;

[SuppressMessage("Alis.Reactive.Contract", "ALIS028", Justification = "Drift on purpose: a_source_the_framework_did_not_make_is_refused_with_what_a_source_is proves the view refuses a source the build did not see, as one a library generates when the app runs.")]
public sealed class HandWrittenRounds : ITypedSource<IEnumerable<CareRound>>;
