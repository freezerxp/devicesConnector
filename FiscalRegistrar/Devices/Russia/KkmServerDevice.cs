using devicesConnector.Drivers;
using devicesConnector.Wrappers;

namespace devicesConnector.FiscalRegistrar.Drivers.Russia;

public class KkmServerDevice : IFiscalRegistrarDevice
{
    private KkmServerDriver driver;

    public KkmServerDevice(DeviceConnection.LanConnection lanConnection)
    {
        driver = new KkmServerDriver(lanConnection);
    }

    public void PrintNonFiscalString(string str)
    {
        //throw new NotImplementedException();
    }

    public KkmStatus GetStatus()
    {
        var c = new KkmServerDriver.KkmGetInfo();

        var r = driver.SendCommand(c);

        if (!r)
        {
            throw new Exception();


        }

        var status = new KkmStatus
        {
            SessionStatus = c.Data.Info.SessionState switch
            {
                1 => KkmStatus.SessionStatuses.Close,
                2 => KkmStatus.SessionStatuses.Open,
                3 => KkmStatus.SessionStatuses.OpenMore24Hours,
                _ => KkmStatus.SessionStatuses.Unknown
            },
            CheckStatus = KkmStatus.CheckStatuses.Close,
            CheckNumber = c.Data.CheckNumber,
            SessionNumber = c.Data.SessionNumber,
            SoftwareVersion = c.Data.Info.Firmware_Version,
            FnDateEnd = c.Data.Info.FN_DateEnd
        };





        return status;
    }
}