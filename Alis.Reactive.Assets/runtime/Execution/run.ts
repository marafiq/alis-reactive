import type { Statement } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import type { Context } from "./context";
import { execute } from "./statements";

export function run(statements: readonly Statement[], context: Context): void {
  try {
    for (const statement of statements) execute(statement, context);
  } catch (stopped) {
    if (!(stopped instanceof Stop)) throw stopped;
    console.error(stopped.reason);
  }
}
