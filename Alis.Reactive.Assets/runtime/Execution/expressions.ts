import { operated } from "../Conditions/index";
import { assertNever, notBuilt, type Expression } from "../Schema/index";
import { called, converted, got, heldOrNull, value, type Read } from "../ValuesAndSources/index";
import type { Context } from "./context";
import { read } from "./data";
import { located } from "./objects";

export function evaluate(expression: Expression, context: Context): Read {
  switch (expression.kind) {
    case "lit":
      return value(expression.value);
    case "obj":
      return value(located(context, expression.id).object);
    case "ref":
      return bound(expression.name, context);
    case "fn":
      return value(callable(expression.params, expression.body, context));
    case "signal":
      return value(context.signal);
    case "as":
      return converted(evaluate(expression.expr, context), expression.shape, context.plan.culture, context.plan.timeZone);
    case "get":
      return got(evaluate(expression.target, context), expression.path, expression.members);
    case "data":
      return read(evaluate(expression.target, context), expression.path);
    case "op":
      return operated(expression.op, expression.args.map((arg) => () => evaluate(arg, context)));
    case "call":
      return called(evaluate(expression.target, context), expression.path, expression.members, () => expression.args.map((arg) => heldOrNull(evaluate(arg, context))));
    case "answer":
    case "record":
    case "list":
      throw notBuilt("the expression '" + expression.kind + "'");
    default:
      return assertNever(expression);
  }
}

function bound(name: string, context: Context): Read {
  const held = context.scope.get(name);
  if (held === undefined) throw new Error("The plan reads the name '" + name + "', which no block binds.");
  return typeof held === "function" ? held() : held;
}

function callable(params: readonly string[], body: Expression, context: Context): (...args: unknown[]) => unknown {
  return (...args: unknown[]): unknown => {
    const scope = new Map(context.scope);
    params.forEach((name, position) => scope.set(name, value(args[position])));
    return heldOrNull(evaluate(body, { ...context, scope }));
  };
}
