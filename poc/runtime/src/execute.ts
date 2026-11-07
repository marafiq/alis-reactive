import type { CallExpression, DoStatement, IfStatement, LetStatement, PathStep, SetStatement, Statement } from "./plan";
import { callee, called, evaluate, memberHolder, objectHolding, walk, type Bound, type Context } from "./evaluate";
import { assertNever, heldOrNull, nothing, Stop, value, type Read } from "./read";
import { listenTo } from "./objects";

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
      const branch = execute(holds(evaluate(statement.when, current)) ? statement.then : statement.else, current);
      if (branch.kind === "finished") continue;
      const rest = statements.slice(position + 1);
      const before = current;
      return { kind: "awaiting", settled: branch.settled.then(() => carried(execute(rest, before))) };
    }
    current = step(statement, current);
  }
  return finished;
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
    throw new Stop(callee(call) + " rejected: " + (reason instanceof Error ? reason.message : String(reason)));
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
      return bound(context, statement.name, atItsLine(statement, context));
    case "listen":
      listenTo(statement, context);
      return context;
    default:
      return assertNever(statement);
  }
}

function atItsLine(statement: LetStatement, context: Context): Bound {
  switch (statement.binds) {
    case "line":
      return evaluate(statement.expr, context);
    case "read":
      return { kind: "deferred", expression: statement.expr, context };
    default:
      return assertNever(statement.binds);
  }
}

function bound(context: Context, name: string, read: Bound): Context {
  const scope = new Map(context.scope);
  scope.set(name, read);
  return { ...context, scope };
}

function set(statement: SetStatement, context: Context): void {
  const last = statement.path[statement.path.length - 1];
  if (last === undefined) throw new Error("The plan writes a set with no member to write.");
  const parent = walk(evaluate(statement.target, context), statement.path.slice(0, -1), "required");
  const written = Reflect.set(writtenTo(statement, parent, last), last, heldOrNull(evaluate(statement.value, context)));
  if (!written) throw new Stop("The member '" + String(last) + "' refused the write.");
}

function writtenTo(statement: SetStatement, parent: Read, last: PathStep): object {
  switch (statement.members) {
    case "required":
      return memberHolder(parent, last);
    case "optional":
      return objectHolding(parent, last);
    default:
      return assertNever(statement.members);
  }
}
