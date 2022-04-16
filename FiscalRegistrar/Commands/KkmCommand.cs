using devicesConnector.Drivers;
using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.FiscalRegistrar.Commands;

public abstract class KkmCommand : DeviceCommand
{
    public KkmHelper.KkmTypes KkmType { get; set; }
}