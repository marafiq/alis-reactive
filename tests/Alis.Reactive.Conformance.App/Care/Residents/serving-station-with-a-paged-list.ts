const servingStation = {
  plates: { result: ["Soup", "Fish pie", "Rice pudding"], count: 3 },
};

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

provide("serving-station", servingStation);
