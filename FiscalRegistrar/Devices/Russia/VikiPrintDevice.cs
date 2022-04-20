using System.Diagnostics;
using System.Text.Json;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.Helpers;

namespace devicesConnector.FiscalRegistrar.Devices.Russia;

public partial class VikiPrintDevice : IFiscalRegistrarDevice
{
    private Device _deviceConfig;

    public VikiPrintDevice(Device device)
    {
        _deviceConfig = device;
    }


    public KkmStatus GetStatus()
    {
        LogHelper.Write("Запрос статуса на Вики");

        var status = new KkmStatus();

        var los = GetListOfStatuses(out var fatal, out var current, out var document);

        CheckResult(los);

        status.SessionStatus = Enums.SessionStatuses.Close;

        if (current[2] == 1)
        {
            status.SessionStatus = Enums.SessionStatuses.Open;
        }

        if (current[3] == 1)
        {
            status.SessionStatus = Enums.SessionStatuses.OpenMore24Hours;
        }

        status.CheckStatus = Enums.CheckStatuses.Close;

        if (document[0] != 0)
        {
            status.CheckStatus = Enums.CheckStatuses.Open;
        }

        if (GetInfo(1, RequestType.CounterAndRegisters, out var sessionNumber))
        {
            status.SessionNumber = int.TryParse(sessionNumber, out var n) ? n : 1;
        }


        if (GetInfo(2, RequestType.CounterAndRegisters, out var a))
        {
            status.CheckNumber = int.TryParse(a, out var n) ? n : 1;
        }


        if (GetInfo(1,  RequestType.KkmInfo, out var factoryNumber))
        {
            status.FactoryNumber = factoryNumber;
        }

        if ( GetInfo(2,  RequestType.KkmInfo, out var softwareVersion))
        {
            status.SoftwareVersion = softwareVersion;
        }

        if ( GetInfo(21,  RequestType.KkmInfo, out var modelCode))
        {
            status.Model = modelCode;
        }

        if ( GetInfo(17,  RequestType.KkmInfo, out var sessionStart))
        {
            status.SessionStarted =  GetDateTimeFromString(sessionStart);
        }

        if ( GetInfo(14,  RequestType.KkmInfo, out var fnEnd))
        {
            status.FnDateEnd =  GetDateTimeFromString(fnEnd);
        }


        if (GetCashSum(out var cashSum))
        {
            status.CashSum = cashSum;
        }

        var versionInfo = FileVersionInfo.GetVersionInfo(PiritlibDllPath);
        status.DriverVersion = new Version(versionInfo.FileVersion ?? "1.0.0.0");

        return status;
    }

    public void OpenSession(Cashier cashier)
    {
        //открывается автоматически, метода нет в драйвере
    }

    public void GetReport(Enums.ReportTypes type, Cashier cashier)
    {
        var cashierName = PrepareCashierNameAndInn(cashier);


        var result = type switch
        {
            Enums.ReportTypes.ZReport => lib_zReport(cashierName),
            Enums.ReportTypes.XReport => lib_xReport(cashierName),
            Enums.ReportTypes.XReportWithGoods => throw new NotSupportedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), this, null)
        };

        CheckResult(result);
    }

    public void OpenReceipt(ReceiptData? receipt)
    {

        if (receipt == null)
        {
            throw new NullReferenceException();
        }


        var cashierName = receipt.Cashier.Name;

        DocTypes docType;

        if (receipt.FiscalType == Enums.ReceiptFiscalTypes.Fiscal)
        {
            docType = receipt.OperationType switch
            {
                Enums.ReceiptOperationTypes.Sale => DocTypes.SaleCheck,
                Enums.ReceiptOperationTypes.ReturnSale => DocTypes.ReturnCheck,
                Enums.ReceiptOperationTypes.Buy => throw new NotImplementedException(),
                Enums.ReceiptOperationTypes.ReturnBuy => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException()
            };

            cashierName = PrepareCashierNameAndInn(receipt.Cashier);
        }
        else
        {
            docType = DocTypes.Service;
        }

        // печать чека на бумаге 1 - вкл, 129 - выкл
        //var r = SetPrintCheck(dc.CheckPrinter.FiscalKkm.IsAlwaysNoPrintCheck ? 129 : 1);
        //CheckResult(r);

        var taxIndex = receipt.CountrySpecificData.Deserialize<Objects.CountrySpecificData.Russia.ReceiptData>()?.TaxVariantIndex ?? 0;

        var r = OpenDocument(docType, 1, cashierName, taxIndex);


        //todo
        //if (receipt.FiscalType == Enums.ReceiptFiscalTypes.Fiscal)
        //{


        //    SendDigitalCheck(checkData.AddressForDigitalCheck);

        //    //похоже надо оплачивать это
        //    // SetClientNameInn(checkData.Client);
        //}

        //CheckResult(r);

        //if (dc.CheckPrinter.FiscalKkm.FfdVersion == GlobalDictionaries.Devices.FfdVersions.Ffd120)
        //{
        //    Ffd120CodeValidation(checkData);
        //}

    }

    public void CloseReceipt()
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
        if (File.Exists( PiritlibDllPath) == false)
        {
            throw new FileNotFoundException();
        }

        if (_deviceConfig.Connection.ComPort == null)
        {
            throw new NullReferenceException();
        }

        var openPortResult =  lib_OpenPort(_deviceConfig.Connection.ComPort.PortName, _deviceConfig.Connection.ComPort.Speed);

        CheckResult(openPortResult);

        if (IsNeedInitialization())
        {
            var initResult =  KkmInitialization();
            CheckResult(initResult);
        }
    }


    private void CheckResult(int resultCode)
    {
        if (resultCode == 0)
        {
            return; //успешно выполнено
        }


        var ex = new KkmException(string.Empty, Enums.ErrorTypes.Unknown, resultCode, string.Empty);


        throw ex;
    }

    public void Disconnect()
    {
        throw new NotImplementedException();
    }

    public void CashIn(decimal sum, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void CashOut(decimal sum, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        
    }
}