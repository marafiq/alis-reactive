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
