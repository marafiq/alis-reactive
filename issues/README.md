# Open Issues

This folder now reflects the current open architecture issues against the latest code.

## Current Open Set

- `005-incomplete-conditional-pipelines-fail-with-internal-null-state.md`
- `006-validation-contract-still-fails-open-and-loses-fidelity.md`
- `007-request-payload-format-is-modeled-on-the-wrong-surface.md`
- `008-actionlink-one-request-chain-needs-an-explicit-semantic-contract.md`

## Closed Since The Earlier Review

These were removed from the open list because the latest code now closes them:

- conditions as a first-class composable reaction shape
- stale component registrations across partial re-merge
- computed expression fail-fast in `ExpressionPathHelper`
- command-list surfaces silently accepting richer DSL
- missing validation extractor for `Validate<TValidator>()`
- native event payload shaping via `readExpr`
