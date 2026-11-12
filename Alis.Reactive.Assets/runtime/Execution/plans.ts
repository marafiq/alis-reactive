import { assertNever, readPlan, type PlanDocument } from "../Schema/index";
import { planScriptsWithin } from "./plan-scripts";

export function plansWithin(root: ParentNode): PlanDocument[] {
  const plans: PlanDocument[] = [];
  for (const script of planScriptsWithin(root)) {
    const read = readPlan(script.text);
    switch (read.kind) {
      case "plan":
        plans.push(read.plan);
        break;
      case "refused":
        console.error(read.error);
        break;
      default:
        assertNever(read);
    }
  }
  return plans;
}
