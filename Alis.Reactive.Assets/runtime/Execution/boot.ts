import { pageInputs } from "../InputsAndIds/index";
import { ownerAround, slotsNamedBy } from "../OwnershipAndState/index";
import { plugin, provide, readQueue, type PluginRegistration } from "../Plugins/index";
import { documentPlugin, started } from "../TriggersAndPipelines/index";
import { elementById } from "./objects";
import { plansWithin } from "./plans";
import { start } from "./start";

declare global {
  interface Window {
    __alisPlugins?: PluginRegistration[];
  }
}

export function boot(): void {
  provide("alis.document", () => documentPlugin);
  provide("alis.model", () => pageInputs);
  readQueue(window.__alisPlugins ??= []);
  document.dispatchEvent(new CustomEvent("alis:mount", { detail: { root: document.body } }));
  const found = plansWithin(document);
  const named = new Set(found.flatMap(({ plan }) => slotsNamedBy(plan)));
  for (const { plan, script } of found) start(plan, ownerAround(script, named), elementById, plugin);
  started();
}
