import {
  assertNever,
  notBuilt,
  type DoStatement,
  type LetStatement,
  type PathStep,
  type RequestStatement,
  type SetMembers,
  type SetStatement,
  type Statement,
} from "../Schema/index";
import { heldOrNull, Stop, type Read } from "../ValuesAndSources/index";
import { bind, isAnObject, type Context } from "./context";
import { evaluate } from "./expressions";

export type Step =
  | { readonly kind: "completed"; readonly context: Context }
  | { readonly kind: "block"; readonly statements: readonly Statement[] }
  | { readonly kind: "blocks"; readonly blocks: readonly (readonly Statement[])[] }
  | { readonly kind: "request"; readonly statement: RequestStatement }
  | { readonly kind: "wait"; readonly on: Promise<unknown>; readonly settled: (read: Read) => Context }
  | { readonly kind: "stop"; readonly reason: string };

export function execute(statement: Statement, context: Context): Step {
  switch (statement.kind) {
    case "set":
      write(statement, context);
      return { kind: "completed", context };
    case "do":
      return done(statement, context);
    case "let":
      return bound(statement, context);
    case "if":
      return { kind: "block", statements: holds(evaluate(statement.when, context)) ? statement.then : statement.else };
    case "all":
      return { kind: "blocks", blocks: statement.blocks };
    case "request":
      return { kind: "request", statement };
    case "stop":
      return { kind: "stop", reason: statement.reason };
    case "listen":
      throw notBuilt("the statement '" + statement.kind + "'");
    default:
      return assertNever(statement);
  }
}

function done(statement: DoStatement, context: Context): Step {
  const read = evaluate(statement.expr, context);
  if (!statement.await) return { kind: "completed", context };
  return { kind: "wait", on: Promise.resolve(heldOrNull(read)), settled: () => context };
}

function bound(statement: LetStatement, context: Context): Step {
  switch (statement.binds) {
    case "line": {
      const read = evaluate(statement.expr, context);
      if (!statement.await) return { kind: "completed", context: bind(context, statement.name, read) };
      return { kind: "wait", on: Promise.resolve(heldOrNull(read)), settled: (settled) => bind(context, statement.name, settled) };
    }
    case "read":
      throw notBuilt("a let bound where its name is read");
    default:
      return assertNever(statement.binds);
  }
}

function holds(when: Read): boolean {
  return when.kind === "value" && when.value === true;
}

function write(statement: SetStatement, context: Context): void {
  const [member, ...below] = statement.path;
  if (member === undefined || below.length > 0) throw notBuilt("a set at a path of " + String(statement.path.length) + " steps");
  const holder = holderOf(evaluate(statement.target, context), member, statement.members);
  const written = Reflect.set(holder, member, heldOrNull(evaluate(statement.value, context)));
  if (!written) throw new Stop("The member '" + String(member) + "' refused the write.");
}

function holderOf(target: Read, member: PathStep, members: SetMembers): object {
  switch (members) {
    case "required": {
      if (target.kind === "nothing" || !isAnObject(target.value)) throw new Stop("Nothing holds the member '" + String(member) + "'.");
      if (!(member in target.value)) throw new Stop("The member '" + String(member) + "' is not on the object that should hold it.");
      return target.value;
    }
    case "optional":
      throw notBuilt("a set of an optional member");
    default:
      return assertNever(members);
  }
}
