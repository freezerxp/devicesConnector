using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Devices.Russia;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices;

public class FiscalRegistrarFacade : IDisposable
{
    private readonly IFiscalRegistrarDevice _kkm;

    public FiscalRegistrarFacade(Device device)
    {
        _kkm = (Enums.KkmTypes) device.SubType switch
        {
            Enums.KkmTypes.Atol8 =>
                throw new NotSupportedException() //не поддерживается производителем, нет смысла в реализации
            ,
            Enums.KkmTypes.Atol10 => new AtolDto10(device),
            Enums.KkmTypes.AtolWebServer => throw new NotImplementedException(),
            Enums.KkmTypes.ShtrihM => new ShtihM(device),
            Enums.KkmTypes.VikiPrint => new VikiPrint(device),
            Enums.KkmTypes.Mercury => throw new NotImplementedException(),
            Enums.KkmTypes.KkmServer => new KkmServer(device),
            Enums.KkmTypes.PortDriverRu => new PortDriverRu(device),
            _ => throw new ArgumentOutOfRangeException(nameof(device.SubType), device.SubType, null)
        };

        if (_kkm == null)
        {
            throw new NullReferenceException();
        }

        _kkm.Connect();
    }


    public KkmStatus GetStatus()
    {
        return _kkm.GetStatus();
    }

    public void CancelReceipt()
    {
        _kkm.CancelReceipt();
    }
    public void PrintText(List<string> lines)
    {
        foreach (var line in lines)
        {
            _kkm.PrintText(line);
        }
    }
    public void OpenSession(Cashier cashier)
    {
        _kkm.OpenSession(cashier);
    }

    public void CashInOut(decimal sum, Cashier cashier)
    {
        switch (sum)
        {
            case > 0:
                _kkm.CashIn(sum, cashier);
                break;
            case < 0:
                sum = Math.Abs(sum); //передаем положительное значение для снятия
                _kkm.CashOut(sum, cashier);
                break;
        }
    }

    public void GetReport(Enums.ReportTypes type, Cashier cashier)
    {
        _kkm.GetReport(type, cashier);
    }

    public void PrintFiscalReceipt(ReceiptData receipt)
    {
        //открываем чек
        _kkm.OpenReceipt(receipt);

        //регистрация товаров
        foreach (var item in receipt.Items)
        {
            _kkm.RegisterItem(item);
        }

        //todo: регистрация скидок на чек

        //регистрация платежей

        foreach (var payment in receipt.Payments)
        {
            _kkm.RegisterPayment(payment);
        }

     

        _kkm.CloseReceipt();
    }

    public void CutPaper()
    {
        throw new NotImplementedException();
    }

    public void OpenCashBox()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _kkm?.Disconnect();
    }
}