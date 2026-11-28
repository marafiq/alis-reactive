import { provide } from "./provided";

export interface PluginRegistration {
  readonly name: string;
  readonly instance: unknown;
}

export function readQueue(queue: PluginRegistration[]): void {
  for (const entry of queue) provide(entry.name, () => entry.instance);
  const push = queue.push.bind(queue);
  queue.push = (...entries: PluginRegistration[]): number => {
    for (const entry of entries) provide(entry.name, () => entry.instance);
    return push(...entries);
  };
}
