import type { PlanDocument } from "./plan";

export type PlanRead =
  | { readonly kind: "plan"; readonly plan: PlanDocument }
  | { readonly kind: "refused"; readonly error: string };

interface Versioned {
  readonly version: number;
}

const pageVersion: PlanDocument["version"] = 7;

const runsItsVersion = "this page runs version " + String(pageVersion) + ".";

export function readPlan(text: string): PlanRead {
  let held: unknown;
  try {
    held = JSON.parse(text);
  } catch {
    return refused("This plan's text is not JSON.");
  }
  if (!namesAVersion(held)) return refused("This plan names no plan version; " + runsItsVersion);
  if (!isOfThePagesVersion(held)) return refused("This plan was written for plan version " + String(held.version) + "; " + runsItsVersion);
  return { kind: "plan", plan: held };
}

function namesAVersion(held: unknown): held is Versioned {
  return typeof held === "object" && held !== null && "version" in held && typeof held.version === "number";
}

function isOfThePagesVersion(plan: Versioned): plan is PlanDocument {
  return plan.version === pageVersion;
}

function refused(error: string): PlanRead {
  return { kind: "refused", error };
}
