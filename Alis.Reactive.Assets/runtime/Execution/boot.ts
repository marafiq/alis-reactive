import { page } from "../OwnershipAndState/index";
import { provide } from "../Plugins/index";
import { documentPlugin, started } from "../TriggersAndPipelines/index";
import { plansWithin } from "./plans";
import { start } from "./start";

export function boot(): void {
  provide("alis.document", () => documentPlugin);
  for (const plan of plansWithin(document)) start(plan, page);
  started();
}
