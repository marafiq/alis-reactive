// The runtime's entry. It registers the framework's own plugins, takes the plugins queued in
// window.__alisPlugins (the app's, and the framework's own components'), announces the page's HTML
// with alis:mount so every component is constructed, then starts the page's owner with every plan
// script on the page (grammar.md 3.5) and marks the page booted.

import { pluginOf, registerPlugin } from "./objects/plugins";
import { Owner } from "./objects/owner";
import { discoverPlans } from "./plans/discover";
import { announceMount } from "./plans/mount-events";
import { startOwner } from "./plans/start";
import { documentPlugin } from "./plugins/document";
import { httpPlugin } from "./plugins/http";
import { modelPlugin } from "./plugins/model";
import { formPlugin } from "./plugins/form/form";
import { slotPlugin } from "./plugins/slot";
import { streamPlugin } from "./realtime/stream";
import { hubPlugin } from "./realtime/hub";
import { logger } from "./diagnostics/trace";

const log = logger("boot");

interface PendingPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface RuntimeWindow extends Window {
  __alisPlugins?: PendingPlugin[];
  __alisReactiveBoot?: { readonly booted: true };
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
  const runtimeWindow = window as RuntimeWindow;
  for (const plugin of runtimeWindow.__alisPlugins ?? []) registerPlugin(plugin.name, pluginOf(plugin.name, plugin.instance));
  delete runtimeWindow.__alisPlugins;

  announceMount(document.body);

  const plans = discoverPlans(document, false);
  const planIds = plans.map(plan => plan.planId);
  log.info("booting", { plans: planIds });
  startOwner(Owner.page(), plans);

  document.documentElement.dataset["alisBooted"] = "true";
  runtimeWindow.__alisReactiveBoot = { booted: true };
  document.dispatchEvent(new CustomEvent("alis:booted"));
  log.info("booted", { plans: planIds });
}
