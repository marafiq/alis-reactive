import type { Owner } from "../OwnershipAndState/index";
import type { PlanDocument } from "../Schema/index";
import type { Read } from "../ValuesAndSources/index";

export interface Context {
  readonly plan: PlanDocument;
  readonly owner: Owner;
  readonly scope: ReadonlyMap<string, Read>;
}

export function bind(context: Context, name: string, read: Read): Context {
  const scope = new Map(context.scope);
  scope.set(name, read);
  return { ...context, scope };
}

export function isAnObject(held: unknown): held is object {
  return (typeof held === "object" && held !== null) || typeof held === "function";
}
