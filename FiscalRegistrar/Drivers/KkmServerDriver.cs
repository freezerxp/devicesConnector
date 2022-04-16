using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Xml;

namespace devicesConnector.Wrappers;

public class KkmServerDriver
{
    private DeviceConnection.LanConnection _lanConnection;

    public KkmServerDriver(DeviceConnection.LanConnection lanConnection)
    {
        _lanConnection = lanConnection;
    }

    public KkmServerAnswer SendCommand(KkmServerCommand command)
    {
        command.Timeout = 1;

       var r= DoCommand(command);


        while (r.Status.IsEither(1, 4))
        {
            Thread.Sleep(1_00);
            r=GetResult(command);
        }

        if (r.Status != 0)
        {
            throw new Exception(r.Error);
        }

        return r;
    }

    private KkmServerAnswer GetResult(KkmServerCommand command)
    {
        var c = new GetResultCommand {IdCommand = command.IdCommand, Timeout = 10};
        return DoCommand(c);

        //var rez = (dynamic) JsonNode.Parse(c.Answer);


        //command.Answer = rez["Rezult"].ToString();

        ////почему здесь так? наверно можно десериализовать весь объек же
        //command.DeviceAnswer.Status = (int) rez["Status"];
        //command.DeviceAnswer.Error = rez["Error"].ToString();
    }

    private KkmServerAnswer DoCommand(KkmServerCommand command)
    {
        var urlAddress = _lanConnection.HostUrl;

        if (urlAddress.ToLower().StartsWith(@"http://") == false)
        {
            urlAddress = @"http://" + urlAddress;
        }

        var url = urlAddress + @":" + _lanConnection.PortNumber + @"/Execute";

        var credentialCache = new CredentialCache
        {
            {
                new Uri(url), "Basic",
                new NetworkCredential(_lanConnection.UserLogin, _lanConnection.UserPassword)
            }
        };

        var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
        httpWebRequest.Credentials = credentialCache;
        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        httpWebRequest.Timeout = 120_000;
        httpWebRequest.ReadWriteTimeout = 120_000;
        httpWebRequest.KeepAlive = false;
        httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "must-revalidate");

        var ct = JsonSerializer.Serialize((object) command);

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(ct);
        }

        var httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
        httpResponse.Headers.Add(HttpResponseHeader.CacheControl, "must-revalidate");

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

        ///// <summary>
        ///// Ответ todo: вынести в отдельный класс
        ///// </summary>
        //[JsonIgnore]
        //public string Answer { get; set; }

        ///// <summary>
        ///// Ответ устройства todo: вынести в отдельный класс
        ///// </summary>
        //[JsonIgnore]
        //public DeviceAnswer DeviceAnswer { get; set; }

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

    public class KkmGetInfo : KkmServerCommand
    {
     

        public override string Command { get; } = @"GetDataKKT";


    }

    private class GetResultCommand : KkmServerCommand
    {
        public override string Command { get; } = @"GetRezult";
    }

    public class KkmCashOut : KkmServerCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public decimal Amount { get; set; }

        public override string Command { get; } = @"PaymentCash";
    }

    public class KkmCashIn : KkmServerCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public decimal Amount { get; set; }

        public override string Command { get; } = @"DepositingCash";
    }

    public class KkmCloseShift : KkmServerCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public override string Command { get; } = @"CloseShift";
    }

    public class KkmOpenSession : KkmServerCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public override string Command { get; } = @"OpenShift";
    }

    public class KkmGetXReport : KkmServerCommand
    {
        public override string Command { get; } = @"XReport";
    }

    public class KkmPrintCheck : KkmServerCommand
    {
        public bool IsFiscalCheck { get; set; }

        public int TypeCheck { get; set; }

        // ReSharper disable once UnusedMember.Global
        public bool NotPrint { get; set; }

        //public int NumberCopies { get; set; }

        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public string ClientAddress { get; set; }

        public string ClientInfo { get; set; }

        public string ClientINN { get; set; }

        public int? TaxVariant { get; set; }

        public decimal Cash { get; set; }

        public decimal ElectronicPayment { get; set; }

        public decimal AdvancePayment { get; set; }

        public decimal Credit { get; set; }

        // ReSharper disable once CollectionNeverQueried.Global
        public List<CheckString> CheckStrings { get; set; } = new List<CheckString>();

        public override string Command { get; } = @"RegisterCheck";

        public class CheckString
        {
            public PrintText PrintText { get; set; }

            public PrintImage PrintImage { get; set; }

            public Register Register { get; set; }
        }

        public class Register
        {
            public string Name { get; set; }

            public decimal Quantity { get; set; }

            public decimal Price { get; set; }

            public decimal Amount { get; set; }

            public int Department { get; set; }

            public int Tax { get; set; }

            // public string EAN13 { get; set; }

            public int SignMethodCalculation { get; set; }

            public int SignCalculationObject { get; set; }

            public GoodCode GoodCodeData { get; set; } = null;

            public class GoodCode
            {
                // Тип товара. Список значений: "02" – изделия из меха, "03 - Лекарственные препараты", "05" - табачная продукция, "1520" - обувные товары
                public string StampType { get; set; } = null;

                // Глобальный идентификатор торговой единицы (GTIN) - поле 01 в GS1
                // ReSharper disable once IdentifierTypo
                // ReSharper disable once InconsistentNaming
                public string GTIN { get; set; } = null;

                // Серийный номер КИЗ - поле 21 в GS1
                public string SerialNumber { get; set; } = null;

                public string BarCode { get; set; } = null;
                public bool? ContainsSerialNumber { get; set; } = null;
                public bool? AcceptOnBad { get; set; } = null;
            }
        }

        public class PrintText
        {
            public string Text { get; set; }

            public int Font { get; set; }

            public int Intensity { get; set; }
        }

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

    public class KkmServerKktInfoAnswer
    {
       
            public int CheckNumber { get; set; }

            public int SessionNumber { get; set; }

            public Information Info { get; set; }

            public class Information
            {
                public int SessionState { get; set; }

                public decimal BalanceCash { get; set; }

                // ReSharper disable once InconsistentNaming
                public string Firmware_Version { get; set; }

                // ReSharper disable once InconsistentNaming
                public DateTime FN_DateEnd { get; set; }
            }
        
    }

    public class KkmServerAnswer
    {
        public JsonNode? Rezult { get; set; }

        public int Status { get; set; }
        public string Error { get; set; }

        public string IdCommand { get; set; }
    }

    public class DeviceAnswer
    {
        public int Status { get; set; }

        public string Error { get; set; }

        public string IdCommand { get; set; }
    }
}