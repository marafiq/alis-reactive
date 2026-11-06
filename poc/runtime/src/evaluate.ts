import type { Expression, PathStep, PlanDocument, Presence } from "./plan";
import { assertNever, heldOrNull, nothing, Stop, value, type Read } from "./read";

export interface Context {
  readonly plan: PlanDocument;
  readonly scope: ReadonlyMap<string, Read>;
  readonly locate: (id: string) => object;
}

export function evaluate(expression: Expression, context: Context): Read {
  switch (expression.kind) {
    case "lit":
      return value(expression.value);
    case "obj":
      return value(context.locate(expression.id));
    case "ref":
      return bound(expression.name, context);
    case "get":
      return walk(evaluate(expression.target, context), expression.path, expression.members);
    case "fn":
      return value(callable(expression.params, expression.body, context));
    default:
      return assertNever(expression);
  }
}

export function walk(start: Read, path: readonly PathStep[], members: Presence): Read {
  let holder = start;
  for (const step of path) {
    holder = member(holder, step, members);
    if (holder.kind === "nothing") return holder;
  }
  return holder;
}

export type Holder = { readonly kind: "found"; readonly object: object } | { readonly kind: "absent" };

const absent: Holder = { kind: "absent" };

export function holderOf(read: Read, step: PathStep, members: Presence): Holder {
  if (read.kind === "nothing" || read.value === null || read.value === undefined) {
    if (members === "required") throw new Stop("Nothing holds the member '" + String(step) + "'.");
    return absent;
  }
  const object: object = Object(read.value);
  if (step in object) return { kind: "found", object };
  if (members === "required") throw new Stop("The member '" + String(step) + "' is not on the object that should hold it.");
  return absent;
}

function member(read: Read, step: PathStep, members: Presence): Read {
  const holder = holderOf(read, step, members);
  if (holder.kind === "absent") return nothing;
  const held: unknown = Reflect.get(holder.object, step);
  if (isPromise(held)) throw new Stop("The member '" + String(step) + "' holds a promise where a value was declared.");
  return value(held);
}

function bound(name: string, context: Context): Read {
  const read = context.scope.get(name);
  if (read === undefined) throw new Error("The plan reads the name '" + name + "', which no block binds.");
  return read;
}

function callable(params: readonly string[], body: Expression, context: Context): (...args: unknown[]) => unknown {
  return (...args: unknown[]) => {
    const scope = new Map(context.scope);
    params.forEach((name, position) => scope.set(name, position < args.length ? value(args[position]) : nothing));
    return heldOrNull(evaluate(body, { ...context, scope }));
  };
}

function isPromise(held: unknown): boolean {
  return held instanceof Promise;
}
