using devicesConnector.Common;
using devicesConnector.FiscalRegistrar.Devices;

namespace devicesConnector.Drivers;

public interface IFiscalRegistrarDevice: IDevice
{
    public void PrintNonFiscalString(string str);

    public KkmStatus GetStatus();

    /// <summary>
    /// Открыть смену
    /// </summary>
    public void OpenSession(Cashier cashier);

    /// <summary>
    /// Снять отчет
    /// </summary>
    public void GetReport(Enums.ReportTypes type, Cashier cashier);

    /// <summary>
    /// Напечатать фискальный чек
    /// </summary>
    public void PrintFiscalReceipt();

    /// <summary>
    /// Напечатать НЕфискальный чек
    /// </summary>
    public void PrintNonFiscalReceipt();

    /// <summary>
    /// Внести наличные
    /// </summary>
    public void CashIn(decimal sum, Cashier cashier);

    /// <summary>
    /// Изъять наличные
    /// </summary>
    public void CashOut(decimal sum, Cashier cashier);
}