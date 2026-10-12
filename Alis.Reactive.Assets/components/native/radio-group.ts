// NativeRadioGroup: the hidden input rendered with data-alis-radio-group holds the picked radio's
// value and raises change when the user picks one, so the group reads and commits as one input.

import { onMount } from "../hooks";

onMount("[data-alis-radio-group]", element => {
  const group = element as HTMLInputElement;
  group.parentElement!.addEventListener("change", event => {
    const radio = event.target as HTMLInputElement;
    if (radio.type !== "radio") return;
    group.value = radio.value;
    group.dispatchEvent(new Event("change", { bubbles: true }));
  });
});
