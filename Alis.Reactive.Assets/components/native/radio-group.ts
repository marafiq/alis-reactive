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

  Object.defineProperty(group, "focus", {
    value: (): void => (radios.find(radio => radio.checked) ?? radios[0])?.focus(),
    configurable: true,
  });

  group.parentElement!.addEventListener("change", event => {
    const radio = event.target as HTMLInputElement;
    if (radio.type !== "radio") return;
    inputValue.set!.call(group, radio.value);
    group.dispatchEvent(new Event("change", { bubbles: true }));
  });
});
