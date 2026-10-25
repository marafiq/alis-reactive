// Components the sandbox brings, constructed the way any app constructs its widgets: the app owns
// construction and destruction, the framework only finds the object (BroughtComponents/*.cs).
// alis:mount fires before the framework wires a piece of HTML (the page at boot, a partial when it
// is injected); alis:unmount fires before the framework removes it.

import TomSelect from "tom-select";
import "tom-select/dist/css/tom-select.css";
import { VisitPlanner, visitPlannerHosts } from "./visit-planner";
import { Thermostat, thermostatHosts } from "./thermostat";
import "./house-confirm";

interface MountEventDetail {
  readonly root: HTMLElement;
}

interface TomSelectHost extends HTMLSelectElement {
  tomselect?: TomSelect;
}

function tomSelectHosts(root: HTMLElement): TomSelectHost[] {
  return Array.from(root.querySelectorAll<TomSelectHost>("select.tom-select"));
}

// Dropdowns open in <body> so they float above cards; that is also why a removed select must be
// destroyed on alis:unmount — its dropdown lives outside the HTML the framework removes.
document.addEventListener("alis:mount", event => {
  const { root } = (event as CustomEvent<MountEventDetail>).detail;
  for (const host of tomSelectHosts(root)) {
    if (host.tomselect === undefined) new TomSelect(host, { allowEmptyOption: false, dropdownParent: "body" });
  }
  for (const host of visitPlannerHosts(root)) {
    if (host.visitPlanner === undefined) new VisitPlanner(host);
  }
  for (const host of thermostatHosts(root)) {
    if (host.thermostat === undefined) new Thermostat(host);
  }
});

document.addEventListener("alis:unmount", event => {
  const { root } = (event as CustomEvent<MountEventDetail>).detail;
  for (const host of tomSelectHosts(root)) {
    host.tomselect?.destroy();
  }
  for (const host of visitPlannerHosts(root)) {
    host.visitPlanner?.destroy();
  }
  for (const host of thermostatHosts(root)) {
    host.thermostat?.destroy();
  }
});
