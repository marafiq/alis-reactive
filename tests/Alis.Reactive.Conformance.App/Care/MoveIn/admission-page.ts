import { initials } from "../../Components/initials";

interface AdmissionPrintReference {
  readonly draftId: string;
  readonly revision: number;
}

class AdmissionPage {
  get url(): string {
    return window.location.href;
  }

  prepare(mode: "admission", packet: AdmissionPrintReference): void {
    const sheet = document.getElementById("admission-print-sheet");
    const field = sheet?.querySelector<HTMLInputElement>("input[data-print-reference]");
    if (field?.dataset["printReference"] === undefined)
      throw new Error("The print preview has no draft reference.");
    const reference: unknown = JSON.parse(field.dataset["printReference"]);
    if (typeof reference !== "object" || reference === null || !("draftId" in reference) || !("revision" in reference)
      || field.value !== packet.draftId || reference.draftId !== packet.draftId || reference.revision !== packet.revision)
      throw new Error("The print preview no longer holds the accepted packet.");
    document.documentElement.dataset["printMode"] = mode;
  }

  async print(): Promise<void> {
    try {
      await document.fonts.ready;
      window.print();
    } finally {
      delete document.documentElement.dataset["printMode"];
    }
  }
}

const plugins: unknown = Reflect.get(window, "__alisPlugins");
const entries = [{ name: "initials", instance: initials }, { name: "admission-page", instance: new AdmissionPage() }];
if (Array.isArray(plugins)) plugins.push(...entries);
else Reflect.set(window, "__alisPlugins", entries);
