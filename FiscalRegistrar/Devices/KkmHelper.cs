using System.Text.Json.Serialization;
using devicesConnector.FiscalRegistrar.Drivers.Russia;

namespace devicesConnector.Drivers;

public class KkmHelper
{
    private DeviceConnection _connection;
    private IFiscalRegistrarDevice _kkm;

    public KkmHelper(DeviceConnection connection, KkmTypes kkmType)
    {
        _connection = connection;

        switch (kkmType)
        {
            case KkmTypes.Atol8:
                break;
            case KkmTypes.Atol10:
                break;
            case KkmTypes.AtolWebServer:
                break;
            case KkmTypes.ShtrihM:
                break;
            case KkmTypes.VikiPrint:
                break;
            case KkmTypes.Mercury:
                break;
            case KkmTypes.KkmServer:
                _kkm = new KkmServerDevice(connection.Lan);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kkmType), kkmType, null);
        }

    }

    /// <summary>
    /// Типы ККМ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum KkmTypes
    {
        //Россия

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



        //Другие страны
    }


    public KkmStatus GetStatus()
    {
        return _kkm.GetStatus();

    }


    public void PrintNonFiscalReceipt(List<string> data)
    {

    }

    
}