declare global {
    interface Window {
        alisFusionText: (value: unknown) => string;
        alisFusionUrl: (value: unknown) => string;
    }
}

const entities: Readonly<Partial<Record<string, string>>> = { "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" };

const keptSchemes: ReadonlySet<string> = new Set(["http:", "https:", "mailto:", "tel:"]);

function held(value: unknown): string {
    if (value === null || value === undefined) return "";
    if (typeof value === "string") return value;
    if (typeof value === "number" || typeof value === "boolean" || typeof value === "bigint") return String(value);
    return JSON.stringify(value);
}

export function asText(value: unknown): string {
    return Array.from(held(value), (character) => entities[character] ?? character).join("");
}

export function asUrl(value: unknown): string {
    const address = held(value);
    const resolved = URL.parse(address, document.baseURI);
    const kept = resolved !== null && keptSchemes.has(resolved.protocol);
    return asText(kept ? address : "#");
}

window.alisFusionText = asText;
window.alisFusionUrl = asUrl;
