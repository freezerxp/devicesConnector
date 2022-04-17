
namespace devicesConnector.Common;

public  class DeviceException:Exception
{
    public int ErrorCode { get; set; }
    public string Message { get; set; }

    public DeviceException(int errorCode, string message)
    {
        ErrorCode = errorCode;
        Message = message;
    }
}