using System;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Conditions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Reactive Plan pipeline extensions for reading from and updating <see cref="FusionSchedule"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use these from a <see cref="ComponentRef{TComponent, TModel}"/> resolved by the pipeline:
    /// <c>p.Component&lt;FusionSchedule&gt;("shift-schedule").SetDataSource(json, j =&gt; j.Assignments)</c>.
    /// </para>
    /// <para>
    /// Schedule is a non-input component: it exposes schedule state and event methods, not
    /// <c>Value()</c> or <c>SetValue()</c>. Data loading is server-driven via <c>SetDataSource</c>.
    /// </para>
    /// <para>
    /// These methods emit Reactive Plan reads, writes, or calls against the rendered
    /// Schedule component.
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

        private static readonly ComponentMethod GetEventsMethod =
            ComponentMethod.Named("getEvents");

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
        /// Reads the active schedule view name, such as <c>Day</c> or <c>Week</c>.
        /// </summary>
        public static TypedComponentSource<string> CurrentView<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.Read(CurrentViewProperty);

        /// <summary>
        /// Reads the currently selected schedule date.
        /// </summary>
        public static TypedComponentSource<DateTime> SelectedDate<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.Read(SelectedDateProperty);

        /// <summary>
        /// Exposes the rendered schedule events as a typed source.
        /// </summary>
        public static TypedComponentSource<FusionScheduleEventData[]> GetEvents<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.Read<FusionScheduleEventData[]>(GetEventsMethod);

        /// <summary>
        /// Replaces schedule event data from a response-body path and refreshes the schedule.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionSchedule, TModel> self,
            ResponseBody<TResponse> source, Expression<Func<TResponse, object?>> path)
            where TModel : class
            where TResponse : class
        {
            var sourcePath = ExpressionPathHelper.ToResponsePath(path);
            self.EmitSet(EventDataSourceProperty, ValueExpression.Read(source.Scope, sourcePath));
            return self.EmitCall(DataBindMethod);
        }

        /// <summary>
        /// Replaces schedule event data from the entire HTTP response body and refreshes the schedule.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> SetDataSource<TModel, TResponse>(
            this ComponentRef<FusionSchedule, TModel> self,
            ResponseBody<TResponse> source)
            where TModel : class
            where TResponse : class
        {
            self.EmitSet(EventDataSourceProperty, ValueExpression.Read(source.Scope, "responseBody"));
            return self.EmitCall(DataBindMethod);
        }

        /// <summary>
        /// Adds one schedule event or an event collection from a value expression.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> AddEvent<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueExpression data)
            where TModel : class
            => self.EmitCall(AddEventMethod, new System.Collections.Generic.List<ValueExpression> { data });

        /// <summary>
        /// Saves an existing schedule event from a value expression.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> SaveEvent<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueExpression data)
            where TModel : class
            => self.EmitCall(SaveEventMethod, new System.Collections.Generic.List<ValueExpression> { data });

        /// <summary>
        /// Deletes a schedule event by identifier.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> DeleteEvent<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueExpression eventId)
            where TModel : class
            => self.EmitCall(DeleteEventMethod, new System.Collections.Generic.List<ValueExpression> { eventId });

        /// <summary>
        /// Opens the built-in schedule editor for event data and an editor action.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> OpenEditor<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, ValueExpression data, string action = "Add")
            where TModel : class
            => self.EmitCall(OpenEditorMethod, new System.Collections.Generic.List<ValueExpression> { data, ValueExpression.Literal(action) });

        /// <summary>
        /// Closes the built-in schedule editor.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> CloseEditor<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.EmitCall(CloseEditorMethod);

        /// <summary>
        /// Re-renders schedule events.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> RefreshEvents<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.EmitCall(RefreshEventsMethod);

        /// <summary>
        /// Prints the current schedule view.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> Print<TModel>(
            this ComponentRef<FusionSchedule, TModel> self)
            where TModel : class
            => self.EmitCall(PrintMethod);

        /// <summary>
        /// Scrolls the schedule to the target time.
        /// </summary>
        public static ComponentRef<FusionSchedule, TModel> ScrollTo<TModel>(
            this ComponentRef<FusionSchedule, TModel> self, string hour)
            where TModel : class
            => self.EmitCall(ScrollToMethod, new System.Collections.Generic.List<ValueExpression> { ValueExpression.Literal(hour) });
    }
}
