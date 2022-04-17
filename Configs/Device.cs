using devicesConnector.Common;

namespace devicesConnector.Configs;

/// <summary>
/// Класс настройки устройств
/// </summary>
public class Device
{

    /// <summary>
    /// Идентификатор устройства
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Имя устройства
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Тип устройства
    /// </summary>
    public Common.Enums.DeviceTypes Type { get; set; }

    /// <summary>
    /// Подтип, например AtolDto10 для ККМ
    /// <see cref="FiscalRegistrar.Objects.Enums.KkmTypes"/>
    /// </summary>
    public int SubType { get; set; }

    /// <summary>
    /// Параметры подключения
    /// </summary>
    public DeviceConnection Connection { get; set; }
}