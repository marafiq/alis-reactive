function aMoment(): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, 150));
}

class Dispenser extends HTMLElement {
  private next = 1;
  private readonly nextLine = document.createElement("p");

  connectedCallback(): void {
    if (this.nextLine.isConnected) return;
    this.replaceChildren(this.nextLine);
    this.show();
  }

  take(): number {
    const number = this.next;
    this.next += 1;
    this.show();
    return number;
  }

  async takeFromTheDesk(): Promise<number> {
    await aMoment();
    return this.take();
  }

  private show(): void {
    this.nextLine.textContent = `Next number: ${this.next}`;
  }
}

customElements.define("fw-dispenser", Dispenser);
