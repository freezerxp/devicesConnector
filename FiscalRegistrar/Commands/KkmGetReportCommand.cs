using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.FiscalRegistrar.Commands;

public class KkmGetReportCommand:KkmCommand
{
    public Cashier Cashier { get; set; }

    public KkmHelper.ReportTypes Type { get; set; }
}