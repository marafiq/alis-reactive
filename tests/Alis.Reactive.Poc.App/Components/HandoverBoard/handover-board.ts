export interface HandoverBoardHost extends HTMLElement {
  board?: HandoverBoard;
}

export interface Handover {
  readonly round: number;
  readonly startsAt: Date;
  readonly endsAt: Date | null;
  readonly beds: readonly number[];
  readonly lead: string | null;
  readonly target: number | null;
}

interface ShiftRecord {
  readonly name: string;
}

export class HandoverBoard {
  readonly #summary: HTMLParagraphElement;
  #round = 3;
  #dose = 0.125;
  #rate = 0.1 + 0.2;
  #due: boolean | string = true;
  #ward: string | number = "Rose";
  #plannedFor = "2026-11-09";
  #startsAt: Date | string = new Date(2026, 10, 9, 7, 0, 0, 0);
  #beds: readonly number[] | number = [7, 9];
  #late: readonly (number | null)[] = [3];
  #shift: string | ShiftRecord = "Night";
  #current: Handover | null = null;

  constructor(host: HTMLElement) {
    this.#summary = document.createElement("p");
    this.#summary.className = "handover-summary";
    host.replaceChildren(
      this.#summary,
      button("Start the handover", () => {
        this.#current = { round: 4, startsAt: new Date(2026, 10, 9, 19, 0, 0, 0), endsAt: new Date(2026, 10, 10, 7, 0, 0, 0), beds: [12], lead: "Ana", target: 1500 };
      }),
      button("Plan for 14:30", () => {
        this.#plannedFor = "2026-11-09T14:30:00";
      }),
      button("Plan after lunch", () => {
        this.#plannedFor = "after lunch";
      }),
      button("Move to the summer", () => {
        this.#startsAt = new Date(2026, 6, 9, 7, 0, 0, 0);
      }),
      button("Start soon", () => {
        this.#startsAt = "soon";
      }),
      button("Dilute the dose", () => {
        this.#dose = 0.0000001;
      }),
      button("Lose the rate", () => {
        this.#rate = Number.NaN;
      }),
      button("Hand due as text", () => {
        this.#due = "yes";
      }),
      button("Hand the ward as a number", () => {
        this.#ward = 4;
      }),
      button("Hand one bed", () => {
        this.#beds = 7;
      }),
      button("Lose a late bed", () => {
        this.#late = [3, null];
      }),
      button("Hand the shift as a record", () => {
        this.#shift = { name: "Night" };
      }),
    );
    this.#summary.textContent = "Handover board ready";
  }

  get round(): number {
    return this.#round;
  }

  get dose(): number {
    return this.#dose;
  }

  get rate(): number {
    return this.#rate;
  }

  get due(): boolean | string {
    return this.#due;
  }

  get ward(): string | number {
    return this.#ward;
  }

  get plannedFor(): string {
    return this.#plannedFor;
  }

  get startsAt(): Date | string {
    return this.#startsAt;
  }

  get beds(): readonly number[] | number {
    return this.#beds;
  }

  get late(): readonly (number | null)[] {
    return this.#late;
  }

  get waiting(): readonly (number | null)[] {
    return [4, null, 6];
  }

  get leads(): readonly (string | null)[] {
    return ["Ana", null, "Ben"];
  }

  get shift(): string | ShiftRecord {
    return this.#shift;
  }

  get current(): Handover | null {
    return this.#current;
  }
}

function button(text: string, press: () => void): HTMLButtonElement {
  const pressed = document.createElement("button");
  pressed.type = "button";
  pressed.textContent = text;
  pressed.addEventListener("click", press);
  return pressed;
}
