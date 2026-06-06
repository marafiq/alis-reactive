import type { ExecContext, PayloadSource } from "../types/index";
import { objectRecordFrom } from "./object-record";

export type ServerValidationPayload =
  | { readonly kind: "available"; readonly response: unknown }
  | { readonly kind: "absent" };

const absentServerValidationPayload: ServerValidationPayload = { kind: "absent" };

export class ExecutionContext {
  private constructor(private readonly values: ExecContext | undefined) {}

  static absent(): ExecutionContext {
    return new ExecutionContext(undefined);
  }

  static empty(): ExecutionContext {
    return new ExecutionContext({});
  }

  static from(values: ExecContext | undefined): ExecutionContext {
    if (values === undefined) return ExecutionContext.absent();

    return new ExecutionContext(values);
  }

  static event(payload: unknown): ExecutionContext {
    return new ExecutionContext({ event: payload });
  }

  get raw(): ExecContext | undefined {
    return this.values;
  }

  asAvailable(): ExecContext {
    return this.values ?? {};
  }

  withRequest(request: unknown): ExecutionContext {
    return new ExecutionContext({ ...this.asAvailable(), request });
  }

  withResponse(response: unknown): ExecutionContext {
    return new ExecutionContext({ ...this.asAvailable(), response });
  }

  // Push an array element onto the element scope stack for per-element evaluation.
  withElement(item: unknown): ExecutionContext {
    const current = this.asAvailable();
    return new ExecutionContext({ ...current, element: [...(current.element ?? []), item] });
  }

  resolvePayload(source: PayloadSource): unknown {
    const values = this.requireValues(source);
    switch (source.scope) {
      case "event":
      case "dispatch":
        return values.event;
      case "success":
      case "error":
        return values.response;
      case "request":
        return values.request;
      case "local":
        return values.local;
      case "element": {
        const stack = values.element;
        return stack === undefined ? undefined : stack[stack.length - 1];
      }
      default: {
        const _: never = source.scope;
        throw new Error(`[alis] unknown payload scope: "${_}"`);
      }
    }
  }

  serverValidationPayload(): ServerValidationPayload {
    const response = objectRecordFrom(this.values?.response);
    if (response === undefined) return absentServerValidationPayload;

    return { kind: "available", response };
  }

  private requireValues(source: PayloadSource): ExecContext {
    if (this.values !== undefined) return this.values;

    throw new Error(`[alis] payload source requires execution context (scope: ${source.scope})`);
  }
}
