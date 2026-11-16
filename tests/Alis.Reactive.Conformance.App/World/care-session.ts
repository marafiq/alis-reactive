interface CareSession {
  renew(): Promise<void>;
  flush(): Promise<void>;
}

async function answered(endpoint: string): Promise<void> {
  const response = await fetch(endpoint, { method: "POST" });
  if (!response.ok) throw new Error("The session endpoint refused with " + String(response.status) + ": " + endpoint);
}

const session: CareSession = {
  renew(): Promise<void> {
    return answered("/world/session/renew");
  },
  flush(): Promise<void> {
    return answered("/world/session/flush");
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

provide("session", session);
