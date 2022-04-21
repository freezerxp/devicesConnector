using System.Runtime.InteropServices;
using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Helpers;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.Helpers;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices.Russia;

public class ShtihM : IFiscalRegistrarDevice
{
    private Device _device;
    private dynamic? _driver;
    private ReceiptData _receipt;


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

        _driver.RegisterNumber = 241;
        _driver.GetCashReg();

        status.CashSum = (decimal) _driver.ContentsOfCashRegister;


        var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()?.FfdVersion;


        if (ffdV != Enums.FFdVersions.Offline)
        {
            _driver.FNGetInfoExchangeStatus();

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
        var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()?.FfdVersion;


        if (ffdV == Enums.FFdVersions.Offline)
        {
            CheckResult(_driver.OpenSession());
        }
        else
        {
            CheckResult(_driver.FNBeginOpenSession());

            SetCashierData(cashier);

            CheckResult(_driver.FNOpenSession());
        }

        // пауза, т.к. при последующей попытке продать товар пишет, что неверный статус ФН
        Thread.Sleep(2_000);
    }

    private void SetCashierData(Cashier cashier)
    {
        WriteOfdAttribute(Enums.OfdAttributes.CashierName, cashier.Name);
        WriteOfdAttribute(Enums.OfdAttributes.CashierInn, cashier.TaxId);
    }


    private void WriteOfdAttribute(Enums.OfdAttributes ofdAttribute, object? value)
    {
        if (value == null || value.ToString().IsNullOrEmpty())
        {
            return;
        }

        _driver.TagNumber = (int) ofdAttribute;

        switch (ofdAttribute)
        {
            case Enums.OfdAttributes.CashierInn:
            case Enums.OfdAttributes.CashierName:
            case Enums.OfdAttributes.ClientEmailPhone:
            {
                if (value.ToString().IsNullOrEmpty()) //не записываю пустое значение
                {
                    return;
                }

                _driver.TagType = 7;
                _driver.TagValueStr = value.ToString();
                break;
            }
            case Enums.OfdAttributes.UnitCode:
                _driver.TagType = 2;
                _driver.TagValueInt = value;
                break;

            //case OfdAttributes.TaxSystem:
            //    break;
            //case OfdAttributes.ClientName:
            //    break;
            //case OfdAttributes.ClientInn:
            //    break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ofdAttribute), ofdAttribute, null);
        }

        CheckResult(_driver.FNSendTag());
    }


    private void CloseSession(Cashier cashier)
    {
        {
            var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()
                ?.FfdVersion;


            if (ffdV == Enums.FFdVersions.Offline)
            {
                CheckResult(_driver.PrintReportWithCleaning());
            }
            else
            {
                CheckResult(_driver.FNBeginCloseSession());

                SetCashierData(cashier);

                CheckResult(_driver.FNCloseSession());
            }
        }
    }

