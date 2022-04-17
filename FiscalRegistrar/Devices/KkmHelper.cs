using System.Text.Json.Serialization;
using devicesConnector.Drivers;
using devicesConnector.FiscalRegistrar.Devices.Russia;

namespace devicesConnector.FiscalRegistrar.Devices;

public class KkmHelper
{

    private IFiscalRegistrarDevice _kkm;

    public KkmHelper(DeviceConnection connection, KkmTypes kkmType)
    {


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


    public KkmStatus GetStatus()
    {

        //todo: подключение к ккм

        return _kkm.GetStatus();

    }

    public void OpenSession(Cashier cashier)
    {
        //todo: connection

        _kkm.OpenSession(cashier);
    }

    public void CashInOut(decimal sum, Cashier cashier)
    {
        switch (sum)
        {
            case > 0:
                _kkm.CashIn(sum, cashier);
                break;
            case < 0:
                sum = Math.Abs(sum); //передаем положительное значение для снятия
                _kkm.CashOut(sum, cashier);
                break;
        }
    }

    public void GetReport(ReportTypes type, Cashier cashier)
    {
        //todo: connection
        _kkm.GetReport (type, cashier);
    }


public void PrintNonFiscalReceipt(List<string> data)
    {

    }

    
}