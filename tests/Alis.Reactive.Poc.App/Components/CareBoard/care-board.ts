export interface CareBoardHost extends HTMLElement {
  careBoard?: CareBoardV1 | CareBoardV2;
}

export class CareBoardV1 {
  readonly #host: HTMLElement;
  readonly #heading: HTMLHeadingElement;
  readonly #lock: HTMLParagraphElement;
  #titleText: string;
  #unlocked = false;

  constructor(host: HTMLElement) {
    this.#host = host;
    this.#titleText = host.dataset["title"] ?? "";
    this.#heading = document.createElement("h2");
    this.#heading.textContent = this.#titleText;
    this.#lock = document.createElement("p");
    this.#lock.textContent = this.status;
    host.replaceChildren(this.#heading, this.#lock);
  }

  get title(): string {
    return this.#titleText;
  }

  set title(text: string) {
    this.#titleText = text;
    this.#heading.textContent = text;
  }

  get status(): string {
    return this.#unlocked ? "Unlocked" : "Locked";
  }

  set pin(code: string) {
    this.#unlocked = code === this.#host.dataset["unlockCode"];
    this.#lock.textContent = this.status;
  }
}

export class CareBoardV2 {
  readonly #heading: HTMLHeadingElement;
  #headingText: string;

  constructor(host: HTMLElement) {
    this.#headingText = host.dataset["title"] ?? "";
    this.#heading = document.createElement("h2");
    this.#heading.textContent = this.#headingText;
    host.replaceChildren(this.#heading);
  }

  get heading(): string {
    return this.#headingText;
  }

  set heading(text: string) {
    this.#headingText = text;
    this.#heading.textContent = text;
  }
}
