class VisitorBook extends EventTarget {
  override addEventListener(type: "signed", callback: ((event: Event) => void) | null, options?: boolean | AddEventListenerOptions): void;
  override addEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions): void;
  override addEventListener(type: string, callback: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions): void {
    super.addEventListener(type, callback, options);
  }

  readonly visits = 3;
}

const book = new VisitorBook();

function provide(name: string, instance: unknown): void {
  const queue: unknown = Reflect.get(window, "__alisPlugins");
  if (Array.isArray(queue)) {
    queue.push({ name, instance });
    return;
  }
  Reflect.set(window, "__alisPlugins", [{ name, instance }]);
}

function elementOf(id: string): HTMLElement {
  const element = document.getElementById(id);
  if (element === null) throw new Error("The page holds no element '" + id + "'.");
  return element;
}

function whenClicked(id: string, act: () => void): void {
  elementOf(id).addEventListener("click", act);
}

elementOf("open-the-book").addEventListener(
  "click",
  () => {
    provide("visitorbook", book);
    elementOf("book-state").textContent = "book ready";
  },
  { once: true },
);

whenClicked("sign-the-book", () => {
  book.dispatchEvent(new Event("signed"));
  elementOf("book-own").textContent = "signed";
});
