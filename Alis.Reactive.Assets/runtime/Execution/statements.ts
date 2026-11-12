import { assertNever, notBuilt, type PathStep, type SetMembers, type SetStatement, type Statement } from "../Schema/index";
import { heldOrNull, Stop, type Read } from "../ValuesAndSources/index";
import { isAnObject, type Context } from "./context";
import { evaluate } from "./expressions";

export function execute(statement: Statement, context: Context): void {
  switch (statement.kind) {
    case "set":
      write(statement, context);
      return;
    case "do":
    case "let":
    case "if":
    case "all":
    case "request":
    case "listen":
    case "stop":
      throw notBuilt("the statement '" + statement.kind + "'");
    default:
      assertNever(statement);
  }
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
