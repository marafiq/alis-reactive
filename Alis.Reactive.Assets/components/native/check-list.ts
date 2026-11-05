import { onMount } from "../hooks";

onMount("[data-alis-check-list]", list => {
  const posted = list.querySelector<HTMLInputElement>("input[type=hidden]")!;
  const boxes = Array.from(list.querySelectorAll<HTMLInputElement>("input[type=checkbox]"));
  const checkedValues = (): string[] => boxes.filter(box => box.checked).map(box => box.value);
  const post = (): void => { posted.value = checkedValues().join(","); };

  Object.defineProperty(list, "value", {
    get: checkedValues,
    set: (values: readonly string[] | null) => {
      for (const box of boxes) box.checked = (values ?? []).includes(box.value);
      post();
    },
    configurable: true,
  });
  post();

  Object.defineProperty(list, "focus", { value: (): void => boxes[0]?.focus(), configurable: true });

  list.addEventListener("change", event => {
    if ((event.target as HTMLInputElement).type === "checkbox") post();
  });
});
