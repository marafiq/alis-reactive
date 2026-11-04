// Native loader markup is a layout singleton rendered by @Html.NativeLoader() as an <alis-loader>
// element. DSL show/hide calls still target it through native set reactions; the element follows its
// own visible class and keeps itself on the page.
export {};

const VISIBLE_CLASS = "alis-loader--visible";

class Loader extends HTMLElement {
  // The auto-hide of the Show in progress. A Hide cancels it and a later Show restarts it, so a
  // timeout never outlives the Hide of the request that set it.
  private pendingAutoHide: ReturnType<typeof setTimeout> | undefined;

  private coversTarget = false;

  constructor() {
    super();
    new MutationObserver(() => this.followVisibleClass())
      .observe(this, { attributes: true, attributeFilter: ["class"] });
  }

  // A targeted loader lives inside the element it covers. When that element leaves the page (Into
  // replaces it, the drawer closes and clears it, any script removes it), the loader goes back to the
  // page hidden before the removing call returns: it is an app-level object, so the next Show or Hide,
  // even in the same reaction, must find it.
  disconnectedCallback(): void {
    const movedWithinThePage = this.isConnected;
    if (movedWithinThePage || !this.coversTarget) return;

    this.uncover();
    this.classList.remove(VISIBLE_CLASS);
    this.setAttribute("aria-hidden", "true");
  }

  private followVisibleClass(): void {
    if (this.classList.contains(VISIBLE_CLASS)) {
      this.cover();
    } else {
      this.uncover();
    }
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
        this.pendingAutoHide = setTimeout(() => {
          this.classList.remove(VISIBLE_CLASS);
          this.setAttribute("aria-hidden", "true");
        }, ms);
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

customElements.define("alis-loader", Loader);
