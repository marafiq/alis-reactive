namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EventsViewModel
    {
        public string? EventName { get; set; }
    }

    /// <summary>The document events the Events page raises and hears.</summary>
    public static class EventsEvents
    {
        public static readonly AppEvent<EventsViewModel> Final = AppEvent.Declare<EventsViewModel>();
        public static readonly AppEvent Test = AppEvent.Declare();
        public static readonly AppEvent<EventsViewModel> TestReceived = AppEvent.Declare<EventsViewModel>();
    }
}