    public void GetReport(Enums.ReportTypes type, Cashier cashier)
    {
        switch (type)
        {
            case Enums.ReportTypes.ZReport:
                CloseSession(cashier);
                break;
            case Enums.ReportTypes.XReport:


                CheckResult(_driver.PrintReportWithoutCleaning());

                break;
            case Enums.ReportTypes.XReportWithGoods:
                throw new NotSupportedException();

            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public void OpenReceipt(ReceiptData? receipt)
    {

        _receipt = receipt;
        

        Payments = new Dictionary<int, decimal>();
      //  CheckDiscountsList = new List<CheckDiscount>();

        for (var i = 1; i < 17; i++)
        {
            Payments.Add(i, 0);
        }

        _driver.CheckType = receipt.OperationType switch
        {
            Enums.ReceiptOperationTypes.Sale => 0, // продажа
            Enums.ReceiptOperationTypes.Buy => 1, // покупка
            Enums.ReceiptOperationTypes.ReturnSale => 2, // возврат продажи
            Enums.ReceiptOperationTypes.ReturnBuy => 3, // возврат покупки
            _ => throw new ArgumentOutOfRangeException()
        };

        CheckResult(_driver.OpenCheck());

        //todo: отключение печати чека
        //if (new ConfigsRepository<Core.Config.Devices>().Get().CheckPrinter.FiscalKkm.IsAlwaysNoPrintCheck)
        //{
        //    OffPrintDoc();
        //}

        var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()
            ?.FfdVersion;

        if (ffdV != Enums.FFdVersions.Offline)
        {
            SetCashierData(receipt.Cashier);
        }

        if (ffdV == Enums.FFdVersions.Ffd120)
        {
            foreach (var item in receipt.Items)
            {
                var ruData = item.CountrySpecificData.Deserialize<Objects.CountrySpecificData.Russia.ReceiptItemData>();

                if (ruData?.MarkingInfo == null || ruData.MarkingInfo.RawCode .IsNullOrEmpty())
                {
                    continue;
                }

             

                //_driver.BarCode = ruData.MarkingInfo.RawCode; //должно быть с заменой?
                _driver.BarCode = RuKkmHelper.PrepareMarkCodeForFfd120(ruData.MarkingInfo.RawCode);
                _driver.ItemStatus = RuKkmHelper.GetMarkingCodeStatus(item, receipt.OperationType);
                _driver.CheckItemMode = 0;

                _driver.TLVDataHex = "";
                _driver.TagNumber = 2108;
                _driver.TagType = (int)ruData.FfdData.Unit; // тип ttByte
                _driver.TagValueInt = 0;
                _driver.GetTagAsTLV();



                _driver.TLVDataHex = _driver.TLVDataHex;//bug: а это для чего??
                CheckResult(_driver.FNCheckItemBarcode());
                CheckResult(_driver.FNAcceptMarkingCode());
            }
        }
    }

    public void CloseReceipt()
    {
        if (_receipt == null)
        {
            throw new ArgumentNullException(nameof(_receipt));
        }

        //todo: устанавливаю скидки по чеку
        //RegisterAllCheckDiscount();


        RegisterAllPayments();

     

        

        var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()
            ?.FfdVersion;

        if (ffdV != Enums.FFdVersions.Offline)
        {
            _driver.TaxType = _receipt.CountrySpecificData.Deserialize<Objects.CountrySpecificData.Russia.ReceiptData>()
                .TaxVariantIndex;


            CheckResult(_driver.FNCloseCheckEx());
        }
        else
        {
            CheckResult(_driver.CloseCheck());
        }
    }

    public void CancelReceipt()
    {
        _receipt = null;
        CheckResult(_driver.CancelCheck());
    }

    public void RegisterItem(ReceiptItem item)
    {
        var goodName = item.Name;

        if (goodName.Length > 100)
        {
            goodName = goodName.Substring(0, 100);
        }

        _driver.StringForPrinting = goodName;
        _driver.Price = item.Price;
        _driver.Quantity = item.Quantity;
        _driver.Department = item.DepartmentIndex;



        var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()
            ?.FfdVersion;


        switch (ffdV)
        {
            case Enums.FFdVersions.Offline:
            case Enums.FFdVersions.Ffd100:
            {
                RegisterGood_OfflineFFD10(item, _receipt.OperationType);
                break;
            }

            case Enums.FFdVersions.Ffd105:
            case Enums.FFdVersions.Ffd110:
            case Enums.FFdVersions.Ffd120:


                RegisterGood_onlineKkm(item, _receipt.OperationType);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        //todo: печать нефискальных строк
       // PrintNonFiscalStrings(good.CommentForFiscalCheck.Select(x => new NonFiscalString(x)).ToList());
        _driver.StringForPrinting = "";
    }


    private void RegisterGood_onlineKkm(ReceiptItem good, Enums.ReceiptOperationTypes receiptType)
    {
        if (good == null)
        {
            throw new ArgumentNullException(nameof(good));
        }

        _driver.CheckType = receiptType switch
        {
            Enums.ReceiptOperationTypes.Sale => 1,
            Enums.ReceiptOperationTypes.ReturnSale => 2,
            _ => throw new NotImplementedException()
        };



        var ruData = good.CountrySpecificData.Deserialize<Objects.CountrySpecificData.Russia.ReceiptItemData>();

        if (ruData?.FfdData == null)
        {
            throw new NullReferenceException();
        }

        _driver.Tax1 = good.TaxRateIndex ?? 0;
        _driver.PaymentItemSign = ruData.FfdData.Subject;
        _driver.PaymentTypeSign = ruData.FfdData.Method;
        _driver.Summ1Enabled = false;

        CheckResult(_driver.FNOperation());


        if (ruData.MarkingInfo != null && ruData.MarkingInfo.RawCode.IsNullOrEmpty() == false)
        {
            var ffdV = _device.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()
                ?.FfdVersion;

            if (ffdV == Enums.FFdVersions.Ffd120)
            {
               

                _driver.BarCode =RuKkmHelper.PrepareMarkCodeForFfd120(ruData.MarkingInfo.RawCode);
                CheckResult(_driver.FNSendItemBarcode());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

      



    }

    private void RegisterGood_OfflineFFD10(ReceiptItem good, Enums.ReceiptOperationTypes checkType)
    {
        switch (checkType)
        {
            case Enums.ReceiptOperationTypes.Sale:
            {
                CheckResult(_driver.Sale());
                break;
            }
            case Enums.ReceiptOperationTypes.ReturnSale:
            {
                CheckResult(_driver.ReturnSale());
                break;
            }
            default:
                throw new NotImplementedException();
        }

        //скидка на весь чек
        //if (dc.CheckPrinter.FiscalKkm.FfdVersion != GlobalDictionaries.Devices.FfdVersions.OfflineKkm)
        //{
        //    return;
        //}

        //if (good.DiscountSum > 0)
        //{
        //    ShtrihDriver.StringForPrinting = @" ";
        //    ShtrihDriver.Summ1 = good.DiscountSum;
        //    CheckResult(ShtrihDriver.Discount());
        //}
    }

    public void RegisterPayment(ReceiptPayment payment)
    {

        if (payment.MethodIndex is < 1 or > 16)
        {
            throw new ArgumentOutOfRangeException();
        }

        Payments[payment.MethodIndex] += payment.Sum;

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
        _driver.Summ1 = sum;
        CheckResult(_driver.CashIncome());
    }

    public void CashOut(decimal sum, Cashier cashier)
    {
        _driver.Summ1 = sum;
        CheckResult(_driver.CashOutcome());
    }

    public void CutPaper()
    {
        WaitPrint();

        _driver.CutType = true;
        var r = _driver.CutCheck();

        CheckResult(r);
    }

    public void OpenCashBox()
    {
        throw new NotImplementedException();
    }

    private void RegisterAllPayments()
    {
     
        //возможно как-то иначе это реализовать?

        _driver.Summ1 = Payments[1];
        _driver.Summ2 = Payments[2];
        _driver.Summ3 = Payments[3];
        _driver.Summ4 = Payments[4];
        _driver.Summ5 = Payments[5];
        _driver.Summ6 = Payments[6];
        _driver.Summ7 = Payments[7];
        _driver.Summ8 = Payments[8];
        _driver.Summ9 = Payments[9];
        _driver.Summ10 = Payments[10];
        _driver.Summ11 = Payments[11];
        _driver.Summ12 = Payments[12];
        _driver.Summ13 = Payments[13];
        _driver.Summ14 = Payments[14];
        _driver.Summ15 = Payments[15];
        _driver.Summ16 = Payments[16];
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