import { plugin } from "../Plugins/index";
import type { InputEntry } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import type { Places } from "./forms";

const noticeContract = "notice";
const noKey = "";

function onThePage(id: string, named: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Stop(named + " is not on the page.");
  return element;
}

function messageOf(entry: InputEntry): HTMLElement {
  return onThePage(entry.binding.message, "The message element '" + entry.binding.message + "' of input '" + entry.id + "'");
}

function inputOf(entry: InputEntry): HTMLElement {
  return onThePage(entry.id, "The input '" + entry.id + "'");
}

function listOf(messages: readonly string[]): HTMLUListElement {
  const list = document.createElement("ul");
  for (const message of messages) {
    const item = document.createElement("li");
    item.textContent = message;
    list.append(item);
  }
  return list;
}

export const documentPlaces: Places = {
  showMessage(entry: InputEntry, message: string): void {
    const shown = messageOf(entry);
    shown.textContent = message;
    shown.hidden = false;
    inputOf(entry).classList.add(entry.binding.invalidClass);
  },
  clearMessage(entry: InputEntry): void {
    const shown = messageOf(entry);
    shown.textContent = "";
    shown.hidden = true;
    inputOf(entry).classList.remove(entry.binding.invalidClass);
  },
  showSummary(summaryId: string, messages: readonly string[]): void {
    const summary = onThePage(summaryId, "The summary '" + summaryId + "' of a request's failures");
    summary.replaceChildren(listOf(messages));
    summary.hidden = messages.length === 0;
  },
  notice(): unknown {
    return plugin(noticeContract, noKey);
  },
};
