import type { CallExpression, DoStatement, IfStatement, LetStatement, PathStep, RequestStatement, SetStatement, Statement } from "./plan";
import { bind, callee, called, evaluate, memberHolder, objectHolding, walk, type Bound, type Context } from "./evaluate";
import { assertNever, heldOrNull, Stop, value, type Read } from "./read";
import { listenTo } from "./objects";
import { requireNoSlotWithin } from "./held-slots";
import { requestFrame } from "./request";
import type { Frame, Settled, Stepped } from "./run";

type Awaited = DoStatement | LetStatement;

type Plain = Exclude<Statement, IfStatement | RequestStatement>;

export interface Block {
  readonly kind: "block";
  readonly statements: readonly Statement[];
  readonly next: number;
  readonly context: Context;
}

export interface AwaitingCall {
  readonly kind: "call";
  readonly block: Block;
  readonly statement: Awaited;
  readonly call: CallExpression;
}

export function blockStep(block: Block): Stepped {
  const statement = block.statements[block.next];
  if (statement === undefined) return { kind: "goes", top: [] };
  const advanced: Block = { ...block, next: block.next + 1 };
  if (awaits(statement)) return awaitedCall(statement, advanced);
  switch (statement.kind) {
    case "if":
      return { kind: "goes", top: [advanced, branchOf(statement, advanced.context)] };
    case "request":
      return { kind: "goes", top: [advanced, requestFrame(statement, advanced.context)] };
    default:
      return { kind: "goes", top: [{ ...advanced, context: step(statement, advanced.context) }] };
  }
}

export function awaitSettled(waiting: AwaitingCall, settled: Settled): readonly Frame[] {
  switch (settled.kind) {
    case "fulfilled":
      return [{ ...waiting.block, context: after(waiting.statement, waiting.block.context, value(settled.held)) }];
    case "rejected":
      throw rejection(waiting.call, settled.reason);
    default:
      return assertNever(settled);
  }
}

function branchOf(statement: IfStatement, context: Context): Block {
  return { kind: "block", statements: holds(evaluate(statement.when, context)) ? statement.then : statement.else, next: 0, context };
}

function awaitedCall(statement: Awaited, block: Block): Stepped {
  const call = statement.expr;
  if (call.kind !== "call") throw new Error("The plan awaits an expression that is no call.");
  const result = called(call, block.context);
  return { kind: "waits", waiting: { kind: "call", block, statement, call }, on: result.kind === "absent" ? undefined : result.held };
}

function rejection(call: CallExpression, reason: unknown): Stop {
  if (reason instanceof Stop) return reason;
  return new Stop(callee(call) + " rejected: " + (reason instanceof Error ? reason.message : String(reason)));
}

function holds(read: Read): boolean {
  return read.kind === "value" && read.value === true;
}

function awaits(statement: Statement): statement is Awaited {
  return (statement.kind === "do" || statement.kind === "let") && statement.await;
}

function after(statement: Awaited, context: Context, read: Read): Context {
  return statement.kind === "let" ? bind(context, statement.name, read) : context;
}

function step(statement: Plain, context: Context): Context {
  switch (statement.kind) {
    case "set":
      set(statement, context);
      return context;
    case "do":
      evaluate(statement.expr, context);
      return context;
    case "let":
      return bind(context, statement.name, atItsLine(statement, context));
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
