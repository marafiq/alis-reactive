// The one evaluator. Expressions are synchronous (grammar.md 3.3): only statements await, so a value
// is computed here without ever waiting. An async member's call returns its promise, which the
// awaiting statement settles; the result is shaped when it arrives.

import type { Expr, OpExpr, RecordExpr, Shape } from "../types/index";
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
    case "call":   return invoke(expr.target, expr.path, expr.args, expr.argShapes, expr.shape, scope);
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

function invoke(
  target: Expr,
  path: readonly string[],
  argExprs: readonly Expr[],
  argShapes: readonly Shape[],
  shape: Shape,
  scope: Scope,
): unknown {
  const receiver = evaluate(target, scope);
  const args = argExprs.map((arg, index) => shapeArgument(evaluate(arg, scope), argShapes[index]));
  const result = call(receiver, path, args, modeOf(target), labelOf(target));
  if (isPromise(result)) return result.then(value => shaped(value, shape));
  return shaped(result, shape);
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
    case "and":      return ops.truthy(at(0)) && ops.truthy(at(1));
    case "or":       return ops.truthy(at(0)) || ops.truthy(at(1));
    case "not":      return !ops.truthy(at(0));
    case "cond":     return ops.truthy(at(0)) ? at(1) : at(2);
    case "coalesce": {
      const left = at(0);
      return left === null || left === undefined ? at(1) : left;
    }
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
    case "matches":  return ops.matches(at(0), at(1), args.length > 2 ? at(2) : "");
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

export function isPromise(value: unknown): value is Promise<unknown> {
  return typeof value === "object" && value !== null && typeof (value as { then?: unknown }).then === "function";
}
