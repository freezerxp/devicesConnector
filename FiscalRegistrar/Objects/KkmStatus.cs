using System.Text.Json.Serialization;

namespace devicesConnector
{
    /// <summary>
    /// Статус ККМ
    /// </summary>
    public class KkmStatus
    {
    
        /// <summary>
        /// Состояние чека
        /// </summary>
        public CheckStatuses CheckStatus { get; set; } = CheckStatuses.Unknown;

        /// <summary>
        /// Состояние смены
        /// </summary>
        public SessionStatuses SessionStatus { get; set; } = SessionStatuses.Unknown;

        /// <summary>
        /// Статус ККМ
        /// </summary>
        public KkmStatuses KkmState { get; set; } = KkmStatuses.Unknown;

        /// <summary>
        /// Версия драйвера
        /// </summary>
        public Version DriverVersion { get; set; } = new Version(@"1.0.0.0");

        /// <summary>
        /// Номер текущего (если открыт) или следующего чека
        /// </summary>
        public int CheckNumber { get; set; } = 1;

        /// <summary>
        /// Номер текущей смены (если открыта) или следующей
        /// </summary>
        public int SessionNumber { get; set; } = 1;

        /// <summary>
        /// Начало смены (если открыта)
        /// </summary>
        public DateTime? SessionStarted { get; set; }

        /// <summary>
        /// Сумма наличности в ККМ
        /// </summary>
        public decimal CashSum { get; set; }

        /// <summary>
        /// Заводской номер
        /// </summary>
        public string FactoryNumber { get; set; }

        /// <summary>
        /// Версия прошивки
        /// </summary>
        public string SoftwareVersion { get; set; }

        /// <summary>
        /// Название модели
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Дата окончания НФ
        /// </summary>
        public DateTime FnDateEnd { get; set; }


        public RuKkm? RuKkmInfo { get; set; }

    

        /// <summary>
        /// Информация по РФ ККМ
        /// </summary>
        public class RuKkm
        {
            /// <summary>
            /// Последняя отправка данных в ОФД (если есть не отправленные)
            /// </summary>
            public DateTime OfdLastSendDateTime { get; set; }

            /// <summary>
            /// Кол-во не отправленных документов в ОФД
            /// </summary>
            public int OfdNotSendDocuments { get; set; }
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
    }
}