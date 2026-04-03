import type { Plan, RequestValidation } from "./plan";

export interface TraceContext {
  readonly traceId: string;
  readonly spanId: string;
  readonly traceFlags: string;
  readonly parentSpanId?: string;
}

export interface ExecContext {
  readonly plan: Plan;
  readonly event?: unknown;
  readonly eventObject?: unknown;
  readonly response?: unknown;
  readonly request?: unknown;
  readonly local?: Record<string, unknown>;
  readonly validation?: RequestValidation;
  readonly trace?: TraceContext;
}
