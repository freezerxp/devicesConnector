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
        public Enums.CheckStatuses CheckStatus { get; set; } = Enums.CheckStatuses.Unknown;

        /// <summary>
        /// Состояние смены
        /// </summary>
        public Enums.SessionStatuses SessionStatus { get; set; } = Enums.SessionStatuses.Unknown;

        /// <summary>
        /// Статус ККМ
        /// </summary>
        public Enums.KkmStatuses KkmState { get; set; } = Enums.KkmStatuses.Unknown;

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

        /// <summary>
        /// Информация по РФ ККМ
        /// </summary>
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
    }
}