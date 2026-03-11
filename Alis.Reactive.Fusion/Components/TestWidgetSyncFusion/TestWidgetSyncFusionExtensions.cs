namespace Alis.Reactive.Fusion.Components
{
    public static class TestWidgetSyncFusionExtensions
    {
        public static ComponentRef<TestWidgetSyncFusion, TModel> SetValue<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self, string value)
            where TModel : class => self.Emit("var c=el.ej2_instances[0]; c.value=val", value);

        public static ComponentRef<TestWidgetSyncFusion, TModel> Focus<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit("el.ej2_instances[0].focus()");

        public static ComponentRef<TestWidgetSyncFusion, TModel> SetItems<TModel>(
            this ComponentRef<TestWidgetSyncFusion, TModel> self)
            where TModel : class => self.Emit("var c=el.ej2_instances[0]; c.setItems(val)");
    }
}
