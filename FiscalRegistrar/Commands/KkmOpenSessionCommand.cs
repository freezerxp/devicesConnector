namespace devicesConnector.FiscalRegistrar.Commands;

/// <summary>
/// Команда открытия смены на ККМ
/// </summary>
public class KkmOpenSessionCommand : KkmCommand
{
    /// <summary>
    /// Кассир, открывающий смену
    /// </summary>
    public Cashier Cashier { get; set; } = new Cashier
    {
        Name = "Кассир",
        TaxId = new string('0',12)
    };
}