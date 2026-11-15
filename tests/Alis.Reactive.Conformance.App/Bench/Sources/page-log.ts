const pageLog = {
  log(text: string | null): void {
    const line = document.createElement("li");
    line.textContent = text;
    elementOf("page-log").append(line);
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

provide("pageLog", pageLog);
