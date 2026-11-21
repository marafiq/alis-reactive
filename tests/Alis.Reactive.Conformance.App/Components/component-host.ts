type Restart = (element: HTMLElement) => boolean;

const restartsOfEachMarker: Restart[] = [];

export function restarts(restart: Restart): void {
    restartsOfEachMarker.push(restart);
}

async function servesTheHarness(): Promise<void> {
    for (;;) {
        const order = await fetch("/world/host/orders", { method: "POST" });
        if (order.status !== 200)
            return;
        const report = restartOf(await order.text());
        await fetch("/world/host/done", { method: "POST", body: report });
    }
}

function restartOf(id: string): string {
    const element = document.getElementById(id);
    if (element === null)
        return "The page holds no element '" + id + "'.";
    try {
        return restartsOfEachMarker.some((restart) => restart(element)) ? "" : "The element '" + id + "' holds no component the conformance library mounted.";
    }
    catch (error) {
        return error instanceof Error ? error.message : String(error);
    }
}

servesTheHarness().catch(() => undefined);
