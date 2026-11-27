import { providePlugin } from "../../Components/plugin-queue";

interface RoundQuery {
    skip(count: number): RoundQuery;
    take(count: number): RoundQuery;
}

export interface WardRoundQueries {
    readonly itinerary: RoundQuery;
    readonly firstStop: RoundQuery;
    readonly standbyStop: RoundQuery;
    readonly lastStop: RoundQuery;
}

declare const ej: {
    readonly base: { readonly L10n: { load(messages: Record<string, { dropdowns: { noRecordsTemplate: string } }>): void } };
    readonly data: { readonly Query: new () => RoundQuery };
};

const queries: WardRoundQueries = {
    itinerary: new ej.data.Query(),
    firstStop: new ej.data.Query().take(1),
    standbyStop: new ej.data.Query().skip(1).take(1),
    lastStop: new ej.data.Query().skip(3).take(1)
};
providePlugin("ward-round-queries", queries);

ej.base.L10n.load({
    "en-US": { dropdowns: { noRecordsTemplate: "No ward vacancies." } },
    "fr-CA": { dropdowns: { noRecordsTemplate: "Aucune place disponible." } }
});
