using devicesConnector.Common;

namespace devicesConnector.FiscalRegistrar.Commands;

/// <summary>
/// Абстрактный класс команды к ККМ
/// </summary>
public abstract class KkmCommand : DeviceCommand
{
    /// <summary>
    /// Тип ККМ
    /// </summary>
    public Enums.KkmTypes KkmType { get; set; }

 
}