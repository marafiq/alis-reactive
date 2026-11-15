export interface ConformanceStart {
    version: 0 | 1 | 2;
    text: string | null;
    value: string | null;
    options: string[];
    count: number;
    record: RecordStart;
    amount: number;
    outer: OuterStart;
    held: HeldStart;
    readOnlyText: string;
    currentItem: string | null;
    currentItemLacksAct: boolean;
    currentItemLacksValue: boolean;
    bindingCountPending: boolean;
    enabled: boolean;
    starts: StartsStart;
    bindings: BindingStart[];
    items: ItemStart[];
    nullableItems: ItemStart[] | null;
    raises: RaiseStart[];
    records: RecordStart[];
    builtBy: string | null;
    settleEndpoint: string;
    countEndpoint: string;
    itemEndpoint: string;
    textEndpoint: string;
    heldCountEndpoint: string;
    recordEndpoint: string;
    checkEndpoint: string;
}
export interface RecordStart {
    name: string | null;
    count: number;
    day?: string;
    detail: DetailStart | null;
}
export interface DetailStart {
    name?: string;
}
export interface BindingStart {
    name: string;
    renamed_member: string;
    constructor?: string;
}
export interface StartsStart {
    populated: SpotStart;
    getterOnly: SpotStart;
    replaced: SpotStart;
}
export interface SpotStart {
    place?: string;
}
export interface OuterStart {
    text: string;
    count: number;
}
export interface HeldStart {
    items: ItemStart[];
    total: number;
    matchingItems: number;
}
export interface ItemStart {
    name: string;
    active: boolean;
    value: number;
    optionalCount: number | null;
    optionalScore: number | null;
    lacksName: boolean;
    lacksActive: boolean;
}
export interface RaiseStart {
    button: string;
    nullableText: string | null;
    optionalText: string | null;
    text: string | null;
    noArgument: boolean;
    flag: boolean;
    count: number;
    emptyRecord: boolean;
    record: RecordStart | null;
}
