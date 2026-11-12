import { assertNever, notBuilt, type Expression } from "../Schema/index";
import { heldOrNull, value, type Read } from "../ValuesAndSources/index";
import type { Context } from "./context";
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
    case "get":
    case "data":
    case "as":
    case "call":
    case "answer":
    case "record":
    case "list":
    case "op":
    case "signal":
      throw notBuilt("the expression '" + expression.kind + "'");
    default:
      return assertNever(expression);
  }
}

function bound(name: string, context: Context): Read {
  const held = context.scope.get(name);
  if (held === undefined) throw new Error("The plan reads the name '" + name + "', which no block binds.");
  return held;
}

function callable(params: readonly string[], body: Expression, context: Context): (...args: unknown[]) => unknown {
  return (...args: unknown[]): unknown => {
    const scope = new Map(context.scope);
    params.forEach((name, position) => scope.set(name, value(args[position])));
    return heldOrNull(evaluate(body, { ...context, scope }));
  };
}
