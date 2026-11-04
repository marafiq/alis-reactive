import type { CallExpr, Expr, OpExpr, RecordExpr } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { Environment } from "./environment";
import { call, read } from "./paths";
import { isMissing, shaped } from "./shape";
import * as ops from "./operators";

export interface Objects {
  object(id: string): unknown;
}

export interface Scope {
  readonly env: Environment;
  readonly objects: Objects;
}

export function childScope(scope: Scope): Scope {
  return { env: scope.env.child(), objects: scope.objects };
}

export function rootScope(objects: Objects): Scope {
  return { env: Environment.root(), objects };
}

export function evaluate(expr: Expr, scope: Scope): unknown {
  switch (expr.kind) {
    case "lit":    return expr.value;
    case "obj":    return scope.objects.object(expr.id);
    case "ref":    return scope.env.lookup(expr.name);
    case "get":    return read(evaluate(expr.target, scope), expr.path, expr.members, labelOf(expr.target));
    case "as":     return shaped(evaluate(expr.expr, scope), expr.shape);
    case "call":   return evaluateCall(expr, scope, false);
    case "fn":     return closure(expr.params, expr.body, scope);
    case "record": return record(expr, scope);
    case "list":   return expr.items.map(item => evaluate(item, scope));
    case "op":     return operate(expr, scope);
    default:       return assertNever(expr, "expression");
  }
}

export function labelOf(target: Expr): string {
  return target.kind === "obj" ? `#${target.id}` : target.kind === "ref" ? target.name : `(${target.kind})`;
}

export function evaluateStatement(expr: Expr, scope: Scope): unknown {
  return expr.kind === "call" ? evaluateCall(expr, scope, true) : evaluate(expr, scope);
}

function evaluateCall(expr: CallExpr, scope: Scope, mayReturnPromise: boolean): unknown {
  const target = evaluate(expr.target, scope);
  const args = expr.args.map(arg => evaluate(arg, scope));
  const label = labelOf(expr.target);
  const result = call(target, expr.path, args, expr.members, label);
  if (isPromise(result) && !mayReturnPromise) throw unawaitedPromise(label, expr.path);
  return result;
}

function unawaitedPromise(label: string, path: readonly string[]): Error {
  const member = path.length === 0 ? label : `${label}.${path.join(".")}`;
  return new Error(`[alis] ${member} returned a promise where the plan reads a value; declare it with MethodAsync`);
}

function closure(params: readonly string[], body: Expr, scope: Scope): (...args: unknown[]) => unknown {
  return (...args: unknown[]) => {
    const env = scope.env.child();
    params.forEach((name, index) => env.bind(name, args[index]));
    return evaluate(body, { env, objects: scope.objects });
  };
}

function record(expr: RecordExpr, scope: Scope): Record<string, unknown> {
  const output: Record<string, unknown> = {};
  for (const field of expr.fields) output[field.name] = evaluate(field.value, scope);
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
    case "mod":      return ops.arithmetic(expr.op, at(0), at(1));
    case "neg":      return ops.negate(at(0));
    case "truthy":   return ops.truthy(at(0));
    case "empty":    return ops.empty(at(0));
    case "matches":  return ops.matches(at(0), at(1), at(2));
    case "toNumber": return shaped(at(0), { kind: "number" });
    case "toDate":   return shaped(at(0), { kind: "date" });
    case "toBoolean":return shaped(at(0), { kind: "boolean" });
    case "toText":   return shaped(at(0), { kind: "string" });
    case "toArray":  return ops.toArray(at(0));
    case "finite":   return ops.finite(at(0));
    case "order":    return ops.order(at(0), at(1));
    case "merge":    return ops.merge(args.map((_, index) => at(index)));
    default:         return assertNever(expr.op, "operator");
  }
}

function coalesce(count: number, at: (index: number) => unknown): unknown {
  for (let index = 0; index < count - 1; index++) {
    const value = at(index);
    if (!isMissing(value)) return value;
  }
  return at(count - 1);
}

export function isPromise(value: unknown): value is Promise<unknown> {
  return typeof value === "object" && value !== null && typeof (value as { then?: unknown }).then === "function";
}
