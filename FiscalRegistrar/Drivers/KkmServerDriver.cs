using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using devicesConnector.Common;

namespace devicesConnector.FiscalRegistrar.Drivers;

/// <summary>
/// Драйвер ККМ Сервера
/// </summary>
public class KkmServerDriver
{
    /// <summary>
    /// Параметры подключения к ККМ Серверу
    /// </summary>
    private DeviceConnection.LanConnection _lanConnection;

    public KkmServerDriver(DeviceConnection.LanConnection lanConnection)
    {
        _lanConnection = lanConnection;
    }

    /// <summary>
    /// Отправка команды на ККМ Сервер
    /// </summary>
    /// <param name="command">Команда</param>
    /// <returns>Объект ответа</returns>
    /// <exception cref="Exception"></exception>
    public KkmServerAnswer SendCommand(KkmServerCommand command)
    {
        command.Timeout = 1;

        var r = DoCommand(command);


        while (r.Status.IsEither(1, 4))
        {
            Thread.Sleep(1_00);
            r = GetResult(command.IdCommand);
        }


        var rezult = r.Rezult.Deserialize<KkmServerAnswer>();

        if (rezult.Status != 0)
        {
            throw new Exception(rezult.Error);
        }

        return r;
    }

    /// <summary>
    /// Получение результата выполнения команды
    /// </summary>
    /// <param name="commandUid">Идентификатор команды</param>
    /// <returns></returns>
    private KkmServerAnswer GetResult(Guid commandUid)
    {
        var c = new GetResultCommand {IdCommand = commandUid, Timeout = 10};
        return DoCommand(c);
    }

    /// <summary>
    /// Отправка команды на ккм-сервер
    /// </summary>
    /// <param name="command">Команда</param>
    /// <returns>Объект ответа</returns>
    private KkmServerAnswer DoCommand(KkmServerCommand command)
    {
        var urlAddress = _lanConnection.HostUrl;

        if (urlAddress.ToLower().StartsWith(@"http://") == false)
        {
            urlAddress = @"http://" + urlAddress;
        }

        var url = urlAddress + @":" + _lanConnection.PortNumber + @"/Execute";

        //базовая авторизация
        var credentialCache = new CredentialCache
        {
            {
                new Uri(url), "Basic",
                new NetworkCredential(_lanConnection.UserLogin, _lanConnection.UserPassword)
            }
        };

        //создание запроса
        var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
        httpWebRequest.Credentials = credentialCache;
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        httpWebRequest.Timeout = 120_000;
        httpWebRequest.ReadWriteTimeout = 120_000;
        httpWebRequest.KeepAlive = false;
        httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "must-revalidate");

