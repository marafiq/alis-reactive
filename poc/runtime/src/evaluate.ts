import type { CallExpression, Expression, PathStep, PlanDocument, ReadPresence } from "./plan";
import { assertNever, heldOrNull, nothing, Stop, value, type Read } from "./read";
import { operate } from "./operators";
import { readData } from "./data";

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
    case "data":
      return readData(evaluate(expression.target, context), expression.path);
    case "call":
      return invoke(expression, context);
    case "fn":
      return value(callable(expression.params, expression.body, context));
    case "op":
      return operate(expression, (argument) => evaluate(argument, context), context.plan.culture);
    default:
      return assertNever(expression);
  }
}

export function walk(start: Read, path: readonly PathStep[], members: ReadPresence): Read {
  let holder = start;
  for (const step of path) {
    holder = member(holder, step, members);
    if (holder.kind === "nothing") return holder;
  }
  return holder;
}

export type Holder = { readonly kind: "found"; readonly object: object } | { readonly kind: "absent" };

const absent: Holder = { kind: "absent" };

export function holderOf(read: Read, step: PathStep, members: ReadPresence): Holder {
  if (read.kind === "nothing" || read.value === null || read.value === undefined) {
    if (members === "required") throw new Stop("Nothing holds the member '" + String(step) + "'.");
    return absent;
  }
  const object: object = Object(read.value);
  if (step in object) return { kind: "found", object };
  if (members === "optional") return absent;
  throw new Stop("The member '" + String(step) + "' is not on the value that should hold it.");
}

function member(read: Read, step: PathStep, members: ReadPresence): Read {
  const holder = holderOf(read, step, members);
  if (holder.kind === "absent") return nothing;
  const held: unknown = Reflect.get(holder.object, step);
  if (isPromise(held)) throw new Stop("The member '" + String(step) + "' holds a promise where a value was declared.");
  return value(held);
}

export type Called = { readonly kind: "absent" } | { readonly kind: "returned"; readonly held: unknown };

const notCalled: Called = { kind: "absent" };

export function called(call: CallExpression, context: Context): Called {
  const last = methodName(call);
  const holder = holderOf(walk(evaluate(call.target, context), call.path.slice(0, -1), call.members), last, call.members);
  if (holder.kind === "absent") return notCalled;
  const method: unknown = Reflect.get(holder.object, last);
  if (typeof method !== "function") throw new Stop("The member '" + String(last) + "' is not a method.");
  const args = call.args.map((argument) => heldOrNull(evaluate(argument, context)));
  return { kind: "returned", held: Reflect.apply(method, holder.object, args) };
}

export function methodName(call: CallExpression): PathStep {
  const last = call.path[call.path.length - 1];
  if (last === undefined) throw new Error("The plan writes a call with no method to call.");
  return last;
}

function invoke(call: CallExpression, context: Context): Read {
  const result = called(call, context);
  if (result.kind === "absent") return nothing;
  if (isPromise(result.held)) {
    void result.held.catch(() => nothing);
    throw new Stop("The method '" + String(methodName(call)) + "' returned a promise; a method that returns one is declared with MethodAsync.");
  }
  return value(result.held);
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

function isPromise(held: unknown): held is Promise<unknown> {
  return held instanceof Promise;
}
