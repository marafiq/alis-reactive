import { mountMarked } from "../../Components/component-view";

interface StockQuery {
    using(manager: object): StockQuery;
    addParams(name: string, value: () => string): StockQuery;
}

interface StockBox {
    readonly value: string | null;
    readonly query: StockQuery | null;
    setProperties(properties: { query: StockQuery | null }, mute: boolean): void;
}

declare const ej: {
    readonly dropdowns: { readonly ComboBox: new () => StockBox };
    readonly data: {
        readonly DataManager: new (options: { url: string; adaptor: object }) => object;
        readonly UrlAdaptor: new () => object;
        readonly Query: new () => StockQuery;
    };
};

mountMarked("data-equipment-stock", element => {
    const endpoint = element.getAttribute("data-equipment-stock");
    const input = element.querySelector("input.e-combobox");
    const instances: unknown = input === null ? undefined : Reflect.get(input, "ej2_instances");
    const box: unknown = Array.isArray(instances) ? instances[0] : undefined;
    if (endpoint === null || !(box instanceof ej.dropdowns.ComboBox))
        throw new Error("The stock desk requires its ComboBox and stock endpoint.");
    const original = box.query;
    const manager = new ej.data.DataManager({ url: endpoint, adaptor: new ej.data.UrlAdaptor() });
    const query = new ej.data.Query().using(manager).addParams("aid", () => box.value ?? "");
    box.setProperties({ query }, true);
    return () => {
        box.setProperties({ query: original }, true);
    };
});
