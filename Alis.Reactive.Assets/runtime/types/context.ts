// Runtime values addressable by PayloadSource.scope while a reaction executes.

export interface ExecContext {
  /** Trigger payload: component event args, CustomEvent detail, SSE/SignalR, or dispatch data. */
  readonly event?: unknown;
  /** HTTP response body exposed to success and error response routes. */
  readonly response?: unknown;
  /** Resolved outgoing request input before fetch. */
  readonly request?: unknown;
  /** Local payload object used by reactions that target PayloadSource scope "local". */
  readonly local?: Record<string, unknown>;
  /** Array-operation stack; the last item is PayloadSource scope "element". */
  readonly element?: readonly unknown[];
}
