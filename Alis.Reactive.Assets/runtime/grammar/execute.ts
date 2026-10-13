// The six statements (grammar.md 3.2). A block runs synchronously until its first awaited statement
// and continues when that settles; a block with no awaited statement never returns a promise, so a
// payload write before the first await lands before the vendor reads it.

import type { Expr, SetStatement, Statement } from "../types/index";
import { assertNever } from "../shared/assert-never";
import { scope as logScope } from "../diagnostics/trace";
import { toJavaScriptString } from "../shared/javascript-string";
import { childScope, evaluate, evaluateStatement, isPromise, labelOf, modeOf, type Scope } from "./evaluate";
import { write } from "./paths";
import { shaped } from "./shape";

const log = logScope("execute");

export type Completion = void | Promise<void>;

export function executeBlock(statements: readonly Statement[], scope: Scope): Completion {
  return runFrom(statements, 0, scope);
}

function runFrom(statements: readonly Statement[], start: number, scope: Scope): Completion {
  for (let index = start; index < statements.length; index++) {
    const completion = execute(statements[index]!, scope);
    if (isPromise(completion)) return completion.then(() => runFrom(statements, index + 1, scope));
  }
}

function execute(statement: Statement, scope: Scope): Completion {
  switch (statement.kind) {
    case "set": return set(statement, scope);
    case "do":  return perform(statement.expr, statement.await, scope);
    case "let": return bind(statement.name, statement.expr, statement.await, scope);
    case "if":  return branch(statement, scope);
    case "all": return all(statement.blocks, scope);
    case "try": return attempt(statement.body, statement.finally, scope);
    default:    return assertNever(statement, "statement");
  }
}

function set(statement: SetStatement, scope: Scope): void {
  const target = evaluate(statement.target, scope);
  const value = shaped(evaluate(statement.value, scope), statement.shape);
  log.trace("set", { target: traceName(statement.target), path: statement.path, value });
  write(target, statement.path, value, modeOf(statement.target), labelOf(statement.target));
}

function perform(expr: Expr, awaits: boolean, scope: Scope): Completion {
  if (expr.kind === "call") log.trace("call", { target: traceName(expr.target), path: expr.path });
  const result = evaluateStatement(expr, scope);
  if (!isPromise(result)) return;
  if (awaits) return result.then(() => undefined);
  result.catch(error => log.error("call.rejected", { error: toJavaScriptString(error) }));
}

// A trace names a located object by its id, the way the view named it.
function traceName(target: Expr): string {
  return target.kind === "obj" ? target.id : labelOf(target);
}

function bind(name: string, expr: Expr, awaits: boolean, scope: Scope): Completion {
  const value = awaits ? evaluateStatement(expr, scope) : evaluate(expr, scope);
  scope.env.bind(name, value);
}

function branch(statement: Extract<Statement, { kind: "if" }>, scope: Scope): Completion {
  for (const arm of statement.arms) {
    if (evaluate(arm.when, scope)) return executeBlock(arm.then, childScope(scope));
  }
  return executeBlock(statement.else, childScope(scope));
}

// Every block starts; the statement completes when all have settled. A failure is logged and does
// not stop the others (Promise.allSettled).
function all(blocks: readonly (readonly Statement[])[], scope: Scope): Completion {
  const pending: Promise<void>[] = [];
  for (const block of blocks) {
    try {
      const completion = executeBlock(block, childScope(scope));
      if (isPromise(completion)) pending.push(completion);
    } catch (error) {
      log.error("all.block-failed", { error: toJavaScriptString(error) });
    }
  }
  if (pending.length === 0) return;
  return Promise.allSettled(pending).then(results => {
    for (const result of results) {
      if (result.status === "rejected") log.error("all.block-failed", { error: toJavaScriptString(result.reason) });
    }
  });
}

// The body runs in the enclosing block's scope, so its top-level bindings stay visible after the
// try: code after a try runs only when the body completed (grammar.md 3.1).
function attempt(body: readonly Statement[], cleanup: readonly Statement[], scope: Scope): Completion {
  let completion: Completion;
  try {
    completion = executeBlock(body, scope);
  } catch (error) {
    return rethrowAfter(executeBlock(cleanup, childScope(scope)), error);
  }

  if (!isPromise(completion)) return executeBlock(cleanup, childScope(scope));
  return completion.then(
    () => executeBlock(cleanup, childScope(scope)),
    error => rethrowAfter(executeBlock(cleanup, childScope(scope)), error),
  );
}

function rethrowAfter(cleanup: Completion, error: unknown): Completion {
  if (isPromise(cleanup)) return cleanup.then(() => { throw error; });
  throw error;
}
