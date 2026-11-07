export interface MedicationCartHost extends HTMLElement {
  cart?: MedicationCart;
}

interface DoseSeed {
  readonly medicine: string;
}

export class MedicationCart {
  readonly doses: readonly DoseSeed[];
  readonly featured: DoseSeed;

  constructor(host: HTMLElement) {
    this.doses = JSON.parse(host.dataset["medicationCart"] ?? "[]") as readonly DoseSeed[];
    this.featured = JSON.parse(host.dataset["featuredDose"] ?? "{}") as DoseSeed;
    const list = document.createElement("ul");
    list.className = "cart-doses";
    for (const dose of this.doses) {
      const line = document.createElement("li");
      line.textContent = dose.medicine;
      list.append(line);
    }
    host.replaceChildren(list);
  }
}