        //приведение к объекту для корректной сериализации
        var ct = JsonSerializer.Serialize((object) command);

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(ct);
        }

        var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
        httpResponse.Headers.Add(HttpResponseHeader.CacheControl, "must-revalidate");

        //читаем результат
        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        {
            var answer = streamReader.ReadToEnd();

            var ksa = JsonSerializer.Deserialize<KkmServerAnswer>(answer);

            return ksa;
        }
    }


    /// <summary>
    /// Абстрактный класс команды
    /// </summary>
    public abstract class KkmServerCommand
    {
        /// <summary>
        /// Имя команды
        /// </summary>
        public abstract string Command { get; }

        /// <summary>
        /// Идентификатор команды
        /// </summary>
        public Guid IdCommand { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Таймаут ожидания
        /// </summary>
        public int Timeout { get; set; } = 60_000;
    }

    /// <summary>
    /// Команда открытия денежного ящика
    /// </summary>
    public class KkmOpenDrawer : KkmServerCommand
    {
        public override string Command { get; } = @"OpenCashDrawer";
    }

    /// <summary>
    /// Получение информации о ККМ
    /// </summary>
    public class KkmGetInfo : KkmServerCommand
    {
        public override string Command { get; } = @"GetDataKKT";
    }

    /// <summary>
    /// Команда получения результата ранее выполненной команды
    /// </summary>
    private class GetResultCommand : KkmServerCommand
    {
        public override string Command { get; } = @"GetRezult";
    }

    /// <summary>
    /// Изъятие наличности
    /// </summary>
    public class KkmCashOut : KkmServerCommandWithCashier
    {
        /// <summary>
        /// Сумма изъятия
        /// </summary>
        public decimal Amount { get; set; }

        public override string Command { get; } = @"PaymentCash";
    }

    public class KkmServerCommandWithCashier : KkmServerCommand
    {
        /// <summary>
        /// Имя кассира
        /// </summary>
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// ИНН Кассира
        /// </summary>
        public string CashierVATIN { get; set; }

        public override string Command { get; }
    }

    /// <summary>
    /// Команда внесения наличности
    /// </summary>
    public class KkmCashIn : KkmServerCommandWithCashier
    {
        /// <summary>
        /// Сумма внесения
        /// </summary>
        public decimal Amount { get; set; }

        public override string Command { get; } = @"DepositingCash";
    }

    /// <summary>
    /// Команда закрытия смены
    /// </summary>
    public class KkmCloseShift : KkmServerCommandWithCashier
    {
        public override string Command { get; } = @"CloseShift";
    }

    /// <summary>
    /// Команда открытия смены
    /// </summary>
    public class KkmOpenSession : KkmServerCommandWithCashier
    {
        public override string Command { get; } = @"OpenShift";
    }

    /// <summary>
    /// Команда Х-отчет
    /// </summary>
    public class KkmGetXReport : KkmServerCommand
    {
        public override string Command { get; } = @"XReport";
    }

    /// <summary>
    /// Команда печати чека
    /// </summary>
    public class KkmPrintCheck : KkmServerCommandWithCashier
    {
        /// <summary>
        /// Фискальный чек?
        /// </summary>
        public bool IsFiscalCheck { get; set; }

        /// <summary>
        /// Тип чека
        /// </summary>
        public int TypeCheck { get; set; }

        /// <summary>
        /// Не печатать?
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public bool NotPrint { get; set; }

        /// <summary>
        /// Адрес клиента для отправки электронного чека
        /// </summary>
        public string ClientAddress { get; set; }

        /// <summary>
        /// Информация о клиенте
        /// </summary>
        public string ClientInfo { get; set; }

        /// <summary>
        /// ИНН клиента
        /// </summary>
        public string ClientINN { get; set; }

        /// <summary>
        /// СНО
        /// </summary>
        public int? TaxVariant { get; set; }

        /// <summary>
        /// Сумма оплаты наличными
        /// </summary>
        public decimal Cash { get; set; }

        /// <summary>
        /// Сумма оплаты электронными
        /// </summary>
        public decimal ElectronicPayment { get; set; }

        /// <summary>
        /// Сумма оплаты авансом (предоплата)
        /// </summary>
        public decimal AdvancePayment { get; set; }

        /// <summary>
        /// Сумма оплаты кредитом
        /// </summary>
        public decimal Credit { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public List<CheckString> CheckStrings { get; set; } = new List<CheckString>();

        public override string Command { get; } = @"RegisterCheck";

        /// <summary>
        /// Строка в чеке
        /// </summary>
        public class CheckString
        {
            public PrintText PrintText { get; set; }

            public PrintImage PrintImage { get; set; }

            public Register Register { get; set; }
        }

        /// <summary>
        /// Фискальная позиция в чеке
        /// </summary>
        public class Register
        {
            /// <summary>
            /// Название
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Кол-во
            /// </summary>
            public decimal Quantity { get; set; }

            /// <summary>
            /// Цена БЕЗ скидки
            /// </summary>
            public decimal Price { get; set; }

            /// <summary>
            /// Конечная сумма позиции
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Отдел, секция
            /// </summary>
            public int Department { get; set; }

            /// <summary>
            /// Тип налога
            /// </summary>
            public int Tax { get; set; }

            // public string EAN13 { get; set; }

            /// <summary>
            /// Признак способа расчета. тег ОФД 1214
            /// </summary>
            public int SignMethodCalculation { get; set; }

            /// <summary>
            /// Признак предмета расчета. тег ОФД 1212
            /// </summary>
            public int SignCalculationObject { get; set; }

            /// <summary>
            /// Информация о маркировке
            /// </summary>
            public GoodCode GoodCodeData { get; set; } = null;

            public class GoodCode
            {
              
                /// <summary>
                /// Полный код маркировки
                /// </summary>
                public string BarCode { get; set; } = null;

                // Для некоторых товаров нужно передавать ШК EAN-13, тогда это поле устанавливайте в 'false'
                public bool? ContainsSerialNumber { get; set; } = null;
                public bool? AcceptOnBad { get; set; } = null;
            }
        }

        /// <summary>
        /// Строка текста
        /// </summary>
        public class PrintText
        {
            /// <summary>
            /// Текст
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Шрифт
            /// </summary>
            public int Font { get; set; }

            public int Intensity { get; set; }
        }

        /// <summary>
        /// Картина в base 64
        /// </summary>
        public class PrintImage
        {
            public string Image { get; set; }
        }
    }

    //public class TerminalDoPayment : KkmServerCommand
    //{
    //    //CardNumber

    //    public decimal Amount { get; set; }

    //    public string ReceiptNumber { get; set; }

    //    [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

    //    public override string Command { get; } = @"PayByPaymentCard";

    //    // ReSharper disable once ClassNeverInstantiated.Global
    //    public class Information
    //    {
    //        public string Slip { get; set; }

    //        // ReSharper disable once InconsistentNaming
    //        public string RRNCode { get; set; }

    //        public string AuthorizationCode { get; set; }
    //    }
    //}

    //public class TerminalReturnPayment : KkmServerCommand
    //{
    //    //CardNumber

    //    public decimal Amount { get; set; }

    //    public string ReceiptNumber { get; set; }

    //    [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

    //    public override string Command { get; } = @"ReturnPaymentByPaymentCard";

    //    // ReSharper disable once ClassNeverInstantiated.Global
    //    public class Information
    //    {
    //        public string Slip { get; set; }

    //        // ReSharper disable once InconsistentNaming
    //        public string RRNCode { get; set; }

    //        public string AuthorizationCode { get; set; }
    //    }
    //}

    //public class TerminalCloseSession : KkmServerCommand
    //{
    //    [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

    //    public override string Command { get; } = @"Settlement";

    //    // ReSharper disable once ClassNeverInstantiated.Global
    //    public class Information
    //    {
    //        public string Slip { get; set; }
    //    }
    //}

    //public class TerminalGetReport : KkmServerCommand
    //{
    //    public bool Detailed { get; set; } = true;

    //    [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

    //    public override string Command { get; } = @"TerminalReport";

    //    // ReSharper disable once ClassNeverInstantiated.Global
    //    public class Information
    //    {
    //        public string Slip { get; set; }
    //    }
    //}

    /// <summary>
    /// Ответ на запрос состояния ККМ
    /// </summary>
    public class KkmServerKktInfoAnswer : KkmServerAnswer
    {
        /// <summary>
        /// Номер чека
        /// </summary>
        public int CheckNumber { get; set; }

        /// <summary>
        /// Номер смены
        /// </summary>
        public int SessionNumber { get; set; }

        /// <summary>
        /// Информация
        /// </summary>
        public Information Info { get; set; }

        public class Information
        {
            /// <summary>
            /// Состояние смены
            /// </summary>
            public int SessionState { get; set; }

            /// <summary>
            /// Сумма наличных
            /// </summary>
            public decimal BalanceCash { get; set; }

            /// <summary>
            /// Версия прошивки
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public string Firmware_Version { get; set; }

            /// <summary>
            /// Дата окончания ФН
            /// </summary>
            // ReSharper disable once InconsistentNaming
            public DateTime FN_DateEnd { get; set; }
        }
    }

    /// <summary>
    /// Ответ ккм сервера на выполненную команду
    /// </summary>
    public class KkmServerAnswer
    {
        /// <summary>
        /// Объект ответа команды
        /// </summary>
        public JsonNode? Rezult { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Текст ошибки
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Идентификатор выполненной команды
        /// </summary>
        public string IdCommand { get; set; }
    }
}