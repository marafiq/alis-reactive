import { Stop } from "../ValuesAndSources/index";

type ObjectFor = (key: string) => unknown;
type Provision = { readonly kind: "provided"; readonly objectFor: ObjectFor } | { readonly kind: "duplicate" };
const provided = new Map<string, Provision>();

export function provide(name: string, objectFor: ObjectFor): void {
  provided.set(name, provided.has(name) ? { kind: "duplicate" } : { kind: "provided", objectFor });
}

export function provides(name: string): boolean {
  return provided.has(name);
}

export function plugin(name: string, key: string): unknown {
  const provision = provided.get(name);
  if (provision === undefined) throw new Stop("No script provides the plugin '" + name + "'.");
  if (provision.kind === "duplicate") throw new Stop("More than one script provides the plugin '" + name + "'.");
  return provision.objectFor(key);
}
