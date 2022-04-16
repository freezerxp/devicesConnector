using System.Text.Json.Serialization;

namespace devicesConnector;

/// <summary>
/// Кассир
/// </summary>
public class Cashier
{
    /// <summary>
    /// Имя
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    /// <summary>
    /// Налоговый идентификатор (ИНН)
    /// </summary>
    [JsonPropertyName("TaxId")]
    public string? TaxId { get; set; }
}