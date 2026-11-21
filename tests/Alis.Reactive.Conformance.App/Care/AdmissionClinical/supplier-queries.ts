import { mountMarked } from "../../Components/component-view";
import { providePlugin } from "../../Components/plugin-queue";

interface DeliveryQuery {
    addParams(name: string, value: () => string): DeliveryQuery;
}

interface SupplierBox {
    readonly value: string | null;
    readonly dataSource: object;
    setProperties(properties: { dataSource: object }, mute: boolean): void;
}

export interface SupplierQueries {
    readonly availability: DeliveryQuery;
}

declare const ej: {
    readonly dropdowns: { readonly ComboBox: new () => SupplierBox };
    readonly data: {
        readonly DataManager: new (options: { url: string; adaptor: object }) => object;
        readonly UrlAdaptor: new () => object;
        readonly Query: new () => DeliveryQuery;
    };
};

mountMarked("data-supplier-catalogue", element => {
    const endpoint = element.getAttribute("data-supplier-catalogue");
    const input = element.querySelector("input.e-combobox");
    const instances: unknown = input === null ? undefined : Reflect.get(input, "ej2_instances");
    const box: unknown = Array.isArray(instances) ? instances[0] : undefined;
    if (endpoint === null || !(box instanceof ej.dropdowns.ComboBox))
        throw new Error("The supplier desk requires its ComboBox and catalogue endpoint.");
    const previous = box.dataSource;
    const manager = new ej.data.DataManager({ url: endpoint, adaptor: new ej.data.UrlAdaptor() });
    box.setProperties({ dataSource: manager }, false);
    const queries: SupplierQueries = {
        availability: new ej.data.Query().addParams("supplier", () => box.value ?? "")
    };
    providePlugin("supplier-queries", queries);
    return () => {
        box.setProperties({ dataSource: previous }, true);
    };
});
