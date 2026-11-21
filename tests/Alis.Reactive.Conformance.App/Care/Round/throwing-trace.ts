interface CareTraceStore {
  ended(span: unknown): void;
}

const store: CareTraceStore = {
  ended(): void {
    throw new Error("The care home's trace store refused the span.");
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

provide("trace", store);
