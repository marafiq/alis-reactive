export function isThePagesOrigin(url: string, page: string): boolean {
  return new URL(url, page).origin === new URL(page).origin;
}
