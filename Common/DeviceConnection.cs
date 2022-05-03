using devicesConnector.Helpers;
using System.IO.Ports;
using System.Text.Json.Serialization;

namespace devicesConnector.Common;

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
        public SerialPort SerialPort { get; set; }

        private AutoResetEvent ReceiveNow { get; set; }

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
        public Handshake Handshake { get; set; } = Handshake.None;

        /// <summary>
        /// Стоп биты
        /// </summary>
        public StopBits StopBit { get; set; } = StopBits.One;

        #region Работа с COM портом

        /// <summary>
        /// Настроить COM порт
        /// </summary>
        public void SetSerialPort()
        {
            this.SerialPort = new SerialPort(this.PortName, this.Speed)
            {
                Parity = this.Parity,
                Handshake = this.Handshake,
                DataBits = this.DataBit,
                StopBits = this.StopBit,
                RtsEnable = true,
                DtrEnable = true,
                WriteTimeout = this.TimeOut,
                ReadTimeout = this.TimeOut
            };
        }

        /// <summary>
        /// Открыть COM порт
        /// </summary>
        /// <returns>При успешном открытии вернет true, иначе false</returns>
        public bool Open()
        {
            try
            {
                if (SerialPort == null)
                    return false;
                
                if (SerialPort.IsOpen)
                {
                    if (!Close())
                    {
                        LogHelper.Write("Не удалось закрыть COM порт");
                        return false;
                    }
                }

                this.ReceiveNow = new AutoResetEvent(false);
                SerialPort.Open();
                SerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
                LogHelper.Write("Успешное открытие COM порта");
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Write("Не удалось открыть COM порт");
                return false;
            }
        }

        /// <summary>
        /// Закрываем COM порт
        /// </summary>
        /// <returns>Если успешно вернёт true, иначе false</returns>
        public bool Close()
        {
            try
            {
                if (SerialPort == null)
                    return false;

                if (!SerialPort.IsOpen)
                    return false;

                SerialPort.DataReceived -= SerialPort_DataReceived;
                SerialPort.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                {
                    ReceiveNow.Set();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public byte[] ExecuteCommand(byte[] cmd, int countByte = 1)
        {
            try
            {
                SerialPort.DiscardOutBuffer();
                SerialPort.DiscardInBuffer();
                ReceiveNow.Reset();
                SerialPort.Write(cmd, 0, cmd.Length);
                SerialPort.ReadTimeout = TimeOut;

                byte[] input = ReadResponse(countByte);
                return input;
            }
            catch
            {
                return null;
            }
        }

        private byte[] ReadResponse(int byteCount)
        {
            byte[] buffer = new byte[byteCount];
            try
            {
                List<byte> listByte = new List<byte>();
                while (listByte.Count < byteCount)
                {
                    byte b = (byte)SerialPort.ReadByte();
                    listByte.Add(b);
                }
                buffer = listByte.ToArray();

                return buffer;
            }
            catch (Exception ex)
            {
                LogHelper.Write($"Method [ComPortConnection.ReadResponse]; {ex.Message}");
                return buffer;
            }
        }

        #endregion
    }
}