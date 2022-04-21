using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Devices.Russia;
using devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices;

public class FiscalRegistrarFacade:IDisposable
{


    private IFiscalRegistrarDevice _kkm;

    public FiscalRegistrarFacade(Device device)
    {


        switch ((Enums.KkmTypes)device.SubType)
        {
            case Enums.KkmTypes.Atol8:
                break;
            case Enums.KkmTypes.Atol10:
                _kkm = new AtolDto10(device);
                break;
            case Enums.KkmTypes.AtolWebServer:
                break;
            case Enums.KkmTypes.ShtrihM:
                break;
            case Enums.KkmTypes.VikiPrint:
                _kkm = new VikiPrint(device);
                break;
            case Enums.KkmTypes.Mercury:
                break;
            case Enums.KkmTypes.KkmServer:
                _kkm = new KkmServer(device);
                break;
            case Enums.KkmTypes.PortDriverRu:
                _kkm = new PortDriverRu(device);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(device.SubType), device.SubType, null);
        }

        _kkm.Connect();

    }






    public KkmStatus GetStatus()
    {

        //todo: подключение к ккм

        return _kkm.GetStatus();

    }
    public void CancelReceipt()
    {

        //todo: подключение к ккм

        _kkm.CancelReceipt();

    }
    public void OpenSession(Cashier cashier)
    {
        //todo: connection

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
        //todo: connection
        _kkm.GetReport (type, cashier);
    }

    public void PrintFiscalReceipt(ReceiptData receipt)
    {
        
        //todo: проверка ККМ на готовность

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

        //закрытие чека

        _kkm.CloseReceipt();

        //todo: отрезка чека
    }

    public void Dispose()
    {
        _kkm?.Disconnect();
    }
}