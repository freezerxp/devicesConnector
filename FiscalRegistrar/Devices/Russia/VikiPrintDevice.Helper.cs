using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.FiscalRegistrar.Helpers;
using devicesConnector.FiscalRegistrar.Objects;
using devicesConnector.FiscalRegistrar.Objects.CountrySpecificData.Russia;
using devicesConnector.Helpers;

namespace devicesConnector.FiscalRegistrar.Devices.Russia;

public partial class VikiPrintDevice
{
    /// <summary>
    /// Типы документов
    /// </summary>
    private enum DocTypes
    {
        /// <summary>
        /// Сервисный (нефискальный)
        /// </summary>
        Service = 1,

        /// <summary>
        /// Продажа
        /// </summary>
        SaleCheck,

        /// <summary>
        /// Возврат
        /// </summary>
        ReturnCheck,

        /// <summary>
        /// Внесение
        /// </summary>
        CashIncome,

        /// <summary>
        /// Изъятие
        /// </summary>
        CashOutcome
    }


    private enum RequestType
    {
        CounterAndRegisters,

        KkmInfo
    }

    /// <summary>
    /// Преобразование строки в дату
    /// </summary>
    /// <param name="str">Строка дата/время</param>
    /// <returns></returns>
    public static DateTime GetDateTimeFromString(string str)
    {
        var df = @".ddMMyy.HHmmss";

        if (str.Length == 7)
        {
            df = @".ddMMyy";
        }


        if (!DateTime.TryParseExact(str, df, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (date.Year < 2000)
        {
            date = date.AddYears(2000);
        }

        //var date = DateTime.ParseExact(str, "dd.MM.yy.HH.mm.ss", System.Globalization.CultureInfo.InvariantCulture);


        return date;
    }

    /// <summary>
    /// Подготовка ФИО + ИНН кассира
    /// (с преобразованием кодировки 1251 -> 866
    /// </summary>
    /// <param name="cashier"></param>
    /// <returns></returns>
    private string PrepareCashierNameAndInn(Cashier cashier)
    {
        var ffdV = _deviceConfig.DeviceSpecificConfig.Deserialize<KkmConfig>()?.FfdVersion;


        string result;

        if (cashier.TaxId is {Length: >= 10} && ffdV > Enums.FFdVersions.Ffd100)
        {
            result = cashier.TaxId + @"&" + cashier.Name;
        }
        else
        {
            result = cashier.Name;
        }

        result = C1251To866(result);

        return result;
    }


    private void SendDigitalCheck(string address)
    {
        if (address.IsNullOrEmpty())
        {
            return;
        }


        var r = lib_setClientAddress(address);


        CheckResult(r);


        //включение/отключение печати чека
        //if ()
        //{
        //    r = Driver.SetPrintCheck(129);
        //    CheckResult(r);
        //}
    }

    private void Ffd120CodeValidation(ReceiptData receipt)
    {

        foreach (var item in receipt.Items)
        {
            var ruInfo = item.CountrySpecificData.Deserialize<Objects.CountrySpecificData.Russia.ReceiptItemData>();

            if (ruInfo?.MarkingInfo == null)
            {
                continue;
            }

            ruInfo.MarkingInfo.ValidationResultKkm = 0;

            var markCode = RuKkmHelper.PrepareMarkCodeForFfd120(ruInfo.MarkingInfo.RawCode);

            var status = RuKkmHelper.GetMarkingCodeStatus(item, receipt.OperationType);

            MarkCodeValidation(out var validationResult, markCode, item.Quantity, (int)status, (int)ruInfo.FfdData.Unit);

            ruInfo.MarkingInfo.ValidationResultKkm = validationResult;
            item.CountrySpecificData = JsonSerializer.SerializeToNode(ruInfo);

            AcceptMarkingCode();
        }



    
    }


    /// <summary>
    /// Получить информацию из ККМ
    /// </summary>
    /// <param name="requestNumber">Номер запроса</param>
    /// <param name="type">Тип запроса</param>
    /// <param name="answer">Ответ</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static bool GetInfo(ushort requestNumber, RequestType type, out string answer)
    {
        var ans = new MData();


        switch (type)
        {
            case RequestType.CounterAndRegisters:
                lib_getCountersAndRegisters(ref ans, requestNumber);

                break;
            case RequestType.KkmInfo:
                lib_getKktInfo(ref ans, requestNumber);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }


        if (ans.errCode == 0)
        {
            answer = DataToString(ans);
            LogHelper.Write($"answer: {answer}; errorCode: {ans.errCode}");
            return true;
        }

        answer = string.Empty;
        return false;
    }

    /// <summary>
    /// Получить список статусов
    /// </summary>
    /// <param name="fatalStatus">Фатальный статус</param>
    /// <param name="currentStatus">Текущий статус</param>
    /// <param name="documentStatus">Статус документа</param>
    /// <returns></returns>
    public static int GetListOfStatuses(out int[] fatalStatus, out int[] currentStatus,
        out int[] documentStatus)
    {
        var errCode = getStatusFlags(out var fatalS, out var curS, out var docS);

        fatalStatus = fatalS.ToBitsArray();
        currentStatus = curS.ToBitsArray(9);

        var docString = Convert.ToString(docS, 2)
            .PadLeft(8, '0');

        documentStatus = new[]
        {
            ConvertFromBinary(docString.Substring(4, 4)),
            ConvertFromBinary(docString.Substring(0, 4))
        };


        return errCode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static int ConvertFromBinary(string input)
    {
        return Convert.ToInt32(input, 2);
    }


    /// <summary>
    /// Установка реквизита маркировки для ФФД ниже 1.2
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool SetMarkedRequisite(ReceiptItemData.RuMarkingInfo info)
    {
        throw new NotSupportedException();

        //var hex = info.GetHexStringAttribute();

        //var l = hex.Split(@" ".ToCharArray()[0]);

        //var newHexStr = string.Empty;

        //foreach (var s in l)
        //{
        //    newHexStr += @"$" + s.ToUpper();
        //}


        //var r = libSetExtraRequisite(newHexStr);
        //return r == 0;
    }


    /// <summary>
    /// Напечатать нефискальную строку
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static int PrintString(string text)
    {
        var r = lib_printString(C1251To866(text), 1);
        //LogHelper.Debug("Код результата печати строки: " + _resultCode);
        return r
            ;
    }




    /// <summary>
    /// Регистрация позиции
    /// </summary>
    /// <param name="name"></param>
    /// <param name="barcode"></param>
    /// <param name="quantity"></param>
    /// <param name="price"></param>
    /// <param name="taxRateNumber"></param>
    /// <param name="sectionNumber"></param>
    /// <param name="ffdPaymentMode"></param>
    /// <param name="ffdGoodType"></param>
    /// <returns></returns>
    public static int AddPosition(string name, string barcode, decimal quantity, decimal price,
        int taxRateNumber, int sectionNumber, int ffdPaymentMode,
        int ffdGoodType)
    {
        var _resultCode = lib_addPositionEx(C1251To866(name), barcode, (double) quantity, (double) price,
            (byte) taxRateNumber, 0, (byte) sectionNumber,
            ffdPaymentMode, ffdGoodType);


        return _resultCode;
    }


 


    private static Encoding CP866()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var enc = Encoding.GetEncoding(866);

        return enc;
    }

    /// <summary>
    /// Преобразование в строку
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static string DataToString(MData data)
    {
        try
        {
            var newB2 = new List<byte>();


            for (var i = 8; i <= data.dataLength - 5; i++)
            {
                if (data.data[i] >= 32)
                {
                    newB2.Add(data.data[i]);
                }

                if (data.data[i] == 28)
                {
                    newB2.Add(46);
                }
            }


            var r = CP866().GetString(newB2.ToArray());


            return r;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return string.Empty;
        }
    }

    /// <summary>
    /// Конвертация кодировки строки
    /// </summary>
    /// <param name="str1251"></param>
    /// <returns></returns>
    private static string C1251To866(string str1251)
    {
        var bytes = CP866().GetBytes(str1251);
        var newBytes = Encoding.Convert(Encoding.GetEncoding(866), CP866(), bytes);
        return Encoding.GetEncoding(1251).GetString(newBytes);
    }

    /// <summary>
    /// Открыть документ
    /// </summary>
    /// <param name="docType"></param>
    /// <param name="section"></param>
    /// <param name="cashierName"></param>
    /// <param name="taxSystem"></param>
    /// <returns></returns>
    private static int OpenDocument(DocTypes docType, int section, string cashierName,
        int taxSystem = 0)
    {
        if (taxSystem == 0)
        {
            return Lib_openDocument((int) docType, section, cashierName);
        }

        return lib_openDocumentEx((int) docType, section, cashierName, 0,
            taxSystem);
    }

    private static int CloseDocument()
    {
        var ans = new MData();
        lib_closeDocument(ref ans, 0);

        return ans.errCode;
    }


    private static int MarkCodeValidation(out int result , string fullCode, decimal q, int itemState, int unit,
        string qFractional = "")
    {
        var ans = new MData();
        result = 0;
        var qStr = q.ToString(CultureInfo.InvariantCulture);


        lib_MarkCodeValidation(ref ans, C1251To866(fullCode), qStr, itemState, unit);
        
        var rs = DataToString(ans);

        if (rs.IsNullOrEmpty() == false)
        {
            var arr = rs.Split('.');

            if (arr.Any() && int.TryParse(arr[0], out var num))
            {
                result = num;
            }
        }

        return ans.errCode;
    }

    private static int AcceptMarkingCode()
    {
        var ans = new MData();

        lib_ConfirmMarkCode(ref ans);

        return ans.errCode;
    }

    private static int AddItemMarkingCode(string fullCode, int itemState, int unit, int validResult)
    {
        var r = lib_AddMarkCode(fullCode, itemState, unit, validResult);

        return r;
    }



    private static int SetClientAddress(string address)
    {
        if (address.IsNullOrEmpty())
        {
            return 0;
        }

        var r = lib_setClientAddress(address);
        return r;
    }

    private static bool SetClientInn(string inn)
    {
        if (inn.IsNullOrEmpty())
        {
            return true;
        }

        var r = lib_SetBuyerInn(inn);
        return r == 0;
    }

    private static bool SetClientName(string name)
    {
        if (name.IsNullOrEmpty())
        {
            return true;
        }

        var r = lib_SetBuyerInn(name);
        return r == 0;
    }

    private static int SetPrintCheck(int i)
    {
        var r = lib_WriteSettingsTable(1, 7, $"{i}");
        return r;
    }


    private bool IsNeedInitialization()
    {
        var los = GetListOfStatuses(out var fatal, out var current, out var document);

        CheckResult(los);

        return current[0] == 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MData
    {
        // unsafe
        public int errCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] data;

        public int dataLength;
    }


    #region DllImports

    /// <summary>
    /// Относительный путь к dll
    /// </summary>
    private const string PiritlibDllPath = "dll\\kkm\\vikiprint\\PiritLib.dll";

    /// <summary>
    /// Открыть порт
    /// </summary>
    /// <param name="port">Имя порта</param>
    /// <param name="speed">Скорость</param>
    /// <returns></returns>
    // не требуют преобразования текста
    [DllImport(PiritlibDllPath, EntryPoint = "openPort", CallingConvention = CallingConvention.StdCall)]
    private static extern int lib_OpenPort(string port, int speed);

    /// <summary>
    /// Промотка бумаги
    /// </summary>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "scrollPaper")]
    private static extern int lib_ScrollPaper();

    /// <summary>
    /// Закрыть порт
    /// </summary>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "closePort", CallingConvention = CallingConvention.StdCall)]
    private static extern int lib_ClosePort();

    /// <summary>
    /// Промежуточный итог
    /// </summary>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libSubTotal")]
    private static extern int subTotal();

    /// <summary>
    /// Отрезать документ
    /// </summary>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libCutDocument")]
    private static extern int lib_cutDocument();

    /// <summary>
    /// Отменить документ
    /// </summary>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libCancelDocument")]
    private static extern int lib_CancelDocument();

    /// <summary>
    /// Напечатать нефискальную строку
    /// </summary>
    /// <param name="text">Теекст</param>
    /// <param name="font">Шрифт</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libPrintString")]
    private static extern int lib_printString(string text, int font);

    /// <summary>
    /// Напечатать х-отчет
    /// </summary>
    /// <param name="userName">ФИО сотрудника</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libPrintXReport")]
    private static extern int lib_xReport(string userName);

    /// <summary>
    /// Z-отчет (закрытие смены)
    /// </summary>
    /// <param name="userName">ФИО сотрудника</param>
    /// <param name="typeOfOrder">??</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libPrintZReport")]
    private static extern int lib_zReport(string userName, int typeOfOrder = 0);

    /// <summary>
    /// Открыть документ
    /// </summary>
    /// <param name="typeOfDoc">Тип документа</param>
    /// <param name="sectionNum">Номер секции</param>
    /// <param name="userName">ФИО кассира</param>
    /// <param name="docNum">Номер документа</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libOpenDocument", CallingConvention = CallingConvention.StdCall)]
    private static extern int Lib_openDocument(int typeOfDoc, int sectionNum, string userName, int docNum = 0);

    /// <summary>
    /// Расширенный метода открытия документа
    /// </summary>
    /// <param name="typeOfDoc">Тип документа</param>
    /// <param name="sectionNum">Номер секции</param>
    /// <param name="userName">ФИО кассира</param>
    /// <param name="docNum">Номер документа</param>
    /// <param name="taxN">Номер СНО?</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libOpenDocumentEx", CallingConvention = CallingConvention.StdCall)]
    private static extern int lib_openDocumentEx(int typeOfDoc, int sectionNum, string userName, int docNum = 0,
        int taxN = 0);

    /// <summary>
    /// Получить доп. информацию
    /// </summary>
    /// <param name="numRequest">Номер запроса</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libGetExErrorInfo")]
    private static extern int lib_getExErrorInfo(byte numRequest);

    /// <summary>
    /// Записать в таблицу настроек
    /// </summary>
    /// <param name="number">Номер</param>
    /// <param name="index">Индекс</param>
    /// <param name="data"Данные></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libWriteSettingsTable")]
    private static extern int lib_WriteSettingsTable(byte number, int index, string data);

    /// <summary>
    /// Внесение/изъятие наличности
    /// </summary>
    /// <param name="info">Информация?</param>
    /// <param name="sum">Сумма</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libCashInOut")]
    private static extern int lib_cashInOut(string info, long sum);

    /// <summary>
    /// Инициализация ККМ
    /// </summary>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "commandStart")]
    private static extern int lib_commandStart();

    /// <summary>
    /// Регистрация позиции ФФД 1.0
    /// </summary>
    /// <param name="goodName">Название товара</param>
    /// <param name="barcode">Штрих-код</param>
    /// <param name="qty">кол-во</param>
    /// <param name="price">Цена</param>
    /// <param name="taxNum">Индекс НДС</param>
    /// <param name="numGoodsPos">??</param>
    /// <param name="numDepart">Номер секции</param>
    /// <param name="coefType">??</param>
    /// <param name="coefName">??</param>
    /// <param name="coefValue">??</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libAddPosition", CallingConvention = CallingConvention.StdCall)]
    private static extern int lib_addPosition(string goodName, string barcode, double qty, double price,
        byte taxNum, int numGoodsPos = 0, byte numDepart = 0, byte coefType = 0, string coefName = "",
        double coefValue = 0);

    /// <summary>
    /// Регистрация позиции ФФД 105/110
    /// </summary>
    /// <param name="goodName"></param>
    /// <param name="barcode"></param>
    /// <param name="count"></param>
    /// <param name="price"></param>
    /// <param name="taxNum"></param>
    /// <param name="numGoodsPos"></param>
    /// <param name="numDepart"></param>
    /// <param name="signMethodCalculation"></param>
    /// <param name="signCalculationObject"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libAddPositionEx", CallingConvention = CallingConvention.StdCall)]
    private static extern int lib_addPositionEx(string goodName, string barcode, double count, double price,
        byte taxNum, int numGoodsPos = 0, byte numDepart = 0, int signMethodCalculation = 4,
        int signCalculationObject = 1);


    /// <summary>
    /// Регистрация позиции ФФД 1.2
    /// </summary>
    /// <param name="goodName"></param>
    /// <param name="barcode"></param>
    /// <param name="count"></param>
    /// <param name="price"></param>
    /// <param name="taxNumber"></param>
    /// <param name="numGoodsPos"></param>
    /// <param name="numDepart"></param>
    /// <param name="coefType"></param>
    /// <param name="coefValue"></param>
    /// <param name="signMethodCalculation"></param>
    /// <param name="signCalculationObject"></param>
    /// <param name="quantityName"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libAddPositionLarge", CallingConvention = CallingConvention.StdCall)]
    private static extern int lib_addPositionLarge(string goodName, string barcode, double count, double price,
        byte taxNumber, int numGoodsPos = 0, byte numDepart = 0, byte coefType = 0, double coefValue = 0,
        int signMethodCalculation = 4, int signCalculationObject = 1, string quantityName = "");

    /// <summary>
    /// Регистрация платежа
    /// </summary>
    /// <param name="typeOfPayment">Тип платежа</param>
    /// <param name="sum">Сумма</param>
    /// <param name="comment">Комментарий</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libAddPayment")]
    private static extern int lib_addPayment(byte typeOfPayment, long sum, string comment);

    /// <summary>
    /// Закрыть документ
    /// </summary>
    /// <param name="md"></param>
    /// <param name="cutPaper">Отрезать?</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libCloseDocument")]
    private static extern int lib_closeDocument(ref MData md, int cutPaper = 1);

    /// <summary>
    /// Получить информацию о ККМ
    /// </summary>
    /// <param name="data">Ссылка на данные для записи ответа</param>
    /// <param name="numRequest">Номер запроса</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libGetKKTInfo")]
    private static extern int lib_getKktInfo(ref MData data, ushort numRequest);

    /// <summary>
    /// Получить значение счетчиков
    /// </summary>
    /// <param name="data">Ссылка на данные для записи ответа</param>
    /// <param name="numRequest">Номер запроса</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libGetCountersAndRegisters")]
    private static extern int lib_getCountersAndRegisters(ref MData data, ushort numRequest);

    /// <summary>
    /// Добавить скидку
    /// </summary>
    /// <param name="type">Тип</param>
    /// <param name="discountName">Название скидки</param>
    /// <param name="sum">Сумма</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libAddDiscount")]
    private static extern int lib_addDiscount(byte type, string discountName, int sum);

    /// <summary>
    /// Получить значение статусов
    /// </summary>
    /// <param name="fatalStatus"></param>
    /// <param name="currentFlagStatus"></param>
    /// <param name="documentStatus"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "getStatusFlags")]
    private static extern int getStatusFlags(out int fatalStatus, out int currentFlagStatus,
        out int documentStatus);

    /// <summary>
    /// Отправка электронного чека покупателю
    /// </summary>
    /// <param name="buyerAddress">Адрес для отправки чека</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libSetBuyerAddress")]
    private static extern int lib_setClientAddress(string buyerAddress);

    /// <summary>
    /// Установить ФИО покупателя
    /// </summary>
    /// <param name="buyerName">ФИО покупателя</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libSetBuyerName")]
    private static extern int lib_SetBuyerName(string buyerName);

    /// <summary>
    /// Установить ИНН покупателя
    /// </summary>
    /// <param name="buyerInn">ИНН покупателя</param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libSetBuyerInn")]
    private static extern int lib_SetBuyerInn(string buyerInn);

    /// <summary>
    /// Напечатать реквизит ОФД
    /// </summary>
    /// <param name="codeReq"></param>
    /// <param name="attributeText"></param>
    /// <param name="reqName"></param>
    /// <param name="reqSt"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libPrintRequsitOFD")]
    private static extern int
        lib_PrintRequsitOFD(int codeReq, byte attributeText, string reqName, string reqSt);

    /// <summary>
    /// Установка дополнительного реквизита
    /// </summary>
    /// <param name="nomenclatureCode"></param>
    /// <param name="extReq"></param>
    /// <param name="measureName"></param>
    /// <param name="agentSign"></param>
    /// <param name="supplierINN"></param>
    /// <param name="supplierPhone"></param>
    /// <param name="supplierName"></param>
    /// <param name="operatorAddress"></param>
    /// <param name="operatorINN"></param>
    /// <param name="operatorName"></param>
    /// <param name="operatorPhone"></param>
    /// <param name="payAgentOperation"></param>
    /// <param name="payAgentPhone"></param>
    /// <param name="recOperatorPhone"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libSetExtraRequisite")]
    private static extern int libSetExtraRequisite(string nomenclatureCode, string extReq = " ",
        string measureName = " ",
        int agentSign = 0, string supplierINN = "000000000000", string supplierPhone = "",
        string supplierName = "", string operatorAddress = "",
        string operatorINN = "000000000000", string operatorName = "", string operatorPhone = "",
        string payAgentOperation = "",
        string payAgentPhone = "", string recOperatorPhone = "");

    /// <summary>
    /// Валидация кода маркировки
    /// </summary>
    /// <param name="data"></param>
    /// <param name="markCode"></param>
    /// <param name="quantity"></param>
    /// <param name="itemState"></param>
    /// <param name="quantityMode"></param>
    /// <param name="workMode"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libMarkCodeValidation")]
    private static extern int lib_MarkCodeValidation(ref MData data, string markCode, string quantity = "",
        int itemState = 0, int quantityMode = 0, int workMode = 1);

    /// <summary>
    /// Подтверждение КМ
    /// </summary>
    /// <param name="data"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libConfirmMarkCode")]
    private static extern int lib_ConfirmMarkCode(ref MData data, int mode = 1);

    /// <summary>
    /// Добавить КМ КМ
    /// </summary>
    /// <param name="markCode"></param>
    /// <param name="itemState"></param>
    /// <param name="quantityMode"></param>
    /// <param name="validationResult"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libAddMarkCode")]
    private static extern int lib_AddMarkCode(string markCode = "", int itemState = 0, int quantityMode = 0,
        int validationResult = 0);

    /// <summary>
    /// Установить значение доп. реквизита
    /// </summary>
    /// <param name="nomenclatureCode"></param>
    /// <param name="extReq"></param>
    /// <param name="measureName"></param>
    /// <param name="agentSign"></param>
    /// <param name="supplierINN"></param>
    /// <param name="supplierPhone"></param>
    /// <param name="supplierName"></param>
    /// <param name="operatorAddress"></param>
    /// <param name="operatorINN"></param>
    /// <param name="operatorName"></param>
    /// <param name="operatorPhone"></param>
    /// <param name="payAgentOperation"></param>
    /// <param name="payAgentPhone"></param>
    /// <param name="recOperatorPhone"></param>
    /// <returns></returns>
    [DllImport(PiritlibDllPath, EntryPoint = "libSetExtraRequisite")]
    private static extern int libSetExtraRequisite2(string nomenclatureCode, string extReq = "",
        string measureName = "",
        int agentSign = 0, string supplierINN = "", string supplierPhone = "", string supplierName = "",
        string operatorAddress = "",
        string operatorINN = "", string operatorName = "", string operatorPhone = "",
        string payAgentOperation = "",
        string payAgentPhone = "", string recOperatorPhone = "");

    #endregion
}