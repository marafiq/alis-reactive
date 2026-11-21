import type { Owner } from "../OwnershipAndState/index";
import type { PlanDocument } from "../Schema/index";
import type { Traced } from "../Tracing/index";
import type { Read } from "../ValuesAndSources/index";

export type Binding = Read | (() => Read);

export interface Context {
  readonly owner: Owner;
  readonly plan: PlanDocument;
  readonly scope: ReadonlyMap<string, Binding>;
  readonly signal: AbortSignal;
  readonly trace: Traced;
}

export function bind(context: Context, name: string, binding: Binding): Context {
  const scope = new Map(context.scope);
  scope.set(name, binding);
  return { ...context, scope };
}

export function under(context: Context, signal: AbortSignal): Context {
  return { ...context, signal };
}

export function within(context: Context, trace: Traced): Context {
  return { ...context, trace };
}

export function isAnObject(held: unknown): held is object {
  return (typeof held === "object" && held !== null) || typeof held === "function";
}
