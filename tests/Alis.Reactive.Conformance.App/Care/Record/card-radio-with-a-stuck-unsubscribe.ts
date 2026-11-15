type Listener = () => void;

const listeners = new Map<string, Set<Listener>>();

function listenersOf(event: string): Set<Listener> {
  const held = listeners.get(event);
  if (held !== undefined) return held;
  const added = new Set<Listener>();
  listeners.set(event, added);
  return added;
}

interface CardRadio {
  on(event: string, listener: Listener): void;
  off(event: string): void;
  transmit(): void;
}

const cardRadio: CardRadio = {
  on(event: string, listener: Listener): void {
    listenersOf(event).add(listener);
  },
  off(event: string): void {
    throw new Error("The card radio is stuck, so its listener of '" + event + "' stays.");
  },
  transmit(): void {
    for (const listener of [...listenersOf("call")]) listener();
  },
};

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

provide("card-radio", cardRadio);
