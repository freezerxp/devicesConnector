using System.Text.Json;
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

     var r=  driver.SendCommand(c);

     var answer =r.Rezult.Deserialize< KkmServerDriver.KkmServerKktInfoAnswer> ();

        var status = new KkmStatus
        {
            SessionStatus = answer.Info.SessionState switch
            {
                1 => KkmStatus.SessionStatuses.Close,
                2 => KkmStatus.SessionStatuses.Open,
                3 => KkmStatus.SessionStatuses.OpenMore24Hours,
                _ => KkmStatus.SessionStatuses.Unknown
            },
            CheckStatus = KkmStatus.CheckStatuses.Close,
            CheckNumber = answer.CheckNumber,
            SessionNumber = answer.SessionNumber,
            SoftwareVersion = answer.Info.Firmware_Version,
            FnDateEnd = answer.Info.FN_DateEnd
        };





        return status;
    }
}