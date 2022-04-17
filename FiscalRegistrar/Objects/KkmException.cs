using devicesConnector.Common;

namespace devicesConnector.FiscalRegistrar.Objects;

public class KkmException: Exception
{
    public Enums.ErrorTypes Error { get; set; }
    public int? KkmErrorCode { get; set; }
    public string? KkmErrorDescription { get; set; }

    public KkmException(string? msg, Enums.ErrorTypes error, int? kkmErrorCode, string? kkmErrorDescription): base(msg)
    {
        Error = error;
        KkmErrorCode = kkmErrorCode;
        KkmErrorDescription = kkmErrorDescription;
    }
}