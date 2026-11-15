import { operated } from "../Conditions/index";
import { plugin } from "../Plugins/index";
import { assertNever, type CallExpression, type Expression, type FnExpression } from "../Schema/index";
import { called, converted, got, heldOrNull, value, type Read, type Returns } from "../ValuesAndSources/index";
import type { Context } from "./context";
import { read } from "./data";
import { elementById, located } from "./objects";

export function evaluate(expression: Expression, context: Context): Read {
  switch (expression.kind) {
    case "lit":
      return value(expression.value);
    case "obj":
      return value(located(context, expression.id, elementById, plugin).object);
    case "ref":
      return bound(expression.name, context);
    case "fn":
      return value((...args: unknown[]): unknown => applied(expression, args, context));
    case "signal":
      return value(context.signal);
    case "as":
      return converted(evaluate(expression.expr, context), expression.shape, context.plan.culture, context.plan.timeZone);
    case "get":
      return got(evaluate(expression.target, context), expression.path, expression.members);
    case "data":
      return read(evaluate(expression.target, context), expression.path);
    case "op":
      return operated(expression.op, expression.args.map((arg) => () => evaluate(arg, context)), context.plan.culture.collation);
    case "call":
      return call(expression, context, "value");
    case "answer":
      return value((...args: unknown[]): unknown => {
        const scope = new Map(context.scope);
        scope.set(expression.arguments, value(args));
        return heldOrNull(evaluate(expression.body, { ...context, scope }));
      });
    case "record":
      return value(Object.fromEntries(expression.fields.map((field): [string, unknown] => [field.name, heldOrNull(evaluate(field.value, context))])));
    case "list":
      return value(expression.items.map((item) => heldOrNull(evaluate(item, context))));
    default:
      return assertNever(expression);
  }
}

function bound(name: string, context: Context): Read {
  const held = context.scope.get(name);
  if (held === undefined) throw new Error("The plan reads the name '" + name + "', which no block binds.");
  return typeof held === "function" ? held() : held;
}

export function applied(fn: FnExpression, args: readonly unknown[], context: Context): unknown {
  const scope = new Map(context.scope);
  fn.params.forEach((name, position) => scope.set(name, value(args[position])));
  return heldOrNull(evaluate(fn.body, { ...context, scope }));
}

export function awaited(expression: Expression, context: Context): Read {
  return expression.kind === "call" ? call(expression, context, "awaited") : evaluate(expression, context);
}

function call(expression: CallExpression, context: Context, returns: Returns): Read {
  return called(evaluate(expression.target, context), expression.path, expression.members, () => expression.args.map((arg) => heldOrNull(evaluate(arg, context))), returns);
}
