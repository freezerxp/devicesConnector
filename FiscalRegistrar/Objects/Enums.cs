using System.Text.Json.Serialization;

namespace devicesConnector;

public class Enums
{
    /// <summary>
    /// Типы ККМ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KkmTypes
    {

        #region Россия

        /// <summary>
        /// АТОЛ ДТО8
        /// </summary>
        Atol8,

        /// <summary>
        /// АТОЛ ДТО10
        /// </summary>
        Atol10,

        /// <summary>
        /// АТОЛ Веб-сервер
        /// </summary>
        AtolWebServer,

        /// <summary>
        /// Штрих-М
        /// </summary>
        ShtrihM,

        /// <summary>
        /// Вики-Принт
        /// </summary>
        VikiPrint,

        /// <summary>
        /// Меркурий
        /// </summary>
        Mercury,

        /// <summary>
        /// ККМ-Сервер
        /// </summary>
        KkmServer

        #endregion



        //Другие страны
    }

    /// <summary>
    /// Тип суточного отчета
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ReportTypes
    {
        /// <summary>
        /// Z-отчет (закрытие смены)
        /// </summary>
        ZReport,

        /// <summary>
        /// х-отчет
        /// </summary>
        XReport,

        /// <summary>
        /// х-отчет с товарами
        /// </summary>
        XReportWithGoods
    }

    /// <summary>
    /// Фискальные типы чеков
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum ReceiptFiscalTypes
    {
        /// <summary>
        /// Фискальный
        /// </summary>
        Fiscal,

        /// <summary>
        /// Нефискальный
        /// </summary>
        NonFiscal,
        
        /// <summary>
        /// Сервисный
        /// </summary>
        Service
    }

    /// <summary>
    /// Тип чека по виду операции
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum ReceiptOperationTypes
    {
        /// <summary>
        /// Продажа (приход)
        /// </summary>
        Sale,

        /// <summary>
        /// Возврат продажи (прихода)
        /// </summary>
        ReturnSale,

        /// <summary>
        /// Покупка (расход)
        /// </summary>
        Buy,

        /// <summary>
        /// Возврат покупки(расхода)
        /// </summary>
        ReturnBuy
    }

    /// <summary>
    /// Страны 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Countries //для учета особенностей работы кассовой техники
    {
        /// <summary>
        /// Россия
        /// </summary>
        Russia
    }

    /// <summary>
    /// Версии ФФД
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FFdVersions
    {
        /// <summary>
        /// Оффлайн-касса
        /// </summary>
        Offline,
        /// <summary>
        /// 1.0
        /// </summary>
        Ffd100,

        /// <summary>
        /// 1.05
        /// </summary>
        Ffd105,

        /// <summary>
        /// 1.1
        /// </summary>
        Ffd110,

        /// <summary>
        /// 1.2
        /// </summary>
        Ffd120
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
}