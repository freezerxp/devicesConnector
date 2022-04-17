namespace devicesConnector.FiscalRegistrar.Objects;

/// <summary>
/// Интерфейс специфичных для региона данных
/// </summary>
public interface ICountrySpecificReceiptItemData
{

}

/// <summary>
/// Специфичные для РФ данные позиции чека
/// </summary>
public class RuReceiptItemData: ICountrySpecificReceiptItemData
{
    /// <summary>
    /// Данные по ФФД
    /// </summary>
    public RuFfdInfo FfdData { get; set; }

    /// <summary>
    /// Данные по ФФД
    /// </summary>
    public class RuFfdInfo
    {

        /// <summary>
        /// Признак предмета расчета (тег 1212)
        /// </summary>
        public Enums.FfdCalculationSubjects Subject { get; set; }

        /// <summary>
        /// Признак способа расчета (тег 1214)
        /// </summary>
        public Enums.FfdCalculationMethods Method { get; set; }
    }
}




/// <summary>
/// Данные чека, специфичные для региона
/// </summary>
public interface ICountrySpecificReceiptData
{

}


/// <summary>
/// Специфические данные чека для РФ 
/// </summary>
public class RuReceiptData : ICountrySpecificReceiptData
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