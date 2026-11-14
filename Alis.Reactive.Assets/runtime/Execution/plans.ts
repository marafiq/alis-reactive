import { assertNever, readPlan, type PlanDocument } from "../Schema/index";
import { planScriptsWithin } from "./plan-scripts";

export interface Found {
  readonly plan: PlanDocument;
  readonly script: HTMLScriptElement;
}

export function plansWithin(root: ParentNode): Found[] {
  const found: Found[] = [];
  for (const script of planScriptsWithin(root)) {
    const read = readPlan(script.text);
    switch (read.kind) {
      case "plan":
        found.push({ plan: read.plan, script });
        break;
      case "refused":
        console.error(read.error);
        break;
      default:
        assertNever(read);
    }
  }
  return found;
}
