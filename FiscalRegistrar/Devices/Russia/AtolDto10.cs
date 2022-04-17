using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using devicesConnector.Common;
using devicesConnector.Configs;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.Helpers;
using Enums = devicesConnector.FiscalRegistrar.Objects.Enums;

namespace devicesConnector.FiscalRegistrar.Devices.Russia;

public class AtolDto10 : IFiscalRegistrarDevice
{
    /// <summary>
    /// Драйвер
    /// </summary>
    private dynamic? _driver;

    /// <summary>
    /// Настройки устройства
    /// </summary>
    private Device _deviceConfig;
    

    public AtolDto10(Device device)
    {
        _deviceConfig = device;
    }


    public KkmStatus GetStatus()
    {
        //получаем состояние ККМ
        var status = new KkmStatus {KkmState = GetKkmState()};


        //получаем информацию о смене
        _driver.setParam(_driver.LIBFPTR_PARAM_DATA_TYPE, _driver.LIBFPTR_DT_SHIFT_STATE);
        _driver.queryData();

        status.SessionStatus = _driver.getParamInt(_driver.LIBFPTR_PARAM_SHIFT_STATE) switch
        {
            0 => Enums.SessionStatuses.Close,
            1 => Enums.SessionStatuses.Open,
            2 => Enums.SessionStatuses.OpenMore24Hours,
            _ => status.SessionStatus
        };

        status.SessionNumber = _driver.getParamInt(_driver.LIBFPTR_PARAM_SHIFT_NUMBER);
        status.SessionStarted =
            ((DateTime) _driver.getParamDateTime(_driver.LIBFPTR_PARAM_DATE_TIME)).AddHours(-24);

        //получаем информацию о чеке
        _driver.setParam(_driver.LIBFPTR_PARAM_DATA_TYPE, _driver.LIBFPTR_DT_RECEIPT_STATE);
        _driver.queryData();

        status.CheckStatus =
            _driver.getParamInt(_driver.LIBFPTR_PARAM_RECEIPT_TYPE) == (int) _driver.LIBFPTR_RT_CLOSED
                ? Enums.CheckStatuses.Close
                : Enums.CheckStatuses.Open;

        status.CheckNumber = (int) _driver.getParamInt(_driver.LIBFPTR_PARAM_RECEIPT_NUMBER) + 1;

        //версия драйвера
        status.DriverVersion = new Version(_driver.version().ToString());


        //информация о ККМ: прошивка, модель, ЗН
        _driver.setParam(_driver.LIBFPTR_PARAM_DATA_TYPE, _driver.LIBFPTR_DT_STATUS);
        _driver.queryData();

        status.FactoryNumber = _driver.getParamString(_driver.LIBFPTR_PARAM_SERIAL_NUMBER);
        status.Model = _driver.getParamString(_driver.LIBFPTR_PARAM_MODEL_NAME);
        status.SoftwareVersion = _driver.getParamString(_driver.LIBFPTR_PARAM_UNIT_VERSION);

        //информация об обмене с ОФД
        _driver.setParam(_driver.LIBFPTR_PARAM_FN_DATA_TYPE, _driver.LIBFPTR_FNDT_OFD_EXCHANGE_STATUS);
        _driver.fnQueryData();

        status.RuKkmInfo = new KkmStatus.RuKkm()
        {
            OfdNotSendDocuments = _driver.getParamInt(_driver.LIBFPTR_PARAM_DOCUMENTS_COUNT),
            OfdLastSendDateTime = _driver.getParamDateTime(_driver.LIBFPTR_PARAM_DATE_TIME)
        };


        //дата окончания ФН
        _driver.setParam(_driver.LIBFPTR_PARAM_FN_DATA_TYPE, _driver.LIBFPTR_FNDT_VALIDITY);
        _driver.fnQueryData();

        status.FnDateEnd = _driver.getParamDateTime(_driver.LIBFPTR_PARAM_DATE_TIME);

        return status;
    }


    private Enums.KkmStatuses GetKkmState()
    {
        var s = Enums.KkmStatuses.Ready;

        // запрашиваю статус

        //проверят открыта ли крышка
        _driver.setParam(_driver.LIBFPTR_PARAM_DATA_TYPE, _driver.LIBFPTR_DT_SHORT_STATUS);
        _driver.queryData();

        if (_driver.getParamBool(_driver.LIBFPTR_PARAM_COVER_OPENED))
        {
            return Enums.KkmStatuses.CoverOpen;
        }

        //наличие бумаги
        if (_driver.getParamBool(_driver.LIBFPTR_PARAM_RECEIPT_PAPER_PRESENT) == false)
        {
            return Enums.KkmStatuses.NoPaper;
        }

        //проверяем дату последней отправки в ОФД
        _driver.setParam(_driver.LIBFPTR_PARAM_FN_DATA_TYPE, _driver.LIBFPTR_FNDT_OFD_EXCHANGE_STATUS);
        _driver.fnQueryData();

        var noSendDocs = _driver.getParamInt(_driver.LIBFPTR_PARAM_DOCUMENTS_COUNT);

        if (noSendDocs > 0)
        {
            if ((DateTime.Now - _driver.getParamDateTime(_driver.LIBFPTR_PARAM_DATE_TIME)).TotalDays > 28)
            {
                return Enums.KkmStatuses.OfdDocumentsToMany;
            }
        }


        return s;
    }

