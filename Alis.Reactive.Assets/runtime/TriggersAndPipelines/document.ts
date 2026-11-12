import { notBuilt } from "../Schema/index";

type Listener = () => void;

const ready = "ready";

const waitingForTheStart: Listener[] = [];

export const documentPlugin = {
  addEventListener(event: string, listener: Listener): void {
    if (event !== ready) throw notBuilt("a handler of the document's event '" + event + "'");
    waitingForTheStart.push(listener);
  },

  removeEventListener(event: string, listener: Listener): void {
    if (event !== ready) throw notBuilt("a handler of the document's event '" + event + "'");
    const place = waitingForTheStart.indexOf(listener);
    if (place >= 0) waitingForTheStart.splice(place, 1);
  },
};

export function started(): void {
  for (const listener of waitingForTheStart.splice(0)) listener();
}
