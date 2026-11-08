import type { CallExpression, DoStatement, IfStatement, LetStatement, PathStep, SetStatement, Statement } from "./plan";
import { callee, called, evaluate, memberHolder, objectHolding, walk, type Bound, type Context } from "./evaluate";
import { assertNever, heldOrNull, nothing, Stop, value, type Read } from "./read";
import { listenTo } from "./objects";
import { requireNoSlotWithin } from "./held-slots";

type Awaited = DoStatement | LetStatement;

type Stepped = Exclude<Statement, IfStatement>;

interface Block {
  readonly statements: readonly Statement[];
  readonly next: number;
  readonly context: Context;
}

type Frames = readonly Block[];

type Step =
  | { readonly kind: "completed"; readonly context: Context }
  | { readonly kind: "opened"; readonly block: Block }
  | { readonly kind: "waits"; readonly statement: Awaited; readonly settled: Promise<Read> };

type Turn =
  | { readonly status: "waiting"; readonly below: Frames; readonly top: Block; readonly statement: Awaited; readonly settled: Promise<Read> }
  | { readonly status: "ended" };

export function begin(statements: readonly Statement[], context: Context, stopped: (stop: Stop) => void): void {
  drive([{ statements, next: 0, context }], stopped);
}

function drive(frames: Frames, stopped: (stop: Stop) => void): void {
  let turn: Turn;
  try {
    turn = ranUntilItWaits(frames);
  } catch (reason) {
    ended(reason, stopped);
    return;
  }
  if (turn.status === "ended") return;
  const { below, top, statement, settled } = turn;
  void settled.then(
    (read) => drive([...below, { ...top, context: after(statement, top.context, read) }], stopped),
    (reason: unknown) => ended(reason, stopped),
  );
}

function ended(reason: unknown, stopped: (stop: Stop) => void): void {
  if (!(reason instanceof Stop)) throw reason;
  stopped(reason);
}

function ranUntilItWaits(start: Frames): Turn {
  let frames = start;
  for (;;) {
    const top = frames[frames.length - 1];
    if (top === undefined) return { status: "ended" };
    const below = frames.slice(0, -1);
    const statement = top.statements[top.next];
    if (statement === undefined) {
      frames = below;
      continue;
    }
    const step = stepped(statement, top.context);
    const advanced: Block = { ...top, next: top.next + 1 };
    switch (step.kind) {
      case "completed":
        frames = [...below, { ...advanced, context: step.context }];
        break;
      case "opened":
        frames = [...below, advanced, step.block];
        break;
      case "waits":
        return { status: "waiting", below, top: advanced, statement: step.statement, settled: step.settled };
      default:
        return assertNever(step);
    }
  }
}

function stepped(statement: Statement, context: Context): Step {
  if (awaits(statement)) return { kind: "waits", statement, settled: settle(statement, context) };
  if (statement.kind === "if") {
    const branch = holds(evaluate(statement.when, context)) ? statement.then : statement.else;
    return { kind: "opened", block: { statements: branch, next: 0, context } };
  }
  return { kind: "completed", context: step(statement, context) };
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
    if (reason instanceof Stop) throw reason;
    throw new Stop(callee(call) + " rejected: " + (reason instanceof Error ? reason.message : String(reason)));
  };
}

function after(statement: Awaited, context: Context, read: Read): Context {
  return statement.kind === "let" ? bound(context, statement.name, read) : context;
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
  const holder = writtenTo(statement, parent, last);
  if (statement.content) requireNoSlotWithin(elementWritten(holder));
  const written = Reflect.set(holder, last, heldOrNull(evaluate(statement.value, context)));
  if (!written) throw new Stop("The member '" + String(last) + "' refused the write.");
}

function elementWritten(holder: object): Node {
  if (!(holder instanceof Node)) throw new Error("The plan replaces the content of an object that is no element.");
  return holder;
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