    public void OpenSession(Cashier cashier)
    {
        OperatorLogin(cashier);

        _driver.openShift();
        _driver.checkDocumentClosed();

        CheckResult();
    }

    private void OperatorLogin(Cashier cashier)
    {
        WriteOfdAttribute((int) Enums.OfdAttributes.CashierName, cashier.Name);

        var ffdV = _deviceConfig.DeviceSpecificConfig.Deserialize<Objects.CountrySpecificData.Russia.KkmConfig>()?.FfdVersion;

        if (ffdV > Enums.FFdVersions.Ffd100 && cashier.TaxId != null)
        {
            WriteOfdAttribute((int) Enums.OfdAttributes.CashierInn, cashier.TaxId);
        }

        _driver.operatorLogin();
    }

    /// <summary>
    /// Запись атрибута офд
    /// </summary>
    /// <param name="ofdAttributeNumber">Номер атрибута</param>
    /// <param name="value">значение атрибута</param>
    private void WriteOfdAttribute(int ofdAttributeNumber, object value)
    {
        if (string.IsNullOrEmpty(value.ToString()))
        {
            return;
        }

        _driver.setParam(ofdAttributeNumber, value);

        CheckResult();
    }

    private void CheckResult()
    {
        var resultCode = (int) _driver.errorCode;

        if (resultCode == 0)
        {
            return; //успешно выполнено
        }

        if (resultCode == 177) //ожидание команды продолжения печати (но это не точно)
        {
            _driver.continuePrint();
            resultCode = (int) _driver.errorCode;


            if (resultCode == 0)
            {
                return;
            }
        }

        var kkmResultDescription = (string) _driver.errorDescription;


        var errType = resultCode switch
        {
            1 => Enums.ErrorTypes.NoConnection, //соединение не установлено
            2 => Enums.ErrorTypes.NoConnection,
            3 => Enums.ErrorTypes.PortBusy,
            4 => Enums.ErrorTypes.NoConnection, //порт недоступен
            5 => Enums.ErrorTypes.NonCorrectData,
            35 => Enums.ErrorTypes.UnCorrectDateTime, //Проверьте дату и время
            44 => Enums.ErrorTypes.NoPaper,
            45 => Enums.ErrorTypes.NoPaper,
            47 => Enums.ErrorTypes.NeedService,
            68 => Enums.ErrorTypes.SessionMore24Hour,
            137 => Enums.ErrorTypes.TooManyOfflineDocuments,
            _ => Enums.ErrorTypes.Unknown
        };

        throw new KkmException(null, errType, resultCode, kkmResultDescription);
    }

    public void GetReport(Enums.ReportTypes type, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void OpenReceipt(ReceiptData? receipt)
    {
        throw new NotImplementedException();
    }

    public void CloseReceipt()
    {
        throw new NotImplementedException();
    }

    public void RegisterItem(ReceiptItem item)
    {
        throw new NotImplementedException();
    }

    public void RegisterPayment(ReceiptPayment payment)
    {
        throw new NotImplementedException();
    }

    public void PrintText(string text)
    {
        throw new NotImplementedException();
    }

    public void Connect()
    {
        _driver = CommonHelper.CreateObject(@"AddIn.Fptr10");

        if (_driver == null)
        {
            throw new NullReferenceException();
        }


        if (_deviceConfig.Connection.ConnectionType == DeviceConnection.ConnectionTypes.ComPort)
        {
            if (_deviceConfig.Connection.ComPort == null)
            {
                throw new NullReferenceException();
            }

            _driver.setSingleSetting(_driver.LIBFPTR_SETTING_MODEL, _driver.LIBFPTR_MODEL_ATOL_AUTO.ToString());
            _driver.setSingleSetting(_driver.LIBFPTR_SETTING_PORT, _driver.LIBFPTR_PORT_COM.ToString());
            _driver.setSingleSetting(_driver.LIBFPTR_SETTING_COM_FILE, $"{_deviceConfig.Connection.ComPort.PortName}");
            _driver.setSingleSetting(_driver.LIBFPTR_SETTING_BAUDRATE, _driver.LIBFPTR_PORT_BR_115200.ToString());
            _driver.applySingleSettings();
        }
        else
        {
            throw new NotImplementedException();
        }


        _driver.open();
        CheckResult();
    }

    public void Disconnect()
    {
        if (_driver == null)
        {
            return;
        }

        _driver.close();
        _driver.destroy();

        //высовобождение com-объекта
        Marshal.ReleaseComObject(_driver);
        _driver = null;
    }

    public void CashIn(decimal sum, Cashier cashier)
    {
        throw new NotImplementedException();
    }

    public void CashOut(decimal sum, Cashier cashier)
    {
        throw new NotImplementedException();
    }
}