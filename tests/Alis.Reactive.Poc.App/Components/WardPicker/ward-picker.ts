export interface WardPickerHost extends HTMLElement {
  picker?: WardPicker;
}

export class WardPicker extends EventTarget {
  readonly #posted: HTMLInputElement;
  readonly #shown: HTMLParagraphElement;

  constructor(host: HTMLElement) {
    super();
    const posted = host.querySelector<HTMLInputElement>("input[type=hidden]");
    const wards = host.dataset["wardPicker"];
    if (posted === null || wards === undefined) {
      throw new Error("A ward picker reads its wards and its posted ward from the markup its render helper writes.");
    }

    this.#posted = posted;
    this.#shown = document.createElement("p");
    for (const ward of wards.split("|")) {
      host.append(choice(ward, () => this.#choose(ward)));
    }

    host.append(this.#shown);
    this.#draw();
  }

  get selected(): string | null {
    return this.#posted.value === "" ? null : this.#posted.value;
  }

  set selected(ward: string | null) {
    this.#posted.value = ward ?? "";
    this.#draw();
  }

  #choose(ward: string): void {
    this.selected = ward;
    this.dispatchEvent(new Event("change"));
  }

  #draw(): void {
    this.#shown.textContent = this.selected === null ? "no ward" : "ward " + this.selected;
  }
}

function choice(ward: string, choose: () => void): HTMLButtonElement {
  const button = document.createElement("button");
  button.type = "button";
  button.textContent = ward;
  button.addEventListener("click", choose);
  return button;
}
