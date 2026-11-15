import { ownerAround, slotsNamedBy } from "../OwnershipAndState/index";
import { plugin, provide } from "../Plugins/index";
import { documentPlugin, started } from "../TriggersAndPipelines/index";
import { elementById } from "./objects";
import { plansWithin } from "./plans";
import { start } from "./start";

export function boot(): void {
  provide("alis.document", () => documentPlugin);
  const found = plansWithin(document);
  const named = new Set(found.flatMap(({ plan }) => slotsNamedBy(plan)));
  for (const { plan, script } of found) start(plan, ownerAround(script, named), elementById, plugin);
  started();
}
