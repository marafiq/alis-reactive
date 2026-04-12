# Tracing & Observability Improvements

> Codex xhigh audit findings — practical improvements with file:line evidence.

**Current state:** ~66 log points across 14 files (Codex audit count). evaluateValue, event firing callbacks, branch matching, and validation passes are all silent. Trigger registration IS logged (trigger.ts:49) but actual event dispatch is not. No correlation IDs. No runtime activation without redeploy.

**Dependency:** Priority 1 (correlation headers) should land AFTER the HTTP Headers plan, which adds `Request.headers` support. Correlation IDs then ride the same mechanism via `.Header("traceparent", ...)` or automatic injection in http.ts.

---

## Priority 1: Correlation ID

ExecContext carries no trace fields. Parallel reactions interleave. HTTP requests have no correlation headers.

**Fix:** Add `trace: { actionId, planId }` to ExecContext. Generate actionId on event trigger. Pass through all reactions. Set as HTTP header.

## Priority 2: Event Firing + Reaction Lifecycle

Today: registration logged, actual event firing silent (trigger.ts L50, L66).

**Fix:** Log event-fired, reaction-start, reaction-end, branch-matched at debug level.

## Priority 3: Instrument evaluateValue + Resolver

The central value read path is completely dark (evaluate.ts has zero log calls).

**Fix:** Log producer kind, source, member, result (summarized — no PHI) at trace level.

## Priority 4: Structured Validation Results

ruleFails() returns bool only. When-skipped rules silent. Passes silent.

**Fix:** Return structured result { outcome: pass|skip|fail, reason, target } instead of bool.

## Priority 5: Runtime Activation

Today: `data-trace` attribute on plan element, requires redeploy.

**Fix:** Check URL param `?alisTrace=debug`, localStorage `alis:trace`, and expose `window.alis.trace.setLevel()` for console activation.

## Priority 6: Lazy Payloads + Structured Output

Current emit() stringifies eagerly. Flat string output not expandable in DevTools.

**Fix:** Accept `() => data` lazy factory. Use `console.log(tag, msg, payload)` for structured expandable output.

---

## Performance Note

JSON.stringify does NOT run when off (trace.ts L32-34 — guard before build). BUT payload objects ARE allocated at call sites even when off. Lazy factories fix this.
