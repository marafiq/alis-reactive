using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Mutation extensions for <see cref="FusionSchedule"/> in a reactive pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Obtain a <see cref="ComponentRef{TComponent, TModel}"/> via the pipeline:
    /// <c>p.Component&lt;FusionSchedule&gt;("shift-schedule").SetDataSource(json, j =&gt; j.Assignments)</c>.
    /// </para>
    /// <para>
    /// Non-input component: no <c>Value()</c> read or <c>SetValue()</c>.
    /// Data loading is server-driven via SetDataSource.
    /// </para>
    /// </remarks>
    public static class FusionScheduleExtensions
    {
        private static readonly FusionSchedule Component = new FusionSchedule();

        private static readonly ComponentProperty<string> CurrentViewProperty =
            ComponentProperty<string>.Named("currentView");

        private static readonly ComponentProperty<DateTime> SelectedDateProperty =
            ComponentProperty<DateTime>.Named("selectedDate");

        private static readonly ComponentProperty<object> EventDataSourceProperty =
            ComponentProperty<object>.Mapped("eventDataSource", "eventSettings.dataSource");

        private static readonly ComponentMethod DataBindMethod =
            ComponentMethod.Named("dataBind");

        private static readonly ComponentMethod AddEventMethod =
            ComponentMethod.Named("addEvent").WithArgs<object>();

        private static readonly ComponentMethod SaveEventMethod =
            ComponentMethod.Named("saveEvent").WithArgs<object>();

        private static readonly ComponentMethod DeleteEventMethod =
            ComponentMethod.Named("deleteEvent").WithArgs<object>();

        private static readonly ComponentMethod OpenEditorMethod =
            ComponentMethod.Named("openEditor").WithArgs<object, string>();

        private static readonly ComponentMethod CloseEditorMethod =
            ComponentMethod.Named("closeEditor");

        private static readonly ComponentMethod RefreshEventsMethod =
            ComponentMethod.Named("refreshEvents");

        private static readonly ComponentMethod PrintMethod =
            ComponentMethod.Named("print");

        private static readonly ComponentMethod ScrollToMethod =
            ComponentMethod.Named("scrollTo").WithArgs<string>();

        /// <summary>
        /// Reads the active view ("Day", "Week", "WorkWeek", "Month", "Agenda").
        /// Use in conditions or gather to send the current view type to the server.
        /// Runtime: reads ej2.currentView
        /// </summary>
        public static TypedComponentSource<string> CurrentView<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.Read(CurrentViewProperty);

        /// <summary>
        /// Reads the currently selected date.
        /// Runtime: reads ej2.selectedDate
        /// </summary>
        public static TypedComponentSource<DateTime> SelectedDate<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.Read(SelectedDateProperty);

        /// <summary>
        /// Replaces the schedule event data from an HTTP response body with a path selector.
        /// Runtime: ej2.eventSettings.dataSource = responseBody.path; ej2.dataBind()
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionSchedule, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            self.EmitSet(EventDataSourceProperty, ValueProducer.Read(source.Scope, sourcePath));
            return self.EmitCall(DataBindMethod);
        }

        /// <summary>
        /// Replaces the schedule event data with the entire HTTP response body.
        /// Runtime: ej2.eventSettings.dataSource = responseBody; ej2.dataBind()
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionSchedule, TModel> self,
            ResponseBody<TResponse> source)
            where TModel : class
            where TResponse : class
        {
            self.EmitSet(EventDataSourceProperty, ValueProducer.Read(source.Scope, "responseBody"));
            return self.EmitCall(DataBindMethod);
        }

        /// <summary>
        /// Adds one or more events to the schedule.
        /// Runtime: ej2.addEvent(data)
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> AddEvent<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueProducer data)
            where TModel : class
            => self.EmitCall(AddEventMethod, new System.Collections.Generic.List<ValueProducer> { data });

        /// <summary>
        /// Updates an existing event.
        /// Runtime: ej2.saveEvent(data)
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> SaveEvent<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueProducer data)
            where TModel : class
            => self.EmitCall(SaveEventMethod, new System.Collections.Generic.List<ValueProducer> { data });

        /// <summary>
        /// Deletes an event by ID.
        /// Runtime: ej2.deleteEvent(id)
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> DeleteEvent<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueProducer eventId)
            where TModel : class
            => self.EmitCall(DeleteEventMethod, new System.Collections.Generic.List<ValueProducer> { eventId });

        /// <summary>
        /// Opens the built-in event editor programmatically.
        /// Runtime: ej2.openEditor(data, action)
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> OpenEditor<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueProducer data, string action = "Add")
            where TModel : class
            => self.EmitCall(OpenEditorMethod, new System.Collections.Generic.List<ValueProducer> { data, ValueProducer.Literal(action) });

        /// <summary>
        /// Closes the event editor.
        /// Runtime: ej2.closeEditor()
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> CloseEditor<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.EmitCall(CloseEditorMethod);

        /// <summary>
        /// Re-renders all events.
        /// Runtime: ej2.refreshEvents()
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> RefreshEvents<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshEventsMethod);

        /// <summary>
        /// Prints the current schedule view.
        /// Runtime: ej2.print()
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> Print<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.EmitCall(PrintMethod);

        /// <summary>
        /// Scrolls to a specific time in the schedule.
        /// Runtime: ej2.scrollTo(hour)
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> ScrollTo<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, string hour)
            where TModel : class
            => self.EmitCall(ScrollToMethod, new System.Collections.Generic.List<ValueProducer> { ValueProducer.Literal(hour) });
    }
}
