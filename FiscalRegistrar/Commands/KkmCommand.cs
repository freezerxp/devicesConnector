using System.Text.Json.Nodes;
using devicesConnector.Common;

namespace devicesConnector.FiscalRegistrar.Commands;

/// <summary>
/// Абстрактный класс команды к ККМ
/// </summary>
public abstract class KkmCommand : DeviceCommand
{

    /// <summary>
    /// Специфичные для страны данные
    /// </summary>
    public JsonNode? CountrySpecificData { get; set; }

 
}