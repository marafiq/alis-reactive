// The one evaluator. Expressions are synchronous (grammar.md 3.3): only statements await, so a value
// is computed here without ever waiting. Only the call at the top of a `do` or `let` may return a
// promise, which that statement settles or lets run; the result is shaped when it arrives. A promise
// anywhere else is a member the plan reads as sync, so the call fails at the object (3.4).

import type { CallExpr, Expr, OpExpr, RecordExpr, Shape } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { Environment } from "./environment";
import { call, read, type PathMode } from "./paths";
import { shaped } from "./shape";
import * as ops from "./operators";

/** Resolves the located objects of the plan being run. */
export interface World {
  object(id: string): unknown;
}

export interface Scope {
  readonly env: Environment;
  readonly world: World;
}

export function childScope(scope: Scope): Scope {
  return { env: scope.env.child(), world: scope.world };
}

export function evaluate(expr: Expr, scope: Scope): unknown {
  switch (expr.kind) {
    case "lit":    return shaped(expr.value, expr.shape);
    case "obj":    return scope.world.object(expr.id);
    case "ref":    return scope.env.lookup(expr.name);
    case "get":    return shaped(read(evaluate(expr.target, scope), expr.path, modeOf(expr.target), labelOf(expr.target)), expr.shape);
    case "call":   return invoke(expr, scope, false);
    case "fn":     return closure(expr.params, expr.body, scope);
    case "record": return record(expr, scope);
    case "list":   return expr.items.map(item => evaluate(item, scope));
    case "op":     return operate(expr, scope);
    default:       return assertNever(expr, "expression");
  }
}

/** A path rooted at a located object is checked; every other path is optional (grammar.md 1.3). */
export function modeOf(target: Expr): PathMode {
  return target.kind === "obj" ? "checked" : "optional";
}

export function labelOf(target: Expr): string {
  return target.kind === "obj" ? `#${target.id}` : target.kind === "ref" ? target.name : `(${target.kind})`;
}

/** A statement's expression: a call at its top may return a promise for the statement to settle. */
export function evaluateStatement(expr: Expr, scope: Scope): unknown {
  return expr.kind === "call" ? invoke(expr, scope, true) : evaluate(expr, scope);
}

function invoke(expr: CallExpr, scope: Scope, mayReturnPromise: boolean): unknown {
  const receiver = evaluate(expr.target, scope);
  const args = expr.args.map((arg, index) => shapeArgument(evaluate(arg, scope), expr.argShapes[index]));
  const label = labelOf(expr.target);
  const result = call(receiver, expr.path, args, modeOf(expr.target), label);
  if (!isPromise(result)) return shaped(result, expr.shape);
  if (!mayReturnPromise) throw unawaitedPromise(label, expr.path);
  return result.then(value => shaped(value, expr.shape));
}

function unawaitedPromise(label: string, path: readonly string[]): Error {
  const member = path.length === 0 ? label : `${label}.${path.join(".")}`;
  return new Error(`[alis] ${member} returned a promise where the plan reads a value; declare the method with CallAsync or InvokeAsync`);
}

function shapeArgument(value: unknown, shape: Shape | undefined): unknown {
  return shape === undefined ? value : shaped(value, shape);
}

function closure(params: readonly string[], body: Expr, scope: Scope): (...args: unknown[]) => unknown {
  return (...args: unknown[]) => {
    const env = scope.env.child();
    params.forEach((name, index) => env.bind(name, args[index]));
    return evaluate(body, { env, world: scope.world });
  };
}

function record(expr: RecordExpr, scope: Scope): Record<string, unknown> {
  const output: Record<string, unknown> = {};
  for (const entry of expr.entries) {
    if (entry.kind === "field") {
      output[entry.name] = evaluate(entry.value, scope);
      continue;
    }
    const spread = evaluate(entry.value, scope);
    if (spread !== null && typeof spread === "object") Object.assign(output, spread);
  }
  return output;
}

function operate(expr: OpExpr, scope: Scope): unknown {
  const args = expr.args;
  const at = (index: number): unknown => evaluate(args[index]!, scope);

  switch (expr.op) {
    case "and":      return args.every((_, index) => ops.truthy(at(index)));
    case "or":       return args.some((_, index) => ops.truthy(at(index)));
    case "not":      return !ops.truthy(at(0));
    case "cond":     return ops.truthy(at(0)) ? at(1) : at(2);
    case "coalesce": return coalesce(args.length, at);
    case "eq":       return ops.equal(at(0), at(1));
    case "neq":      return !ops.equal(at(0), at(1));
    case "lt":       return ops.lessThan(at(0), at(1));
    case "lte":      return ops.lessThanOrEqual(at(0), at(1));
    case "gt":       return ops.greaterThan(at(0), at(1));
    case "gte":      return ops.greaterThanOrEqual(at(0), at(1));
    case "add":
    case "sub":
    case "mul":
    case "div":
    case "mod":      return ops.arithmetic(expr.op, at(0), at(1));
    case "neg":      return ops.negate(at(0));
    case "truthy":   return ops.truthy(at(0));
    case "empty":    return ops.empty(at(0));
    case "matches":  return ops.matches(at(0), at(1), at(2));
    case "toNumber": return ops.toNumber(at(0));
    case "toDate":   return ops.toDate(at(0));
    case "toBoolean":return ops.toBoolean(at(0));
    case "toText":   return ops.toText(at(0));
    case "toArray":  return ops.toArray(at(0));
    case "finite":   return ops.finite(at(0));
    case "order":    return ops.order(at(0), at(1));
    default:         return assertNever(expr.op, "operator");
  }
}

// The first operand that is not missing, evaluated left to right; the last one otherwise.
function coalesce(count: number, at: (index: number) => unknown): unknown {
  for (let index = 0; index < count - 1; index++) {
    const value = at(index);
    if (value !== null && value !== undefined) return value;
  }
  return at(count - 1);
}

export function isPromise(value: unknown): value is Promise<unknown> {
  return typeof value === "object" && value !== null && typeof (value as { then?: unknown }).then === "function";
}
