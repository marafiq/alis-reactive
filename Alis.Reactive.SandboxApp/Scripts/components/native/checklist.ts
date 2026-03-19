// checklist.ts — self-initializing side-effect module
// Syncs checked checkbox values into:
//   1. container.value (string[]) — for evalRead + gather (array semantics)
//   2. hidden input .value (CSV string) — for MVC form submission
// The container div carries [data-reactive-checklist] and the element ID.
// Checkbox change events naturally bubble to the container — no synthetic dispatch needed.
export {}; // Module marker — prevents TS global-scope collisions

function init(): void {
  const containers = document.querySelectorAll<HTMLElement>("[data-reactive-checklist]");

  for (const container of containers) {
    const hidden = container.querySelector('input[type="hidden"]') as HTMLInputElement | null;
    if (!hidden) continue;

    // Initialize container.value as string[] from hidden input's CSV
    const initial = hidden.value.split(",").filter(Boolean);
    (container as any).value = initial;

    container.addEventListener("change", () => {
      const values: string[] = [];
      const checkboxes = container.querySelectorAll<HTMLInputElement>('input[type="checkbox"]');
      for (const cb of checkboxes) {
        if (cb.checked) values.push(cb.value);
      }
      // Update both: array on container (for runtime), CSV on hidden (for MVC)
      (container as any).value = values;
      hidden.value = values.join(",");
    });
  }
}

init();
