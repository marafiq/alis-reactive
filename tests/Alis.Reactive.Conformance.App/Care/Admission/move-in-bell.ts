let rings = 0;

interface MoveInBell {
  ring(): number;
}

const bell: MoveInBell = {
  ring(): number {
    rings += 1;
    return rings;
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

provide("move-in-bell", bell);
