### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
ALIS001 | Alis.Reactive | Error | Incomplete conditional chain
ALIS002 | Alis.Reactive | Error | NativeActionLink must stay a single request chain
ALIS003 | Alis.Reactive | Error | Duplicate reactive event registration
ALIS004 | Alis.Reactive | Error | Control flow is not allowed inside reactive callbacks
ALIS005 | Alis.Reactive.Validation | Info | Server-only validation rules are not supported in client validation
ALIS006 | Alis.Reactive.Validation | Warning | Server-only conditions are not supported in client validation
ALIS007 | Alis.Reactive.HttpPipeline | Error | Chained requests can only be configured once per response branch
ALIS008 | Alis.Reactive.HttpPipeline | Error | Multiple top-level HTTP requests in one pipeline overwrite each other
