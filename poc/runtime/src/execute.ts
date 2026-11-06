import type { SetStatement, Statement } from "./plan";
import { evaluate, holderOf, walk, type Context } from "./evaluate";
import { assertNever, heldOrNull, Stop } from "./read";

export function execute(statements: readonly Statement[], context: Context): void {
  for (const statement of statements) step(statement, context);
}

function step(statement: Statement, context: Context): void {
  switch (statement.kind) {
    case "set":
      return set(statement, context);
    default:
      return assertNever(statement.kind);
  }
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
