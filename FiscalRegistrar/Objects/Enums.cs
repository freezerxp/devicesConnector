﻿using System.Text.Json.Serialization;

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
    /// Состояние смены
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SessionStatuses
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown,

        /// <summary>
        /// Открыта
        /// </summary>
        Open,

        /// <summary>
        /// Открыта более 24 часов
        /// </summary>
        OpenMore24Hours,

        /// <summary>
        /// Закрыта
        /// </summary>
        Close
    }

    /// <summary>
    /// Состояние чека
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CheckStatuses
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown,

        /// <summary>
        /// Открыт
        /// </summary>
        Open,

        /// <summary>
        /// Закрыт
        /// </summary>
        Close
    }

    /// <summary>
    /// Состояния ККМ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KkmStatuses
    {
        /// <summary>
        /// Неизвестно
        /// </summary>
        Unknown,

        /// <summary>
        /// Готова к работе
        /// </summary>
        Ready,

        /// <summary>
        /// Нет бумаги
        /// </summary>
        NoPaper,

        /// <summary>
        /// Слишком много документов, не отправленных в ОФД
        /// </summary>
        OfdDocumentsToMany,

        /// <summary>
        /// Открыта крышка
        /// </summary>
        CoverOpen,

        /// <summary>
        /// Ошибка оборудования
        /// </summary>
        HardwareError,
        /// <summary>
        /// Необходимо допечатать документ
        /// </summary>
        NeedToContinuePrint
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
    /// Номера атрибутов ОФД
    /// </summary>
    public enum OfdAttributes
    {
        /// <summary>
        /// ИНН Кассира
        /// </summary>
        CashierInn = 1203,

        /// <summary>
        /// Адрес для отправки электронного чека
        /// </summary>
        ClientEmailPhone = 1008,

        /// <summary>
        /// Имя Кассира
        /// </summary>
        CashierName = 1021,

        /// <summary>
        /// СНО
        /// </summary>
        TaxSystem = 1055,

        /// <summary>
        /// ФИО Покупателя
        /// </summary>
        ClientName = 1227,

        /// <summary>
        /// ИНН Покупателя
        /// </summary>
        ClientInn = 1228,

        /// <summary>
        /// Единицы измерения
        /// </summary>
        UnitCode = 2108
    }

    /// <summary>
    /// Перечень вариантов ошибок ККМ
    /// </summary>
    public enum ErrorTypes
    {
        /// <summary>
        /// неизвестная ошибка
        /// </summary>
        Unknown,
        /// <summary>
        /// Необходимо обратиться в сервис
        /// </summary>
        NeedService,
        /// <summary>
        /// Нет бумаги
        /// </summary>
        NoPaper,
        /// <summary>
        /// Смена открыта более 24 часов
        /// </summary>
        SessionMore24Hour,
        /// <summary>
        /// Некорректный индекс способа оплаты
        /// </summary>
        UnCorrectPaymentIndex,
        /// <summary>
        /// Нет связи
        /// </summary>
        NoConnection,
        /// <summary>
        /// Некорректные даные
        /// </summary>
        NonCorrectData,
        /// <summary>
        /// Порт занят
        /// </summary>
        PortBusy,
        /// <summary>
        /// Крышка открыта
        /// </summary>
        CoverOpen,
        /// <summary>
        /// Слишком много не отправленных в ОФД документов
        /// </summary>
        TooManyOfflineDocuments,
        /// <summary>
        /// Некорректные дата/время в ККМ
        /// </summary>
        UnCorrectDateTime,
        /// <summary>
        /// Ошибка подключения,
        /// </summary>
        ConnectionError
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