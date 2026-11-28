export function initials(name: string): string {
  return name
    .split(" ")
    .filter((word) => word !== "")
    .map((word) => word.charAt(0))
    .join("");
}
