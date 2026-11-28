import { mountsEach, providesPlugin } from "../../NativeComponents/mounted";

const marker = "data-native-reconnect-offer";

type Kept = { readonly kind: "none" } | { readonly kind: "kept"; readonly retry: () => void };

function retryButtonOf(offer: HTMLElement): HTMLButtonElement {
  const button = offer.children.item(1);
  if (!(button instanceof HTMLButtonElement)) throw new Error("The reconnect offer '" + offer.id + "' holds no retry button.");
  return button;
}

function keeps(retry: unknown): () => void {
  if (typeof retry !== "function") throw new Error("The reconnect offer was shown with no retry to call.");
  return () => {
    Reflect.apply(retry, undefined, []);
  };
}

function mountsOffer(offer: HTMLElement): void {
  let kept: Kept = { kind: "none" };
  retryButtonOf(offer).addEventListener("click", () => {
    if (kept.kind === "none") throw new Error("The reconnect offer's retry was clicked before the offer was shown.");
    kept.retry();
  });
  providesPlugin("reconnect", {
    show(retry: unknown): void {
      kept = { kind: "kept", retry: keeps(retry) };
      offer.hidden = false;
    },
    hide(): void {
      offer.hidden = true;
    },
  });
}

export function mountsReconnectOffers(): void {
  mountsEach(marker, HTMLElement, mountsOffer);
}
