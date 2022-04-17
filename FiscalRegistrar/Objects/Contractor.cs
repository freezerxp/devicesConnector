namespace devicesConnector.FiscalRegistrar.Objects;

/// <summary>
/// Контрагент
/// </summary>
public class Contractor
{
    /// <summary>
    /// Фио
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Налоговый идентификатора (ИНН)
    /// </summary>
    public string TaxId { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Телефон
    /// </summary>
    public string Phone { get; set; }
}