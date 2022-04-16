using System.Text.Json.Serialization;

namespace devicesConnector;

public class RuKkmInfo
{
    public RuFfdInfo FfdData { get; set; }


}

/// <summary>
/// 
/// </summary>
public class RuFfdInfo
{
    public FfdCalculationSubjects Subject { get; set; }

    public FfdCalculationMethods Method { get; set; }
}

/// <summary>
/// Признак способа расчета (1214)
/// </summary>

public enum FfdCalculationMethods
{
    None,

    /// <summary>
    /// Полная предоплата (100%)
    /// </summary>
    PrePaymentFull,

    /// <summary>
    /// Предоплата
    /// </summary>
    Prepayment,

    /// <summary>
    /// Аванс
    /// </summary>
    AdvancePayment,

    /// <summary>
    /// Полный расчет
    /// </summary>
    FullPayment,

    /// <summary>
    /// Частичная оплата и кредит
    /// </summary>
    PartPaymentAndCredit,

    /// <summary>
    /// Передача в кредит (полностью)
    /// </summary>
    FullCredit,

    /// <summary>
    /// Платеж по кредиту
    /// </summary>
    PaymentForCredit
}

/// <summary>
/// Признак предмета расчета (1212)
/// </summary>

public enum FfdCalculationSubjects
{
    /// <summary>
    /// Не указано
    /// </summary>
    None,

    /// <summary>
    /// Товар, кроме подакцизных
    /// </summary>
    SimpleGood,

    /// <summary>
    /// Подакцизный товар
    /// </summary>
    ExcisableGood,

    /// <summary>
    /// Работа
    /// </summary>
    Work,

    /// <summary>
    /// Услуга
    /// </summary>
    Service,

    /// <summary>
    /// Ставка азартной игры
    /// </summary>
    GamePayment,

    /// <summary>
    /// Выигрыш азартной игры
    /// </summary>
    GameWin,

    /// <summary>
    /// Лотерейный билет
    /// </summary>
    LotteryPayment,

    /// <summary>
    /// Выигрыш в лотерею
    /// </summary>
    LotteryWin,

    /// <summary>
    /// Результат интеллектуальной деятельности
    /// </summary>
    Rid,

    /// <summary>
    /// Платеж
    /// </summary>
    Payment,

    /// <summary>
    /// Агентское вознаграждение
    /// </summary>
    AgentPayment,

    /// <summary>
    /// Выплата
    /// </summary>
    WithPayment,

    /// <summary>
    /// Иной предмет расчета
    /// </summary>
    Other,

    /// <summary>
    /// Имущественное право
    /// </summary>
    PropertyLaw,

    /// <summary>
    /// Внереализационный доход
    /// </summary>
    NonOperatingIncome,

    /// <summary>
    /// Иные платежи и взносы
    /// </summary>
    OtherPayment,

    /// <summary>
    /// Торговый сбор
    /// </summary>
    TradeFee,

    /// <summary>
    /// Курортный сбор
    /// </summary>
    ResortFee,

    /// <summary>
    /// Залог
    /// </summary>
    Deposit,

    /// <summary>
    /// Расход
    /// </summary>
    Expenditure,

    /// <summary>
    /// Взносы на ОПС ИП
    /// </summary>
    PensionInsuranceIP,

    /// <summary>
    /// Взносы на ОПС
    /// </summary>
    PensionInsurance,

    /// <summary>
    /// Взносы на ОМС ИП
    /// </summary>
    MedicalInsuranceIP,

    /// <summary>
    /// Взносы на ОМС 
    /// </summary>
    MedicalInsurance,

    /// <summary>
    /// Взносы на ОСС
    /// </summary>
    SocialInsurance,

    /// <summary>
    /// Платеж казино
    /// </summary>
    CasinoPayment,

    /// <summary>
    /// Выдача денежных средств
    /// </summary>
    OutOfFunds,

    /// <summary>
    /// АТНМ
    /// </summary>
    Atnm = 30,

    /// <summary>
    /// АТМ
    /// </summary>
    Atm,

    /// <summary>
    /// ТНМ
    /// </summary>
    Tnm,

    /// <summary>
    /// ТМ
    /// </summary>
    Tm
}