import { pageInputs } from "../InputsAndIds/index";
import { slotsNamedBy, type Owner, type Placed } from "../OwnershipAndState/index";
import { Slots } from "../PartialsAndSlots/index";
import { plugin, provide, readQueue, type PluginRegistration } from "../Plugins/index";
import { reconnectContract, Streams } from "../Realtime/index";
import type { PlanDocument } from "../Schema/index";
import { tracing } from "../Tracing/index";
import { DocumentPlugin, raisesOnClick } from "../TriggersAndPipelines/index";
import { checksLive, pageForms } from "../Validation/index";
import { elementById } from "./objects";
import { scriptsOtherThanPlansWithin } from "./plan-scripts";
import { planReadsWithin, plansWithin } from "./plans";
import { cutsRunsInTheirStep } from "./run";
import { start } from "./start";

declare global {
  interface Window {
    __alisPlugins?: PluginRegistration[];
  }
}

type Announced = "alis:mount" | "alis:unmount";

function announces(event: Announced): (root: Placed) => void {
  return (root) => {
    document.dispatchEvent(new CustomEvent(event, { detail: { root } }));
  };
}

function startsWith(streams: Streams): (plan: PlanDocument, owner: Owner) => void {
  return (plan, owner) => {
    streams.holds(plan, owner);
    start(plan, owner, elementById, plugin, (trace) => tracing.planTrace(trace));
  };
}

function parsed(html: string): DocumentFragment {
  const template = document.createElement("template");
  template.innerHTML = html;
  return template.content;
}

function runsScripts(container: HTMLElement): void {
  for (const inert of scriptsOtherThanPlansWithin(container)) {
    const script = document.createElement("script");
    for (const attribute of inert.attributes) script.setAttribute(attribute.name, attribute.value);
    script.text = inert.text;
    inert.replaceWith(script);
  }
}

export function boot(): void {
  const streams = new Streams({
    opens: (url) => new EventSource(url),
    reconnect: () => reconnectContract(plugin("reconnect", "")),
    logs: (error) => {
      console.error(error);
    },
  });
  const starts = startsWith(streams);
  const documentPlugin = new DocumentPlugin(document, location);
  raisesOnClick(document, documentPlugin);
  checksLive(document);
  const slots = new Slots<HTMLElement, DocumentFragment>({
    elementById,
    parsed,
    plansIn: planReadsWithin,
    replaces: (container, content) => {
      container.replaceChildren(content);
    },
    empties: (container) => {
      container.replaceChildren();
    },
    runsScripts,
    mounts: announces("alis:mount"),
    unmounts: announces("alis:unmount"),
    starts,
    started: () => {
      documentPlugin.started();
    },
    cuts: cutsRunsInTheirStep,
    logs: (error) => {
      console.error(error);
    },
  });
  provide("alis.document", () => documentPlugin);
  provide("alis.model", () => pageInputs);
  provide("alis.form", () => pageForms);
  provide("alis.slot", (key) => slots.of(key));
  provide("alis.stream", (key) => streams.of(key));
  readQueue(window.__alisPlugins ??= []);
  announces("alis:mount")(document.body);
  const found = plansWithin(document);
  const named = new Set(found.flatMap(({ plan }) => slotsNamedBy(plan)));
  for (const { plan, script } of found) starts(plan, slots.ownerAround(script, named));
  documentPlugin.started();
}
