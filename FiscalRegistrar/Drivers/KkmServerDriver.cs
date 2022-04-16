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

    public bool SendCommand(KkmServerCommand command)
    {

        command.Timeout = 1;

        if (!DoCommand(command))
        {
            return false;
        }

        if (command.DeviceAnswer.Status.IsEither(1, 4))
        {
            bool r;
            do
            {
                Thread.Sleep(1_00);
                r = GetResult(command);
            } while (command.DeviceAnswer.Status.IsEither(1, 4) && r);
        }



        var result = command.DeviceAnswer.Status == 0;

        if (result == false)
        {
            throw new Exception( command.DeviceAnswer.Error);
        }

        return true;
    }

    private bool GetResult(KkmServerCommand command)
    {
        var c = new GetResultCommand { IdCommand = command.IdCommand, Timeout = 10 };
        var r = DoCommand(c);

        var rez = (dynamic)JsonNode.Parse(c.Answer);


        command.Answer = rez["Rezult"].ToString();

        //почему здесь так? наверно можно десериализовать весь объек же
        command.DeviceAnswer.Status = JsonSerializer.Deserialize<DeviceAnswer>(command.Answer).Status;
        command.DeviceAnswer.Error = JsonSerializer.Deserialize<DeviceAnswer>(command.Answer).Error;

        return r;
    }

    private bool DoCommand(KkmServerCommand command)
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

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Credentials = credentialCache;
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            httpWebRequest.Timeout = 120_000;
            httpWebRequest.ReadWriteTimeout = 120_000;
            httpWebRequest.KeepAlive = false;
            httpWebRequest.Headers.Add(HttpRequestHeader.CacheControl, "must-revalidate");
            var ct = JsonSerializer.Serialize((object)command);

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(ct);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            httpResponse.Headers.Add(HttpResponseHeader.CacheControl, "must-revalidate");
            using (var streamReader =
                new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                command.Answer = streamReader.ReadToEnd();
            }


            command.DeviceAnswer = JsonSerializer.Deserialize<DeviceAnswer>(command.Answer);

            return true;
   
    }

    private interface IKkmSeverCommand
    {
        string Command { get; }


    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class KkmServerCommand 
    {


        [JsonIgnore] public string Answer { get; set; }

        [JsonIgnore] public DeviceAnswer DeviceAnswer { get; set; }

     
        public Guid IdCommand { get; set; } = Guid.NewGuid();

        public int Timeout { get; set; } = 60_000;
    }

    public class KkmOpenDrawer : KkmServerCommand, IKkmSeverCommand
    {
        public string Command { get; } = @"OpenCashDrawer";
    }

    public class KkmGetInfo : KkmServerCommand, IKkmSeverCommand
    {
        [JsonIgnore] public KkmInfo Data => JsonSerializer.Deserialize<KkmInfo>(Answer);

        public string Command { get; set; } = @"GetDataKKT";

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
        public class KkmInfo
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
    }

    private class GetResultCommand : KkmServerCommand, IKkmSeverCommand
    {
        public string Command { get; } = @"GetRezult";
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class KkmCashOut : KkmServerCommand, IKkmSeverCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public decimal Amount { get; set; }

        public string Command { get; } = @"PaymentCash";
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class KkmCashIn : KkmServerCommand, IKkmSeverCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public decimal Amount { get; set; }

        public string Command { get; } = @"DepositingCash";
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class KkmCloseShift : KkmServerCommand, IKkmSeverCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public string Command { get; } = @"CloseShift";
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class KkmOpenSession : KkmServerCommand, IKkmSeverCommand
    {
        public string CashierName { get; set; }

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public string CashierVATIN { get; set; }

        public string Command { get; } = @"OpenShift";
    }

    public class KkmGetXReport : KkmServerCommand, IKkmSeverCommand
    {
        public string Command { get; } = @"XReport";
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class KkmPrintCheck : KkmServerCommand, IKkmSeverCommand
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

        public string Command { get; } = @"RegisterCheck";

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

    public class TerminalDoPayment : KkmServerCommand, IKkmSeverCommand
    {
        //CardNumber

        public decimal Amount { get; set; }

        public string ReceiptNumber { get; set; }

        [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

        public string Command { get; } = @"PayByPaymentCard";

        // ReSharper disable once ClassNeverInstantiated.Global
        public class Information
        {
            public string Slip { get; set; }

            // ReSharper disable once InconsistentNaming
            public string RRNCode { get; set; }

            public string AuthorizationCode { get; set; }
        }
    }

    public class TerminalReturnPayment : KkmServerCommand, IKkmSeverCommand
    {
        //CardNumber

        public decimal Amount { get; set; }

        public string ReceiptNumber { get; set; }

        [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

        public string Command { get; } = @"ReturnPaymentByPaymentCard";

        // ReSharper disable once ClassNeverInstantiated.Global
        public class Information
        {
            public string Slip { get; set; }

            // ReSharper disable once InconsistentNaming
            public string RRNCode { get; set; }

            public string AuthorizationCode { get; set; }
        }
    }

    public class TerminalCloseSession : KkmServerCommand, IKkmSeverCommand
    {
        [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

        public string Command { get; } = @"Settlement";

        // ReSharper disable once ClassNeverInstantiated.Global
        public class Information
        {
            public string Slip { get; set; }
        }
    }

    public class TerminalGetReport : KkmServerCommand, IKkmSeverCommand
    {
        public bool Detailed { get; set; } = true;

        [JsonIgnore] public Information Data => JsonSerializer.Deserialize<Information>(Answer);

        public string Command { get; } = @"TerminalReport";

        // ReSharper disable once ClassNeverInstantiated.Global
        public class Information
        {
            public string Slip { get; set; }
        }
    }

    public class DeviceAnswer
    {
        public int Status { get; set; }

        public string Error { get; set; }

        public string IdCommand { get; set; } 
    }
}