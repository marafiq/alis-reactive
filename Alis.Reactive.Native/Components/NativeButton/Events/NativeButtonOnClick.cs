namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeButton.Click (DOM "click" event).
    /// Button clicks carry no data payload — this class is an empty marker
    /// used by the generic inference chain in .Reactive().
    /// </summary>
    public class NativeButtonClickArgs
    {
        public NativeButtonClickArgs() { }
    }
}
