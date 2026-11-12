import { Stop } from "../ValuesAndSources/index";

type ObjectFor = (key: string) => unknown;

const provided = new Map<string, ObjectFor>();

export function provide(name: string, objectFor: ObjectFor): void {
  provided.set(name, objectFor);
}

export function plugin(name: string, key: string): unknown {
  const objectFor = provided.get(name);
  if (objectFor === undefined) throw new Stop("No script provides the plugin '" + name + "'.");
  return objectFor(key);
}
