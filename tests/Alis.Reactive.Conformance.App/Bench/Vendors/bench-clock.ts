type Listener = () => void;

const listeners = new Map<string, Set<Listener>>();

function listenersOf(event: string): Set<Listener> {
  const held = listeners.get(event);
  if (held !== undefined) return held;
  const added = new Set<Listener>();
  listeners.set(event, added);
  return added;
}

let offThrows = false;

const benchClock = {
  on(event: string, listener: Listener): void {
    listenersOf(event).add(listener);
  },
  off(event: string, listener: Listener): void {
    if (offThrows) throw new Error("The bench clock's off is broken, so its listener of '" + event + "' stays.");
    listenersOf(event).delete(listener);
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

function elementOf(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Error("The page holds no element '" + id + "'.");
  return element;
}

function whenClicked(id: string, act: () => void): void {
  elementOf(id).addEventListener("click", act);
}

provide("bench-clock", benchClock);

whenClicked("break-off", () => {
  offThrows = true;
});

whenClicked("tick", () => {
  for (const listener of [...listenersOf("tick")]) listener();
});
