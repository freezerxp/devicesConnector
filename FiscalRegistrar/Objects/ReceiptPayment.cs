namespace devicesConnector.FiscalRegistrar.Objects;

/// <summary>
/// Платеж в кассовом чеке
/// </summary>
public class ReceiptPayment
{
    /// <summary>
    /// Сумма платежа
    /// </summary>
    public decimal Sum { get; set; }

    /// <summary>
    /// Индекс способа оплаты в ККМ
    /// </summary>
    public int Index { get; set; }
}