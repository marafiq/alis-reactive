export interface FluidBalanceHost extends HTMLElement {
  balance?: FluidBalance;
}

export class FluidBalance {
  readonly #intakeShown: HTMLParagraphElement;
  readonly #weightShown: HTMLParagraphElement;
  readonly #recheckShown: HTMLParagraphElement;
  #intake = 0;
  #weight: number | null = null;
  #rechecking = false;
  #finishRecheck: () => void = () => undefined;

  constructor(host: HTMLElement) {
    this.#intakeShown = shown("balance-intake");
    this.#weightShown = shown("balance-weight");
    this.#recheckShown = shown("balance-recheck");
    host.replaceChildren(
      this.#intakeShown,
      this.#weightShown,
      this.#recheckShown,
      button("Add 100 ml", () => {
        this.intake = this.#intake + 100;
      }),
      button("Add 500 ml", () => {
        this.intake = this.#intake + 500;
      }),
      button("Weigh 70 kg", () => {
        this.#weight = 70;
        this.#draw();
      }),
      button("Clear the weight", () => {
        this.#weight = null;
        this.#draw();
      }),
      button("Finish the recheck", () => this.#finishRecheck()),
    );
    this.#recheckShown.textContent = "idle";
    this.#draw();
  }

  get intake(): number {
    return this.#intake;
  }

  set intake(millilitres: number) {
    this.#intake = millilitres;
    this.#draw();
  }

  get weight(): number | null {
    return this.#weight;
  }

  get rechecking(): boolean {
    return this.#rechecking;
  }

  get recordedAt(): string {
    return "2026-11-06T08:00:00";
  }

  recheck(): Promise<void> {
    this.#rechecking = true;
    this.#recheckShown.textContent = "rechecking";
    return new Promise((resolve) => {
      this.#finishRecheck = () => {
        this.#rechecking = false;
        this.#recheckShown.textContent = "rechecked";
        resolve();
      };
    });
  }

  #draw(): void {
    this.#intakeShown.textContent = String(this.#intake) + " ml";
    this.#weightShown.textContent = this.#weight === null ? "no weight" : String(this.#weight) + " kg";
  }
}

function shown(className: string): HTMLParagraphElement {
  const paragraph = document.createElement("p");
  paragraph.className = className;
  return paragraph;
}

function button(text: string, press: () => void): HTMLButtonElement {
  const pressed = document.createElement("button");
  pressed.type = "button";
  pressed.textContent = text;
  pressed.addEventListener("click", press);
  return pressed;
}
