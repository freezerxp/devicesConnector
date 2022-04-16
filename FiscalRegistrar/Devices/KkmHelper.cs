using devicesConnector.FiscalRegistrar.Drivers.Russia;

namespace devicesConnector.Drivers;

public class KkmHelper
{
    private DeviceConnection _connection;

    public KkmHelper(DeviceConnection connection)
    {
        _connection = connection;
    }


    public KkmStatus GetStatus()
    {
        IFiscalRegistrarDevice kkm;

        kkm = new KkmServerDevice(_connection.Lan);

        return kkm.GetStatus();

    }


    public void PrintNonFiscalReceipt(List<string> data)
    {

    }

    
}