export interface FluidBalanceHost extends HTMLElement {
  balance?: FluidBalance;
}

export class FluidBalance {
  readonly #intakeShown: HTMLParagraphElement;
  readonly #weightShown: HTMLParagraphElement;
  readonly #recheckShown: HTMLParagraphElement;
  readonly #measuredShown: HTMLParagraphElement;
  readonly #recountShown: HTMLParagraphElement;
  #intake = 0;
  #weight: number | null = null;
  #rechecking = false;
  #finishRecheck: () => void = () => undefined;
  #finishRecount: () => void = () => undefined;

  constructor(host: HTMLElement) {
    this.#intakeShown = shown("balance-intake");
    this.#weightShown = shown("balance-weight");
    this.#recheckShown = shown("balance-recheck");
    this.#measuredShown = shown("balance-measured");
    this.#recountShown = shown("balance-recount");
    host.replaceChildren(
      this.#intakeShown,
      this.#weightShown,
      this.#recheckShown,
      this.#measuredShown,
      this.#recountShown,
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
      button("Finish the recount", () => this.#finishRecount()),
    );
    this.#recheckShown.textContent = "idle";
    this.#measuredShown.textContent = "not measured";
    this.#recountShown.textContent = "not recounted";
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

  measure(): number {
    this.#measuredShown.textContent = "measured at " + String(this.#intake) + " ml";
    return this.#intake;
  }

  recount(): Promise<number> {
    this.#recountShown.textContent = "recounting";
    return new Promise((resolve) => {
      this.#finishRecount = () => {
        this.#recountShown.textContent = "recounted at " + String(this.#intake) + " ml";
        resolve(this.#intake);
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
