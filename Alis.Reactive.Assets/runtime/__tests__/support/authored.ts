import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import type { ModelPluginData, PlanDocument } from "../../types/index";

const goldens = resolve(process.cwd(), "..", "tests", "Alis.Reactive.PlanTests", "Goldens");

export function authored(fixture: string, test: string): PlanDocument {
  return JSON.parse(readFileSync(resolve(goldens, fixture, `${test}.json`), "utf8")) as PlanDocument;
}

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

export function type(input: HTMLElement, value: string): void {
  (input as HTMLInputElement).value = value;
  input.dispatchEvent(new Event("change"));
}
