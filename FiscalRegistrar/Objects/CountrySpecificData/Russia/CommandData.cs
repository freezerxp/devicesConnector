namespace devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;

/// <summary>
/// Специфичные для РФ данные команды к ККМ
/// </summary>
public class CommandData
{
    /// <summary>
    /// Версия ФФД
    /// </summary>
    public Enums.FFdVersions FfdVersion { get; set; }
}