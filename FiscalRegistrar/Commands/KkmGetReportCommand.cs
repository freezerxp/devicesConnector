using devicesConnector.FiscalRegistrar.Devices;
using devicesConnector.FiscalRegistrar.Objects;

namespace devicesConnector.FiscalRegistrar.Commands;

public class KkmGetReportCommand:KkmCommand
{
    public Cashier Cashier { get; set; }

    public Enums.ReportTypes ReportType { get; set; }
}