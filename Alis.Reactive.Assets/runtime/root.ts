// The runtime's entry. It registers the framework's own modules, takes the plugins queued in
// window.__alisPlugins (the app's, and the framework's own components'), announces the page's HTML
// with alis:mount so every component is constructed, then starts the page's owner with every plan
// script on the page (grammar.md 3.5) and marks the page booted.

import { registerModule, registerPlugin } from "./objects/modules";
import { Owner } from "./objects/owner";
import { discoverPlans } from "./plans/discover";
import { announceMount } from "./plans/mount-events";
import { startOwner } from "./plans/start";
import { documentModule } from "./modules/document";
import { httpModule } from "./modules/http";
import { modelModule } from "./modules/model";
import { formModule } from "./modules/form/form";
import { slotModule } from "./modules/slot";
import { streamModule } from "./realtime/stream";
import { hubModule } from "./realtime/hub";
import { scope } from "./diagnostics/trace";

const log = scope("boot");

interface PendingPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface RuntimeWindow extends Window {
  __alisPlugins?: PendingPlugin[];
  __alisReactiveBoot?: { readonly booted: true; readonly planId: string };
}

registerModule("alis.document", documentModule);
registerModule("alis.http", httpModule);
registerModule("alis.model", modelModule);
registerModule("alis.form", formModule);
registerModule("alis.slot", slotModule);
registerModule("alis.stream", streamModule);
registerModule("alis.hub", hubModule);

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", start, { once: true });
} else {
  start();
}

function start(): void {
  const runtimeWindow = window as RuntimeWindow;
  for (const plugin of runtimeWindow.__alisPlugins ?? []) registerPlugin(plugin.name, plugin.instance);
  delete runtimeWindow.__alisPlugins;

  announceMount(document.body);

  const plans = discoverPlans(document, false);
  const planId = plans[0]?.planId ?? "";
  log.info("booting", { plans: plans.map(plan => plan.planId) });
  startOwner(Owner.page(), plans);

  document.documentElement.dataset["alisBooted"] = "true";
  runtimeWindow.__alisReactiveBoot = { booted: true, planId };
  document.dispatchEvent(new CustomEvent("alis:booted", { detail: { planId } }));
  log.info("booted", { planId });
}
