namespace devicesConnector.FiscalRegistrar.Commands;

/// <summary>
/// Команда внесения/изъятия наличных из ККМ
/// </summary>
public class KkmCashInOutCommand : KkmCommand
{
    /// <summary>
    /// Кассир
    /// </summary>
    public Cashier Cashier { get; set; }

    /// <summary>
    /// Сумма. Положительная - внесение, отрицательная - изъятие
    /// </summary>
    public decimal Sum { get; set; }
}