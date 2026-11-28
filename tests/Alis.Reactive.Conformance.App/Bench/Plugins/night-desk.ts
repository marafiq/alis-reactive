interface NightClock {
  readonly hour: string;
  onStruck: (() => void) | null;
}

const clock: NightClock = { hour: "12", onStruck: null };

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

provide("nightclock", clock);

whenClicked("strike-midnight", () => {
  clock.onStruck?.();
});
