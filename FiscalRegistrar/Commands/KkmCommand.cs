using devicesConnector.Drivers;

namespace devicesConnector.FiscalRegistrar.Commands;

public abstract class KkmCommand : DeviceCommand
{
    public KkmHelper.KkmTypes KkmType { get; set; }
}