// ExecContext — carries execution-scoped data through the reaction tree.
// Each payload scope maps to a concrete runtime value.

export interface ExecContext {
  /**
   * Event payload — the triggering data for this reaction.
   * Sources: component event args, custom event detail, SSE/SignalR message, OR dispatch data.
   * Resolved by PayloadSource with scope "event" or "dispatch".
   */
  readonly event?: unknown;
  /**
   * HTTP response body — set by HTTP response routing on success or error.
   * Resolved by PayloadSource with scope "success" or "error".
   */
  readonly response?: unknown;
  /**
   * Outgoing request payload — the resolved request input before HTTP fetch.
   * Resolved by PayloadSource with scope "request".
   */
  readonly request?: unknown;
  /**
   * Local scratch data for intermediate computation.
   * Resolved by PayloadSource with scope "local". Not currently used.
   */
  readonly local?: Record<string, unknown>;
  /**
   * Element scope stack for array operations — the innermost (top) entry is the
   * current element under iteration. Grown by ExecutionContext.withElement(item).
   * Resolved by PayloadSource with scope "element" (reads top of stack).
   */
  readonly element?: readonly unknown[];
}
