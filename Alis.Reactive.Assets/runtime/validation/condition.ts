import type { ExecContext, Plan, ValidationCondition } from "../types";
import { evaluateCondition } from "../conditions/conditions";

export function evaluateValidationCondition(
  condition: ValidationCondition,
  plan: Plan,
  ctx?: ExecContext,
): boolean {
  return evaluateCondition(condition, plan, ctx);
}
