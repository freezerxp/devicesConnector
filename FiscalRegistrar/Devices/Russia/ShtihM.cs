using System.Runtime.InteropServices;
using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.Helpers;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices.Russia;

public class ShtihM : IFiscalRegistrarDevice
{
    private Device _device;
    private dynamic? _driver;


    public ShtihM(Device device)
    {
        _device = device;
    }

    private Dictionary<int, decimal> Payments { get; set; }

    public void Dispose()
    {
        Disconnect();
    }

    public KkmStatus GetStatus()
    {
        var status = new KkmStatus
        {
            KkmState = GetKkmState()
        };

        CheckResult(_driver.GetECRStatus());

        status.SessionStatus = _driver.ECRMode switch
        {
            4 => Enums.SessionStatuses.Close,
            2 => Enums.SessionStatuses.Open,
            8 => Enums.SessionStatuses.Open, //если есть открытый чек, значит смена тоже открыта
            3 => Enums.SessionStatuses.OpenMore24Hours,
            _ => Enums.SessionStatuses.Unknown
        };

        status.FactoryNumber = _driver.SerialNumber;
        status.SoftwareVersion = _driver.ECRSoftVersion;

        //получаем состояние чека

        status.CheckStatus = _driver.ECRMode == 8
            ? Enums.CheckStatuses.Open
            : Enums.CheckStatuses.Close;

        _driver.RegisterNumber = 148;

        status.CheckNumber = _driver.GetOperationReg == 0
            ? (int) Convert.ToInt32(_driver.ContentsOfOperationRegister)
            : 1;

        status.DriverVersion = new Version(_driver.DriverVersion.ToString());
        status.SessionNumber = _driver.SessionNumber + 1;

        _driver.GetDeviceMetrics();
        status.Model = _driver.UDescription;


        var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()?.FfdVersion;


        if (ffdV != Enums.FFdVersions.Offline)
        {
            CheckResult(_driver.FNGetInfoExchangeStatus());

            status.RuKkmInfo = new KkmStatus.RuKkm
            {
                OfdLastSendDateTime = _driver.Date,
                OfdNotSendDocuments = _driver.MessageCount
            };
        }


        WaitPrint();

        return status;
    }

    public void OpenSession(Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void GetReport(Enums.ReportTypes type, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void OpenReceipt(ReceiptData? receipt)
    {
        throw new NotImplementedException();
    }

    public void CloseReceipt()
    {
        throw new NotImplementedException();
    }

    public void CancelReceipt()
    {
        throw new NotImplementedException();
    }

    public void RegisterItem(ReceiptItem item)
    {
        throw new NotImplementedException();
    }

    public void RegisterPayment(ReceiptPayment payment)
    {
        throw new NotImplementedException();
    }

    public void PrintText(string text)
    {
        throw new NotImplementedException();
    }

    public void Connect()
    {
        _driver = CommonHelper.CreateObject(@"AddIn.DRvFR");

        if (_driver == null)
        {
            throw new NullReferenceException();
        }


        var connType = _device.Connection.ConnectionType;


        if (connType == DeviceConnection.ConnectionTypes.ComPort)
        {
            var comPort = _device.Connection.ComPort;

            if (comPort == null)
            {
                throw new NullReferenceException();
            }

            _driver.ConnectionType = 0; //локально
            _driver.ComNumber = comPort.PortNumber;
            _driver.BaudRate = 6; //115200
        }

        if (connType == DeviceConnection.ConnectionTypes.Lan)
        {
            var lan = _device.Connection.Lan;

            if (lan == null)
            {
                throw new NullReferenceException();
            }

            _driver.ConnectionType = 6; //TCP-сокет
            _driver.UseIPAddress = true;
            _driver.IPAddress = lan.HostUrl;
            _driver.TCPPort = lan.PortNumber;
        }


        var connectResult = _driver.Connect();

        CheckResult(connectResult);


        WaitPrint();
    }

    public void Disconnect()
    {
        if (_driver == null)
        {
            return;
        }

        CheckResult(_driver.Disconnect());

        Marshal.ReleaseComObject(_driver);

        _driver = null;
    }

    public void CashIn(decimal sum, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void CashOut(decimal sum, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void CutPaper()
    {
        throw new NotImplementedException();
    }

    public void OpenCashBox()
    {
        throw new NotImplementedException();
    }

    private Enums.KkmStatuses GetKkmState()
    {
        var s = Enums.KkmStatuses.Ready;

        // запрашиваю статус

        CheckResult(_driver.GetECRStatus());

        if (_driver.ReceiptRibbonIsPresent == false)
        {
            s = Enums.KkmStatuses.NoPaper;
        }

        if (_driver.LidPositionSensor)
        {
            s = Enums.KkmStatuses.CoverOpen;
        }


        return s;
    }

    /// <summary>
    /// Проверка выполнения команды
    /// </summary>
    /// <param name="errCode"></param>
    /// <exception cref="KkmException"></exception>
    private void CheckResult(int? errCode)
    {
        errCode ??= (int) _driver.ResultCode;

        if (errCode == 0)
        {
            return; //успешно выполнено
        }

        if (errCode == 88) //ожидание команды продолжения печати 
        {
            _driver.ContinuePrint();
            errCode = (int) _driver.ResultCode;

            CheckResult(errCode);
        }

        var errType = errCode switch
        {
            -1 => Enums.ErrorTypes.NoConnection,
            _ => Enums.ErrorTypes.Unknown
        };

        var kkmResultDescription = (string) _driver.ResultCodeDescription;

        throw new KkmException(string.Empty, errType, errCode, kkmResultDescription);
    }

    /// <summary>
    /// Ожидание окончания предыдущей команды
    /// </summary>
    private void WaitPrint()
    {
        for (var i = 0; i < 10; i++)
        {
            _driver.GetECRStatus();
            int s = _driver.ECRAdvancedMode;

            if (s.IsEither(4, 5)) //если идет печать предыдущей команды, ждем
            {
                Thread.Sleep(1000);
            }
            else
            {
                return;
            }
        }
    }
}