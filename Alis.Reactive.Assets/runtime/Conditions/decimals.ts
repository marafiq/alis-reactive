import { decimalOf, decimalsCompared } from "../ArrayLambdas/index";
import { nothing, value, type Read } from "../ValuesAndSources/index";

export function decimalsEqual(left: Read, right: Read): Read {
  if (left.kind === "nothing" || right.kind === "nothing") return nothing;
  if (left.value === null || right.value === null) return value(left.value === null && right.value === null);
  return value(decimalsCompared(left.value, right.value));
}

export function precisionScaled(number: Read, precision: Read, scale: Read): Read {
  if (precision.kind === "nothing" || typeof precision.value !== "number") throw new Error("The plan's precisionScale carries no precision.");
  if (scale.kind === "nothing" || typeof scale.value !== "number") throw new Error("The plan's precisionScale carries no scale.");
  if (number.kind === "nothing" || number.value === null) return nothing;
  const read = decimalOf(number.value);
  let digits = read.digits < 0n ? -read.digits : read.digits;
  let decimals = read.scale;
  while (decimals > 0 && digits % 10n === 0n) {
    digits /= 10n;
    decimals -= 1;
  }
  const counted = digits === 0n ? 0 : digits.toString().length;
  return value(decimals <= scale.value && counted - decimals <= precision.value - scale.value);
}
