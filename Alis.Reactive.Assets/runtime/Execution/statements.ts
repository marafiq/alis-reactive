import { heldSlotsAtOrIn } from "../OwnershipAndState/index";
import {
  assertNever,
  type DoStatement,
  type Expression,
  type LetStatement,
  type PathStep,
  type RequestStatement,
  type SetMembers,
  type SetStatement,
  type Statement,
} from "../Schema/index";
import type { Attributes, Traced } from "../Tracing/index";
import { got, heldOrNull, Stop, type Read } from "../ValuesAndSources/index";
import { bind, isAnObject, type Context } from "./context";
import { awaited, evaluate } from "./expressions";
import { listenTo } from "./listen";
import { elementById } from "./objects";

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
      listenTo(statement, context, elementById);
      return { kind: "completed", context };
    default:
      return assertNever(statement);
  }
}

function done(statement: DoStatement, context: Context): Step {
  if (!statement.await) {
    evaluate(statement.expr, context);
    return { kind: "completed", context };
  }
  const method = context.trace.span("alis.method", methodOf(statement.expr));
  return { kind: "wait", on: endsOnSettle(Promise.resolve(heldOrNull(awaited(statement.expr, context))), method), settled: () => context };
}

function bound(statement: LetStatement, context: Context): Step {
  switch (statement.binds) {
    case "line": {
      if (!statement.await) return { kind: "completed", context: bind(context, statement.name, evaluate(statement.expr, context)) };
      const method = context.trace.span("alis.method", methodOf(statement.expr));
      return {
        kind: "wait",
        on: endsOnSettle(Promise.resolve(heldOrNull(awaited(statement.expr, context))), method),
        settled: (settled) => bind(context, statement.name, settled),
      };
    }
    case "read":
      return { kind: "completed", context: bind(context, statement.name, () => evaluate(statement.expr, context)) };
    default:
      return assertNever(statement.binds);
  }
}

function methodOf(expr: Expression): Attributes {
  if (expr.kind !== "call") return {};
  const name = expr.path.at(-1);
  return typeof name === "string" ? { "alis.method": name } : {};
}

function endsOnSettle(on: Promise<unknown>, method: Traced): Promise<unknown> {
  void on.then(
    () => {
      method.end("completed");
    },
    () => {
      method.end("stopped");
    },
  );
  return on;
}

function holds(when: Read): boolean {
  return when.kind === "value" && when.value === true;
}

function write(statement: SetStatement, context: Context): void {
  const holder = got(evaluate(statement.target, context), statement.path.slice(0, -1), statement.members);
  for (const member of statement.path.slice(-1)) {
    const object = holderOf(holder, member, statement.members);
    if (statement.content) requireNoHeldSlotAtOrIn(object);
    const written = Reflect.set(object, member, heldOrNull(evaluate(statement.value, context)));
    if (!written) throw new Stop("The member '" + String(member) + "' refused the write.");
  }
}

function requireNoHeldSlotAtOrIn(element: object): void {
  const [first] = heldSlotsAtOrIn(element, elementById);
  if (first === undefined) return;
  throw new Stop(
    "The step sets the content of an element that is or holds the slot '" +
      first +
      "', which a live plan names or whose content has a live owner, so what the slot loaded would keep running under content it no longer shows. Write the content into an element beside the slot.",
  );
}

function holderOf(holder: Read, member: PathStep, members: SetMembers): object {
  if (holder.kind === "nothing" || !isAnObject(holder.value)) throw new Stop("Nothing holds the member '" + String(member) + "'.");
  switch (members) {
    case "required":
      if (!(member in holder.value)) throw new Stop("The member '" + String(member) + "' is not on the object that should hold it.");
      return holder.value;
    case "optional":
      return holder.value;
    default:
      return assertNever(members);
  }
}
