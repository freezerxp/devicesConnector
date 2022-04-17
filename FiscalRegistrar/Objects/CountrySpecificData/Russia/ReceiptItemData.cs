namespace devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;

/// <summary>
/// Специфичные для РФ данные позиции чека
/// </summary>
public class ReceiptItemData
{
    /// <summary>
    /// Данные по ФФД
    /// </summary>
    public RuFfdInfo FfdData { get; set; }

    /// <summary>
    /// Информация о маркировке
    /// </summary>
    public RuMarkingInfo MarkingInfo { get; set; }
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

    /// <summary>
    /// Информация о маркировке
    /// </summary>
    public class RuMarkingInfo
    {
        /// <summary>
        /// Полный код маркировки 
        /// </summary>
        public string RawCode { get; set; }
    }
}