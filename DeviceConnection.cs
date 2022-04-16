using System.Text.Json.Serialization;

namespace devicesConnector;

using System.IO.Ports;

public class DeviceConnection
{
    /// <summary>
    /// Тип подключения
    /// </summary>
    public ConnectionTypes ConnectionType { get; set; } = ConnectionTypes.NotSet;

    /// <summary>
    /// Ком-порт
    /// </summary>
    public ComPortConnection? ComPort { get; set; } 

    /// <summary>
    /// Лан-порт
    /// </summary>
    public LanConnection? Lan { get; set; } 

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ConnectionTypes
    {
        /// <summary>
        /// Не указан, или не требуется указывать
        /// </summary>
        NotSet,

        /// <summary>
        /// Com-порт
        /// </summary>
        ComPort,

        /// <summary>
        /// Локальная сеть
        /// </summary>
        Lan
    }

    public class LanConnection
    {
        public string HostUrl { get; set; } = string.Empty;

        public int? PortNumber { get; set; }

        public string UserLogin { get; set; } = string.Empty;

        public string UserPassword { get; set; } = string.Empty;

    }

    /// <summary>
    /// Ком-порт
    /// </summary>
    public class ComPortConnection
    {
        /// <summary>
        /// Имя порта
        /// </summary>
        public string PortName { get; set; } = @"COM1";

        /// <summary>
        /// Номер порта
        /// </summary>
        public int PortNumber => int.Parse(string.Join("", PortName.Where(char.IsNumber)));

        /// <summary>
        /// Скорость порта
        /// </summary>
        public int Speed { get; set; } = 9_600;

        /// <summary>
        /// Тайм-аут ожидания ответа
        /// </summary>
        public int TimeOut { get; set; } = 3_000;

        /// <summary>
        /// Биты данных
        /// </summary>
        public int DataBit { get; set; } = 8;

        /// <summary>
        /// Четность
        /// </summary>
        public Parity Parity { get; set; } = Parity.None;

        /// <summary>
        /// Управление
        /// </summary>
        public Handshake Handshake { get; set; }

        /// <summary>
        /// Стоп биты
        /// </summary>
        public StopBits StopBit { get; set; } = StopBits.One;
    }
}