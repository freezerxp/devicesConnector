namespace devicesConnector.Common
{
    public class Enums
    {
        /// <summary>
        /// Типы устройств
        /// </summary>
        public enum DeviceTypes
        {
            /// <summary>
            /// ККМ, фискальный регистратор
            /// </summary>
            FiscalRegistrar = 1,

            /// <summary>
            /// Весы
            /// </summary>
            Scale = 2,

            /// <summary>
            /// Весы с печатью этикеток
            /// </summary>
            ScaleWithPrinter = 3,

            /// <summary>
            /// Эквайринг-терминал
            /// </summary>
            AcquiringTerminal = 4
        }
    }
}