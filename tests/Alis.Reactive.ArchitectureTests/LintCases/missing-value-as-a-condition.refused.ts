export function name(element: HTMLElement | null): string {
  if (!element) {
    return "";
  }

  return element.id;
}
