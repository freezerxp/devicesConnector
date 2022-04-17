using devicesConnector.Common;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Devices.Russia;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices;

public class FiscalRegistrarFacade
{

    private IFiscalRegistrarDevice _kkm;

    public FiscalRegistrarFacade(Device device)
    {


        switch ((Enums.KkmTypes)device.SubType)
        {
            case Enums.KkmTypes.Atol8:
                break;
            case Enums.KkmTypes.Atol10:
                break;
            case Enums.KkmTypes.AtolWebServer:
                break;
            case Enums.KkmTypes.ShtrihM:
                break;
            case Enums.KkmTypes.VikiPrint:
                break;
            case Enums.KkmTypes.Mercury:
                break;
            case Enums.KkmTypes.KkmServer:
                _kkm = new KkmServerDevice(device.Connection.Lan);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(device.SubType), device.SubType, null);
        }

    }






    public KkmStatus GetStatus()
    {

        //todo: подключение к ккм

        return _kkm.GetStatus();

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

}