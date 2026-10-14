// NativeRadioGroup: the hidden input rendered with data-alis-radio-group is the component. It holds the
// chosen radio's value and raises change when the user picks one, so the group reads and commits as
// one input. Writing its value checks the radio with that value and no other, none for empty text;
// a write raises no event, as a script's write to any native input raises none.

import { onMount } from "../hooks";

const inputValue = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value")!;

onMount("[data-alis-radio-group]", element => {
  const group = element as HTMLInputElement;
  const radios = Array.from(group.parentElement!.querySelectorAll<HTMLInputElement>("input[type=radio]"));
  const chosen = (): string => inputValue.get!.call(group) as string;

  Object.defineProperty(group, "value", {
    get: chosen,
    set: (value: string) => {
      inputValue.set!.call(group, value);
      const held = chosen();
      for (const radio of radios) radio.checked = held !== "" && radio.value === held;
    },
    configurable: true,
  });

  group.parentElement!.addEventListener("change", event => {
    const radio = event.target as HTMLInputElement;
    if (radio.type !== "radio") return;
    inputValue.set!.call(group, radio.value);
    group.dispatchEvent(new Event("change", { bubbles: true }));
  });
});
