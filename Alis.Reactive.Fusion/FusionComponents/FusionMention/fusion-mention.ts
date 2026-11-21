import { mountsEach } from "../mounted";

const marker = "data-fusion-mention";

function hostIdOf(bridge: HTMLSpanElement): string {
  const written = bridge.getAttribute(marker);
  if (written === null) throw new Error("The mention '" + bridge.id + "' carries no " + marker + ".");
  const start: unknown = JSON.parse(written);
  if (typeof start !== "object" || start === null || !("host" in start) || typeof start.host !== "string") {
    throw new Error("The mention '" + bridge.id + "' names no element EJ2 renders it on.");
  }
  return start.host;
}

function handsOver(bridge: HTMLSpanElement): void {
  const hostId = hostIdOf(bridge);
  const host = document.getElementById(hostId);
  if (host === null) throw new Error("The mention '" + bridge.id + "' finds no element '" + hostId + "' EJ2 renders it on.");
  const instances: unknown = Reflect.get(host, "ej2_instances");
  if (!Array.isArray(instances) || instances.length === 0) {
    throw new Error("The mention '" + bridge.id + "' finds no EJ2 Mention on '" + hostId + "': link ej2.min.js and EJ2's script manager before alis-reactive-fusion.js.");
  }
  Reflect.set(bridge, "ej2_instances", instances);
}

export function mountsMentions(): void {
  mountsEach(marker, HTMLSpanElement, handsOver);
}
