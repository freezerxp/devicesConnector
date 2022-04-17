using devicesConnector.Common;
using devicesConnector.FiscalRegistrar.Objects;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices;

/// <summary>
/// Интерфейс фискальных регистраторов
/// </summary>
public interface IFiscalRegistrarDevice: IDevice
{

    /// <summary>
    /// Получить статус ККМ
    /// </summary>
    /// <returns></returns>
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
    /// Открыть чек
    /// </summary>
    /// <param name="receipt">Данные чека</param>
    public void OpenReceipt(ReceiptData? receipt);

    /// <summary>
    /// Закрытие чека
    /// </summary>
    public void CloseReceipt ();

    /// <summary>
    /// Регистрация позиции в чеке
    /// </summary>
    /// <param name="item">Позиция чека</param>
    public void RegisterItem(ReceiptItem item);

    /// <summary>
    /// Регистрация платежа 
    /// </summary>
    /// <param name="payment">Платеж</param>
    public void RegisterPayment(ReceiptPayment payment);

    /// <summary>
    /// Печать нефискального текста
    /// </summary>
    /// <param name="text">Строка текста для печати</param>
    public void PrintText( string text);

    /// <summary>
    /// Подключение к ККМ
    /// </summary>
    public void Connect();

    /// <summary>
    /// Отключение от устройства
    /// </summary>
    public void Disconnect();


    

    /// <summary>
    /// Внести наличные
    /// </summary>
    public void CashIn(decimal sum, Cashier cashier);

    /// <summary>
    /// Изъять наличные
    /// </summary>
    public void CashOut(decimal sum, Cashier cashier);
}