import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import type { HubClient } from "./hubs";

const name = "alis.signalr";

const client: HubClient = {
  connection: (url, waits) => new HubConnectionBuilder().withUrl(url).withAutomaticReconnect([...waits]).configureLogging(LogLevel.None).build(),
};

function provide(instance: HubClient): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

provide(client);
