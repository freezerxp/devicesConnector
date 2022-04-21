namespace devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;

/// <summary>
/// Специфические данные чека для РФ 
/// </summary>
public class ReceiptData
{
   

    /// <summary>
    /// Адрес для отправки электронного чека
    /// </summary>
    public string? DigitalReceiptAddress { get; set; }

    /// <summary>
    /// Индекс СНО
    /// </summary>
    public int TaxVariantIndex { get; set; }
}