const VISIBLE = "alis-loader--visible";

class LoaderElement extends HTMLElement {
  private pendingAutoHide: ReturnType<typeof setTimeout> | undefined;
  private coversTarget = false;

  constructor() {
    super();
    new MutationObserver(() => this.followVisibleClass())
      .observe(this, { attributes: true, attributeFilter: ["class"] });
  }

  get target(): string {
    return this.dataset.target ?? "";
  }

  set target(elementId: string) {
    this.dataset.target = elementId;
  }

  get timeout(): number {
    return Number(this.dataset.timeout ?? 0);
  }

  set timeout(ms: number) {
    this.dataset.timeout = `${ms}`;
  }

  show(): void {
    this.classList.add(VISIBLE);
    this.removeAttribute("aria-hidden");
  }

  hide(): void {
    this.classList.remove(VISIBLE);
    this.setAttribute("aria-hidden", "true");
  }

  disconnectedCallback(): void {
    const movedWithinThePage = this.isConnected;
    if (movedWithinThePage || !this.coversTarget) return;

    this.uncover();
    this.hide();
  }

  private followVisibleClass(): void {
    if (this.classList.contains(VISIBLE)) this.cover();
    else this.uncover();
  }

  private cover(): void {
    const targetId = this.dataset.target;
    if (targetId) {
      const target = document.getElementById(targetId);
      if (target) {
        target.style.position = "relative";
        target.appendChild(this);
        this.coversTarget = true;
      }
    }

    clearTimeout(this.pendingAutoHide);
    const timeout = this.dataset.timeout;
    if (timeout) {
      const ms = parseInt(timeout, 10);
      if (ms > 0) {
        this.pendingAutoHide = setTimeout(() => this.hide(), ms);
      }
    }
  }

  private uncover(): void {
    clearTimeout(this.pendingAutoHide);
    this.coversTarget = false;
    if (this.parentElement !== document.body) {
      document.body.appendChild(this);
    }
    delete this.dataset.target;
    delete this.dataset.timeout;
    const message = document.getElementById("alis-loader-message");
    if (message) message.textContent = "";
  }
}

customElements.define("alis-loader", LoaderElement);
