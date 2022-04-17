namespace devicesConnector.FiscalRegistrar.Commands;

/// <summary>
/// Команда печати фискального чека
/// </summary>
public class KkmPrintFiscalReceiptCommand: KkmCommand
{
    /// <summary>
    /// Данные чека
    /// </summary>
    public ReceiptData ReceiptData { get; set; }
}