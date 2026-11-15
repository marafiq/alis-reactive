import { decimalsCompared } from "../ArrayLambdas/index";
import { nothing, value, type Read } from "../ValuesAndSources/index";

export function decimalsEqual(left: Read, right: Read): Read {
  if (left.kind === "nothing" || right.kind === "nothing") return nothing;
  if (left.value === null || right.value === null) return value(left.value === null && right.value === null);
  return value(decimalsCompared(left.value, right.value));
}
