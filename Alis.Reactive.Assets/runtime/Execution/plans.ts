import { assertNever, readPlan, type PlanDocument, type PlanRead } from "../Schema/index";
import { planScriptsWithin } from "./plan-scripts";

export interface Found {
  readonly plan: PlanDocument;
  readonly script: HTMLScriptElement;
}

export interface ReadScript {
  readonly read: PlanRead;
  readonly script: HTMLScriptElement;
}

export function planReadsWithin(root: ParentNode): ReadScript[] {
  return planScriptsWithin(root).map((script) => ({ read: readPlan(script.text), script }));
}

export function plansWithin(root: ParentNode): Found[] {
  const found: Found[] = [];
  for (const { read, script } of planReadsWithin(root)) {
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
