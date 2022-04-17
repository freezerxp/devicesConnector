using devicesConnector.Common;

namespace devicesConnector.FiscalRegistrar.Commands;

public abstract class KkmCommand : DeviceCommand
{
    public Enums.KkmTypes KkmType { get; set; }
}