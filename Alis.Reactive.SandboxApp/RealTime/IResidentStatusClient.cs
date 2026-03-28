namespace Alis.Reactive.SandboxApp.RealTime;

public interface IResidentStatusClient
{
    Task StatusChanged(ResidentStatusPayload payload);
}
