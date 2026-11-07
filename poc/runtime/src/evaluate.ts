import type { CallExpression, Expression, PathStep, PlanDocument, ReadPresence } from "./plan";
import { assertNever, heldOrNull, nothing, Stop, value, type Read } from "./read";
import { operate } from "./operators";
import { readData } from "./data";
import { converted } from "./shapes";
import type { Owner } from "./owners";

export interface Context {
  readonly plan: PlanDocument;
  readonly scope: ReadonlyMap<string, Bound>;
  readonly locate: (id: string) => unknown;
  readonly owner: Owner;
}

export type Bound = Read | Deferred;

interface Deferred {
  readonly kind: "deferred";
  readonly expression: Expression;
  readonly context: Context;
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
    case "answer":
      return value(answer(expression.arguments, expression.body, context));
    case "op":
      return operate(expression, (argument) => evaluate(argument, context), context.plan.culture);
    case "as":
      return converted(evaluate(expression.expr, context), expression.shape);
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

type HeldValue = { readonly kind: "value"; readonly value: NonNullable<unknown> };

function holdsSomething(read: Read): read is HeldValue {
  return read.kind === "value" && read.value !== null && read.value !== undefined;
}

export function objectHolding(read: Read, step: PathStep): object {
  if (!holdsSomething(read)) throw new Stop("Nothing holds the member '" + String(step) + "'.");
  return Object(read.value);
}

export function memberHolder(read: Read, step: PathStep): object {
  const object = objectHolding(read, step);
  if (step in object) return object;
  throw new Stop("The member '" + String(step) + "' is not on the value that should hold it.");
}

function holderOrAbsent(read: Read, step: PathStep): Holder {
  const object = objectHolding(read, step);
  return step in object ? { kind: "found", object } : absent;
}

export function holderOf(read: Read, step: PathStep, members: ReadPresence): Holder {
  switch (members) {
    case "required":
      return { kind: "found", object: memberHolder(read, step) };
    case "requiredIfHeld":
      return holdsSomething(read) ? { kind: "found", object: memberHolder(read, step) } : absent;
    case "optionalMember":
      return holderOrAbsent(read, step);
    case "optional":
      return holdsSomething(read) ? holderOrAbsent(read, step) : absent;
    default:
      return assertNever(members);
  }
}

function mayBeAbsent(members: ReadPresence): boolean {
  switch (members) {
    case "optional":
    case "optionalMember":
      return true;
    case "required":
    case "requiredIfHeld":
      return false;
    default:
      return assertNever(members);
  }
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
  if (call.path.length === 0) return calledItself(call, context);
  const last = methodName(call);
  const holder = holderOf(walk(evaluate(call.target, context), call.path.slice(0, -1), call.members), last, call.members);
  if (holder.kind === "absent") return notCalled;
  const method: unknown = Reflect.get(holder.object, last);
  if (method === undefined && mayBeAbsent(call.members)) return notCalled;
  if (typeof method !== "function") throw new Stop("The member '" + String(last) + "' is not a method.");
  const args = call.args.map((argument) => heldOrNull(evaluate(argument, context)));
  return { kind: "returned", held: Reflect.apply(method, holder.object, args) };
}

function calledItself(call: CallExpression, context: Context): Called {
  const target = evaluate(call.target, context);
  if (target.kind === "nothing" || typeof target.value !== "function") throw new Stop("The object called as a function is not a function.");
  const args = call.args.map((argument) => heldOrNull(evaluate(argument, context)));
  return { kind: "returned", held: Reflect.apply(target.value, undefined, args) };
}

export function callee(call: CallExpression): string {
  return call.path.length === 0 ? "The object called as a function" : "The method '" + String(methodName(call)) + "'";
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
    throw new Stop(callee(call) + " returned a promise; a method that returns one is declared with MethodAsync.");
  }
  return value(result.held);
}

function bound(name: string, context: Context): Read {
  const held = context.scope.get(name);
  if (held === undefined) throw new Error("The plan reads the name '" + name + "', which no block binds.");
  return held.kind === "deferred" ? evaluate(held.expression, held.context) : held;
}

function callable(params: readonly string[], body: Expression, context: Context): (...args: unknown[]) => unknown {
  return (...args: unknown[]) => {
    const scope = new Map(context.scope);
    params.forEach((name, position) => scope.set(name, position < args.length ? value(args[position]) : nothing));
    return heldOrNull(evaluate(body, { ...context, scope }));
  };
}

function answer(argumentsName: string, body: Expression, context: Context): (...args: unknown[]) => unknown {
  return (...args: unknown[]) => {
    const called = new Map(context.scope);
    called.set(argumentsName, value(args));
    return heldOrNull(evaluate(body, { ...context, scope: called }));
  };
}

function isPromise(held: unknown): held is Promise<unknown> {
  return held instanceof Promise;
}
