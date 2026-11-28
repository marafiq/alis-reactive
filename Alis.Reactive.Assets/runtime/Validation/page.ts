import { pageInputs } from "../InputsAndIds/index";
import type { InputEntry } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import { PageForms } from "./forms";
import { documentPlaces } from "./places";

export const pageForms = new PageForms(pageInputs, documentPlaces);

export function checksLive(page: Document): void {
  page.addEventListener("input", (event) => {
    eachInputAround(page, event.target, (entry) => { pageForms.clear(entry); });
  }, true);
  page.addEventListener("blur", (event) => {
    eachInputAround(page, event.target, (entry) => { pageForms.checkLive(entry); });
  }, true);
}

function eachInputAround(page: Document, target: EventTarget | null, run: (entry: InputEntry) => void): void {
  if (!(target instanceof Node)) return;
  for (const entry of pageInputs.entries()) {
    if (page.getElementById(entry.id)?.contains(target) !== true) continue;
    try {
      run(entry);
    } catch (stopped) {
      if (!(stopped instanceof Stop)) throw stopped;
      console.error(stopped.reason);
    }
  }
}
