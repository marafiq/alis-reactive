// NativeHiddenField: the hidden input rendered with data-alis-hidden-field raises change (bubbling) when
// its value is written with a new value, so a value the page writes (a room chosen elsewhere, a
// resident the server names) commits as an input the user changes does. HTML raises no event for a
// hidden input. Writing the value it already holds raises nothing, as a Syncfusion component raises
// change only for a new value, so a change handler that writes the same value back does not loop.

import { onMount } from "../hooks";

const inputValue = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value")!;

onMount("[data-alis-hidden-field]", element => {
  const field = element as HTMLInputElement;
  const held = (): string => inputValue.get!.call(field) as string;

  Object.defineProperty(field, "value", {
    get: held,
    set: (value: string) => {
      const before = held();
      inputValue.set!.call(field, value);
      if (held() !== before) field.dispatchEvent(new Event("change", { bubbles: true }));
    },
    configurable: true,
  });
});
