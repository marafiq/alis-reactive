import { assertNever, Stop } from "./read";

interface Provided {
  readonly name: string;
  readonly instance: unknown;
}

interface ProvidedQueue {
  push(...provided: Provided[]): number;
}

interface PluginWindow extends Window {
  __alisPlugins?: Provided[] | ProvidedQueue;
}

type Registration = { readonly kind: "provided"; readonly instance: unknown } | { readonly kind: "twice" };

const registry = new Map<string, Registration>();

const twice: Registration = { kind: "twice" };

export function openRegistry(): void {
  const host = window as PluginWindow;
  const queued = host.__alisPlugins;
  host.__alisPlugins = {
    push: (...provided: Provided[]) => {
      provided.forEach(register);
      return registry.size;
    },
  };
  if (Array.isArray(queued)) queued.forEach(register);
}

export function plugin(name: string): unknown {
  const registration = registry.get(name);
  if (registration === undefined) throw new Stop("No script provides the plugin '" + name + "'. The page's own code provides it on window.__alisPlugins.");
  switch (registration.kind) {
    case "provided":
      return registration.instance;
    case "twice":
      throw new Stop("Two scripts provide the plugin '" + name + "', and one script provides each plugin.");
    default:
      return assertNever(registration);
  }
}

function register(provided: Provided): void {
  if (registry.has(provided.name)) {
    registry.set(provided.name, twice);
    console.error("[alis] two scripts provide the plugin '" + provided.name + "'; a step that reaches it stops from now on.");
    return;
  }
  registry.set(provided.name, { kind: "provided", instance: provided.instance });
}
