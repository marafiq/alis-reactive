import type { BindingStart, RecordStart } from "./start-options";
export type RecordData = RecordStart;
export type EmptyRecord = Readonly<Partial<RecordStart>>;
export interface TakenRecord {
    readonly name: string | null;
    readonly code: string;
    readonly day: unknown;
}
export interface Linked {
    readonly day: unknown;
}
export type BindingData = Partial<BindingStart>;
