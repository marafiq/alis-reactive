import { mountsTheElement } from "../../NativeComponents/mounted";

const drawerId = "alis-drawer";
const titleId = "alis-drawer-title";
const closeId = "alis-drawer-close";
const visible = "alis-drawer--visible";
const sizes = ["sm", "md", "lg"] as const;
const startingSize: DrawerSize = "md";

type DrawerSize = (typeof sizes)[number];
type ClosedBy = "escape" | "closeButton" | "close";

function sizeClass(size: DrawerSize): string {
  return "alis-drawer--" + size;
}

function sizeNamed(written: unknown): DrawerSize {
  const size = sizes.find((each) => each === written);
  if (size === undefined) throw new Error("The drawer takes the size sm, md or lg, and was given " + String(written) + ".");
  return size;
}

function partNamed<TElement extends HTMLElement>(id: string, kind: new () => TElement): TElement {
  const part = document.getElementById(id);
  if (!(part instanceof kind)) throw new Error("The drawer's part '" + id + "' is missing or no " + kind.name + ".");
  return part;
}

function mountsDrawer(drawer: HTMLElement): void {
  const title = partNamed(titleId, HTMLHeadingElement);
  const closeButton = partNamed(closeId, HTMLButtonElement);
  let size: DrawerSize = startingSize;
  let open = false;

  const resizes = (next: DrawerSize): void => {
    drawer.classList.remove(sizeClass(size));
    drawer.classList.add(sizeClass(next));
    size = next;
  };

  const resets = (): void => {
    drawer.classList.remove(visible);
    drawer.setAttribute("aria-hidden", "true");
    resizes(startingSize);
    title.textContent = "";
    open = false;
  };

  const closes = (closedBy: ClosedBy): void => {
    if (!open) return;
    const closing = Object.assign(new Event("beforeClose"), { closedBy, cancel: false });
    drawer.dispatchEvent(closing);
    const cancelled: unknown = Reflect.get(closing, "cancel");
    if (cancelled === true) return;
    resets();
    drawer.dispatchEvent(new Event("closed"));
  };

  Object.defineProperty(drawer, "size", {
    configurable: true,
    get: () => size,
    set: (written: unknown) => {
      resizes(sizeNamed(written));
    },
  });
  Object.defineProperty(drawer, "open", {
    configurable: true,
    value: () => {
      drawer.classList.add(visible);
      drawer.removeAttribute("aria-hidden");
      open = true;
    },
  });
  Object.defineProperty(drawer, "close", {
    configurable: true,
    value: () => {
      closes("close");
    },
  });
  closeButton.addEventListener("click", () => {
    closes("closeButton");
  });
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") closes("escape");
  });
}

export function mountsTheDrawer(): void {
  mountsTheElement(drawerId, HTMLElement, mountsDrawer);
}
