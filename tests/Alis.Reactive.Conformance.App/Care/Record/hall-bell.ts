let rings = 0;

interface HallBell {
  ring(): number;
}

const bell: HallBell = {
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

provide("hall-bell", bell);
