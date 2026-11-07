import type { CallExpression, DoStatement, IfStatement, LetStatement, SetStatement, Statement } from "./plan";
import { called, evaluate, holderOf, methodName, walk, type Context } from "./evaluate";
import { assertNever, heldOrNull, nothing, Stop, value, type Read } from "./read";

export type Run = { readonly kind: "finished" } | { readonly kind: "awaiting"; readonly settled: Promise<void> };

const finished: Run = { kind: "finished" };

type Awaited = DoStatement | LetStatement;

type Stepped = Exclude<Statement, IfStatement>;

export function execute(statements: readonly Statement[], context: Context): Run {
  let current = context;
  for (const [position, statement] of statements.entries()) {
    if (awaits(statement)) {
      const rest = statements.slice(position + 1);
      const before = current;
      const settled = settle(statement, before).then((read) => carried(execute(rest, after(statement, before, read))));
      return { kind: "awaiting", settled };
    }
    if (statement.kind === "if") {
      const branch = execute(chosen(statement, current), current);
      if (branch.kind === "finished") continue;
      const rest = statements.slice(position + 1);
      const before = current;
      return { kind: "awaiting", settled: branch.settled.then(() => carried(execute(rest, before))) };
    }
    current = step(statement, current);
  }
  return finished;
}

function chosen(statement: IfStatement, context: Context): readonly Statement[] {
  const arm = statement.arms.find((candidate) => holds(evaluate(candidate.when, context)));
  return arm === undefined ? statement.else : arm.then;
}

function holds(read: Read): boolean {
  return read.kind === "value" && read.value === true;
}

function awaits(statement: Statement): statement is Awaited {
  return (statement.kind === "do" || statement.kind === "let") && statement.await;
}

function settle(statement: Awaited, context: Context): Promise<Read> {
  const expression = statement.expr;
  if (expression.kind !== "call") return Promise.resolve(evaluate(expression, context));
  const result = called(expression, context);
  if (result.kind === "absent") return Promise.resolve(nothing);
  return Promise.resolve(result.held).then(value, rejected(expression));
}

function rejected(call: CallExpression): (reason: unknown) => never {
  return (reason) => {
    throw new Stop("The method '" + String(methodName(call)) + "' rejected: " + (reason instanceof Error ? reason.message : String(reason)));
  };
}

function after(statement: Awaited, context: Context, read: Read): Context {
  return statement.kind === "let" ? bound(context, statement.name, read) : context;
}

function carried(run: Run): Promise<void> {
  return run.kind === "finished" ? Promise.resolve() : run.settled;
}

function step(statement: Stepped, context: Context): Context {
  switch (statement.kind) {
    case "set":
      set(statement, context);
      return context;
    case "do":
      evaluate(statement.expr, context);
      return context;
    case "let":
      return bound(context, statement.name, evaluate(statement.expr, context));
    default:
      return assertNever(statement);
  }
}

function bound(context: Context, name: string, read: Read): Context {
  const scope = new Map(context.scope);
  scope.set(name, read);
  return { ...context, scope };
}

function set(statement: SetStatement, context: Context): void {
  const last = statement.path[statement.path.length - 1];
  if (last === undefined) throw new Error("The plan writes a set with no member to write.");
  const parent = walk(evaluate(statement.target, context), statement.path.slice(0, -1), statement.members);
  const holder = holderOf(parent, last, statement.members);
  if (holder.kind === "absent") return;
  const written = Reflect.set(holder.object, last, heldOrNull(evaluate(statement.value, context)));
  if (!written) throw new Stop("The member '" + String(last) + "' refused the write.");
}
