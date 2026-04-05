// ExecContext — carries execution-scoped data through the reaction tree.
// Each payload scope maps to a concrete runtime value.

export interface ExecContext {
  /** The triggering event payload (component event args, custom event detail, SSE/SignalR message). */
  readonly event?: unknown;
  /** The HTTP response body (set by request handler on success/error). */
  readonly response?: unknown;
  /** The gathered request body (set before HTTP fetch). */
  readonly request?: unknown;
  /** Local scratch data for intermediate computation. */
  readonly local?: Record<string, unknown>;
}
