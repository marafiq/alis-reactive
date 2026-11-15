interface AgencyShiftLog {
  entries: number;
}

const log: AgencyShiftLog = { entries: 9 };

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

provide("shiftlog", log);
