// The runtime's entry. It registers the framework's own plugins, takes the plugins queued in
// window.__alisPlugins (the app's, and the framework's own components'), announces the page's HTML
// with alis:mount so every component is constructed, then starts the plan scripts on the page under
// their owners (grammar.md 3.5) and marks the page booted.

import type { PlanDocument } from "./types/index";
import { pluginOf, registerPlugin } from "./objects/plugins";
import { Owner } from "./objects/owner";
import { discoverPlans, type DiscoveredPlan } from "./plans/discover";
import { announceMount } from "./plans/mount-events";
import { startOwners, type OwnedPlans } from "./plans/start";
import { documentPlugin } from "./plugins/document";
import { httpPlugin } from "./plugins/http";
import { modelPlugin } from "./plugins/model";
import { formPlugin } from "./plugins/form/form";
import { slotOwner, slotPlugin } from "./plugins/slot";
import { streamPlugin } from "./realtime/stream";
import { hubPlugin } from "./realtime/hub";
import { levelOf, logger, setLevel } from "./diagnostics/trace";

const log = logger("boot");

interface PendingPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface RuntimeWindow extends Window {
  __alisPlugins?: PendingPlugin[];
}

registerPlugin("alis.document", documentPlugin);
registerPlugin("alis.http", httpPlugin);
registerPlugin("alis.model", modelPlugin);
registerPlugin("alis.form", formPlugin);
registerPlugin("alis.slot", slotPlugin);
registerPlugin("alis.stream", streamPlugin);
registerPlugin("alis.hub", hubPlugin);

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", start, { once: true });
} else {
  start();
}

function start(): void {
  setLevel(levelOf(document.documentElement));
  const runtimeWindow = window as RuntimeWindow;
  for (const plugin of runtimeWindow.__alisPlugins ?? []) registerPlugin(plugin.name, pluginOf(plugin.name, plugin.instance));
  delete runtimeWindow.__alisPlugins;

  announceMount(document.body);

  const discovered = discoverPlans(document, false);
  const planIds = discovered.map(({ plan }) => plan.planId);
  log.info("booting", { plans: planIds });
  startOwners(ownersAtBoot(discovered));

  // The one mark of a booted page: state on the root element, readable by whoever arrives later.
  document.documentElement.dataset["alisBooted"] = "true";
  log.info("booted", { plans: planIds });
}

// A plan whose script sits inside an element some plan loads into (an alis.slot a plan names) starts
// under the owner of the innermost such element, so the slot's first load or unload releases it as it
// releases loaded content. Every other plan is the page's. The owners start as loads would have started
// them: the page's first, then each slot's, an outer slot's before the slot inside it.
function ownersAtBoot(discovered: readonly DiscoveredPlan[]): OwnedPlans[] {
  const slotIds = new Set(discovered.flatMap(({ plan }) => slotsNamedBy(plan)));
  const pagePlans: PlanDocument[] = [];
  const plansBySlot = new Map<HTMLElement, PlanDocument[]>();
  for (const { plan, script } of discovered) {
    const slot = innermostSlot(script, slotIds);
    if (slot === null) {
      pagePlans.push(plan);
      continue;
    }
    const slotPlans = plansBySlot.get(slot);
    if (slotPlans) slotPlans.push(plan);
    else plansBySlot.set(slot, [plan]);
  }
  const outerFirst = [...plansBySlot].sort(([one], [other]) => inDocumentOrder(one, other));
  return [
    { owner: Owner.page(), plans: pagePlans },
    ...outerFirst.map(([slot, plans]) => ({ owner: slotOwner(slot.id), plans })),
  ];
}

function slotsNamedBy(plan: PlanDocument): string[] {
  return Object.values(plan.objects).flatMap(locator =>
    locator.kind === "plugin" && locator.name === "alis.slot" ? [locator.key] : []);
}

function innermostSlot(script: HTMLElement, slotIds: ReadonlySet<string>): HTMLElement | null {
  for (let element = script.parentElement; element !== null; element = element.parentElement) {
    if (slotIds.has(element.id)) return element;
  }
  return null;
}

// An element comes before the elements inside it.
function inDocumentOrder(one: HTMLElement, other: HTMLElement): number {
  return one.compareDocumentPosition(other) & Node.DOCUMENT_POSITION_FOLLOWING ? -1 : 1;
}
