namespace devicesConnector.FiscalRegistrar.Commands;

/// <summary>
/// Команда открытия смены на ККМ
/// </summary>
public class PrintTextCommand : KkmCommand
{
    /// <summary>
    /// Строки для печати
    /// </summary>
    public List<string> Lines { get; set; }
}