using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.Drivers;
using devicesConnector.FiscalRegistrar.Drivers;

namespace devicesConnector.FiscalRegistrar.Devices.Russia;

public class KkmServerDevice : IFiscalRegistrarDevice
{
    private KkmServerDriver _driver;

    public KkmServerDevice(DeviceConnection.LanConnection lanConnection)
    {
        _driver = new KkmServerDriver(lanConnection);
    }

    public void PrintNonFiscalString(string str)
    {
        //throw new NotImplementedException();
    }

    public KkmStatus GetStatus()
    {
        var c = new KkmServerDriver.KkmGetInfo();

     var r=  _driver.SendCommand(c);

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
            FnDateEnd = answer.Info.FN_DateEnd,
            CashSum = answer.Info.BalanceCash
            
        };





        return status;
    }

    public void OpenSession(Cashier cashier)
    {
        var c = new KkmServerDriver.KkmOpenSession();
        _driver.SendCommand(c);

    }

    public void GetReport(Enums.ReportTypes type, Cashier cashier)
    {
        switch (type)
        {
            case Enums.ReportTypes.ZReport:
                _driver.SendCommand(new KkmServerDriver.KkmCloseShift
                {
                    CashierName = cashier.Name,
                    CashierVATIN = cashier.TaxId
                });
                break;
            case Enums.ReportTypes.XReport:
                _driver.SendCommand(new KkmServerDriver.KkmGetXReport
                {

                });
                break;
            case Enums.ReportTypes.XReportWithGoods:
                throw new NotSupportedException();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

      
    }

    public void PrintFiscalReceipt()
    {
        throw new NotImplementedException();
    }

    public void PrintNonFiscalReceipt()
    {
        throw new NotImplementedException();
    }

    public void CashIn(decimal sum, Cashier cashier)
    {
        var c = new KkmServerDriver.KkmCashIn
        {
            CashierName = cashier.Name,
            CashierVATIN = cashier.TaxId,
            Amount = sum
        };

        _driver.SendCommand(c);

    }

    public void CashOut(decimal sum, Cashier cashier)
    {
        var c = new KkmServerDriver.KkmCashOut
        {
            CashierName = cashier.Name,
            CashierVATIN = cashier.TaxId,
            Amount = sum
        };

        _driver.SendCommand(c);
    }
}