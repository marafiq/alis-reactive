// The plans the C# builders wrote: the golden plans of tests/Alis.Reactive.PlanTests, read as the
// view would embed them, with the plain DOM they name put on the page.

import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import type { ModelPluginData, PlanDocument } from "../../types/index";

// Vitest runs from the workspace root (Alis.Reactive.Assets); the goldens live beside the C# tests.
const goldens = resolve(process.cwd(), "..", "tests", "Alis.Reactive.PlanTests", "Goldens");

/** The plan a C# test wrote and reviewed, by its fixture and test name. */
export function authored(fixture: string, test: string): PlanDocument {
  return JSON.parse(readFileSync(resolve(goldens, fixture, `${test}.json`), "utf8")) as PlanDocument;
}

/**
 * Every element the plan locates, on the page: an input where the model binds one, a plain element
 * elsewhere. The page's forms and error slots are the caller's, as a view lays them out.
 */
export function mount(plan: PlanDocument): Record<string, HTMLElement> {
  const inputIds = new Set<string>();
  for (const locator of Object.values(plan.objects)) {
    if (locator.kind === "plugin" && locator.data.kind === "model") {
      for (const id of Object.keys((locator.data as ModelPluginData).inputs)) inputIds.add(id);
    }
  }
  const elements: Record<string, HTMLElement> = {};
  for (const locator of Object.values(plan.objects)) {
    if (locator.kind !== "element") continue;
    const element = document.createElement(inputIds.has(locator.id) ? "input" : "div");
    element.id = locator.id;
    document.body.appendChild(element);
    elements[locator.id] = element;
  }
  return elements;
}

/** Types into an input and commits it, as a person leaving the field does. */
export function type(input: HTMLElement, value: string): void {
  (input as HTMLInputElement).value = value;
  input.dispatchEvent(new Event("change"));
}
